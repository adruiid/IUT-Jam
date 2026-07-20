using UnityEngine;

public class PryInteract : Interactable
{
    [SerializeField] private LootEntry[] loot;

    private bool collected;


    private void Reset()
    {
        type = InteractionType.Pry;
        prompt = "Search";
    }

    public override bool CanInteract(PlayerInteractor player)
    {
        return base.CanInteract(player) && !collected;
    }

    public override void Interacted(PlayerInteractor player)
    {
        if (collected)
            return;

        foreach (var entry in loot)
        {
            for (int i = 0; i < entry.amount; i++)
            {
                InventoryManager.instance.AddItem(entry.item);
            }

            PickupPopup.Instance.Show(entry.item, entry.amount, this);
        }

        collected = true;
    }
}
