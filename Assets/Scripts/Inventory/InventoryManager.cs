using UnityEngine;
using System;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [Header("UI")]
    [SerializeField] private InventorySlot[] inventorySlots;
    [SerializeField] private GameObject inventoryItemPrefab;

    [SerializeField] private int maxStackable;

    public event Action OnInventoryChanged;

    private void Awake()
    {
        instance = this;
    }

    public void AddItem(Items item)
    {
        
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null && itemInSlot.GetItem()== item && itemInSlot.count < maxStackable && itemInSlot.GetItem().stackable)
            {
                Debug.Log("Increased count of " + item.itemName);
                itemInSlot.count++;
                OnInventoryChanged?.Invoke();
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
                Debug.Log("Added new " + item.itemName);
                SpawnNewItem(item, slot);
                OnInventoryChanged?.Invoke();
                return;
            }
        }

        
    }

    public void AddCraftedItem(CraftableItem item)
    {
        foreach (ResourceRequirement requirement in item.requirements)
        {
            InventoryItem itemInSlot = null;
            foreach (InventorySlot slot in inventorySlots)
            {
                itemInSlot = slot.GetComponentInChildren<InventoryItem>();

                if (itemInSlot == null) continue;

                if (itemInSlot.GetItem() is ResourceItems resouce && resouce.resourceType == requirement.resourceType)
                {
                    RemoveItem(itemInSlot.GetItem(), requirement.amount);
                    break;
                    
                }
            }
        }
        
    }

    public void RemoveItem(Items item, int count)
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot == null) continue;

            if (itemInSlot.GetItem() == item && itemInSlot.count >= count)
            {
                itemInSlot.count -= count;
                OnInventoryChanged?.Invoke();
                if (itemInSlot.count == 0)
                {
                    Debug.Log("Object count reached 0, destroying");
                    RemoveItem(item);
                    return;
                }
                itemInSlot.RefreshCount();
                Debug.Log("Removed " + count + " " + item.itemName);
                return;
                
            }
        }

        Debug.Log("Object to remove not found");
    }

    public void RemoveItem(Items item)
    {
        for(int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot.GetItem() == item)
            {
                Debug.Log("Removed " + item.itemName);
                Destroy(itemInSlot.gameObject);
                OnInventoryChanged?.Invoke();
                return;
            }
        }

        Debug.Log("Object to remove not found");
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

    public int GetResourceCount(ResourceType type)
    {
        int count = 0;

        foreach(InventorySlot slot in inventorySlots)
        {
            InventoryItem inventoryItem = slot.GetComponentInChildren<InventoryItem>();
            if (inventoryItem == null)
                continue;

            if (inventoryItem.GetItem() is ResourceItems resouce && resouce.resourceType == type)
            {
                count += slot.GetComponentInChildren<InventoryItem>().count;
            }
        }
        Debug.Log("Found " + count + " of type " + type.ToString());
        return count;
    }

    public bool ResourceItemCountPresent(CraftableItem item)
    {
        foreach (ResourceRequirement requirement in item.requirements)
        {
            int temp = GetResourceCount(requirement.resourceType);
            if (temp < requirement.amount)
            {
                Debug.Log(requirement.resourceType.ToString() + " is insuffecient");
                return false;
            }

        }
        Debug.Log("All crafting requirement present");
        return true;
    }

    private void SpawnNewItem(Items item, InventorySlot slot)
    {
        GameObject newItem = Instantiate(inventoryItemPrefab, slot.transform);
        newItem.GetComponent<InventoryItem>().InitialiseItem(item);
    }
}
