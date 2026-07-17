using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryOpenClose : MonoBehaviour
{
    [SerializeField] private GameObject inventoryCanvas;
    private bool inventoryActive = false;
    public static InventoryOpenClose instance;


    private void Awake()
    {
        instance = this;

        inventoryCanvas.GetComponent<Canvas>().enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) && !PauseMenuManager.instance.isPaused)
        {
            InventoryStatus();
        }
    }

    public void InventoryStatus()
    {
        inventoryActive = !inventoryActive;
        inventoryCanvas.GetComponent<Canvas>().enabled = inventoryActive;
        EventSystem.current.SetSelectedGameObject(null);
        
    }

    public void InventoryStatus(bool status)
    {
        inventoryActive = status;
        inventoryCanvas.GetComponent<Canvas>().enabled = inventoryActive;
        EventSystem.current.SetSelectedGameObject(null);
    }
}
