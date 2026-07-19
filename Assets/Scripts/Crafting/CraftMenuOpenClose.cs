using UnityEngine;

public class CraftMenuOpenClose : MonoBehaviour
{
    [SerializeField] private GameObject craftingCanvas;

    private bool craftMenuActive = false;

    public static CraftMenuOpenClose instance;

    public bool CraftMenuBlock=false;

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
        if (CraftMenuBlock) return;

        craftMenuActive = !craftMenuActive;
        craftingCanvas.GetComponent<Canvas>().enabled =craftMenuActive;
    }

    public void CraftMenuStatus(bool status)
    {
        if (CraftMenuBlock) return;
        craftMenuActive = status;
        craftingCanvas.GetComponent<Canvas>().enabled = craftMenuActive;
    }
}

