using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using StarterAssets;

/// <summary>
/// Click-to-walk-then-do-something. Walks the player to a target with a NavMeshAgent
/// (yielding the ThirdPersonController for the duration) and invokes a callback on
/// arrival. Used for auto-interact (walk to an interactable) and auto-melee (walk to
/// an enemy). Any manual movement input cancels the walk and the queued action.
/// </summary>
[DisallowMultipleComponent]
public class PlayerAutoInteract : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInteractor interactor;
    [SerializeField] private NavMeshAgent agent;
    [Tooltip("The movement controller (ThirdPersonController). Disabled while auto-walking.")]
    [SerializeField] private Behaviour movementController;
    [Tooltip("Disabled while auto-walking so only the agent moves the transform.")]
    [SerializeField] private CharacterController characterController;
    [Tooltip("Player animator, driven so the walk animation plays during auto-walk.")]
    [SerializeField] private Animator animator;

    [Header("Pathing")]
    [Tooltip("Auto-move speed. Match ThirdPersonController.SprintSpeed so the run animation blends in.")]
    [SerializeField] private float moveSpeed = 5.335f;
    [Tooltip("How fast the character turns to face the path, in degrees/sec (agent default is 120).")]
    [SerializeField] private float turnSpeed = 720f;
    [Tooltip("How close to arrive before acting. Keep <= your interaction/melee range.")]
    [SerializeField] private float stoppingDistance = 1.2f;
    [Tooltip("Search radius for a walkable point near the target (its footprint isn't walkable).")]
    [SerializeField] private float targetSampleRadius = 2f;

    [Header("Animation")]
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string motionSpeedParam = "MotionSpeed";

    private Interactable _target;   // set only for interactable walks (validity + interact)
    private Action _onArrive;       // invoked on arrival
    private Vector3 _destPoint;     // where we're walking to (for facing on arrival)
    private bool _walking;
    private int _speedHash, _motionHash;

    // Camera target is a child of the player; freeze its world rotation during the walk
    // so the body turning to face movement doesn't drag the camera around.
    private Transform _cameraTarget;
    private Quaternion _cameraTargetRotation;

    public bool IsWalking => _walking;

    private void Awake()
    {
        _speedHash = Animator.StringToHash(speedParam);
        _motionHash = Animator.StringToHash(motionSpeedParam);

        if (interactor == null) interactor = GetComponent<PlayerInteractor>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (characterController == null) characterController = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        if (!(movementController is ThirdPersonController))
        {
            ThirdPersonController tpc = GetComponentInParent<ThirdPersonController>();
            if (tpc == null) tpc = FindAnyObjectByType<ThirdPersonController>();
            if (tpc != null) movementController = tpc;
        }
        if (movementController == null)
            Debug.LogWarning("PlayerAutoInteract: no ThirdPersonController found to disable during auto-walk.", this);

        if (movementController is ThirdPersonController camTpc && camTpc.CinemachineCameraTarget != null)
            _cameraTarget = camTpc.CinemachineCameraTarget.transform;

        if (agent != null) agent.enabled = false; // off until we need it
    }

    /// <summary>Walk to an interactable and interact on arrival. False (no-op) if unreachable.</summary>
    public bool GoTo(Interactable target)
    {
        if (target == null) return false;
        return BeginWalkTo(target.transform.position, target,
                           () => { if (interactor != null) interactor.InteractWith(target); });
    }

    /// <summary>Walk to a world point and invoke onArrive there. False (no-op) if unreachable.</summary>
    public bool GoToPoint(Vector3 worldPos, Action onArrive)
    {
        return BeginWalkTo(worldPos, null, onArrive);
    }

    private bool BeginWalkTo(Vector3 worldPos, Interactable target, Action onArrive)
    {
        if (agent == null) return false;

        // Snap start + destination onto the NavMesh and confirm a full path (failsafe).
        if (!NavMesh.SamplePosition(transform.position, out var startHit, targetSampleRadius, NavMesh.AllAreas)) return false;
        if (!NavMesh.SamplePosition(worldPos, out var destHit, targetSampleRadius, NavMesh.AllAreas)) return false;

        var path = new NavMeshPath();
        if (!NavMesh.CalculatePath(startHit.position, destHit.position, NavMesh.AllAreas, path)
            || path.status != NavMeshPathStatus.PathComplete)
            return false;

        _target = target;
        _onArrive = onArrive;
        _destPoint = destHit.position;
        _walking = true;

        if (movementController != null) movementController.enabled = false;
        if (characterController != null) characterController.enabled = false;
        if (_cameraTarget != null) _cameraTargetRotation = _cameraTarget.rotation;

        agent.enabled = true;
        agent.Warp(transform.position);
        agent.speed = moveSpeed;
        agent.angularSpeed = turnSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.updateRotation = true;
        agent.isStopped = false;
        agent.SetDestination(_destPoint);
        return true;
    }

    private void Update()
    {
        if (!_walking) return;

        if (ManualMoveRequested()) { Cancel(); return; }

        // If walking to an interactable, bail if it becomes non-interactable (e.g. destroyed).
        if (_target != null && !_target.CanInteract(interactor)) { Cancel(); return; }

        if (animator != null)
        {
            animator.SetFloat(_speedHash, agent.velocity.magnitude);
            animator.SetFloat(_motionHash, 1f);
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            Arrive();
    }

    private void LateUpdate()
    {
        if (_walking && _cameraTarget != null)
            _cameraTarget.rotation = _cameraTargetRotation;
    }

    private void Arrive()
    {
        Action cb = _onArrive;
        Vector3 dest = _destPoint;
        EndWalk();

        // Face the destination before acting.
        Vector3 dir = dest - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(dir);

        cb?.Invoke();
    }

    /// <summary>Stop auto-walking and drop the queued action.</summary>
    public void Cancel()
    {
        if (_walking) EndWalk();
    }

    private void EndWalk()
    {
        _walking = false;
        _target = null;
        _onArrive = null;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;
        }
        if (characterController != null) characterController.enabled = true;
        if (movementController != null) movementController.enabled = true;

        if (animator != null) animator.SetFloat(_speedHash, 0f);
    }

    private bool ManualMoveRequested()
    {
        var kb = Keyboard.current;
        if (kb != null && (kb.wKey.isPressed || kb.aKey.isPressed || kb.sKey.isPressed || kb.dKey.isPressed ||
                           kb.upArrowKey.isPressed || kb.downArrowKey.isPressed ||
                           kb.leftArrowKey.isPressed || kb.rightArrowKey.isPressed))
            return true;

        var gp = Gamepad.current;
        if (gp != null && gp.leftStick.ReadValue().sqrMagnitude > 0.04f) return true;

        return false;
    }
}
