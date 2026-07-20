using UnityEngine;

public class ShoppingMenu : MonoBehaviour
{
    [SerializeField] private GameObject shoppingCanvas;

    private bool shopMenuActive = false;

    public static ShoppingMenu instance;

    [SerializeField] private AudioSource source;

    [SerializeField] private AudioClip shopKeeperNoises;

    private void Awake()
    {
        instance = this;
        shoppingCanvas.GetComponent<Canvas>().enabled = false;
    }

    public void ShopMenuStatus()
    {
        shopMenuActive = !shopMenuActive;
        if (CraftMenuOpenClose.instance.craftMenuActive) CraftMenuOpenClose.instance.CraftMenuStatus(!shopMenuActive);
        CraftMenuOpenClose.instance.CraftMenuBlock = shopMenuActive;
        shoppingCanvas.GetComponent<Canvas>().enabled = shopMenuActive;

        if (shopMenuActive == true) source.PlayOneShot(shopKeeperNoises);
    }

    public void ShopMenuStatus(bool status)
    {
        shopMenuActive = status;
        if (CraftMenuOpenClose.instance.craftMenuActive) CraftMenuOpenClose.instance.CraftMenuStatus(!status);
        CraftMenuOpenClose.instance.CraftMenuBlock = shopMenuActive;
        shoppingCanvas.GetComponent<Canvas>().enabled = shopMenuActive;

        if (shopMenuActive == true) source.PlayOneShot(shopKeeperNoises);
    }
}
