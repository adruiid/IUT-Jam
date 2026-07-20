using UnityEngine;

public class ShoppingMenu : MonoBehaviour
{
    [SerializeField] private GameObject shoppingCanvas;

    private bool shopMenuActive = false;

    public static ShoppingMenu instance;

    private void Awake()
    {
        instance = this;
        shoppingCanvas.GetComponent<Canvas>().enabled = false;
    }

    public void ShopMenuStatus()
    {
        shopMenuActive = !shopMenuActive;
        CraftMenuOpenClose.instance.CraftMenuStatus(!shopMenuActive);
        CraftMenuOpenClose.instance.CraftMenuBlock = shopMenuActive;
        shoppingCanvas.GetComponent<Canvas>().enabled = shopMenuActive;
    }

    public void ShopMenuStatus(bool status)
    {
        shopMenuActive = status;
        CraftMenuOpenClose.instance.CraftMenuStatus(!status);
        CraftMenuOpenClose.instance.CraftMenuBlock = shopMenuActive;
        shoppingCanvas.GetComponent<Canvas>().enabled = shopMenuActive;
    }
}
