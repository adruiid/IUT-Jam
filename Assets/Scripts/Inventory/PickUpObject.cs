using UnityEngine;

public class PickUpObject : MonoBehaviour
{
    [SerializeField] private Items item;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Added 1 "+item.itemName);
            InventoryManager.instance.AddItem(item);
            Destroy(gameObject);
        }
    }
}
