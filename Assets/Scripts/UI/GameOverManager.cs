using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOverCanvas;

    [Header("UI")]
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private CanvasGroup returnButton;

    [Header("Timings")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float spacingDuration = 1f;
    [SerializeField] private AudioSource gameOverMusicSource;

    [SerializeField] private AudioClip gameOverMusic;



    [ContextMenu("Game Over")]
    public void GameOver()
    {
        StartCoroutine(ShowGameOverScreen());
    }

    private IEnumerator ShowGameOverScreen()
    {


        AudioSource[] sources = FindObjectsByType<AudioSource>();

        foreach (AudioSource source in sources)
        {
            if (source != gameOverMusicSource)
                source.enabled = false;
        }

        gameOverMusicSource.PlayOneShot(gameOverMusic);

        gameOverCanvas.SetActive(true);

        // Initial state
        SetAlpha(background, 0);
        SetAlpha(gameOverText, 0);

        gameOverText.characterSpacing = 91.1f;

        returnButton.alpha = 0;
        returnButton.interactable = false;
        returnButton.blocksRaycasts = false;

        // 1. Fade background
        yield return FadeImage(background, 0, 1, fadeDuration);

        // 2. Fade text
        yield return FadeTMP(gameOverText, 0, 1, fadeDuration);

        // 3. Animate character spacing
        yield return LerpCharacterSpacing(91.1f, 16.6f, spacingDuration);

        // 4. Fade button
        yield return FadeCanvasGroup(returnButton, 0, 1, fadeDuration);

        returnButton.interactable = true;
        returnButton.blocksRaycasts = true;

        yield return null;

        Time.timeScale = 0f;
    }

    IEnumerator FadeImage(Image image, float from, float to, float duration)
    {
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;

            Color c = image.color;
            c.a = Mathf.Lerp(from, to, t / duration);
            image.color = c;

            yield return null;
        }
    }

    IEnumerator FadeTMP(TextMeshProUGUI text, float from, float to, float duration)
    {
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;

            Color c = text.color;
            c.a = Mathf.Lerp(from, to, t / duration);
            text.color = c;

            yield return null;
        }
    }

    IEnumerator LerpCharacterSpacing(float from, float to, float duration)
    {
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            gameOverText.characterSpacing = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
    }

    IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
    }

    void SetAlpha(Graphic graphic, float alpha)
    {
        Color c = graphic.color;
        c.a = alpha;
        graphic.color = c;
    }
}