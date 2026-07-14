using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Threading;
using TMPro;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Items holdingItem;

    [Header("UI")]
    [SerializeField]private Image image;
    [SerializeField] private TextMeshProUGUI countText;

    [HideInInspector] public Transform parentAfterDrag;

    public int count = 1;

    private void Awake()
    {
        image = GetComponent<Image>();
        countText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void InitialiseItem(Items item)
    {
        holdingItem = item;
        image.sprite = item.sprite;
        RefreshCount();
    }

    public void RefreshCount()
    {
        countText.text = count.ToString();
        bool textActive = count > 1;
        countText.gameObject.SetActive(textActive);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        image.raycastTarget = false;
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        image.raycastTarget = true;
        transform.SetParent(parentAfterDrag);
    }

    public Items GetItem()
    {
        return holdingItem;
    }
}
