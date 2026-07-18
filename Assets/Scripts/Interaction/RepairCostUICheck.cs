using TMPro;
using UnityEngine;

public class RepairCostUICheck : MonoBehaviour
{
    [SerializeField] private PlayerInteractor playerInteractor;
    [SerializeField] private GameObject repairCostPanel;
    [SerializeField] private TextMeshProUGUI repairCostText;
    void Update()
    {
        if (playerInteractor.Hovered is RepairableStructure repairable && repairable.NeedsRepair)
        {
            repairCostPanel.SetActive(true);
            Vector3 worldPos = repairable.transform.position + Vector3.up * 0.5f;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            RectTransform popupRect = repairCostPanel.GetComponent<RectTransform>();
            popupRect.position = screenPos;

            foreach (var cost in repairable.Costs)
            {
                string colorCurrent = InventoryManager.instance.GetResourceCount(cost.resource.resourceType) < cost.amount ? "red" : "white";
                repairCostText.text += $"{cost.resource.resourceType}: <color={colorCurrent}>{cost.amount}</color> ";
            }

        }
        else
        {
            repairCostPanel.SetActive(false);
            repairCostText.text = "";
        }
    }


}
