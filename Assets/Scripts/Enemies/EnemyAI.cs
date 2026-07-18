using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Melee horde enemy. Chases the player with a NavMeshAgent (avoids terrain + other
/// enemies via local avoidance), and attacks when in range with a telegraphed windup
/// and a cooldown. Built to scale to many instances: paths are recomputed on an
/// interval (staggered), not every frame.
///
/// Prefab needs: NavMeshAgent, a Collider (on your Enemy layer, for bullets),
/// EnemyHealth, an Animator with a Speed float + Attack (and optional Die) triggers.
/// The player must be tagged "Player" and implement IDamageable to take hits.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyHealth))]
[DisallowMultipleComponent]
public class EnemyAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private string playerTag = "Player";

    [Header("Movement")]
    [Tooltip("Chase speed (m/s). Applied to the NavMeshAgent on start.")]
    [SerializeField] private float moveSpeed = 3.5f;
    [Tooltip("How often (s) the path to the player is recomputed. Higher = cheaper for big hordes.")]
    [SerializeField] private float repathInterval = 0.2f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private float attackDamage = 10f;
    [Tooltip("Minimum time between the START of consecutive attacks (the cooldown).")]
    [SerializeField] private float attackCooldown = 1.5f;
    [Tooltip("Delay from attack start to when damage lands. Sync with the anim's hit frame.")]
    [SerializeField] private float attackWindup = 0.4f;
    [Tooltip("Time after the hit before the enemy moves again (attack recovery).")]
    [SerializeField] private float attackRecovery = 0.3f;
    [Tooltip("Range multiplier when the hit lands, so a barely-fleeing player still gets clipped.")]
    [SerializeField] private float hitRangeLeniency = 1.25f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string attackTrigger = "Attack";
    [Tooltip("Optional. Leave empty to skip the death animation.")]
    [SerializeField] private string dieTrigger = "Die";

    [Header("Death")]
    [Tooltip("Seconds to keep the corpse before destroying (lets the death anim play).")]
    [SerializeField] private float deathDelay = 2f;

    private NavMeshAgent _agent;
    private EnemyHealth _health;
    private Collider _collider;
    private Transform _player;

    private enum State { Chasing, Attacking, Dead }
    private State _state = State.Chasing;

    private float _nextRepath;
    private float _nextAttackTime;
    private int _speedHash;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _health = GetComponent<EnemyHealth>();
        _collider = GetComponentInChildren<Collider>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (!string.IsNullOrEmpty(speedParam)) _speedHash = Animator.StringToHash(speedParam);

        _health.Died += OnDied;
    }

    private void OnDestroy()
    {
        if (_health != null) _health.Died -= OnDied;
    }

    private void Start()
    {
        AcquirePlayer();
        _agent.stoppingDistance = attackRange * 0.9f;             // stop just inside attack range
        _agent.avoidancePriority = Random.Range(30, 71);          // vary priorities -> smoother crowds
        _nextRepath = Time.time + Random.value * repathInterval;  // stagger repaths across the horde
    }

    private void Update()
    {
        if (_state == State.Dead) return;

        if (_player == null) { AcquirePlayer(); if (_player == null) return; }

        // Walk animation from actual movement speed.
        if (animator != null && !string.IsNullOrEmpty(speedParam))
            animator.SetFloat(_speedHash, _agent.velocity.magnitude);

        if (_state == State.Attacking) return; // the attack coroutine owns this window

        // Chase (repath on interval, not every frame).
        if (Time.time >= _nextRepath)
        {
            if (_agent.isOnNavMesh) _agent.SetDestination(_player.position);
            _nextRepath = Time.time + repathInterval;
        }

        // Attack when in range and off cooldown.
        if (Time.time >= _nextAttackTime && InRange(attackRange))
            StartCoroutine(AttackSequence());
    }

    private IEnumerator AttackSequence()
    {
        _state = State.Attacking;
        _nextAttackTime = Time.time + attackCooldown; // cooldown measured from attack start

        _agent.isStopped = true;
        _agent.velocity = Vector3.zero;
        FacePlayer();

        if (animator != null && !string.IsNullOrEmpty(attackTrigger))
            animator.SetTrigger(attackTrigger);

        yield return new WaitForSeconds(attackWindup);

        // Fair: only connect if the player is still close (they can back off in time).
        if (_state != State.Dead && _player != null && InRange(attackRange * hitRangeLeniency))
            DealDamage();

        yield return new WaitForSeconds(attackRecovery);

        if (_state != State.Dead)
        {
            _agent.isStopped = false;
            _state = State.Chasing;
        }
    }

    private void DealDamage()
    {
        var damageable = _player.GetComponentInParent<IDamageable>();
        if (damageable != null) damageable.TakeDamage(attackDamage);
    }

    private void OnDied()
    {
        _state = State.Dead;
        StopAllCoroutines();

        if (_agent != null)
        {
            if (_agent.isOnNavMesh) _agent.isStopped = true;
            _agent.enabled = false;                 // stop pathing + avoidance
        }
        if (_collider != null) _collider.enabled = false; // corpse doesn't block or eat bullets

        if (animator != null && !string.IsNullOrEmpty(dieTrigger))
            animator.SetTrigger(dieTrigger);

        Destroy(gameObject, deathDelay);
    }

    private void AcquirePlayer()
    {
        var go = GameObject.FindGameObjectWithTag(playerTag);
        if (go != null) _player = go.transform;
    }

    // Horizontal range check (isometric: ignore small Y differences).
    private bool InRange(float range)
    {
        Vector3 a = transform.position; a.y = 0f;
        Vector3 b = _player.position; b.y = 0f;
        return (a - b).sqrMagnitude <= range * range;
    }

    private void FacePlayer()
    {
        Vector3 dir = _player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(dir);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
