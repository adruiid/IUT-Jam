using UnityEngine;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private GameObject resumeGameButton;
    [SerializeField] private GameObject settingButton;
    [SerializeField] private GameObject returnSettingButton;
    [SerializeField] private GameObject exitToMenuButton;

    [Header("UI and Groups")]
    [SerializeField] private GameObject pauseCanvas;
    [SerializeField] private GameObject pauseGroup;
    [SerializeField] private GameObject settingCanvas;

    [Header("Audio")]
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip buttonpressClip;

    public bool isPaused;
    private bool onSettingMenu;

    public static PauseMenuManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        onSettingMenu = false;
        isPaused = false;
        pauseCanvas.SetActive(false);
        settingCanvas.SetActive(false);


        resumeGameButton.GetComponent<Button>().onClick.AddListener(PauseUnpause);
        settingButton.GetComponent<Button>().onClick.AddListener(OnSettingButton);
        returnSettingButton.GetComponent<Button>().onClick.AddListener(OnSettingButton);
        exitToMenuButton.GetComponent<Button>().onClick.AddListener(OnExitButton);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseUnpause();
            if (onSettingMenu) OnSettingButton();
        }
    }

    private void PauseUnpause()
    {
        InventoryOpenClose.instance.InventoryStatus(false);
        source.PlayOneShot(buttonpressClip);
        isPaused = !isPaused;
        pauseCanvas.SetActive(isPaused);
        pauseGroup.SetActive(!onSettingMenu);
        Time.timeScale = isPaused ? 0f : 1f;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;
    }

    private void OnSettingButton()
    {
        onSettingMenu = !onSettingMenu;
        source.PlayOneShot(buttonpressClip);
        pauseGroup.SetActive(!onSettingMenu);
        settingCanvas.SetActive(onSettingMenu);
    }

    private void OnExitButton()
    {
        //Exiting
    }
}
