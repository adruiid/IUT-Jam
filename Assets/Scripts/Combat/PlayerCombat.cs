using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using StarterAssets;

/// <summary>
/// Script 1 — on the Player. Combat INPUT + ANIMATION; tells the Weapon (script 2)
/// when to fire/reload. The weapon is always out (no draw/holster). Firing rules:
///   - Left-click on empty space (not UI, not an interactable) fires toward the cursor.
///   - While the fire button is held, the player rotates to face the aim point.
///   - R reloads (bolt-action: tiny magazine, infinite reserve).
/// </summary>
[DisallowMultipleComponent]
public class PlayerCombat : MonoBehaviour
{
    [Header("Weapon")]
    [Tooltip("Prefab with the Weapon (script 2) on it. Instantiated once at the mount, always visible.")]
    [SerializeField] private GameObject weaponPrefab;
    [Tooltip("Where the weapon is held (e.g. right-hand bone).")]
    [SerializeField] private Transform weaponMount;
    [Tooltip("Desired WORLD scale of the weapon. Compensates for a scaled hand bone " +
             "(Mixamo rigs often have near-zero bone scale). Tweak if the gun looks too big/small.")]
    [SerializeField] private Vector3 weaponWorldScale = Vector3.one;

    [Header("Aiming (toward mouse cursor)")]
    [SerializeField] private Camera aimCamera;
    [Tooltip("What the cursor ray hits to find the aim point. Exclude Player and Bullet layers.")]
    [SerializeField] private LayerMask aimMask = ~0;
    [SerializeField] private float aimRayDistance = 200f;
    [Tooltip("How fast the player turns to face the aim while firing (deg/sec).")]
    [SerializeField] private float aimTurnSpeed = 720f;

    [Header("Firing")]
    [Tooltip("Off = one shot per click (bolt-action). On = hold to fire.")]
    [SerializeField] private bool fullAuto = false;
    [Tooltip("Movement is paused for this long after each shot (0 = never pause). Reload never pauses.")]
    [SerializeField] private float shootMovementLockDuration = 0.3f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string fireTrigger = "Fire";
    [SerializeField] private string reloadTrigger = "Reload";

    [Header("Reload")]
    [SerializeField] private Key reloadKey = Key.R;
    [Tooltip("Reload duration — match your reload animation length.")]
    [SerializeField] private float reloadTime = 1.2f;

    [Header("References (auto-found if empty)")]
    [SerializeField] private PlayerInteractor interactor;
    [SerializeField] private PlayerAutoInteract autoInteract;

    private Weapon _weapon;
    private GameObject _weaponInstance;
    private bool _weaponVisible = true;
    private bool _reloading;
    private float _reloadEndTime;
    private float _shootLockUntil;
    private ThirdPersonController _movementController;

    private Camera Cam => aimCamera != null ? aimCamera : Camera.main;

    private void Awake()
    {
        if (interactor == null) interactor = GetComponent<PlayerInteractor>();
        if (autoInteract == null) autoInteract = GetComponent<PlayerAutoInteract>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        _movementController = GetComponentInParent<ThirdPersonController>();
        if (_movementController == null) _movementController = FindAnyObjectByType<ThirdPersonController>();

        // Weapon is always out (except during interactions): spawn it once.
        if (weaponPrefab != null && weaponMount != null)
        {
            _weaponInstance = Instantiate(weaponPrefab, weaponMount);
            _weaponInstance.transform.localPosition = Vector3.zero;
            _weaponInstance.transform.localRotation = Quaternion.identity;
            _weaponInstance.transform.localScale = CompensateScale(weaponMount, weaponWorldScale);
            _weaponInstance.SetActive(true); // ensure visible even if the prefab root was inactive
            _weaponVisible = true;
            _weapon = _weaponInstance.GetComponent<Weapon>();
            if (_weapon == null)
                Debug.LogWarning("PlayerCombat: weapon prefab has no Weapon component — it won't fire.", this);
        }
        else
        {
            Debug.LogWarning("PlayerCombat: assign both Weapon Prefab and Weapon Mount — no gun will spawn/fire.", this);
        }
    }

    private void Update()
    {
        // Hide the gun while doing an interaction (chop/mine/dig/repair) and show it after.
        // Shopkeeper has no interaction animation, so it never hides the gun.
        bool interacting = interactor != null && interactor.IsInteracting;
        SetWeaponVisible(!interacting);
        if (interacting) return;                                     // interaction owns movement lock
        if (autoInteract != null && autoInteract.IsWalking) return;  // autopath owns movement

        // Pause movement briefly while shooting (never during reload).
        SetMovementLocked(Time.time < _shootLockUntil);

        HandleReload();
        HandleFire();
    }

    private void SetMovementLocked(bool locked)
    {
        if (_movementController != null) _movementController.MovementLocked = locked;
    }

    private void SetWeaponVisible(bool visible)
    {
        if (_weaponInstance == null || visible == _weaponVisible) return;
        _weaponVisible = visible;
        _weaponInstance.SetActive(visible);
    }

    private void HandleFire()
    {
        if (_reloading) return;
        if (interactor != null && interactor.IsInteracting) return;   // busy chopping/mining/repairing
        if (autoInteract != null && autoInteract.IsWalking) return;   // auto-running to an interactable

        var mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.isPressed) return;

        if (IsPointerOverUI()) return;                                // clicking UI
        if (interactor != null && interactor.Hovered != null) return; // clicking an interactable -> interact, not fire

        // Valid combat input: face the cursor and (try to) fire.
        Vector3 aimPoint = GetAimPoint();
        FaceAim(aimPoint);

        bool wantFire = fullAuto ? mouse.leftButton.isPressed : mouse.leftButton.wasPressedThisFrame;
        if (!wantFire || _weapon == null) return;

        // Empty mag: auto-reload instead of firing.
        if (_weapon.CurrentAmmo <= 0)
        {
            StartReload();
            return;
        }

        if (_weapon.TryFire(aimPoint))
        {
            if (animator != null && !string.IsNullOrEmpty(fireTrigger)) animator.SetTrigger(fireTrigger);
            _shootLockUntil = Time.time + shootMovementLockDuration; // pause movement briefly
        }
    }

    private void HandleReload()
    {
        if (_reloading)
        {
            if (Time.time >= _reloadEndTime)
            {
                _reloading = false;
                if (_weapon != null) _weapon.Reload();
            }
            return;
        }

        if (_weapon == null || _weapon.IsFull) return;

        if (Keyboard.current != null && Keyboard.current[reloadKey].wasPressedThisFrame)
            StartReload();
    }

    private void StartReload()
    {
        _reloading = true;
        _reloadEndTime = Time.time + reloadTime;
        if (animator != null && !string.IsNullOrEmpty(reloadTrigger)) animator.SetTrigger(reloadTrigger);
        if (_weapon != null) _weapon.PlayReloadSfx();
    }

    // Rotate the body horizontally toward the aim point. Done in Update so the
    // ThirdPersonController's LateUpdate camera-pinning keeps the camera steady.
    private void FaceAim(Vector3 aimPoint)
    {
        Vector3 dir = aimPoint - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;
        Quaternion target = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, aimTurnSpeed * Time.deltaTime);
    }

    private Vector3 GetAimPoint()
    {
        var cam = Cam;
        if (cam == null || Mouse.current == null) return transform.position + transform.forward * 10f;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        // Prefer a real surface hit (gives correct height for shooting up/down).
        if (Physics.Raycast(ray, out RaycastHit hit, aimRayDistance, aimMask, QueryTriggerInteraction.Ignore))
            return hit.point;

        // Fallback: intersect a horizontal plane at the player's height. This always
        // yields a valid point in ANY direction, so the character can aim 360* even
        // when the ray misses all colliders (which otherwise limited turning to ~180*).
        Plane ground = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));
        if (ground.Raycast(ray, out float dist))
            return ray.GetPoint(dist);

        return transform.position + transform.forward * 10f;
    }

    private bool IsPointerOverUI() => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

    /// <summary>
    /// Local scale that yields <paramref name="desiredWorld"/> world scale under a
    /// (possibly scaled) parent bone. Fixes props spawning tiny/huge on rig hands.
    /// </summary>
    public static Vector3 CompensateScale(Transform parent, Vector3 desiredWorld)
    {
        Vector3 p = parent.lossyScale;
        return new Vector3(
            Mathf.Approximately(p.x, 0f) ? desiredWorld.x : desiredWorld.x / p.x,
            Mathf.Approximately(p.y, 0f) ? desiredWorld.y : desiredWorld.y / p.y,
            Mathf.Approximately(p.z, 0f) ? desiredWorld.z : desiredWorld.z / p.z);
    }
}
