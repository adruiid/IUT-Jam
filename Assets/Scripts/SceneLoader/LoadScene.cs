using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadScene : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Image loadingBarFill;

    private void Awake()
    {
        // Keeps the GameObject alive across scenes
    }

    public void LoadGame()
    {
        StartCoroutine(LoadSceneAsync(1));
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        StartCoroutine(LoadSceneAsync(SceneManager.GetActiveScene().buildIndex));
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    IEnumerator LoadSceneAsync(int sceneId)
    {
        loadingScreen.SetActive(true);

        yield return null;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneId);
        operation.allowSceneActivation = false;

        float timer = 0f;
        float minimumTime = 2f;
        float displayedProgress = 0f;

        while (operation.progress < 0.9f || timer < minimumTime)
        {
            timer += Time.deltaTime;

            float target = Mathf.Clamp01(operation.progress / 0.9f);

            displayedProgress = Mathf.MoveTowards(
                displayedProgress,
                target,
                Time.deltaTime * 0.5f);

            loadingBarFill.fillAmount = displayedProgress;

            yield return null;
        }

        while (displayedProgress < 1f)
        {
            displayedProgress = Mathf.MoveTowards(
                displayedProgress,
                1f,
                Time.deltaTime);

            loadingBarFill.fillAmount = displayedProgress;
            yield return null;
        }

        operation.allowSceneActivation = true;
    }
}

