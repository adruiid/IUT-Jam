using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    UnityEngine.UI.Outline outline;
    Color originalColor;

    private void Awake()
    {
        outline = GetComponent<UnityEngine.UI.Outline>();
        originalColor = GetComponent<Image>().color;
    }

    private void Start()
    {
        RefreshSlot();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount == 0)
        {
            InventoryItem inventoryItem = eventData.pointerDrag.GetComponent<InventoryItem>();
            inventoryItem.parentAfterDrag = transform;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        outline.enabled = true;

        if (transform.childCount != 0)
        {
            InventoryManager.instance.DisplayDescription(transform.GetComponentInChildren<InventoryItem>().GetItem());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        outline.enabled = false;
        InventoryManager.instance.HideDescription();
    }


    private void RefreshSlot()
    {
        Image slotImage = GetComponent<Image>();

        if (transform.childCount == 0)
        {
            slotImage.color = originalColor;
            return;
        }

        InventoryItem item = GetComponentInChildren<InventoryItem>();

        if (item.GetItem().itemName == "Gun(Broken)")
        {
            slotImage.color = Color.red;
            ColorUtility.TryParseHtmlString("#632F2F", out Color darkRed);
            item.GetComponent<Image>().color = darkRed;
        }
        else
        {
            slotImage.color = Color.white;
            item.GetComponent<Image>().color = originalColor;
        }
    }
}
