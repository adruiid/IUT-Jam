using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    UnityEngine.UI.Outline outline;

    private void Awake()
    {
        outline = GetComponent<UnityEngine.UI.Outline>();
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
}
