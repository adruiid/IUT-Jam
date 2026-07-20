using CTI;
using UnityEngine;

public class AddItemAutomatically : MonoBehaviour
{
    [SerializeField] private Items[] items;

    public void SendToPlayer()
    {
        foreach(Items containItem in items)
        {
            int temp = 0;
            for(int i=0;i<=Random.Range(1, 3); i++)
            {
                temp = i;
                InventoryManager.instance.AddItem(containItem);
            }
            PickupPopup.Instance.Show(containItem, temp-1, transform);
        }
    }

    public void SendToPlayerOnce()
    {
        foreach(Items containItem in items)
        {
            InventoryManager.instance.AddItem(containItem);
            PickupPopup.Instance.Show(containItem, 1, transform);
        }
    }
}
