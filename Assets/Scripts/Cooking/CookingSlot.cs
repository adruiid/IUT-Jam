using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class CookingSlot : MonoBehaviour
{
    [SerializeField] private CookableItem holdingItem;

    [Header("UI")]
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI requirementText;

    public Button button { get; private set; }

    RectTransform rect;

    private Vector2 originalPos;

    private void Awake()
    {
        button = GetComponent<Button>();
        image.sprite = holdingItem.sprite;
        nameText.text = holdingItem.itemName;
        rect = GetComponent<RectTransform>();
        originalPos = rect.anchoredPosition;
    }

    private void Start()
    {
        InventoryManager.instance.OnInventoryChanged += Refresh;
        Refresh();
    }

    private void Refresh()
    {
        requirementText.text = "";
        foreach (ResourceRequirement requirement in holdingItem.requirements)
        {
            string colorCurrent = InventoryManager.instance.GetResourceCount(requirement.resourceType) < requirement.amount ? "red" : "white";
            requirementText.text += $"{requirement.resourceType}: <color={colorCurrent}>{requirement.amount}</color> ";
        }
    }

    public CookableItem GetHoldingItem()
    {
        return holdingItem;
    }

    public void NotPresent()
    {
        StopAllCoroutines();
        StartCoroutine(Shake());
    }

    private IEnumerator Shake()
    {
        float duration = 0.3f;
        float strength = 5f;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            rect.anchoredPosition = originalPos + Random.insideUnitCircle * strength;

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }

}
