using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Health for an enemy. Implements IDamageable so bullets damage it. Raises a C#
/// event (Died) for the AI and UnityEvents for designer hooks (hit flash, drops, score).
/// </summary>
[DisallowMultipleComponent]
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 30f;

    [Tooltip("Fired each time damage is taken (hit flash / SFX).")]
    public UnityEvent onDamaged;
    [Tooltip("Fired once when health reaches 0 (drops / score / VFX).")]
    public UnityEvent onDied;

    /// <summary>Raised once on death, for the AI to react to.</summary>
    public event Action Died;

    public float Current { get; private set; }
    public float Max => maxHealth;
    public bool IsDead { get; private set; }

    private void Awake() => Current = maxHealth;

    public void TakeDamage(float amount)
    {
        if (IsDead || amount <= 0f) return;

        Current -= amount;
        onDamaged?.Invoke();

        if (Current <= 0f)
        {
            IsDead = true;
            onDied?.Invoke();
            Died?.Invoke();
        }
    }

    /// <summary>Kill instantly (e.g. daybreak purge). Fires the normal death events.</summary>
    public void Kill()
    {
        if (IsDead) return;
        Current = 0f;
        IsDead = true;
        onDied?.Invoke();
        Died?.Invoke();
    }
}
