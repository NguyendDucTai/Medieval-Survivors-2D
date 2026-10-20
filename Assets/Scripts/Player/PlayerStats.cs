using System;
using System.Collections;
using UnityEngine;
using VampireSurvivors.Data;
using VampireSurvivors.MetaProgression;

namespace VampireSurvivors.Player
{
    public class PlayerStats : MonoBehaviour
    {
        [Header("Health & Defense Settings")]
        public float maxHealth = 100f;
        public float currentHealth;
        public float armor = 0f;
        public float hpRegen = 0f;
        public int reviveCount = 0;

        [Header("Movement & Combat Stats")]
        public float moveSpeed = 5f;
        public float damageMultiplier = 1.0f;
        public float pickupRadius = 3f;
        public float cooldownReduction = 0f;
        public float critChance = 0.05f;
        public float critMultiplier = 2.0f;

        [Header("Economy & Multipliers")]
        public float goldMultiplier = 1.0f;
        public float expMultiplier = 1.0f;

        [Header("Level & Experience")]
        public int currentLevel = 1;
        public float currentExp = 0f;
        public float expToNextLevel = 100f;

        [Header("In-Run Statistics")]
        public int inRunGold = 0;
        public int inRunKills = 0;
        public float inRunDamageDealt = 0f;

        public event Action<float, float> OnHealthChanged;
        public event Action<float, float> OnExpChanged;
        public event Action<int> OnLevelUp;
        public event Action OnPlayerDeath;
        public event Action<int> OnGoldChanged;
        public event Action OnPlayerRevived;

        private bool isInvulnerable = false;
        private SpriteRenderer spriteRenderer;
        private float lastDamageTime = -10f;
        public float damageCooldown = 0.25f; // Brief invulnerability window (i-frame)
        private float lastShakeTime = -10f;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            ApplyMetaUpgrades();

            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnExpChanged?.Invoke(currentExp, expToNextLevel);
            OnGoldChanged?.Invoke(inRunGold);
        }

        private void Update()
        {
            // Health Regeneration per second
            if (hpRegen > 0 && currentHealth > 0 && currentHealth < maxHealth)
            {
                Heal(hpRegen * Time.deltaTime);
            }
        }

        public void ApplyMetaUpgrades()
        {
            if (UpgradeManager.Instance == null) return;

            maxHealth += UpgradeManager.Instance.GetMaxHealthBonus();
            armor += UpgradeManager.Instance.GetArmorBonus();
            moveSpeed *= UpgradeManager.Instance.GetMoveSpeedMultiplier();
            damageMultiplier *= UpgradeManager.Instance.GetDamageMultiplier();
            cooldownReduction = Mathf.Clamp(cooldownReduction + UpgradeManager.Instance.GetCooldownReduction(), 0f, 0.5f);
            pickupRadius *= UpgradeManager.Instance.GetPickupRadiusMultiplier();
            goldMultiplier *= UpgradeManager.Instance.GetGoldGainMultiplier();
            expMultiplier *= UpgradeManager.Instance.GetExpGainMultiplier();
            critChance += UpgradeManager.Instance.GetCritChanceBonus();
            hpRegen += UpgradeManager.Instance.GetRegenPerSecond();
            reviveCount += UpgradeManager.Instance.GetReviveCount();

            Debug.Log($"<color=cyan>[PlayerStats Initialized]</color> HP:{maxHealth:F0} | Armor:{armor:F0} | Speed:{moveSpeed:F1} | DmgMult:{damageMultiplier:F2} | Revives:{reviveCount}");
        }

        public void TakeDamage(float amount)
        {
            if (isInvulnerable || currentHealth <= 0) return;
            if (Time.time - lastDamageTime < damageCooldown) return;
            lastDamageTime = Time.time;

            // Apply flat armor reduction (minimum 1 damage)
            float finalDamage = Mathf.Max(1f, amount - armor);
            currentHealth -= finalDamage;
            currentHealth = Mathf.Max(0, currentHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // Subtle, crisp screen bump with cooldown to prevent jarring camera spam
            if (Time.time - lastShakeTime > 0.25f)
            {
                lastShakeTime = Time.time;
                var cameraJuice = Camera.main != null ? Camera.main.GetComponent<Core.CameraJuice>() : null;
                if (cameraJuice != null) cameraJuice.Shake(0.04f, 0.12f);
            }

            StartCoroutine(FlashDamageFeedback());

            if (currentHealth <= 0)
            {
                if (reviveCount > 0)
                {
                    StartCoroutine(PerformRevive());
                }
                else
                {
                    Die();
                }
            }
        }

        private IEnumerator FlashDamageFeedback()
        {
            if (spriteRenderer != null)
            {
                Color orig = spriteRenderer.color;
                spriteRenderer.color = new Color(1f, 0.35f, 0.35f, 0.85f);
                yield return new WaitForSeconds(0.12f);
                if (spriteRenderer != null && !isInvulnerable)
                {
                    spriteRenderer.color = orig;
                }
            }
        }

        private IEnumerator PerformRevive()
        {
            reviveCount--;
            isInvulnerable = true;
            currentHealth = maxHealth * 0.5f;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnPlayerRevived?.Invoke();

            Debug.Log($"<color=yellow>[SECOND WIND!]</color> Player revived with 50% HP! Remaining revives: {reviveCount}");

            // Push back nearby enemies
            Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, 6f);
            foreach (var col in nearby)
            {
                if (col.CompareTag("Enemy"))
                {
                    var rb = col.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        Vector2 pushDir = (col.transform.position - transform.position).normalized;
                        rb.AddForce(pushDir * 15f, ForceMode2D.Impulse);
                    }
                }
            }

            // Flashing invulnerability for 2.5s
            float flashDuration = 2.5f;
            float elapsed = 0f;
            while (elapsed < flashDuration)
            {
                if (spriteRenderer != null)
                {
                    Color c = spriteRenderer.color;
                    c.a = (c.a == 1f) ? 0.3f : 1f;
                    spriteRenderer.color = c;
                }
                yield return new WaitForSeconds(0.15f);
                elapsed += 0.15f;
            }

            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = 1f;
                spriteRenderer.color = c;
            }
            isInvulnerable = false;
        }

        public void Heal(float amount)
        {
            if (currentHealth <= 0) return;
            currentHealth += amount;
            currentHealth = Mathf.Min(maxHealth, currentHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void AddExperience(float amount)
        {
            if (currentHealth <= 0) return;
            float actualExp = amount * expMultiplier;
            currentExp += actualExp;
            while (currentExp >= expToNextLevel)
            {
                currentExp -= expToNextLevel;
                LevelUp();
            }
            OnExpChanged?.Invoke(currentExp, expToNextLevel);
        }

        public void AddGold(int amount)
        {
            int actualGold = Mathf.Max(1, Mathf.RoundToInt(amount * goldMultiplier));
            inRunGold += actualGold;
            OnGoldChanged?.Invoke(inRunGold);
        }

        public void RecordEnemyKill(float damage)
        {
            inRunKills++;
            inRunDamageDealt += damage;
        }

        private void LevelUp()
        {
            currentLevel++;
            expToNextLevel = Mathf.Round(expToNextLevel * 1.25f);
            OnLevelUp?.Invoke(currentLevel);

            if (UI.LevelUpUI.Instance != null)
            {
                UI.LevelUpUI.Instance.OpenLevelUp();
            }

            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.CheckProgress(AchievementType.LevelReached, currentLevel);
            }
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

            // Notify Core GameManager
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.GameOver();
            }
        }
    }
}
