using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Threading;
using TMPro;
using System.Collections;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private Items holdingItem;

    [Header("UI")]
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI countText;

    [HideInInspector] public Transform parentAfterDrag;

    public int count = 1;

    [SerializeField] private float _doubleClickDelay = 0.25f;
    private float _lastClickTime;


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
        if (holdingItem.itemName == "Gun(Broken)") return;

        image.raycastTarget = false;
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (holdingItem.itemName == "Gun(Broken)") return;
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (holdingItem.itemName == "Gun(Broken)") return;
        image.raycastTarget = true;
        transform.SetParent(parentAfterDrag);
    }

    public Items GetItem()
    {
        return holdingItem;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (holdingItem.type != ItemType.Consumable) return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (Time.time - _lastClickTime <= _doubleClickDelay)
        {
            ConsumableItemBehaviour();
        }
        else
        {
            
            _lastClickTime = Time.time;
        }


    }

    private void ConsumableItemBehaviour()
    {
        bool consumed = ConsumableRestore.instance.ConsumeItem((ConsumableItems)holdingItem);
        if (consumed) InventoryManager.instance.RemoveItem(holdingItem, 1);
        else StartCoroutine(Shake());
    }

    private IEnumerator Shake()
    {
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.localScale = Vector3.one * Random.Range(0.95f, 1.05f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = Vector3.one;
    }
}
