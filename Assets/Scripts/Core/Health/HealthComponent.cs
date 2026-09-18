using UnityEngine;
using System;

public interface IDamageable
{
    void TakeDamage(float amount, float hitStun);
    void Heal(float amount);
}

public class HealthComponent : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;

    public float CurrentHealth { get; private set; }

    // Eventos
    public event Action<float> OnDamageTaken;
    public event Action OnHealed;
    public event Action OnDeath;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void ApplyDamage(float amount)
    {
        CurrentHealth -= amount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);
        OnDamageTaken?.Invoke(amount);

        if (CurrentHealth <= 0)
        {
            OnDeath?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, maxHealth);
        OnHealed?.Invoke();
    }
}

