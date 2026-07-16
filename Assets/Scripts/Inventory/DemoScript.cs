using UnityEngine;

public class DemoScript : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public Items pickAxeItem;
    public Items axeItem;
    public Items woodItem;
    public Items stoneItem;

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

    [ContextMenu("Add Stone")]
    public void AddStone()
    {
        inventoryManager.AddItem(stoneItem);
    }

    [ContextMenu("Count Wood")]
    public void CountWood()
    {
        Debug.Log(inventoryManager.SearchItemCount(woodItem));
    }

    [ContextMenu("Remove Axe")]
    public void RemoveAxe()
    {
        inventoryManager.RemoveItem(axeItem);
    }


    [ContextMenu("Remove 2 Wood")]
    public void RemoveWoodCount()
    {
        inventoryManager.RemoveItem(woodItem, 2);
    }
}


public class Sample: MonoBehaviour
{
    public void TestFunc()
    {
        Debug.Log("Test works!");
    }
}
