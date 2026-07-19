using UnityEngine;

/// <summary>
/// Auto-turret mounted on a repairable base. While active it rotates on its Y axis to
/// face the closest enemy in range and fires at a fixed rate, dealing damage directly
/// (no projectile — like the dagger) with a muzzle flash + SFX per shot.
///
/// Lifecycle:
///   - Starts as a DISABLED GameObject.
///   - The base's RepairableStructure.onRepaired event calls Activate() -> the turret
///     enables itself with a FULL magazine.
///   - When it runs out of ammo it disables its GameObject and flags the base as needing
///     repair again (SetNeedsRepair(true)), so repairing it reloads + reactivates it.
/// </summary>
[DisallowMultipleComponent]
public class Turret : MonoBehaviour
{
    [Header("Targeting")]
    [Tooltip("The part that rotates on Y to aim (e.g. the turret head). Defaults to this transform.")]
    [SerializeField] private Transform rotationPivot;
    [Tooltip("Layers enemies are on.")]
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private float range = 12f;
    [Tooltip("How fast the turret turns to face its target (deg/sec).")]
    [SerializeField] private float turnSpeed = 360f;
    [Tooltip("Must be aligned within this many degrees of the target before it fires.")]
    [SerializeField] private float aimTolerance = 8f;

    [Header("Firing")]
    [SerializeField] private float damage = 15f;
    [Tooltip("Shots per second.")]
    [SerializeField] private float fireRate = 2f;
    [SerializeField] private int maxAmmo = 30;

    [Header("Feedback")]
    [Tooltip("Muzzle-flash particle system (played per shot).")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fireSfx;

    [Header("Base")]
    [Tooltip("The RepairableStructure on the base. When ammo runs out, its 'needs repair' is set true.")]
    [SerializeField] private RepairableStructure baseStructure;

    private int _currentAmmo;
    private float _nextFireTime;

    private Transform Pivot => rotationPivot != null ? rotationPivot : transform;

    /// <summary>Wire this to the base's RepairableStructure.onRepaired event.</summary>
    public void Activate()
    {
        gameObject.SetActive(true); // OnEnable refills the magazine
    }

    private void OnEnable()
    {
        // Auto-find the base's RepairableStructure (on this object or a parent) if unassigned.
        if (baseStructure == null) baseStructure = GetComponentInParent<RepairableStructure>();

        // Full magazine every time it comes online (after a repair).
        _currentAmmo = maxAmmo;
        _nextFireTime = 0f;
    }

    private void Update()
    {
        Transform target = FindClosestEnemy();
        if (target == null) return;

        AimAt(target);

        if (Time.time >= _nextFireTime && IsAimedAt(target))
            Fire(target);
    }

    private void AimAt(Transform target)
    {
        Vector3 dir = target.position - Pivot.position;
        dir.y = 0f; // Y-axis rotation only
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion want = Quaternion.LookRotation(dir);
        Pivot.rotation = Quaternion.RotateTowards(Pivot.rotation, want, turnSpeed * Time.deltaTime);
    }

    private bool IsAimedAt(Transform target)
    {
        Vector3 dir = target.position - Pivot.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return true;
        return Quaternion.Angle(Pivot.rotation, Quaternion.LookRotation(dir)) <= aimTolerance;
    }

    private void Fire(Transform target)
    {
        _nextFireTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);

        // Direct damage to the target (no projectile).
        var damageable = target.GetComponentInParent<IDamageable>();
        if (damageable != null) damageable.TakeDamage(damage);

        if (muzzleFlash != null) muzzleFlash.Play();
        if (fireSfx != null && audioSource != null) audioSource.PlayOneShot(fireSfx);

        _currentAmmo--;
        if (_currentAmmo <= 0) Deplete();
    }

    private void Deplete()
    {
        // Make the base repairable again, then power down.
        if (baseStructure != null) baseStructure.SetNeedsRepair(true);
        gameObject.SetActive(false);
    }

    private Transform FindClosestEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(Pivot.position, range, enemyMask, QueryTriggerInteraction.Collide);
        Transform best = null;
        float bestSqr = float.MaxValue;
        foreach (var c in hits)
        {
            if (c.GetComponentInParent<IDamageable>() == null) continue;
            float sqr = (c.transform.position - Pivot.position).sqrMagnitude;
            if (sqr < bestSqr) { bestSqr = sqr; best = c.transform; }
        }
        return best;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere((rotationPivot != null ? rotationPivot : transform).position, range);
    }
}
