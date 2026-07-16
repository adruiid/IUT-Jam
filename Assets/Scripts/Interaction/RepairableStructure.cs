using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A structure (turret, fence, ...) that can be repaired if the player has enough
/// resources. Checks + spends through your existing InventoryManager. Call
/// SetNeedsRepair(true) from your damage system to make it repairable again.
/// </summary>
public class RepairableStructure : Interactable
{
    [System.Serializable]
    public struct RepairCost
    {
        [Tooltip("The resource ScriptableObject asset (Wood / Stone / ...).")]
        public ResourceItems resource;
        public int amount;
    }

    [Header("Repair")]
    [Tooltip("Resources consumed to repair.")]
    [SerializeField] private RepairCost[] costs;
    [Tooltip("Starts needing repair? Usually driven by a damage system at runtime.")]
    [SerializeField] private bool needsRepair = true;

    [Tooltip("Fires on a successful repair. Hook the visual/health restore here.")]
    [SerializeField] private UnityEvent onRepaired;
    [Tooltip("Optional feedback when the player lacks resources.")]
    [SerializeField] private UnityEvent onNotEnoughResources;

    private void Reset()
    {
        type = InteractionType.Repairing;
        prompt = "Repair";
    }

    // Only interactable (and only shows a repair reaction) while actually damaged.
    public override bool CanInteract(PlayerInteractor player) => base.CanInteract(player) && needsRepair;

    public override void Interacted(PlayerInteractor player)
    {
        if (!needsRepair) return;

        if (!HasResources())
        {
            Debug.Log($"{name}: not enough resources to repair.", this);
            onNotEnoughResources?.Invoke();
            return;
        }

        SpendResources();
        needsRepair = false;
        onRepaired?.Invoke();
    }

    private bool HasResources()
    {
        if (InventoryManager.instance == null) return false;
        foreach (var cost in costs)
        {
            if (cost.resource == null) continue;
            if (InventoryManager.instance.GetResourceCount(cost.resource.resourceType) < cost.amount)
                return false;
        }
        return true;
    }

    private void SpendResources()
    {
        foreach (var cost in costs)
        {
            if (cost.resource == null) continue;
            // Mirrors InventoryManager.AddCraftedItem's spend. NOTE: RemoveItem draws from a
            // single stack; fine while a resource sits in one slot. If you later split stacks,
            // switch to a per-1 loop.
            InventoryManager.instance.RemoveItem(cost.resource, cost.amount);
        }
    }

    /// <summary>Call from your damage/turret system to mark it broken again.</summary>
    public void SetNeedsRepair(bool value) => needsRepair = value;
}
