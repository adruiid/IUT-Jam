using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

/// <summary>
/// Sits on the player. Every frame it finds the nearest interactable in range
/// and marks it as "focused" (fires FocusChanged so UI can show a prompt).
/// Pressing Interact (E / gamepad) triggers the focused one; left-clicking an
/// interactable that is in range also triggers it.
/// </summary>
[DisallowMultipleComponent]
public class Interactor : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("How close (metres) the player must be to interact.")]
    [SerializeField] private float interactionRange = 3f;
    [Tooltip("Layers that hold interactable objects. Set your interactables to these layers.")]
    [SerializeField] private LayerMask interactableMask = ~0;
    [Tooltip("Point used for range checks. Leave empty to use this object's position.")]
    [SerializeField] private Transform originOverride;

    [Header("Input (new Input System)")]
    [Tooltip("The Interact action (E / gamepad). Drag Player/Interact from InputSystem_Actions. " +
             "Optional if you only want click-to-interact.")]
    [SerializeField] private InputActionReference interactAction;

    [Header("Click to interact")]
    [SerializeField] private bool enableClickToInteract = true;
    [Tooltip("Camera used to cast the click ray. Leave empty to use Camera.main.")]
    [SerializeField] private Camera clickCamera;
    [Tooltip("Max ray length for a click. The range check still applies after the hit.")]
    [SerializeField] private float maxClickDistance = 100f;

    /// <summary>Fires whenever the focused interactable changes (null = nothing in range).</summary>
    public event Action<Interactable> FocusChanged;

    /// <summary>The interactable currently in focus, or null.</summary>
    public Interactable Current { get; private set; }

    // Reused buffer so per-frame detection allocates nothing (no GC on a hot path).
    private readonly Collider[] _hits = new Collider[16];

    private Transform Origin => originOverride != null ? originOverride : transform;

    private void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed += OnInteractPerformed;
            interactAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteractPerformed;
            interactAction.action.Disable();
        }
        SetFocus(null);
    }

    private void Update()
    {
        RefreshFocus();
        if (enableClickToInteract) HandleClick();
    }

    /// <summary>Finds the nearest valid interactable in range and focuses it.</summary>
    private void RefreshFocus()
    {
        Vector3 origin = Origin.position;
        int count = Physics.OverlapSphereNonAlloc(
            origin, interactionRange, _hits, interactableMask, QueryTriggerInteraction.Collide);

        Interactable best = null;
        float bestSqr = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            // GetComponentInParent so the collider can live on a child mesh.
            var interactable = _hits[i].GetComponentInParent<Interactable>();
            if (interactable == null || !interactable.CanInteract(this)) continue;

            float sqr = (interactable.transform.position - origin).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = interactable;
            }
        }

        SetFocus(best);
    }

    private void SetFocus(Interactable next)
    {
        if (next == Current) return;

        if (Current != null) Current.OnUnfocused(this);
        Current = next;
        if (Current != null) Current.OnFocused(this);

        FocusChanged?.Invoke(Current);
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        if (Current != null && Current.CanInteract(this))
            Current.Interact(this);
    }

    private void HandleClick()
    {
        var mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame) return;

        // Ignore clicks that land on UI (buttons, panels, etc.).
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        var cam = clickCamera != null ? clickCamera : Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(mouse.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, maxClickDistance, interactableMask, QueryTriggerInteraction.Collide))
            return;

        var interactable = hit.collider.GetComponentInParent<Interactable>();
        if (interactable == null || !interactable.CanInteract(this)) return;

        // Clicking still respects range so you can't interact across the map.
        float sqr = (interactable.transform.position - Origin.position).sqrMagnitude;
        if (sqr > interactionRange * interactionRange) return;

        interactable.Interact(this);
    }

    // Visualise the interaction range in the editor for easy tuning.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere((originOverride != null ? originOverride : transform).position, interactionRange);
    }
}
