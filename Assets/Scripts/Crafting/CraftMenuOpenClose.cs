using UnityEngine;

public class CraftMenuOpenClose : MonoBehaviour
{
    [SerializeField] private GameObject craftingCanvas;

    private bool craftMenuActive = false;

    public static CraftMenuOpenClose instance;

    private void Awake()
    {
        instance = this;
        craftingCanvas.GetComponent<Canvas>().enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O) && !PauseMenuManager.instance.isPaused)
        {
           CraftMenuStatus();
        }
    }

    public void CraftMenuStatus()
    {
        craftMenuActive = !craftMenuActive;
        craftingCanvas.GetComponent<Canvas>().enabled =craftMenuActive;
    }

    public void CraftMenuStatus(bool status)
    {
        craftMenuActive = status;
        craftingCanvas.GetComponent<Canvas>().enabled = craftMenuActive;
    }
}

