using UnityEngine;

public class DemoScript : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public Items pickAxeItem;
    public Items axeItem;
    public Items woodItem;

    [ContextMenu("Add Axe")]
    public void AddAxe()
    {
        inventoryManager.AddItem(axeItem);
    }

    [ContextMenu("Add PickAxe")]
    public void AddPickAxe()
    {
        inventoryManager.AddItem(pickAxeItem);
    }

    [ContextMenu("Add Wood")]
    public void AddWood()
    {
        inventoryManager.AddItem(woodItem);
    }

    [ContextMenu("Count Wood")]
    public void countWood()
    {
        Debug.Log(inventoryManager.SearchItemCount(woodItem));
    }
}
