using UnityEngine;

/// <summary>
/// The kind of interaction. Drives which player animation + SFX plays.
/// Add a new value here + a matching subclass to introduce a new interaction.
/// </summary>
public enum InteractionType
{
    Logging,
    Mining,
    Repairing,
    Shopkeeper,
    Digging,
}

/// <summary>Contract the PlayerInteractor talks to.</summary>
public interface IInteractable
{
    InteractionType Type { get; }
    string Prompt { get; }
    bool CanInteract(PlayerInteractor player);
    void Interacted(PlayerInteractor player);
}

/// <summary>
/// Base for every interactable. To add a new interaction type, subclass this and
/// implement Interacted() with the behaviour — that's the ONLY place per-type logic
/// lives. The player side (animation + SFX) is data-driven by <see cref="Type"/>.
///
/// Needs a Collider on this object (or a child) so the player can detect it, on the
/// same layer the PlayerInteractor scans.
/// </summary>
[DisallowMultipleComponent]
public abstract class Interactable : MonoBehaviour, IInteractable
{
    [Header("Interactable")]
    [Tooltip("Chooses the player's animation + SFX. Usually fixed per type (set by Reset).")]
    [SerializeField] protected InteractionType type;

    [Tooltip("Optional label for UI prompts.")]
    [SerializeField] protected string prompt = "Interact";

    public InteractionType Type => type;
    public virtual string Prompt => prompt;

    /// <summary>Whether this can be interacted with right now (depleted, already repaired, etc.).</summary>
    public virtual bool CanInteract(PlayerInteractor player) => isActiveAndEnabled;

    /// <summary>The per-type job. Implement in each subclass.</summary>
    public abstract void Interacted(PlayerInteractor player);
}
