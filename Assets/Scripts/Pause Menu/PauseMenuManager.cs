using UnityEngine;
using UnityEngine.EventSystems;
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
    [SerializeField] private GameObject worldcanvas;

    [Header("Audio")]
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip buttonpressClip;

    [Header("Reference")]
    [SerializeField] private PlayerInteractor playerInteractor;

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


        resumeGameButton.GetComponent<Button>().onClick.AddListener(ResumeButton);
        settingButton.GetComponent<Button>().onClick.AddListener(OnSettingButton);
        returnSettingButton.GetComponent<Button>().onClick.AddListener(OnSettingButton);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            PauseUnpause();
            if (onSettingMenu) OnSettingButton();
        }

        playerInteractor.enabled = !isPaused;
    }

    private void ResumeButton()
    {
        isPaused = !isPaused;
        PauseUnpause();
    }

    public void PauseUnpause()
    {
        InventoryOpenClose.instance.InventoryStatus(false);
        CraftMenuOpenClose.instance.CraftMenuStatus(false);
        source.PlayOneShot(buttonpressClip);
        pauseCanvas.SetActive(isPaused);
        pauseGroup.SetActive(!onSettingMenu);
        worldcanvas.SetActive(!isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void OnSettingButton()
    {
        onSettingMenu = !onSettingMenu;
        source.PlayOneShot(buttonpressClip);
        pauseGroup.SetActive(!onSettingMenu);
        worldcanvas.SetActive(!isPaused);
        settingCanvas.SetActive(onSettingMenu);
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void OnExitButton()
    {
        //Exiting
    }
}
