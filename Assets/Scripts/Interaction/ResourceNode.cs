using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A gatherable node — a tree (Logging) or a rock/ore (Mining). Set Type + the
/// drop prefab in the Inspector. After enough hits it breaks and spawns 2-3
/// collectible drops (prefabs with your PickUpObject on them).
/// </summary>
public class ResourceNode : Interactable
{
    [Header("Resource Node")]
    [Tooltip("How many interacts before it breaks.")]
    [SerializeField, Min(1)] private int hitsToBreak = 3;

    [Tooltip("Collectible prefab to spawn (must have PickUpObject + a trigger collider).")]
    [SerializeField] private GameObject dropPrefab;
    [Tooltip("Inclusive range of how many drop. Default 2-3.")]
    [SerializeField, Min(0)] private int minDrops = 2;
    [SerializeField, Min(0)] private int maxDrops = 3;
    [Tooltip("How far drops scatter so they don't stack.")]
    [SerializeField] private float dropScatterRadius = 0.75f;
    [Tooltip("Height above the node to spawn drops.")]
    [SerializeField] private float dropSpawnHeight = 0.5f;

    [Tooltip("Fires when the node breaks. Optional extra hooks (VFX, quest counters).")]
    [SerializeField] private UnityEvent onBroken;

    private int _hitsLeft;

    // Default to a tree; change Type to Mining for rocks/ore in the Inspector.
    private void Reset()
    {
        type = InteractionType.Logging;
        prompt = "Gather";
    }

    private void Awake() => _hitsLeft = hitsToBreak;

    public override bool CanInteract(PlayerInteractor player) => base.CanInteract(player) && _hitsLeft > 0;

    public override void Interacted(PlayerInteractor player)
    {
        _hitsLeft--;
        if (_hitsLeft > 0) return; // still chopping/mining
        SpawnDrops();
        onBroken?.Invoke();
        Destroy(gameObject); // node consumed; swap for Destroy or a stump later
    }

    private void SpawnDrops()
    {
        if (dropPrefab == null) return;

        int count = Random.Range(minDrops, maxDrops + 1); // maxDrops inclusive
        for (int i = 0; i < count; i++)
        {
            Vector2 offset = Random.insideUnitCircle * dropScatterRadius;
            Vector3 pos = transform.position + new Vector3(offset.x, dropSpawnHeight, offset.y);
            Instantiate(dropPrefab, pos, Quaternion.identity);
        }
    }
}
