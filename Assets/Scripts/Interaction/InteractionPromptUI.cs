using UnityEngine;
using TMPro;

/// <summary>
/// Shows/hides an interaction prompt as the player's focus changes.
/// Listens to the Interactor and formats the focused object's prompt,
/// e.g. "[E] Chop". Purely presentational — safe to expand or replace later.
/// </summary>
public class InteractionPromptUI : MonoBehaviour
{
    [Tooltip("The player's Interactor. Auto-found if left empty.")]
    [SerializeField] private Interactor interactor;

    [Tooltip("Root object to show/hide (a panel or the text itself).")]
    [SerializeField] private GameObject promptRoot;

    [Tooltip("Text element that displays the prompt.")]
    [SerializeField] private TMP_Text promptText;

    [Tooltip("{0} is replaced by the interactable's prompt verb.")]
    [SerializeField] private string format = "[E] {0}";

    private void Awake()
    {
        if (interactor == null) interactor = FindFirstObjectByType<Interactor>();
        Hide();
    }

    private void OnEnable()
    {
        if (interactor != null) interactor.FocusChanged += OnFocusChanged;
    }

    private void OnDisable()
    {
        if (interactor != null) interactor.FocusChanged -= OnFocusChanged;
    }

    private void OnFocusChanged(Interactable current)
    {
        if (current == null)
        {
            Hide();
            return;
        }

        if (promptText != null) promptText.text = string.Format(format, current.InteractionPrompt);
        if (promptRoot != null) promptRoot.SetActive(true);
    }

    private void Hide()
    {
        if (promptRoot != null) promptRoot.SetActive(false);
    }
}
