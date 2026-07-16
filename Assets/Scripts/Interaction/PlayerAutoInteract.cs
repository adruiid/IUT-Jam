using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using StarterAssets;

/// <summary>
/// Click-to-walk-then-interact. Given a target, it checks reachability, walks the
/// player there with a NavMeshAgent (yielding the ThirdPersonController for the
/// duration), and fires the interaction on arrival. Any manual movement input
/// cancels the walk AND the queued interaction.
///
/// Setup: add a NavMeshAgent to the Player, bake a NavMesh on your walkable ground,
/// and assign the references below. PlayerInteractor calls GoTo() on an out-of-range click.
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
    [Tooltip("How close to arrive before interacting. Keep <= your interaction range.")]
    [SerializeField] private float stoppingDistance = 1.2f;
    [Tooltip("Search radius for a walkable point near the target (its footprint isn't walkable).")]
    [SerializeField] private float targetSampleRadius = 2f;

    [Header("Animation")]
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string motionSpeedParam = "MotionSpeed";

    private Interactable _target;
    private bool _walking;
    private int _speedHash, _motionHash;

    // Camera target is a child of the player; we freeze its world rotation during the
    // walk so the body turning to face movement doesn't drag the camera around.
    private Transform _cameraTarget;
    private Quaternion _cameraTargetRotation;

    public bool IsWalking => _walking;

    private void Awake()
    {
        _speedHash = Animator.StringToHash(speedParam);
        _motionHash = Animator.StringToHash(motionSpeedParam);

        // Auto-wire anything left unassigned (all live on the Player).
        if (interactor == null) interactor = GetComponent<PlayerInteractor>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (characterController == null) characterController = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        // Resolve the ThirdPersonController by type (reliable). Only search if the
        // assigned reference isn't already one.
        if (!(movementController is ThirdPersonController))
        {
            ThirdPersonController tpc = GetComponentInParent<ThirdPersonController>();
            if (tpc == null) tpc = FindFirstObjectByType<ThirdPersonController>();
            if (tpc != null) movementController = tpc;
        }

        if (movementController == null)
            Debug.LogWarning("PlayerAutoInteract: no ThirdPersonController found to disable during auto-walk.", this);

        // Cache the Cinemachine camera target so we can keep it steady while walking.
        if (movementController is ThirdPersonController camTpc && camTpc.CinemachineCameraTarget != null)
            _cameraTarget = camTpc.CinemachineCameraTarget.transform;

        if (agent != null) agent.enabled = false; // off until we need it
    }

    /// <summary>
    /// Walk to an interactable and interact on arrival. Returns false (does nothing)
    /// if it's unreachable — the failsafe.
    /// </summary>
    public bool GoTo(Interactable target)
    {
        if (target == null || agent == null) return false;

        // Snap start + target onto the NavMesh.
        if (!NavMesh.SamplePosition(transform.position, out var startHit, targetSampleRadius, NavMesh.AllAreas))
            return false; // player isn't near a NavMesh
        if (!NavMesh.SamplePosition(target.transform.position, out var destHit, targetSampleRadius, NavMesh.AllAreas))
            return false; // no walkable ground near the target

        // Confirm a COMPLETE path exists before committing (failsafe).
        var path = new NavMeshPath();
        if (!NavMesh.CalculatePath(startHit.position, destHit.position, NavMesh.AllAreas, path)
            || path.status != NavMeshPathStatus.PathComplete)
            return false; // unreachable -> don't auto-path

        BeginWalk(target, destHit.position);
        return true;
    }

    private void BeginWalk(Interactable target, Vector3 dest)
    {
        _target = target;
        _walking = true;

        // Hand movement over to the agent.
        if (movementController != null) movementController.enabled = false;
        if (characterController != null) characterController.enabled = false;

        // Remember the camera's current orbit so it stays put while the body turns.
        if (_cameraTarget != null) _cameraTargetRotation = _cameraTarget.rotation;

        agent.enabled = true;
        agent.Warp(transform.position);      // ensure it's on the NavMesh
        agent.speed = moveSpeed;
        agent.angularSpeed = turnSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.updateRotation = true;
        agent.isStopped = false;
        agent.SetDestination(dest);
    }

    private void Update()
    {
        if (!_walking) return;

        // Manual movement cancels the walk and the queued interaction.
        if (ManualMoveRequested()) { Cancel(); return; }

        // Target went away or can no longer be interacted with.
        if (_target == null || !_target.CanInteract(interactor)) { Cancel(); return; }

        // Play locomotion while walking.
        if (animator != null)
        {
            animator.SetFloat(_speedHash, agent.velocity.magnitude);
            animator.SetFloat(_motionHash, 1f);
        }

        // Arrived?
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            Arrive();
    }

    private void LateUpdate()
    {
        // Keep the camera orbit fixed while auto-walking (TPC normally does this; it's
        // disabled during the walk, so the body's rotation would otherwise carry it).
        if (_walking && _cameraTarget != null)
            _cameraTarget.rotation = _cameraTargetRotation;
    }

    private void Arrive()
    {
        Interactable target = _target;
        EndWalk();

        // Face the target before interacting.
        Vector3 dir = target.transform.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(dir);

        if (interactor != null) interactor.InteractWith(target);
    }

    /// <summary>Stop auto-walking and drop the queued interaction.</summary>
    public void Cancel()
    {
        if (_walking) EndWalk();
    }

    private void EndWalk()
    {
        _walking = false;
        _target = null;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;
        }
        // Return control to the normal controller.
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
        if (gp != null && gp.leftStick.ReadValue().sqrMagnitude > 0.04f) return true; // ~0.2 deadzone

        return false;
    }
}
