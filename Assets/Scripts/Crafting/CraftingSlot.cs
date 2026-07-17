using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingSlot : MonoBehaviour
{
    [SerializeField] private CraftableItem holdingItem;


    [Header("UI")]
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI requirementText;

    public Button button { get; private set; }

    private void Awake()
    {
        button = GetComponent<Button>();
        image.sprite = holdingItem.sprite;
        nameText.text = holdingItem.itemName;
        typeText.text = holdingItem.type.ToString();   
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

    public CraftableItem GetHoldingItem()
    {
        return holdingItem;
    }
}
