using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Opens a shop when interacted with. No player animation/SFX (leave the
/// Shopkeeper entry out of the PlayerInteractor's reactions list).
/// </summary>
public class Shopkeeper : Interactable
{
    [Header("Shop")]


    [Tooltip("Fires when the shop should open. Hook your shop system here.")]
    [SerializeField] private UnityEvent onShopOpened;

    private void Reset()
    {
        type = InteractionType.Shopkeeper;
        prompt = "Talk";
    }

    public override void Interacted(PlayerInteractor player)
    {
        player.BeginShopping(this);
    }
}
