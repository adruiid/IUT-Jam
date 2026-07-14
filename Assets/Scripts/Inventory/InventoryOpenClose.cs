using Unity.VisualScripting;
using UnityEngine;

public class InventoryOpenClose : MonoBehaviour
{
    [SerializeField] private GameObject inventoryCanvas;
    private bool inventoryActive = false;

    private void Awake()
    {
        inventoryCanvas.GetComponent<Canvas>().enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            InventoryStatus();
        }
    }

    private void InventoryStatus()
    {
        inventoryActive = !inventoryActive;
        inventoryCanvas.GetComponent<Canvas>().enabled=inventoryActive;
        Cursor.lockState = inventoryActive ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = inventoryActive;
    }
}
