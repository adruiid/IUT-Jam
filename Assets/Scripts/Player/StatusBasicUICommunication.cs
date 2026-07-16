using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class StatusBasicUICommunication : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image healthBar;
    [SerializeField] private Image hungerBar;

    [SerializeField] private PlayerStatusBasic playerStatus;

    private void Start()
    {
        playerStatus.onHealthChanged += UpdateHealthBar;
        playerStatus.onHungerChanged += UpdateHungerBar;
    }

    private void UpdateHealthBar()
    {
        healthBar.fillAmount = ((float)playerStatus.GetCurrentHealth() / playerStatus.GetMaxHealth());
    }

    private void UpdateHungerBar()
    {
        hungerBar.fillAmount = ((float)playerStatus.GetCurrentHunger() / playerStatus.GetMaxHunger());
    }
}
