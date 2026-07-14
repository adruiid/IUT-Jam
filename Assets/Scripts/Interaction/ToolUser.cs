using System;
using System.Collections;
using UnityEngine;

/// <summary>The kind of tool an interactable needs. Add more as the game grows.</summary>
public enum ToolType
{
    Axe,
    Pickaxe,
}

/// <summary>
/// Lives on the Player. Owns the tool "mount" (an empty object where tools appear,
/// e.g. in the hand) and the tool objects themselves. When a harvestable is used it
/// calls UseTool(type, onComplete): the matching tool is shown at the mount, its swing
/// animation plays, and onComplete fires when the swing ends (then the tool hides).
///
/// The swing end is detected via an Animation Event (see ToolSwing) for frame-exact
/// timing; if none is set up, a timed fallback (swingDuration) fires it instead.
/// </summary>
[DisallowMultipleComponent]
public class ToolUser : MonoBehaviour
{
    [System.Serializable]
    public class ToolEntry
    {
        public ToolType type;
        [Tooltip("The tool object (with its Animator). Parented to the mount at start.")]
        public GameObject toolObject;
        [Tooltip("Animator trigger fired when the tool is used. Leave empty if the clip plays on enable.")]
        public string animationTrigger = "Use";
    }

    [Header("Mount")]
    [Tooltip("Empty object where tools appear (e.g. the player's hand).")]
    [SerializeField] private Transform toolMount;

    [Header("Tools")]
    [SerializeField] private ToolEntry[] tools;

    [Tooltip("Swing length in seconds. Hides the tool after this, and is the fallback " +
             "for the swing-complete callback if no ToolSwing animation event is set up. " +
             "Set to (at least) your swing clip's length.")]
    [SerializeField] private float swingDuration = 0.6f;

    private Coroutine _fallbackRoutine;
    private GameObject _activeTool;
    private Action _onComplete;
    private bool _swingResolved = true;

    private void Awake()
    {
        // Snap each tool onto the mount (keeping its authored local offset) and hide it.
        foreach (var entry in tools)
        {
            if (entry?.toolObject == null) continue;
            if (toolMount != null) entry.toolObject.transform.SetParent(toolMount, worldPositionStays: false);
            entry.toolObject.SetActive(false);
        }
    }

    /// <summary>
    /// Show the tool for this type and play its swing. <paramref name="onComplete"/> is
    /// invoked once when the swing animation ends, then the tool hides.
    /// </summary>
    public void UseTool(ToolType type, Action onComplete = null)
    {
        ToolEntry entry = GetEntry(type);
        if (entry == null || entry.toolObject == null)
        {
            Debug.LogWarning($"ToolUser: no tool assigned for {type}.", this);
            onComplete?.Invoke(); // don't stall the caller if the tool is missing
            return;
        }

        // Swap out any other visible tool.
        if (_activeTool != null && _activeTool != entry.toolObject) _activeTool.SetActive(false);

        _activeTool = entry.toolObject;
        _activeTool.SetActive(true);

        if (!string.IsNullOrEmpty(entry.animationTrigger))
        {
            var anim = _activeTool.GetComponentInChildren<Animator>();
            if (anim != null) anim.SetTrigger(entry.animationTrigger);
        }

        _onComplete = onComplete;
        _swingResolved = false;

        // Fallback so the swing always resolves even without an animation event.
        if (_fallbackRoutine != null) StopCoroutine(_fallbackRoutine);
        _fallbackRoutine = StartCoroutine(SwingFallback(swingDuration));
    }

    /// <summary>
    /// Marks the current swing as finished: fires the pending callback and hides the tool.
    /// Called by a ToolSwing animation event, or by the fallback timer. Safe to call twice.
    /// </summary>
    public void NotifySwingComplete()
    {
        if (_swingResolved) return;
        _swingResolved = true;

        if (_fallbackRoutine != null) { StopCoroutine(_fallbackRoutine); _fallbackRoutine = null; }

        Action cb = _onComplete;
        _onComplete = null;

        if (_activeTool != null) _activeTool.SetActive(false);
        _activeTool = null;

        cb?.Invoke();
    }

    private IEnumerator SwingFallback(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        NotifySwingComplete();
    }

    private ToolEntry GetEntry(ToolType type)
    {
        foreach (var entry in tools)
            if (entry != null && entry.type == type) return entry;
        return null;
    }
}
