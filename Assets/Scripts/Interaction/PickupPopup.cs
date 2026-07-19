using TMPro;
using UnityEngine;
using System.Collections;

public class PickupPopup : MonoBehaviour
{
    public static PickupPopup Instance;

    [SerializeField] private GameObject textPrefab;
    [SerializeField] private GameObject worldCanvas;
    private GameObject _popup;

    private void Awake()
    {
        Instance = this;
    }

    public void Show(Items item, int amount, Interactable target)
    {
        _popup = Instantiate(textPrefab, worldCanvas.transform);

        Vector3 worldPos = target.transform.position + Vector3.up * 0.5f;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        RectTransform popupRect = _popup.GetComponent<RectTransform>();
        popupRect.position = screenPos;

        _popup.GetComponent<TextMeshProUGUI>().text = $"+{amount} {item.itemName}";

        StartCoroutine(DestroyPopup(_popup));
    }

    public void Show(Items item, int amount, Transform target)
    {
        _popup = Instantiate(textPrefab, worldCanvas.transform);

        Vector3 worldPos = target.position + Vector3.up * Random.Range(0.5f, 3f) + Vector3.right * Random.Range(-2f, 2f);

        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);


        RectTransform popupRect = _popup.GetComponent<RectTransform>();
        popupRect.position = screenPos;

        _popup.GetComponent<TextMeshProUGUI>().text = $"+{amount} {item.itemName}";

        StartCoroutine(DestroyPopup(_popup));
    }

    IEnumerator DestroyPopup(GameObject popup)
    {
        yield return new WaitForSeconds(1f);

        if (popup != null)
        {
            Destroy(popup);
            popup = null;
        }
    }
}