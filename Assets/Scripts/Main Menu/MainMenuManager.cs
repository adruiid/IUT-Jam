using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Button")]
    [SerializeField]private GameObject startGameButton;
    [SerializeField] private GameObject creditButton;
    [SerializeField] private GameObject settingButton;
    [SerializeField] private GameObject returnCreditButton;
    [SerializeField] private GameObject returnSettingButton;


    [Header("UI and Groups")]
    [SerializeField] private GameObject optionGroup;
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private GameObject settingCanvas;
    [SerializeField] private GameObject creditCanvas;

    [Header("Audio")]
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip buttonpressClip;

    private bool onMainCanvas;


    void Start()
    {
        mainCanvas.SetActive(true);
        onMainCanvas = true;
        startGameButton.SetActive(true);
        optionGroup.SetActive(false);
        creditCanvas.SetActive(false);

        startGameButton.GetComponentInChildren<Button>().onClick.AddListener(OnStartGame);
        creditButton.GetComponent<Button>().onClick.AddListener(OnCreditButton);
        returnCreditButton.GetComponent<Button>().onClick.AddListener(OnCreditButton);
        settingButton.GetComponent<Button>().onClick.AddListener(OnSettingButton);
        returnSettingButton.GetComponent<Button>().onClick.AddListener(OnSettingButton);
    }



    private void OnStartGame()
    {
        source.PlayOneShot(buttonpressClip);
        startGameButton.SetActive(false);
        optionGroup.SetActive(true);
    }

    private void OnCreditButton()
    {
        onMainCanvas = !onMainCanvas;
        source.PlayOneShot(buttonpressClip);
        mainCanvas.SetActive(onMainCanvas);
        creditCanvas.SetActive(!onMainCanvas);
    }

    private void OnSettingButton()
    {
        onMainCanvas = !onMainCanvas;
        source.PlayOneShot(buttonpressClip);
        mainCanvas.SetActive(onMainCanvas);
        settingCanvas.SetActive(!onMainCanvas);
    }
}
