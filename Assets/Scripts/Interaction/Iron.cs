/// <summary>Mined with a pickaxe. Assign the small-iron collectible as the Drop Prefab.</summary>
public class Iron : HarvestableInteractable
{
    private void Reset()
    {
        interactionPrompt = "Mine";
        requiredTool = ToolType.Pickaxe;
        hitsToHarvest = 4; // tougher than plain stone
    }

    // Drops + despawn handled by the base. Override OnHarvest(interactor) for extras.
}
