using UnityEngine;

/// <summary>
/// Anything the player can interact with implements this.
/// The Interactor only ever talks to objects through this contract.
/// </summary>
public interface IInteractable
{
    /// <summary>Short verb shown in the UI prompt, e.g. "Chop", "Mine", "Talk", "Repair".</summary>
    string InteractionPrompt { get; }

    /// <summary>Whether this can be interacted with right now (out of stock, already repaired, etc.).</summary>
    bool CanInteract(Interactor interactor);

    /// <summary>Do the thing. Called when the player presses Interact / clicks in range.</summary>
    void Interact(Interactor interactor);
}

/// <summary>
/// Base class for every interactable in the game. To make a new variant
/// (Tree, Stone, Shopkeeper, RepairableBuilding, ...) just:
///
///     public class Tree : Interactable
///     {
///         public override void Interact(Interactor interactor) { /* give log */ }
///     }
///
/// Override CanInteract / OnFocused / OnUnfocused only when you need them.
/// Requires a Collider on this object (or a child) so it can be detected.
/// </summary>
[DisallowMultipleComponent]
public abstract class Interactable : MonoBehaviour, IInteractable
{
    [Header("Interactable")]
    [Tooltip("Verb shown in the interaction prompt.")]
    [SerializeField] protected string interactionPrompt = "Interact";

    public virtual string InteractionPrompt => interactionPrompt;

    // Default: interactable whenever the component is enabled. Variants can add
    // conditions (has materials, in stock, not already full HP, etc.).
    public virtual bool CanInteract(Interactor interactor) => isActiveAndEnabled;

    // The one thing every variant must define.
    public abstract void Interact(Interactor interactor);

    // Optional hooks for feedback while the player is looking at / in range of this.
    // Good place to toggle an outline (you have the DOTween EPOOutline module), a
    // glow, or a floating icon. No-ops by default.
    public virtual void OnFocused(Interactor interactor) { }
    public virtual void OnUnfocused(Interactor interactor) { }
}
