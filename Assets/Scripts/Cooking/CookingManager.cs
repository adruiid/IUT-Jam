using UnityEngine;
using UnityEngine.EventSystems;

public class CookingManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CookingSlot[] cookingSlots;

    private void Start()
    {
        foreach (CookingSlot slots in cookingSlots)
        {
            CookableItem itemType = slots.GetHoldingItem();
            slots.button.onClick.AddListener(() => CookItem(itemType, slots));
        }
    }

    private void CookItem(CookableItem item, CookingSlot slot)
    {
        bool valid = InventoryManager.instance.ResourceItemCountPresent(item); //Check if Inventory Manager has enough resources

        if (valid)
        {
            InventoryManager.instance.AddCookedItem(item); //if enough resource,present, add Item to Inventory
        }
        else
        {
            slot.NotPresent();
        }

        EventSystem.current.SetSelectedGameObject(null);
    }
}
