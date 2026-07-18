using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CraftingManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]private CraftingSlot[] craftingSlots;

    private void Start()
    {
        foreach(CraftingSlot slots in craftingSlots)
        {
            CraftableItem itemType = slots.GetHoldingItem();
            slots.button.onClick.AddListener(() => CraftItem(itemType, slots));
        }
    }

    private void CraftItem(CraftableItem item, CraftingSlot  slot)
    {
        bool valid = InventoryManager.instance.ResourceItemCountPresent(item); //Check if Inventory Manager has enough resources

        if (valid)
        {
            InventoryManager.instance.AddCraftedItem(item); //if enough resource,present, add Item to Inventory
        }
        else
        {
            slot.NotPresent();
        }

        EventSystem.current.SetSelectedGameObject(null);
    }

}
