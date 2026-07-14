/// <summary>Chopped with an axe. Assign the wood collectible as the Drop Prefab.</summary>
public class Tree : HarvestableInteractable
{
    // Defaults applied when the component is first added.
    private void Reset()
    {
        interactionPrompt = "Chop";
        requiredTool = ToolType.Axe;
        hitsToHarvest = 3;
    }

    // Drops + despawn handled by the base. Override OnHarvest(interactor) here for
    // any tree-specific extras (falling animation, VFX, ...).
}
