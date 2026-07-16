using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

/// <summary>
/// Sits on the Player. Each frame it finds the nearest interactable in range.
/// Pressing Interact (E / gamepad) triggers it; left-clicking an in-range
/// interactable triggers it too. On interact it plays the player's animation +
/// SFX for that interaction TYPE, then calls the interactable's Interacted().
///
/// The player never contains per-type behaviour — only the anim/SFX reaction.
/// All variable behaviour lives in the interactable subclasses.
/// </summary>
[DisallowMultipleComponent]
public class PlayerInteractor : MonoBehaviour
{
    /// <summary>Animation + SFX the player performs for one interaction type.</summary>
    [Serializable]
    public class Reaction
    {
        public InteractionType type;
        [Tooltip("Animator trigger to fire. Leave empty for none (e.g. Shopkeeper).")]
        public string animatorTrigger;
        [Tooltip("Sound to play. Leave empty for none.")]
        public AudioClip sfx;
    }

    [Header("Detection")]
    [Tooltip("How close (metres) the player must be to interact.")]
    [SerializeField] private float interactionRange = 3f;
    [Tooltip("Layer(s) your interactables are on (match HighlightObject's interact layer).")]
    [SerializeField] private LayerMask interactableMask = ~0;
    [Tooltip("Point used for range checks. Empty = this object's position.")]
    [SerializeField] private Transform originOverride;

    [Header("Input (new Input System)")]
    [Tooltip("Drag Player/Interact from InputSystem_Actions.")]
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private bool enableClickToInteract = true;
    [Tooltip("Camera for click rays. Empty = Camera.main.")]
    [SerializeField] private Camera clickCamera;
    [SerializeField] private float maxClickDistance = 100f;

    [Header("Player reaction (animation + SFX per type)")]
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;
    [Tooltip("One entry per interaction type. Omit an entry (e.g. Shopkeeper) for no reaction.")]
    [SerializeField] private Reaction[] reactions;

    /// <summary>Fires when the focused interactable changes (null = nothing in range). Hook UI here.</summary>
    public event Action<Interactable> FocusChanged;
    public Interactable Current { get; private set; }

    private readonly Collider[] _hits = new Collider[16]; // reused; no per-frame GC
    private Transform Origin => originOverride != null ? originOverride : transform;

    private void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed += OnInteractInput;
            interactAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteractInput;
            interactAction.action.Disable();
        }
        SetFocus(null);
    }

    private void Update()
    {
        RefreshFocus();
        if (enableClickToInteract) HandleClick();
    }

    // --- Detection -----------------------------------------------------------

    private void RefreshFocus()
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

            float sqr = (interactable.transform.position - origin).sqrMagnitude;
            if (sqr < bestSqr) { bestSqr = sqr; best = interactable; }
        }
        SetFocus(best);
    }

    private void SetFocus(Interactable next)
    {
        if (next == Current) return;
        Current = next;
        FocusChanged?.Invoke(Current);
    }

    // --- Input ---------------------------------------------------------------

    private void OnInteractInput(InputAction.CallbackContext ctx)
    {
        if (Current != null) TryInteract(Current);
    }

    private void HandleClick()
    {
        var mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return; // over UI

        var cam = clickCamera != null ? clickCamera : Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(mouse.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, maxClickDistance, interactableMask, QueryTriggerInteraction.Collide))
            return;

        var interactable = hit.collider.GetComponentInParent<Interactable>();
        if (interactable == null) return;

        // Clicks still respect range so you can't interact across the map.
        if ((interactable.transform.position - Origin.position).sqrMagnitude > interactionRange * interactionRange)
            return;

        TryInteract(interactable);
    }

    // --- Dispatch ------------------------------------------------------------

    private void TryInteract(Interactable target)
    {
        if (target == null || !target.CanInteract(this)) return;

        PlayReaction(target.Type); // player animation + SFX (may be nothing, e.g. Shopkeeper)
        target.Interacted(this);   // the interactable does its own job
    }

    /// <summary>Plays the player's animation + SFX for a type. No entry = no reaction.</summary>
    private void PlayReaction(InteractionType interactionType)
    {
        Reaction r = null;
        foreach (var entry in reactions)
            if (entry != null && entry.type == interactionType) { r = entry; break; }
        if (r == null) return;

        if (animator != null && !string.IsNullOrEmpty(r.animatorTrigger)) animator.SetTrigger(r.animatorTrigger);
        if (audioSource != null && r.sfx != null) audioSource.PlayOneShot(r.sfx);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere((originOverride != null ? originOverride : transform).position, interactionRange);
    }
}
