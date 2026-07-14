using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private InventorySlot[] inventorySlots;
    [SerializeField] private GameObject inventoryItemPrefab;

    [SerializeField] private int maxStackable;

    public void AddItem(Items item)
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null && itemInSlot.GetItem()== item && itemInSlot.count < maxStackable && itemInSlot.GetItem().stackable)
            {
                itemInSlot.count++;
                itemInSlot.RefreshCount();
                return;
            }
        }


        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot == null)
            {
                SpawnNewItem(item, slot);
                return;
            }
        }
    }

    public int SearchItemCount(Items item)
    {
        int count = 0;
        for(int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null && itemInSlot.GetItem() == item)
            {
                count+=itemInSlot.count;
            }
        }
        return count;
    }

    private void SpawnNewItem(Items item, InventorySlot slot)
    {
        GameObject newItem = Instantiate(inventoryItemPrefab, slot.transform);
        newItem.GetComponent<InventoryItem>().InitialiseItem(item);
    }
}
