using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Passive wandering animal (chicken / cow). Grazes to random points within a radius
/// of where it was placed, pausing to idle between moves. Plays a death animation and
/// spawns drops when killed. Uses EnemyHealth (a generic damageable health) + HitFlash
/// for damage + feedback, so it takes bullet/melee damage like anything else.
///
/// Prefab needs: NavMeshAgent, a Collider (on a layer your weapons hit), EnemyHealth,
/// HitFlash, and an Animator with a Speed float + a death trigger.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyHealth))]
[DisallowMultipleComponent]
public class AnimalAI : MonoBehaviour
{
    [Header("Wander (around spawn point)")]
    [Tooltip("How far from its spawn point the animal will roam.")]
    [SerializeField] private float wanderRadius = 6f;
    [SerializeField] private float moveSpeed = 1.5f;
    [Tooltip("Idle pause between wanders (random in this range).")]
    [SerializeField] private float minIdle = 1.5f;
    [SerializeField] private float maxIdle = 4f;
    [Tooltip("How close counts as 'arrived' at a wander point.")]
    [SerializeField] private float arriveDistance = 0.4f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string dieTrigger = "Die";

    [Header("Drops on death (random count)")]
    [SerializeField] private GameObject dropPrefab;
    [SerializeField, Min(0)] private int minDrops = 1;
    [SerializeField, Min(0)] private int maxDrops = 3;
    [SerializeField] private float dropScatterRadius = 0.6f;
    [SerializeField] private float dropSpawnHeight = 0.5f;

    [Header("Hit SFX")]
    [Tooltip("Played when the animal takes damage.")]
    [SerializeField] private AudioClip hitSfx;
    [Tooltip("Source for the hit SFX. Auto-found on this object if empty.")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("Minimum seconds between hit sounds so rapid hits don't overlap.")]
    [SerializeField] private float hitSfxCooldown = 0.15f;

    [Header("Death")]
    [Tooltip("Seconds to keep the corpse before destroying (lets the death anim play).")]
    [SerializeField] private float deathDelay = 2f;
    [Tooltip("Vertical shift on death so the corpse settles on the ground. Negative = down.")]
    [SerializeField] private float deathYOffset = -0.2f;

    private NavMeshAgent _agent;
    private EnemyHealth _health;
    private Collider _collider;
    private Vector3 _home;
    private bool _dead;
    private int _speedHash;
    private float _lastHitSfxTime = -999f;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _health = GetComponent<EnemyHealth>();
        _collider = GetComponentInChildren<Collider>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (!string.IsNullOrEmpty(speedParam)) _speedHash = Animator.StringToHash(speedParam);

        _health.Died += OnDied;
        _health.onDamaged.AddListener(OnDamagedSfx);
    }

    private void OnDestroy()
    {
        if (_health != null)
        {
            _health.Died -= OnDied;
            _health.onDamaged.RemoveListener(OnDamagedSfx);
        }
    }

    // Play the hit sound, throttled so rapid hits don't stack into a buzz.
    private void OnDamagedSfx()
    {
        if (hitSfx == null || Time.time - _lastHitSfxTime < hitSfxCooldown) return;
        _lastHitSfxTime = Time.time;
        if (audioSource != null) audioSource.PlayOneShot(hitSfx);
        else AudioSource.PlayClipAtPoint(hitSfx, transform.position);
    }

    private void Start()
    {
        _home = transform.position;
        _agent.speed = moveSpeed;
        _agent.stoppingDistance = 0f;
        StartCoroutine(WanderLoop());
    }

    private void Update()
    {
        if (_dead) return;

        // Walk/idle blend from actual movement.
        if (animator != null && !string.IsNullOrEmpty(speedParam))
            animator.SetFloat(_speedHash, _agent.velocity.magnitude);
    }

    private IEnumerator WanderLoop()
    {
        while (!_dead)
        {
            if (TryPickPoint(out Vector3 point) && _agent.isOnNavMesh)
            {
                _agent.SetDestination(point);

                // Walk until arrived (with a timeout so it never gets stuck).
                float timeout = 8f, t = 0f;
                while (!_dead && t < timeout &&
                       (_agent.pathPending || _agent.remainingDistance > arriveDistance))
                {
                    t += Time.deltaTime;
                    yield return null;
                }
            }

            // Idle a moment before the next wander.
            float idle = Random.Range(minIdle, maxIdle);
            float e = 0f;
            while (!_dead && e < idle) { e += Time.deltaTime; yield return null; }
        }
    }

    // Random reachable point within wanderRadius of home.
    private bool TryPickPoint(out Vector3 result)
    {
        for (int i = 0; i < 6; i++)
        {
            Vector2 circle = Random.insideUnitCircle * wanderRadius;
            Vector3 candidate = _home + new Vector3(circle.x, 0f, circle.y);
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = _home;
        return false;
    }

    private void OnDied()
    {
        _dead = true;
        StopAllCoroutines();

        if (_agent != null)
        {
            if (_agent.isOnNavMesh) _agent.isStopped = true;
            _agent.enabled = false;                 // stop wandering
        }
        if (_collider != null) _collider.enabled = false; // corpse doesn't block or eat shots

        // Agent no longer holds the body up — settle it onto the ground.
        transform.position += Vector3.up * deathYOffset;

        if (animator != null && !string.IsNullOrEmpty(dieTrigger))
            animator.SetTrigger(dieTrigger);

        SpawnDrops();
        Destroy(gameObject, deathDelay);
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Application.isPlaying ? _home : transform.position, wanderRadius);
    }
}
