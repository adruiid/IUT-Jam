using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Base for anything you harvest with a tool (Tree, Stone, Iron, ...).
/// On each interact: show the required tool + play its swing, play the hit SFX,
/// count the hit. On the final hit it waits for the swing animation to END, then
/// spawns 2-3 (configurable) drops and fires the harvest signal.
///
/// Signal = the onHarvested UnityEvent (Inspector) PLUS the OnHarvest() C# hook.
/// Requires a Collider on this object (or a child) so the Interactor can find it.
/// </summary>
[DisallowMultipleComponent]
public abstract class HarvestableInteractable : Interactable
{
    [Header("Harvesting")]
    [Tooltip("Which tool the player swings at this.")]
    [SerializeField] protected ToolType requiredTool = ToolType.Axe;

    [Tooltip("Number of hits before it's harvested. 1 = single interact.")]
    [SerializeField, Min(1)] protected int hitsToHarvest = 1;

    [Tooltip("Minimum seconds between hits (stops swing spam). 0 = no limit.")]
    [SerializeField, Min(0f)] protected float interactCooldown = 0.5f;

    [Header("Drops (spawned when the swing animation ends)")]
    [Tooltip("The collectible prefab to spawn (wood / small stone / small iron).")]
    [SerializeField] protected GameObject dropPrefab;
    [Tooltip("Inclusive range of how many drop. Default 2-3.")]
    [SerializeField, Min(0)] protected int minDrops = 2;
    [SerializeField, Min(0)] protected int maxDrops = 3;
    [Tooltip("How far drops scatter from the centre, so they don't stack.")]
    [SerializeField] protected float dropScatterRadius = 0.75f;
    [Tooltip("Height above the node to spawn drops at.")]
    [SerializeField] protected float dropSpawnHeight = 0.5f;
    [Tooltip("Disable the node after it's harvested (tree breaks down, rock is depleted).")]
    [SerializeField] protected bool deactivateOnHarvest = true;

    [Header("SFX")]
    [Tooltip("Played on every hit. Optional.")]
    [SerializeField] protected AudioClip hitSfx;
    [Tooltip("If set, plays through this source (no per-hit allocation). If empty, a temp " +
             "source is spawned via PlayClipAtPoint.")]
    [SerializeField] protected AudioSource audioSource;

    [Header("Signal — completed harvest")]
    [Tooltip("Fires once when fully harvested. Wire per-type behaviour here, or override OnHarvest().")]
    [SerializeField] protected UnityEvent onHarvested;

    private int _hitsRemaining;
    private float _lastInteractTime = -999f;
    private bool _harvesting; // true from the final hit until the swing resolves

    // Can't interact while the finishing swing is playing out.
    public override bool CanInteract(Interactor interactor) => base.CanInteract(interactor) && !_harvesting;

    protected virtual void Awake()
    {
        _hitsRemaining = hitsToHarvest;
    }

    public override void Interact(Interactor interactor)
    {
        if (_harvesting) return;
        if (Time.time - _lastInteractTime < interactCooldown) return;
        _lastInteractTime = Time.time;

        PlayHitSfx();

        _hitsRemaining--;
        bool depleted = _hitsRemaining <= 0;

        ToolUser toolUser = interactor != null ? interactor.GetComponent<ToolUser>() : null;

        if (depleted)
        {
            _harvesting = true;
            // Spawn + signal when the swing animation ENDS.
            if (toolUser != null) toolUser.UseTool(requiredTool, () => Harvest(interactor));
            else Harvest(interactor); // no ToolUser -> nothing to wait for
        }
        else if (toolUser != null)
        {
            toolUser.UseTool(requiredTool);
        }
    }

    private void PlayHitSfx()
    {
        if (hitSfx == null) return;
        if (audioSource != null) audioSource.PlayOneShot(hitSfx);
        else AudioSource.PlayClipAtPoint(hitSfx, transform.position);
    }

    // The "signal": spawn drops, fire the inspector event, then the per-type hook.
    private void Harvest(Interactor interactor)
    {
        SpawnDrops();
        onHarvested?.Invoke();
        OnHarvest(interactor);

        _hitsRemaining = hitsToHarvest;
        _harvesting = false;

        if (deactivateOnHarvest) gameObject.SetActive(false);
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

    /// <summary>
    /// Optional per-type extra on a completed harvest (VFX, quest counters, etc.).
    /// Drops + despawn are already handled above. Override only if you need more.
    /// </summary>
    protected virtual void OnHarvest(Interactor interactor) { }
}
