using UnityEngine;
using System;

public class PlayerStatusBasic : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private int maxHealth;
    [SerializeField] private int maxHunger;
    private int currentHealth;
    private int currentHunger;

    public event Action onHealthChanged;
    public event Action onHungerChanged;



    private void Awake()
    {
        currentHealth = maxHealth;
        currentHunger = maxHunger;
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
        return currentHealth;
    }

    public int GetMaxHunger()
    {
        return maxHunger;
    }

    public void SetCurrentHunger(int newHunger)
    {
        currentHunger = newHunger>=0 ? newHunger: 0;
        currentHunger = currentHunger > maxHunger ? maxHunger : currentHunger;
        Debug.Log("Player Current Hunger: " + currentHunger);
        onHungerChanged?.Invoke();
    }
}

