using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using StarterAssets;

/// <summary>
/// Player combat: a gun (only if owned in the inventory) and an always-available melee
/// dagger.
///
/// Held-item rules:
///   - No gun in inventory  -> dagger always in hand, "Equipped" bool OFF (normal walk),
///                             the gun is never even instantiated.
///   - Gun in inventory     -> dagger in hand while holstered (gun rests on the holster
///                             mount). Left-click fires: the gun snaps to the hand mount,
///                             "Equipped" bool ON, dagger hidden; it stays in hand while
///                             shooting and for a few seconds after, then holsters again.
///   - During an interaction (chop/mine/...) both gun and dagger hide (tool shows instead).
///
/// Melee:
///   - V           -> swing at the closest enemy in range (no aiming).
///   - Right-click on an enemy -> walk up to it (auto-path), then swing.
///   Melee stops movement for the swing and instantly holsters the gun. Damage is applied
///   at the hit frame to the closest enemy in range (no real collision needed).
/// </summary>
[DisallowMultipleComponent]
public class PlayerCombat : MonoBehaviour
{
    [Header("Inventory gate")]
    [Tooltip("The gun Items asset to look for in the inventory. If not owned, no gun is used.")]
    [SerializeField] private Items gunItem;

    [Header("Gun")]
    [Tooltip("Prefab with the Weapon (script 2) on it. Instantiated once you own the gun.")]
    [SerializeField] private GameObject gunPrefab;
    [Tooltip("Where the gun rests when holstered (on the body, e.g. back/hip).")]
    [SerializeField] private Transform holsterMount;
    [Tooltip("Where the gun sits in-hand while equipped (aim/shoot pose).")]
    [SerializeField] private Transform gunHandMount;
    [SerializeField] private Vector3 gunWorldScale = Vector3.one;

    [Header("Dagger (melee)")]
    [SerializeField] private GameObject daggerPrefab;
    [SerializeField] private Transform daggerMount;
    [SerializeField] private Vector3 daggerWorldScale = Vector3.one;

    [Header("Aiming (toward mouse cursor)")]
    [SerializeField] private Camera aimCamera;
    [Tooltip("What the cursor ray hits to find the aim point. Exclude Player/Bullet layers.")]
    [SerializeField] private LayerMask aimMask = ~0;
    [SerializeField] private float aimRayDistance = 200f;
    [SerializeField] private float aimTurnSpeed = 720f;

    [Header("Shooting")]
    [Tooltip("Off = one shot per click (bolt-action). On = hold to fire.")]
    [SerializeField] private bool fullAuto = false;
    [Tooltip("Movement pause + weapon-in-hand pose per shot. Set to your fire animation length.")]
    [SerializeField] private float shootDuration = 0.5f;
    [Tooltip("How long the gun stays in hand after the last shot before auto-holstering.")]
    [SerializeField] private float equipDuration = 5f;

    [Header("Melee")]
    [Tooltip("Layers enemies/animals are on (for the closest-target check).")]
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private float meleeRange = 2.5f;
    [SerializeField] private float meleeDamage = 25f;
    [Tooltip("Delay from swing start to when damage lands. Sync with the melee anim's hit frame.")]
    [SerializeField] private float meleeWindup = 0.3f;
    [Tooltip("Time after the hit before the player can move/act again.")]
    [SerializeField] private float meleeRecovery = 0.3f;
    [SerializeField] private Key meleeKey = Key.V;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [Tooltip("Bool set true while the gun is in hand (drives the gun-holding upper-body pose).")]
    [SerializeField] private string equippedBool = "Equipped";
    [SerializeField] private string fireTrigger = "Fire";
    [SerializeField] private string reloadTrigger = "Reload";
    [SerializeField] private string meleeTrigger = "Melee";

    [Header("Reload")]
    [SerializeField] private Key reloadKey = Key.R;
    [SerializeField] private float reloadTime = 1.2f;

    [Header("References (auto-found if empty)")]
    [SerializeField] private PlayerInteractor interactor;
    [SerializeField] private PlayerAutoInteract autoInteract;

    private Weapon _weapon;
    private GameObject _gunInstance;
    private GameObject _daggerInstance;
    private Transform _currentGunMount;

    private bool _hasGun;
    private float _equipUntil;   // gun stays in hand until this time
    private float _shootUntil;   // movement pause per shot
    private bool _reloading;
    private float _reloadEndTime;
    private bool _meleeing;
    private int _equippedHash;

    private ThirdPersonController _movementController;

    // cached visual state to avoid redundant calls
    private bool _gunActive, _daggerActive, _equippedState;

    private Camera Cam => aimCamera != null ? aimCamera : Camera.main;

    private void Awake()
    {
        if (interactor == null) interactor = GetComponent<PlayerInteractor>();
        if (autoInteract == null) autoInteract = GetComponent<PlayerAutoInteract>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        _movementController = GetComponentInParent<ThirdPersonController>();
        if (_movementController == null) _movementController = FindAnyObjectByType<ThirdPersonController>();
        _equippedHash = Animator.StringToHash(equippedBool);

        // Dagger is always available: spawn it once at its mount.
        if (daggerPrefab != null && daggerMount != null)
        {
            _daggerInstance = Instantiate(daggerPrefab, daggerMount);
            _daggerInstance.transform.localPosition = Vector3.zero;
            _daggerInstance.transform.localRotation = Quaternion.identity;
            _daggerInstance.transform.localScale = CompensateScale(daggerMount, daggerWorldScale);
            _daggerActive = true;
        }
    }

    private void Update()
    {
        bool interacting = interactor != null && interactor.IsInteracting;
        bool walking = autoInteract != null && autoInteract.IsWalking;

        _hasGun = HasGun();
        if (_hasGun) EnsureGun();

        // Combat input only when not interacting / auto-walking / mid-reload / mid-melee.
        if (!interacting && !walking)
        {
            HandleReload();
            if (!_reloading && !_meleeing)
            {
                HandleShoot();
                HandleMelee();
            }
            // Pause movement while shooting or meleeing (never during reload / normal walk).
            SetMovementLocked((Time.time < _shootUntil) || _meleeing);
        }

        RefreshHeldItems(interacting);
    }

    // --- Held items ----------------------------------------------------------

    private void RefreshHeldItems(bool interacting)
    {
        if (interacting)
        {
            SetGunActive(false);
            SetDaggerActive(false);
            SetEquipped(false);
            return;
        }

        bool equipped = _hasGun && Time.time < _equipUntil && !_meleeing;

        if (_hasGun)
        {
            SetGunActive(true);
            AttachGun(equipped ? gunHandMount : holsterMount);
        }
        else
        {
            SetGunActive(false);
        }

        // Dagger is in hand whenever the gun is NOT equipped (holstered walk / melee / no gun).
        SetDaggerActive(!equipped);
        SetEquipped(equipped);
    }

    private void EnsureGun()
    {
        if (_gunInstance != null || gunPrefab == null || holsterMount == null) return;

        _gunInstance = Instantiate(gunPrefab, holsterMount);
        _currentGunMount = holsterMount;
        _gunInstance.transform.localPosition = Vector3.zero;
        _gunInstance.transform.localRotation = Quaternion.identity;
        _gunInstance.transform.localScale = CompensateScale(holsterMount, gunWorldScale);
        _weapon = _gunInstance.GetComponent<Weapon>();
        _gunActive = true;
    }

    private void AttachGun(Transform mount)
    {
        if (_gunInstance == null || mount == null || _currentGunMount == mount) return;
        _currentGunMount = mount;

        Transform t = _gunInstance.transform;
        t.SetParent(mount, worldPositionStays: false);
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = CompensateScale(mount, gunWorldScale);
    }

    private void SetGunActive(bool on)
    {
        if (_gunInstance == null || on == _gunActive) return;
        _gunActive = on;
        _gunInstance.SetActive(on);
    }

    private void SetDaggerActive(bool on)
    {
        if (_daggerInstance == null || on == _daggerActive) return;
        _daggerActive = on;
        _daggerInstance.SetActive(on);
    }

    private void SetEquipped(bool on)
    {
        if (on == _equippedState) return;
        _equippedState = on;
        if (animator != null) animator.SetBool(_equippedHash, on);
    }

    // --- Shooting ------------------------------------------------------------

    private void HandleShoot()
    {
        if (!_hasGun) return; // no gun -> can't shoot

        var mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.isPressed) return;
        if (IsPointerOverUI()) return;
        if (interactor != null && interactor.Hovered != null) return; // clicking interactable -> interact

        Vector3 aimPoint = GetAimPoint();
        FaceAim(aimPoint);

        bool wantFire = fullAuto ? mouse.leftButton.isPressed : mouse.leftButton.wasPressedThisFrame;
        if (!wantFire || _weapon == null) return;

        if (_weapon.CurrentAmmo <= 0) { StartReload(); return; } // auto-reload on empty

        if (_weapon.TryFire(aimPoint))
        {
            if (animator != null && !string.IsNullOrEmpty(fireTrigger)) animator.SetTrigger(fireTrigger);
            _shootUntil = Time.time + shootDuration;   // movement pause
            _equipUntil = Time.time + equipDuration;   // keep gun in hand
        }
    }

    // --- Melee ---------------------------------------------------------------

    private void HandleMelee()
    {
        // V: swing at the closest enemy in range.
        if (Keyboard.current != null && Keyboard.current[meleeKey].wasPressedThisFrame)
        {
            BeginMelee();
            return;
        }

        // Right-click on an enemy: walk up to it, then swing.
        var mouse = Mouse.current;
        if (mouse != null && mouse.rightButton.wasPressedThisFrame && !IsPointerOverUI())
        {
            if (TryGetEnemyUnderCursor(out Vector3 enemyPos))
            {
                _equipUntil = 0f; // holster the gun during the approach
                if (autoInteract != null && autoInteract.GoToPoint(enemyPos, BeginMelee)) return;
                BeginMelee(); // no autopath available -> just swing where we are
            }
        }
    }

    private void BeginMelee()
    {
        if (_meleeing) return;
        _meleeing = true;
        _equipUntil = 0f; // gun holsters instantly for the melee

        FaceClosestEnemy();
        if (animator != null && !string.IsNullOrEmpty(meleeTrigger)) animator.SetTrigger(meleeTrigger);

        StartCoroutine(MeleeRoutine());
    }

    private IEnumerator MeleeRoutine()
    {
        yield return new WaitForSeconds(meleeWindup);

        // Guaranteed hit: damage the closest enemy in range at the hit frame (no collision).
        IDamageable victim = FindClosestEnemy();
        if (victim != null) victim.TakeDamage(meleeDamage);

        yield return new WaitForSeconds(meleeRecovery);
        _meleeing = false;
    }

    private IDamageable FindClosestEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, meleeRange, enemyMask, QueryTriggerInteraction.Collide);
        IDamageable best = null;
        float bestSqr = float.MaxValue;
        foreach (var c in hits)
        {
            var d = c.GetComponentInParent<IDamageable>();
            if (d == null) continue;
            float sqr = (c.transform.position - transform.position).sqrMagnitude;
            if (sqr < bestSqr) { bestSqr = sqr; best = d; }
        }
        return best;
    }

    private void FaceClosestEnemy()
    {
        IDamageable closest = FindClosestEnemy();
        if (closest == null) return;
        Vector3 dir = ((Component)closest).transform.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(dir);
    }

    private bool TryGetEnemyUnderCursor(out Vector3 pos)
    {
        pos = default;
        var cam = Cam;
        if (cam == null || Mouse.current == null) return false;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, aimRayDistance, enemyMask, QueryTriggerInteraction.Collide)
            && hit.collider.GetComponentInParent<IDamageable>() != null)
        {
            pos = hit.collider.transform.position;
            return true;
        }
        return false;
    }

    // --- Reload --------------------------------------------------------------

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

        if (!_hasGun || _weapon == null || _weapon.IsFull || _meleeing) return;

        if (Keyboard.current != null && Keyboard.current[reloadKey].wasPressedThisFrame)
            StartReload();
    }

    private void StartReload()
    {
        _reloading = true;
        _reloadEndTime = Time.time + reloadTime;
        _equipUntil = Time.time + equipDuration; // gun in hand for the reload
        if (animator != null && !string.IsNullOrEmpty(reloadTrigger)) animator.SetTrigger(reloadTrigger);
        if (_weapon != null) _weapon.PlayReloadSfx();
    }

    // --- Helpers -------------------------------------------------------------

    private bool HasGun()
    {
        return gunItem != null && InventoryManager.instance != null
               && InventoryManager.instance.SearchItemCount(gunItem) > 0;
    }

    private void SetMovementLocked(bool locked)
    {
        if (_movementController != null) _movementController.MovementLocked = locked;
    }

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
        if (Physics.Raycast(ray, out RaycastHit hit, aimRayDistance, aimMask, QueryTriggerInteraction.Ignore))
            return hit.point;

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
