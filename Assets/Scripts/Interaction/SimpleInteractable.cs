using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A no-code interactable: wire the UnityEvent in the Inspector.
/// Handy for testing the system or for one-off objects (a door, a lever, a sign)
/// that don't need their own class. For Trees/Stones/etc. make a real subclass.
/// </summary>
public class SimpleInteractable : Interactable
{
    [Tooltip("Called when the player interacts with this object.")]
    [SerializeField] private UnityEvent onInteract;

    public override void Interact(Interactor interactor)
    {
        onInteract?.Invoke();
    }

    public void TestInteraction()
    {
        Debug.Log("TestInteraction");
    }
}
