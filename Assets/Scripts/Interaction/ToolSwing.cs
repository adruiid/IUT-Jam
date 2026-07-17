using UnityEngine;

/// <summary>
/// Put this on a tool object (the one with the swing Animator). Then add an
/// Animation Event on the LAST frame of the swing clip that calls SwingComplete().
/// That makes the harvest (drops/SFX signal) land exactly when the animation ends.
///
/// Optional: if you don't add the animation event, ToolUser's timed fallback
/// (swingDuration) resolves the swing instead — this component is just for precision.
/// </summary>
public class ToolSwing : MonoBehaviour
{
    [Tooltip("Owning ToolUser. Auto-found in a parent if left empty.")]
    [SerializeField] private ToolUser toolUser;

    private void Awake()
    {
        if (toolUser == null) toolUser = GetComponentInParent<ToolUser>(includeInactive: true);
    }

    /// <summary>Call this from an Animation Event at the end of the swing clip.</summary>
    public void SwingComplete()
    {
        if (toolUser != null) toolUser.NotifySwingComplete();
    }
}
