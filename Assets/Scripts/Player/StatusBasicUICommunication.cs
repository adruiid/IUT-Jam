using UnityEngine;
using UnityEngine.UI;

public class StatusBasicUICommunication : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image healthBar;
    [SerializeField] private Image hungerBar;

    [SerializeField] private PlayerStatusBasic playerStatus;

    [SerializeField] private float lerpSpeed = 5f;

    private float targetHealthFill;
    private float targetHungerFill;

    private void Start()
    {
        playerStatus.onHealthChanged += UpdateHealthTarget;
        playerStatus.onHungerChanged += UpdateHungerTarget;

        targetHealthFill = (float)playerStatus.GetCurrentHealth() / playerStatus.GetMaxHealth();
        targetHungerFill = (float)playerStatus.GetCurrentHunger() / playerStatus.GetMaxHunger();

        healthBar.fillAmount = targetHealthFill;
        hungerBar.fillAmount = targetHungerFill;
    }

    private void Update()
    {
        healthBar.fillAmount = Mathf.Lerp(
            healthBar.fillAmount,
            targetHealthFill,
            Time.deltaTime * lerpSpeed);

        hungerBar.fillAmount = Mathf.Lerp(
            hungerBar.fillAmount,
            targetHungerFill,
            Time.deltaTime * lerpSpeed);
    }

    private void UpdateHealthTarget()
    {
        targetHealthFill =
            (float)playerStatus.GetCurrentHealth() / playerStatus.GetMaxHealth();
    }

    private void UpdateHungerTarget()
    {
        targetHungerFill =
            (float)playerStatus.GetCurrentHunger() / playerStatus.GetMaxHunger();
    }
}