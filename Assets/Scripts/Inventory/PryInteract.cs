using UnityEngine;

public class PryInteract : Interactable
{
    [SerializeField] private Items item;
    [SerializeField] private int amount = 1;

    private bool collected;

    public Items Item => item;
    public int Amount => amount;

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

        for (int i = 0; i < amount; i++)
        {
            InventoryManager.instance.AddItem(item);
        }
        PickupPopup.Instance.Show(item, amount, this);

        collected = true;
    }
}
