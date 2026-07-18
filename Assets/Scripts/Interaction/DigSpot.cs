using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A spot the player digs (Digging interaction, shovel tool). Takes a few digs, then
/// spawns buried loot and removes itself. Mirrors ResourceNode's flow.
/// </summary>
public class DigSpot : Interactable
{
    [Header("Dig Spot")]
    [Tooltip("How many digs before the loot is uncovered.")]
    [SerializeField, Min(1)] private int digsToComplete = 3;

    [Header("Buried loot")]
    [Tooltip("Collectible prefab to spawn (must have PickUpObject + a trigger collider).")]
    [SerializeField] private GameObject lootPrefab;
    [Tooltip("Inclusive range of how many items are buried here.")]
    [SerializeField, Min(0)] private int minLoot = 1;
    [SerializeField, Min(0)] private int maxLoot = 3;
    [Tooltip("How far loot scatters from the hole so items don't stack.")]
    [SerializeField] private float lootScatterRadius = 0.6f;
    [Tooltip("Height above the spot to spawn loot.")]
    [SerializeField] private float lootSpawnHeight = 0.4f;

    [Tooltip("Fires when the dig completes. Optional extra hooks (dust VFX, quest counters).")]
    [SerializeField] private UnityEvent onDug;

    private int _digsLeft;

    private void Reset()
    {
        type = InteractionType.Digging;
        prompt = "Dig";
    }

    private void Awake() => _digsLeft = digsToComplete;

    public override bool CanInteract(PlayerInteractor player) => base.CanInteract(player) && _digsLeft > 0;

    public override void Interacted(PlayerInteractor player)
    {
        _digsLeft--;
        if (_digsLeft > 0) return; // still digging

        SpawnLoot();
        onDug?.Invoke();
        Destroy(gameObject); // hole exhausted
    }

    private void SpawnLoot()
    {
        if (lootPrefab == null) return;

        int count = Random.Range(minLoot, maxLoot + 1); // maxLoot inclusive
        for (int i = 0; i < count; i++)
        {
            Vector2 offset = Random.insideUnitCircle * lootScatterRadius;
            Vector3 pos = transform.position + new Vector3(offset.x, lootSpawnHeight, offset.y);
            Instantiate(lootPrefab, pos, Quaternion.identity);
        }
    }
}
