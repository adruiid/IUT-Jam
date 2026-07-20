using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Single source of truth for interaction + highlight on the Player. Each frame it
/// tracks TWO targets and outlines both:
///   - Hovered: the interactable under the mouse cursor (any distance)
///   - Nearest: the closest interactable within interaction range
///
/// Input:
///   - Interact key (E) / gamepad North -> interact with the NEAREST in-range object
///   - Left click -> interact with the HOVERED object, but only if it's in range
///
/// On interact it plays the player's animation + SFX for that interaction TYPE and
/// raises onInteractionStart (wire this to lock movement). The interactable's
/// Interacted() runs when the animation ENDS (call OnInteractionAnimationEnd from an
/// Animation Event; a timed fallback runs it anyway so you can never soft-lock), at
/// which point onInteractionEnd fires. Shopkeeper (no animation) runs instantly.
///
/// Replaces the old HighlightObject — outline settings now live here.
/// </summary>
[DisallowMultipleComponent]
public class PlayerInteractor : MonoBehaviour
{
    /// <summary>Animation + SFX the player performs for one interaction type.</summary>
    [System.Serializable]
    public class Reaction
    {
        public InteractionType type;
        [Tooltip("Animator trigger to fire. Empty = no animation (interacts instantly, e.g. Shopkeeper).")]
        public string animatorTrigger;
        [Tooltip("Sound to play when the interaction starts. Optional.")]
        public AudioClip sfx;
        [Tooltip("Item required in the inventory to do this (e.g. Axe for Logging, Pickaxe for Mining). " +
                 "Assign the same Items asset the crafted tool uses. Leave empty for no requirement.")]
        public Items requiredTool;
        [Tooltip("Tool model shown in the hand during this interaction (axe/pickaxe/shovel). " +
                 "Instantiated once at the Tool Mount and reused. Leave empty for none.")]
        public GameObject toolPrefab;

        [Header("Cost")]
        public int hungerCost;
    }

    [Header("Detection")]
    [Tooltip("How close (metres) the player must be to interact.")]
    [SerializeField] private float interactionRange = 3f;
    [Tooltip("Layer(s) your interactables' colliders are on.")]
    [SerializeField] private LayerMask interactableMask = ~0;
    [Tooltip("Point used for range checks. Empty = this object's position.")]
    [SerializeField] private Transform originOverride;

    [Header("Input")]
    [SerializeField] private Key interactKey = Key.E;
    [SerializeField] private bool enableClickToInteract = true;
    [Tooltip("Camera for click/hover rays. Empty = Camera.main.")]
    [SerializeField] private Camera interactionCamera;
    [Tooltip("Max ray length for mouse hover/click.")]
    [SerializeField] private float maxRayDistance = 200f;
    [Tooltip("Optional. If set, clicking an out-of-range interactable walks there first, then interacts.")]
    [SerializeField] private PlayerAutoInteract autoInteract;

    [Header("Player reaction (animation + SFX per type)")]
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;
    [Tooltip("One entry per type. Omit a type (e.g. Shopkeeper) for no animation/SFX.")]
    [SerializeField] private Reaction[] reactions;
    [Tooltip("Fired when an interaction is blocked because the required tool isn't in the inventory.")]
    [SerializeField] private UnityEvent onMissingTool;
    [Tooltip("Hand transform where interaction tools (axe/pickaxe/shovel) appear during an interaction.")]
    [SerializeField] private Transform toolMount;
    [Tooltip("Desired WORLD scale of tools. Compensates for a scaled hand bone (Mixamo rigs).")]
    [SerializeField] private Vector3 toolWorldScale = Vector3.one;
    public static event Action<Items, Interactable> OnMissingToolEvent; //Addded by Niaz

    [Header("Interaction lifecycle (signals)")]
    [Tooltip("Fired when an interaction animation STARTS. Wire to lock movement, e.g. " +
             "ThirdPersonController.SetMovementLocked with the checkbox ON.")]
    [SerializeField] private UnityEvent onInteractionStart;
    [Tooltip("Fired when the animation ENDS (or the fallback fires). Wire to unlock movement, " +
             "e.g. ThirdPersonController.SetMovementLocked with the checkbox OFF.")]
    [SerializeField] private UnityEvent onInteractionEnd;
    [Tooltip("Safety timeout that ends the interaction if no Animation Event fires. " +
             "Set to (about) your longest interaction clip's length.")]
    [SerializeField] private float interactionFallbackSeconds = 2f;

    [Header("Highlight")]
    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField] private float outlineWidth = 7f;

    /// <summary>Interactable under the mouse cursor (any distance). Click target.</summary>
    public Interactable Hovered { get; private set; }
    /// <summary>Nearest interactable in range. E / gamepad target.</summary>
    public Interactable Nearest { get; private set; }

    /// <summary>True while an interaction animation is playing (chop/mine/repair).</summary>
    public bool IsInteracting => _isInteracting;

    private Collider _hoveredCollider; // to range-check the hovered object for clicks
    private readonly Collider[] _hits = new Collider[16]; // reused; no per-frame GC
    private readonly HashSet<Interactable> _outlined = new HashSet<Interactable>();
    private readonly List<Interactable> _outlineRemovals = new List<Interactable>();

    private bool _isInteracting;
    private bool _resolved;
    private Interactable _pendingTarget;
    private Coroutine _fallback;
    private ThirdPersonController _movementController; // locked directly during interactions

    private readonly Dictionary<GameObject, GameObject> _toolInstances = new Dictionary<GameObject, GameObject>();
    private GameObject _activeTool;

    private Transform Origin => originOverride != null ? originOverride : transform;
    private Camera Cam => interactionCamera != null ? interactionCamera : Camera.main;

    private PlayerCombat playerCombat;

    [SerializeField] private float searchHighlightRadius = 20f;
    public bool SearchHeld { get; set; }

    private void Awake()
    {
        _movementController = GetComponentInParent<ThirdPersonController>();
        if (_movementController == null) _movementController = FindAnyObjectByType<ThirdPersonController>();

        playerCombat = GetComponent<PlayerCombat>();
    }

    private void SetMovementLocked(bool locked)
    {
        if (_movementController != null) _movementController.MovementLocked = locked;
    }

    // Shows the tool for this interaction at the hand mount (instantiated once, reused).
    private void ShowTool(GameObject prefab)
    {
        HideTool();
        if (prefab == null || toolMount == null) return;

        if (!_toolInstances.TryGetValue(prefab, out GameObject inst) || inst == null)
        {
            inst = Instantiate(prefab, toolMount);
            inst.transform.localPosition = Vector3.zero;
            inst.transform.localRotation = Quaternion.identity;
            inst.transform.localScale = PlayerCombat.CompensateScale(toolMount, toolWorldScale);
            _toolInstances[prefab] = inst;
        }
        inst.SetActive(true);
        _activeTool = inst;
    }

    private void HideTool()
    {
        if (_activeTool != null) _activeTool.SetActive(false);
        _activeTool = null;
    }

    private void Update()
    {
        // While the interaction animation plays, ignore detection + input entirely.
        // The outlines stay put; movement stays locked until it resolves.
        if (_isInteracting) return;

        RefreshTargets();
        if (Keyboard.current != null && Keyboard.current.tabKey.isPressed || SearchHeld)
        {
            HighlightNearbyInteractables();
        }

        HandleInput();

        if (isCooking)
        {
            if (Vector3.Distance(transform.position, currentCookingInteract.transform.position)
                > cookingExitDistance)
            {
                EndCooking();
            }
        }
    }

    // --- Detection + highlight ----------------------------------------------

    private void RefreshTargets()
    {
        UpdateHovered();               // sets Hovered (+ _hoveredCollider), no range gate
        Nearest = GetNearestInRange(); // range-gated by the overlap sphere
        UpdateOutlines();              // outline BOTH
    }

    public void HighlightNearbyInteractables()
    {
        Vector3 origin = Origin.position;

        int count = Physics.OverlapSphereNonAlloc(
            origin,
            searchHighlightRadius,
            _hits,
            interactableMask,
            QueryTriggerInteraction.Collide);

        for (int i = 0; i < count; i++)
        {
            var interactable = _hits[i].GetComponentInParent<Interactable>();

            if (interactable == null || !interactable.CanInteract(this))
                continue;

            AddOutline(interactable);
        }
    }

    private void UpdateHovered()
    {
        Hovered = null;
        _hoveredCollider = null;

        if (Mouse.current == null || PointerOverUI()) return;

        var cam = Cam;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, interactableMask, QueryTriggerInteraction.Collide))
            return;

        var interactable = hit.collider.GetComponentInParent<Interactable>();
        if (interactable == null || !interactable.CanInteract(this)) return;

        // No range gate here: hovering highlights at any distance. Range is only
        // enforced when you actually click (see HandleInput).
        Hovered = interactable;
        _hoveredCollider = hit.collider;
    }

    private Interactable GetNearestInRange()
    {
        Vector3 origin = Origin.position;
        int count = Physics.OverlapSphereNonAlloc(
            origin, interactionRange, _hits, interactableMask, QueryTriggerInteraction.Collide);

        Interactable best = null;
        float bestSqr = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            var interactable = _hits[i].GetComponentInParent<Interactable>();
            if (interactable == null || !interactable.CanInteract(this)) continue;

            // Distance to the collider's bounds (works for every collider type).
            float sqr = (_hits[i].bounds.ClosestPoint(origin) - origin).sqrMagnitude;
            if (sqr < bestSqr) { bestSqr = sqr; best = interactable; }
        }
        return best;
    }

    private bool HoveredInRange()
    {
        if (Hovered == null) return false;
        Vector3 origin = Origin.position;
        Vector3 point = _hoveredCollider != null
            ? _hoveredCollider.bounds.ClosestPoint(origin)
            : Hovered.transform.position;
        return (point - origin).sqrMagnitude <= interactionRange * interactionRange;
    }

    // Outline both Hovered and Nearest; clear anything that's neither.
    private void UpdateOutlines()
    {
        _outlineRemovals.Clear();
        foreach (var it in _outlined)
            if (it == null || (it != Hovered && it != Nearest))
                _outlineRemovals.Add(it);

        for (int i = 0; i < _outlineRemovals.Count; i++)
        {
            var it = _outlineRemovals[i];
            if (it != null) SetOutline(it.gameObject, false);
            _outlined.Remove(it);
        }

        AddOutline(Hovered);
        AddOutline(Nearest);
    }

    private void AddOutline(Interactable it)
    {
        if (it == null || _outlined.Contains(it)) return;
        SetOutline(it.gameObject, true);
        _outlined.Add(it);
    }

    private void SetOutline(GameObject go, bool on)
    {
        var outline = go.GetComponent<Outline>();
        if (outline == null)
        {
            if (!on) return;
            outline = go.AddComponent<Outline>();
            outline.OutlineColor = outlineColor;
            outline.OutlineWidth = outlineWidth;
        }
        outline.enabled = on;
    }

    // --- Input ---------------------------------------------------------------

    private void HandleInput()
    {
        // E / gamepad: interact with the nearest in-range object (no hover needed).
        if (InteractPressed())
        {
            TryBeginInteract(Nearest);
        }
        // Click: interact with the hovered object if close enough; otherwise walk to it.
        else if (enableClickToInteract && ClickPressed() && !PointerOverUI())
        {
            if (Hovered != null)
            {
                if (HoveredInRange()) TryBeginInteract(Hovered);
                else if (autoInteract != null) autoInteract.GoTo(Hovered); // walks there, then interacts (no-op if unreachable)
            }
        }
    }

    /// <summary>Public entry point so PlayerAutoInteract can trigger the interaction on arrival.</summary>
    public void InteractWith(Interactable target) => TryBeginInteract(target);

    private bool InteractPressed()
    {
        bool kb = Keyboard.current != null && Keyboard.current[interactKey].wasPressedThisFrame;
        bool gp = Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame;
        return kb || gp;
    }

    private bool ClickPressed() => Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

    private bool PointerOverUI() => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

    // --- Interaction lifecycle ----------------------------------------------

    private void TryBeginInteract(Interactable target)
    {
        if (target == null || _isInteracting || !target.CanInteract(this)) return;

        Reaction r = GetReaction(target.Type);

        // Tool gate: some interactions need a tool in the inventory (axe/pickaxe).
        // Checked here (not in CanInteract) so auto-walk still runs you up to the target;
        // the interaction only fails to START if you lack the tool.
        if (r != null && r.requiredTool != null)
        {
            int owned = InventoryManager.instance != null ? InventoryManager.instance.SearchItemCount(r.requiredTool) : 0;
            if (owned <= 0)
            {
                onMissingTool?.Invoke();
                OnMissingToolEvent?.Invoke(r.requiredTool, target);
                return;
            }
        }

        if (r != null && r.hungerCost > 0)
        {
            PlayerStatusBasic status = GetComponent<PlayerStatusBasic>();

            if (status.GetCurrentHunger() < r.hungerCost)
            {
                return;
            }

            status.SetCurrentHunger(status.GetCurrentHunger() - r.hungerCost);
        }

        if (r != null && r.sfx != null && audioSource != null)
            audioSource.PlayOneShot(r.sfx);

        bool hasAnim = r != null && animator != null && !string.IsNullOrEmpty(r.animatorTrigger);
        if (!hasAnim)
        {
            // No animation (e.g. Shopkeeper): resolve immediately, no movement lock.
            target.Interacted(this);
            return;
        }

        _isInteracting = true;
        _resolved = false;
        _pendingTarget = target;
        playerCombat.ForceHolster();
        SetMovementLocked(true);      // lock the controller directly
        ShowTool(r.toolPrefab);       // put the axe/pickaxe/shovel in hand
        onInteractionStart?.Invoke(); // + signal for anything else
        animator.SetTrigger(r.animatorTrigger);

        if (_fallback != null) StopCoroutine(_fallback);
        _fallback = StartCoroutine(InteractionFallback(interactionFallbackSeconds));
    }

    /// <summary>
    /// Call this from an Animation Event on the LAST frame of each interaction clip
    /// (chop / mine / repair). It runs the interactable's effect and unlocks movement.
    /// </summary>
    public void OnInteractionAnimationEnd() => ResolveInteraction();

    private void ResolveInteraction()
    {
        if (!_isInteracting || _resolved) return;
        _resolved = true;

        if (_fallback != null) { StopCoroutine(_fallback); _fallback = null; }

        SetMovementLocked(false);   // unlock the controller directly
        HideTool();                 // put the tool away
        onInteractionEnd?.Invoke(); // + signal for anything else
        _isInteracting = false;

        Interactable target = _pendingTarget;
        _pendingTarget = null;
        if (target != null) target.Interacted(this);
    }

    private IEnumerator InteractionFallback(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        ResolveInteraction(); // safety net if no Animation Event was added
    }

    private Reaction GetReaction(InteractionType interactionType)
    {
        if (reactions == null) return null;
        foreach (var entry in reactions)
            if (entry != null && entry.type == interactionType) return entry;
        return null;
    }

    private void OnDisable()
    {
        // If disabled mid-interaction, don't leave movement locked forever.
        if (_isInteracting) { SetMovementLocked(false); HideTool(); onInteractionEnd?.Invoke(); }
        _isInteracting = false;
        if (_fallback != null) { StopCoroutine(_fallback); _fallback = null; }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere((originOverride != null ? originOverride : transform).position, interactionRange);
    }

    private CookInteract currentCookingInteract;
    private bool isCooking;
    [SerializeField] private float cookingExitDistance = 5f;

    public void BeginCooking(CookInteract station)
    {
        currentCookingInteract = station;
        isCooking = true;

        CookMenu.instance.CraftMenuStatus(true);
    }

    private void EndCooking()
    {
        isCooking = false;
        currentCookingInteract = null;

        CookMenu.instance.CraftMenuStatus(false);
    }
}
