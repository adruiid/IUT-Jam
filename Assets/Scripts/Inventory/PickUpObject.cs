using UnityEngine;

public class PickUpObject : MonoBehaviour
{
    [SerializeField] private Items item;
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        _rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Added 1 "+item.itemName);
            PickupPopup.Instance.Show(item, 1, transform);
            InventoryManager.instance.AddItem(item);
            Destroy(gameObject);
        }
    }
}
