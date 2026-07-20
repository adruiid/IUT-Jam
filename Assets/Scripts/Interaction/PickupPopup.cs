using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PickupPopup : MonoBehaviour
{
    public static PickupPopup Instance;

    [SerializeField] private GameObject textPrefab;
    [SerializeField] private GameObject worldCanvas;
    private GameObject _popup;

    private readonly Queue<(Items item, int amount, Vector3 position)> _queue = new();
    private bool _processingQueue;

    private void Awake()
    {
        Instance = this;
    }

    public void Show(Items item, int amount, Interactable target)
    {
        Show(item, amount, target.transform);
    }

    public void Show(Items item, int amount, Transform target)
    {
        _queue.Enqueue((item, amount, target.position));

        if (!_processingQueue)
            StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        _processingQueue = true;

        while (_queue.Count > 0)
        {
            var (item, amount, position) = _queue.Dequeue();

            GameObject popup = Instantiate(textPrefab, worldCanvas.transform);

            Vector3 worldPos = position + Vector3.up * 0.5f;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            popup.GetComponent<RectTransform>().position = screenPos;
            popup.GetComponent<TextMeshProUGUI>().text = $"+{amount} {item.itemName}";

            StartCoroutine(DestroyPopup(popup));

            yield return new WaitForSeconds(0.8f);
        }

        _processingQueue = false;
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