using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

/// <summary>
/// Script 1 — on the Player. Handles combat INPUT + ANIMATION and tells the Weapon
/// (script 2) when to fire/reload. Firing rules:
///   - Left-click on empty space (not UI, not an interactable) draws the gun and fires.
///   - Hold to full-auto (the Weapon enforces fire rate).
///   - R reloads (refills the magazine; infinite reserve).
///   - Auto-holsters after a few seconds without firing.
/// Aiming is toward the mouse cursor's world point.
/// </summary>
[DisallowMultipleComponent]
public class PlayerCombat : MonoBehaviour
{
    [Header("Weapon")]
    [Tooltip("Prefab with the Weapon (script 2) on it. Instantiated once at the mount.")]
    [SerializeField] private GameObject weaponPrefab;
    [Tooltip("Where the weapon is held (e.g. right-hand bone).")]
    [SerializeField] private Transform weaponMount;

    [Header("Aiming (toward mouse cursor)")]
    [SerializeField] private Camera aimCamera;
    [Tooltip("What the cursor ray hits to find the aim point. Exclude Player and Bullet layers.")]
    [SerializeField] private LayerMask aimMask = ~0;
    [SerializeField] private float aimRayDistance = 200f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string drawTrigger = "Draw";
    [SerializeField] private string holsterTrigger = "Holster";
    [SerializeField] private string fireTrigger = "Fire";
    [SerializeField] private string reloadTrigger = "Reload";

    [Header("Holster / Reload")]
    [Tooltip("Auto-holster after this long (seconds) without firing.")]
    [SerializeField] private float holsterDelay = 3f;
    [SerializeField] private Key reloadKey = Key.R;
    [Tooltip("Reload duration — match your reload animation length.")]
    [SerializeField] private float reloadTime = 1.2f;

    [Header("References (auto-found if empty)")]
    [SerializeField] private PlayerInteractor interactor;
    [SerializeField] private PlayerAutoInteract autoInteract;

    private Weapon _weapon;
    private GameObject _weaponInstance;
    private bool _drawn;
    private bool _reloading;
    private float _lastFireTime = -999f;
    private float _reloadEndTime;

    private Camera Cam => aimCamera != null ? aimCamera : Camera.main;

    private void Awake()
    {
        if (interactor == null) interactor = GetComponent<PlayerInteractor>();
        if (autoInteract == null) autoInteract = GetComponent<PlayerAutoInteract>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        HandleReload();
        HandleFire();
        HandleAutoHolster();
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

        // Valid "shoot" input: keep the gun out and (try to) fire.
        _lastFireTime = Time.time;
        if (!_drawn) Draw();

        Vector3 aimPoint = GetAimPoint();
        if (_weapon != null && _weapon.TryFire(aimPoint))
        {
            if (animator != null && !string.IsNullOrEmpty(fireTrigger)) animator.SetTrigger(fireTrigger);
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

        if (!_drawn || _weapon == null || _weapon.IsFull) return;

        if (Keyboard.current != null && Keyboard.current[reloadKey].wasPressedThisFrame)
        {
            _reloading = true;
            _reloadEndTime = Time.time + reloadTime;
            if (animator != null && !string.IsNullOrEmpty(reloadTrigger)) animator.SetTrigger(reloadTrigger);
        }
    }

    private void HandleAutoHolster()
    {
        if (_drawn && !_reloading && Time.time - _lastFireTime > holsterDelay)
            Holster();
    }

    private void Draw()
    {
        if (_weaponInstance == null)
        {
            _weaponInstance = Instantiate(weaponPrefab, weaponMount);
            _weaponInstance.transform.localPosition = Vector3.zero;
            _weaponInstance.transform.localRotation = Quaternion.identity;
            _weapon = _weaponInstance.GetComponent<Weapon>();
        }
        _weaponInstance.SetActive(true);
        _drawn = true;
        if (animator != null && !string.IsNullOrEmpty(drawTrigger)) animator.SetTrigger(drawTrigger);
    }

    private void Holster()
    {
        _drawn = false;
        if (_weaponInstance != null) _weaponInstance.SetActive(false);
        if (animator != null && !string.IsNullOrEmpty(holsterTrigger)) animator.SetTrigger(holsterTrigger);
    }

    private Vector3 GetAimPoint()
    {
        var cam = Cam;
        if (cam == null || Mouse.current == null) return transform.position + transform.forward * 10f;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, aimRayDistance, aimMask, QueryTriggerInteraction.Ignore))
            return hit.point;
        return ray.GetPoint(aimRayDistance); // nothing hit -> aim far along the ray
    }

    private bool IsPointerOverUI() => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
}
