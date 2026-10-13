using System;
using UnityEngine;

namespace VampireSurvivors.Player
{
    public class PlayerStats : MonoBehaviour
    {
        [Header("Health Settings")]
        public float maxHealth = 100f;
        public float currentHealth;

        [Header("Movement & Combat Stats")]
        public float moveSpeed = 5f;
        public float damageMultiplier = 1.0f;
        public float pickupRadius = 3f;

        [Header("Level & Experience")]
        public int currentLevel = 1;
        public float currentExp = 0f;
        public float expToNextLevel = 100f;

        public event Action<float, float> OnHealthChanged;
        public event Action<float, float> OnExpChanged;
        public event Action<int> OnLevelUp;
        public event Action OnPlayerDeath;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        private void Start()
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnExpChanged?.Invoke(currentExp, expToNextLevel);
        }

        public void TakeDamage(float amount)
        {
            currentHealth -= amount;
            currentHealth = Mathf.Max(0, currentHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            currentHealth += amount;
            currentHealth = Mathf.Min(maxHealth, currentHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void AddExperience(float amount)
        {
            currentExp += amount;
            while (currentExp >= expToNextLevel)
            {
                currentExp -= expToNextLevel;
                LevelUp();
            }
            OnExpChanged?.Invoke(currentExp, expToNextLevel);
        }

        private void LevelUp()
        {
            currentLevel++;
            expToNextLevel = Mathf.Round(expToNextLevel * 1.25f);
            OnLevelUp?.Invoke(currentLevel);
        }

        private void Die()
        {
            OnPlayerDeath?.Invoke();
            Debug.Log("Player has died!");

            Animator anim = GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger("Die");
            }

            PlayerController pc = GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.enabled = false;
            }

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
}
