using UnityEngine;

/// <summary>
/// Script 3 — on the bullet prefab. Flies in a straight line, damages the first thing
/// it hits (via IDamageable), and destroys itself on hit, after travelling too far, or
/// after a lifetime cap. Movement is raycast-based (no Rigidbody/collider needed) so
/// fast bullets don't tunnel through thin targets.
/// </summary>
[DisallowMultipleComponent]
public class Bullet : MonoBehaviour
{
    [Header("Flight")]
    [SerializeField] private float speed = 60f;
    [SerializeField] private float maxDistance = 60f;
    [SerializeField] private float maxLifetime = 5f;

    [Header("Damage")]
    [SerializeField] private float damage = 10f;
    [Tooltip("What the bullet can hit. Exclude the Player and Bullet layers.")]
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("FX")]
    [Tooltip("Optional VFX spawned at the impact point.")]
    [SerializeField] private GameObject hitVfxPrefab;

    private Vector3 _direction;
    private Vector3 _startPos;
    private float _spawnTime;

    private void Awake()
    {
        _startPos = transform.position;
        _spawnTime = Time.time;
        _direction = transform.forward;
    }

    /// <summary>Called by the Weapon right after spawning. Sets direction (and optional damage).</summary>
    public void Init(Vector3 direction, float damageOverride = -1f)
    {
        _direction = direction.normalized;
        if (damageOverride >= 0f) damage = damageOverride;
        transform.rotation = Quaternion.LookRotation(_direction);
    }

    private void Update()
    {
        float step = speed * Time.deltaTime;

        // Raycast the segment we're about to travel; hit = stop there.
        if (Physics.Raycast(transform.position, _direction, out RaycastHit hit, step, hitMask, QueryTriggerInteraction.Ignore))
        {
            Impact(hit.collider, hit.point, hit.normal);
            return;
        }

        transform.position += _direction * step;

        if ((transform.position - _startPos).sqrMagnitude >= maxDistance * maxDistance ||
            Time.time - _spawnTime >= maxLifetime)
        {
            Destroy(gameObject);
        }
    }

    private void Impact(Collider col, Vector3 point, Vector3 normal)
    {
        // GetComponentInParent so the collider can be on a child of the health-owning object.
        var damageable = col.GetComponentInParent<IDamageable>();
        if (damageable != null) damageable.TakeDamage(damage);

        if (hitVfxPrefab != null) Instantiate(hitVfxPrefab, point, Quaternion.LookRotation(normal));

        Destroy(gameObject);
    }
}
