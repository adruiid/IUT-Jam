using UnityEngine;
using System;

public class PlayerStatusBasic : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private int maxHealth;
    [SerializeField] private int maxHunger;
    private int currentHealth;
    private int currentHunger;

    [Header("Audio")]
    [Tooltip("Played when hunger is restored (increases, e.g. eating).")]
    [SerializeField] private AudioClip hungerRestoreSfx;
    [Tooltip("Played when hunger drops to/through the low threshold.")]
    [SerializeField] private AudioClip hungerEmptySfx;
    [Tooltip("Hunger at/below which the low sound plays — only when crossing DOWN to it.")]
    [SerializeField] private int hungerLowThreshold = 0;
    [Tooltip("Source for the SFX. Auto-found on this object if empty.")]
    [SerializeField] private AudioSource audioSource;

    public event Action onHealthChanged;
    public event Action onHungerChanged;



    private void Awake()
    {
        currentHealth = maxHealth;
        currentHunger = maxHunger;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public void SetCurrentHealth(int newHP)
    {
        
        currentHealth = newHP >= 0 ? newHP: 0 ;
        currentHealth = currentHealth > maxHealth ? maxHealth : currentHealth;
        Debug.Log("Player Current Health: " + currentHealth);
        onHealthChanged?.Invoke();
    }

    public int GetCurrentHunger()
    {
        return currentHunger;
    }

    public int GetMaxHunger()
    {
        return maxHunger;
    }

    public void SetCurrentHunger(int newHunger)
    {
        int previous = currentHunger;
        currentHunger = newHunger>=0 ? newHunger: 0;
        currentHunger = currentHunger > maxHunger ? maxHunger : currentHunger;

        // Restore sound when hunger goes up; low sound only when crossing DOWN to the threshold.
        if (currentHunger > previous) PlaySfx(hungerRestoreSfx);
        else if (previous > hungerLowThreshold && currentHunger <= hungerLowThreshold) PlaySfx(hungerEmptySfx);

        Debug.Log("Player Current Hunger: " + currentHunger);
        onHungerChanged?.Invoke();
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip == null) return;
        if (audioSource != null) audioSource.PlayOneShot(clip);
        else AudioSource.PlayClipAtPoint(clip, transform.position);
    }
}

