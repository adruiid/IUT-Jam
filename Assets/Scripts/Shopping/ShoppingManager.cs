using UnityEngine;
using UnityEngine.EventSystems;

public class ShoppingManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private ShoppingSlot[] shoppingSlots;

    private void Start()
    {
        foreach (ShoppingSlot slots in shoppingSlots)
        {
            ShoppableItem itemType = slots.GetHoldingItem();
            slots.button.onClick.AddListener(() => ShopItem(itemType, slots));
        }
    }

    private void ShopItem(ShoppableItem item, ShoppingSlot slot)
    {
        bool valid = InventoryManager.instance.ResourceItemCountPresent(item); //Check if Inventory Manager has enough resources

        if (valid)
        {
            InventoryManager.instance.AddShoppedItem(item); //if enough resource,present, add Item to Inventory
        }
        else
        {
            slot.NotPresent();
        }

        EventSystem.current.SetSelectedGameObject(null);
    }
}
