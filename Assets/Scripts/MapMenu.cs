using UnityEngine;

public class MapMenu : MonoBehaviour
{
    [SerializeField] private GameObject mapCanvas;

    public bool MapMenuActive = false;

    public static MapMenu instance;

    private void Awake()
    {
        instance = this;
        mapCanvas.GetComponent<Canvas>().enabled = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            MapMenuStatus();
        }
    }

    public void MapMenuStatus()
    {
        MapMenuActive = !MapMenuActive;
        if (CraftMenuOpenClose.instance.craftMenuActive) CraftMenuOpenClose.instance.CraftMenuStatus(!MapMenuActive);
        mapCanvas.GetComponent<Canvas>().enabled = MapMenuActive;
    }

    public void MapMenuStatus(bool status)
    {
        MapMenuActive = status;
        if (CraftMenuOpenClose.instance.craftMenuActive) CraftMenuOpenClose.instance.CraftMenuStatus(!MapMenuActive);
        mapCanvas.GetComponent<Canvas>().enabled = MapMenuActive;
    }
}
