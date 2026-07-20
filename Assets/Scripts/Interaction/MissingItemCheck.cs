using System.Collections;
using System.Threading;
using TMPro;
using UnityEngine;

public class MissingItemCheck : MonoBehaviour
{
    [SerializeField] private GameObject worldCanvas;
    [SerializeField] private GameObject textPrefab;
    private GameObject _popup;

    private void Start()
    {
        PlayerInteractor.OnMissingToolEvent += MissingItemResponse;
    }


    public void MissingItemResponse(Items item, Interactable target)
    {
        _popup = Instantiate(textPrefab, worldCanvas.transform);

            Vector3 worldPos = target.transform.position + Vector3.up * 0.5f; 
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            RectTransform popupRect = _popup.GetComponent<RectTransform>();
            popupRect.position = screenPos;

            _popup.GetComponent<TextMeshProUGUI>().text = "Missing " + item.itemName;

            StartCoroutine(DestroyPopup(_popup));

    }

    private IEnumerator DestroyPopup(GameObject popup)
    {
        yield return new WaitForSeconds(1f);

        if (popup != null)
        {
            Destroy(popup);
            popup = null;
        }
    }

    private void OnDestroy()
    {
        PlayerInteractor.OnMissingToolEvent -= MissingItemResponse;
    }
}
