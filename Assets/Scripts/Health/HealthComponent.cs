using UnityEngine;
using System;

public class HealthComponent : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private bool canRegen = false;
    [SerializeField] private float regenRate = 2f;
    [SerializeField] private float regenDelay = 5f;

    private float currentHealth;
    private float timeSinceLastDamage;

    public Action OnDeath;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (canRegen && currentHealth > 0f && currentHealth < maxHealth)
        {
            timeSinceLastDamage += Time.deltaTime;
            if (timeSinceLastDamage >= regenDelay)
            {
                currentHealth = Mathf.Min(currentHealth + regenRate * Time.deltaTime, maxHealth);
            }
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        timeSinceLastDamage = 0f;

        Debug.Log("Current Health: " + currentHealth);

        if (currentHealth <= 0f)
            Die();

        // TODO: Add hit animation, VFX, sound, screen shake, etc.
    }

    public void Die()
    {
        //OnDeath?.Invoke();

        // TODO: Play death animation, drop items, ragdoll, etc.
        Destroy(gameObject);
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public float GetHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public bool IsAlive() => currentHealth > 0f;
}
