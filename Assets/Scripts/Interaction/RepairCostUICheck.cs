using TMPro;
using UnityEngine;

public class RepairCostUICheck : MonoBehaviour
{
    [SerializeField] private PlayerInteractor playerInteractor;
    [SerializeField] private GameObject repairCostPanel;
    [SerializeField] private TextMeshProUGUI repairCostText;
    private RepairableStructure current;
    void Update()
    {
        if (playerInteractor.Hovered is RepairableStructure repairable && repairable.NeedsRepair)
        {
            repairCostPanel.SetActive(true);

            if (current != repairable)
            {
                current = repairable;
                ShowRequirement();
            }

            UpdatePosition();

        }
        else
        {
            current = null;
            repairCostPanel.SetActive(false);
            repairCostText.text = "";
        }
    }

    private void ShowRequirement()
    {
        repairCostText.text = "";
        Vector3 worldPos = current.transform.position + Vector3.up * 0.5f;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        RectTransform popupRect = repairCostPanel.GetComponent<RectTransform>();
        popupRect.position = screenPos;

        foreach (var cost in current.Costs)
        {
            string colorCurrent = InventoryManager.instance.GetResourceCount(cost.resource.resourceType) < cost.amount ? "red" : "white";
            repairCostText.text += $"{cost.resource.resourceType}: <color={colorCurrent}>{cost.amount}</color> ";
        }

    }

    private void UpdatePosition()
    {
        Vector3 worldPos = current.transform.position + Vector3.up * 0.5f;
        repairCostPanel.GetComponent<RectTransform>().position =
            Camera.main.WorldToScreenPoint(worldPos);
    }


}
