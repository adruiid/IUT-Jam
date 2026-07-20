using UnityEngine;

public class CookMenu : MonoBehaviour
{
    [SerializeField] private GameObject cookingCanvas;

    private bool cookMenuActive = false;

    public static CookMenu instance;

    private void Awake()
    {
        instance = this;
        cookingCanvas.GetComponent<Canvas>().enabled = false;
    }

    public void CookMenuStatus()
    {
        cookMenuActive = !cookMenuActive;
        if (CraftMenuOpenClose.instance.craftMenuActive) CraftMenuOpenClose.instance.CraftMenuStatus(!cookMenuActive);
        CraftMenuOpenClose.instance.CraftMenuBlock = cookMenuActive;
        cookingCanvas.GetComponent<Canvas>().enabled = cookMenuActive;
    }

    public void CraftMenuStatus(bool status)
    {
        cookMenuActive = status;
        if (CraftMenuOpenClose.instance.craftMenuActive) CraftMenuOpenClose.instance.CraftMenuStatus(!status);
        CraftMenuOpenClose.instance.CraftMenuBlock = cookMenuActive;
        cookingCanvas.GetComponent<Canvas>().enabled = cookMenuActive;
    }
}
