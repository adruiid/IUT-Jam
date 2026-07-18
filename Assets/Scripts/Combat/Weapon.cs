using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Script 2 — on the weapon prefab. Owns bullet spawning, SFX/VFX, the magazine, and
/// fire-rate timing. PlayerCombat (script 1) calls TryFire()/Reload(). Total ammo is
/// infinite; only the magazine is limited.
/// </summary>
[DisallowMultipleComponent]
public class Weapon : MonoBehaviour
{
    [Header("Firing")]
    [Tooltip("Spawn point for bullets (the barrel tip).")]
    [SerializeField] private Transform muzzle;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletDamage = 10f;
    [Tooltip("Rounds per second.")]
    [SerializeField] private float fireRate = 8f;

    [Header("Ammo")]
    [Tooltip("Rounds per magazine. 1 for a bolt-action (reload after every shot); 2 for a double.")]
    [SerializeField] private int magSize = 1;

    [Header("Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fireSfx;
    [SerializeField] private AudioClip emptySfx;
    [Tooltip("Optional muzzle-flash particle system (a child of the muzzle). Played per shot.")]
    [SerializeField] private ParticleSystem muzzleFlash;

    [Tooltip("Fires whenever ammo changes (fire/reload) — hook your ammo UI here.")]
    public UnityEvent onAmmoChanged;

    public int CurrentAmmo { get; private set; }
    public int MagSize => magSize;
    public bool IsFull => CurrentAmmo >= magSize;

    private float _nextFireTime;

    private void Awake() => CurrentAmmo = magSize;

    /// <summary>
    /// Attempts to fire toward a world-space aim point. Returns true only if a bullet
    /// was actually fired (so the player can play the fire animation). Respects fire rate.
    /// </summary>
    public bool TryFire(Vector3 aimPoint)
    {
        if (Time.time < _nextFireTime) return false;             // rate limited
        _nextFireTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);

        if (CurrentAmmo <= 0)
        {
            if (emptySfx != null && audioSource != null) audioSource.PlayOneShot(emptySfx);
            return false;                                        // out of ammo -> reload
        }

        CurrentAmmo--;
        onAmmoChanged?.Invoke();

        FireBullet(aimPoint);

        if (fireSfx != null && audioSource != null) audioSource.PlayOneShot(fireSfx);
        if (muzzleFlash != null) muzzleFlash.Play();

        return true;
    }

    private void FireBullet(Vector3 aimPoint)
    {
        if (bulletPrefab == null) return;

        Vector3 origin = muzzle != null ? muzzle.position : transform.position;
        Vector3 dir = aimPoint - origin;
        dir.y = 0f; // isometric: bullets fly flat, staying at the muzzle's height
        if (dir.sqrMagnitude < 0.0001f)
        {
            dir = muzzle != null ? muzzle.forward : transform.forward;
            dir.y = 0f;
        }
        dir.Normalize();

        GameObject b = Instantiate(bulletPrefab, origin, Quaternion.LookRotation(dir));
        var bullet = b.GetComponent<Bullet>();
        if (bullet != null) bullet.Init(dir, bulletDamage);
    }

    /// <summary>Refill the magazine (infinite reserve).</summary>
    public void Reload()
    {
        CurrentAmmo = magSize;
        onAmmoChanged?.Invoke();
    }
}
