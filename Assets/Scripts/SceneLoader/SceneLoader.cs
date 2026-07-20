using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(int buildIndex)
    {
        StartCoroutine(LoadRoutine(buildIndex));
    }

    private IEnumerator LoadRoutine(int buildIndex)
    {
        yield return Fade(0f, 1f);

        yield return SceneManager.LoadSceneAsync(buildIndex);

        yield return Fade(1f, 0f);
    }

    private IEnumerator Fade(float start, float end)
    {
        fadeGroup.blocksRaycasts = true;    

        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(start, end, t / fadeDuration);
            yield return null;
        }

        fadeGroup.alpha = end;
        fadeGroup.blocksRaycasts = end > 0.9f;
    }
}
