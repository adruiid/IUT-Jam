/// <summary>Mined with a pickaxe. Assign the small-stone collectible as the Drop Prefab.</summary>
public class Stone : HarvestableInteractable
{
    private void Reset()
    {
        interactionPrompt = "Mine";
        requiredTool = ToolType.Pickaxe;
        hitsToHarvest = 3;
    }

    // Drops + despawn handled by the base. Override OnHarvest(interactor) for extras.
}
