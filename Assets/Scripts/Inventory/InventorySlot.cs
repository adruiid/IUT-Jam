using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
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
        if (transform.childCount != 0)
        {
            InventoryManager.instance.DisplayDescription(transform.GetComponentInChildren<InventoryItem>().GetItem());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InventoryManager.instance.HideDescription();
    }
}
