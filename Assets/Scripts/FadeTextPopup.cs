using TMPro;
using UnityEngine;
using System.Collections;

public class TutorialText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    [Header("Tutorial")]
    [TextArea]
    [SerializeField] private string[] messages;

    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float messageDuration = 5f;
    [SerializeField] private float interval = 2.5f;

    private void Awake()
    {
        SetAlpha(0f);
    }

    private void OnEnable()
    {
        Invoke(nameof(PlayTutorial), 2f);
    }

    public void PlayTutorial()
    {
        StopAllCoroutines();
        StartCoroutine(TutorialRoutine());
    }

    private IEnumerator TutorialRoutine()
    {
        foreach (string message in messages)
        {
            yield return StartCoroutine(ShowMessage(message));

            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator ShowMessage(string message)
    {
        text.text = message;


        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(0f, 1f, t / fadeDuration));
            yield return null;
        }

        SetAlpha(1f);

        yield return new WaitForSeconds(messageDuration);


        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(1f, 0f, t / fadeDuration));
            yield return null;
        }

        SetAlpha(0f);
    }

    private void SetAlpha(float alpha)
    {
        Color color = text.color;
        color.a = alpha;
        text.color = color;
    }
}