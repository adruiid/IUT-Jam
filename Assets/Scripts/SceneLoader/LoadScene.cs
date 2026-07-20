using UnityEngine;

public class LoadScene : MonoBehaviour
{

    private void Awake()
    {
        // Keeps the GameObject alive across scenes
    }

    public void LoadGame()
    {
        SceneLoader.Instance.LoadScene(1);
    }

    public void ReturnToMainMenu()
    {
        SceneLoader.Instance.LoadScene(0);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
