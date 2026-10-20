using System;
using System.Collections;
using UnityEngine;
using VampireSurvivors.Collectibles;
using VampireSurvivors.Player;
using VampireSurvivors.UI;

namespace VampireSurvivors.Enemies
{
    public class EnemyStats : MonoBehaviour
    {
        [Header("Enemy Attributes")]
        public string enemyName = "Enemy";
        public float maxHealth = 45f;
        public float currentHealth;
        public float moveSpeed = 2.0f;
        public float attackDamage = 10f;
        public float expValue = 10f;
        public bool isBoss = false;

        [Header("Drop Settings")]
        public GameObject expGemPrefab;
        public GameObject goldCoinPrefab;
        public GameObject bossChestPrefab;
        [Range(0f, 1f)] public float goldDropChance = 0.35f;

        public event Action OnEnemyDeath;

        private bool isDying = false;
        private SpriteRenderer spriteRenderer;
        private Color originalColor = Color.white;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) originalColor = spriteRenderer.color;

#if UNITY_EDITOR
            if (goldCoinPrefab == null)
            {
                goldCoinPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/GoldCoin.prefab");
            }
            if (bossChestPrefab == null)
            {
                bossChestPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BossChest.prefab");
            }
            if (expGemPrefab == null)
            {
                expGemPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ExpGem.prefab");
            }
#endif
        }

        private void OnEnable()
        {
            currentHealth = maxHealth;
            isDying = false;

            if (spriteRenderer != null) spriteRenderer.color = originalColor;

            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = true;

            EnemyAI ai = GetComponent<EnemyAI>();
            if (ai != null)
            {
                ai.enabled = true;
                ai.SetDead(false);
            }
        }

        public void TakeDamage(float amount, bool isCrit = false)
        {
            if (isDying) return;

            currentHealth -= amount;

            // Spawn floating damage numbers
            if (DamageNumberPool.Instance != null)
            {
                DamageNumberPool.Instance.SpawnDamage(amount, transform.position, isCrit);
            }

            // Sprite flash white on hit
            StartCoroutine(FlashWhite());

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private IEnumerator FlashWhite()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(1f, 0.4f, 0.4f); // Reddish flash
                yield return new WaitForSeconds(0.08f);
                if (!isDying && spriteRenderer != null)
                {
                    spriteRenderer.color = originalColor;
                }
            }
        }

        private void Die()
        {
            if (isDying) return;
            isDying = true;

            OnEnemyDeath?.Invoke();

            // Notify player of kill
            PlayerStats player = FindObjectOfType<PlayerStats>();
            if (player != null)
            {
                player.RecordEnemyKill(maxHealth);
            }

            // Check achievement progress
            if (MetaProgression.AchievementManager.Instance != null)
            {
                int totalKills = player != null ? player.inRunKills : 1;
                MetaProgression.AchievementManager.Instance.CheckProgress(MetaProgression.AchievementType.TotalKills, totalKills);
            }

            // Disable collider
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            // Stop AI
            EnemyAI ai = GetComponent<EnemyAI>();
            if (ai != null) ai.SetDead(true);

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;

            // Play Die animation
            Animator anim = GetComponent<Animator>();
            if (anim != null) anim.SetTrigger("Die");

            // Drop Experience Gem
            SpawnDrops();

            StartCoroutine(DestroyAfterDelay(0.6f));
        }

        private void SpawnDrops()
        {
            // Exp Gem
            if (expGemPrefab != null)
            {
                if (Core.ObjectPooler.Instance != null && Core.ObjectPooler.Instance.poolDictionary.ContainsKey("ExpGem"))
                {
                    Core.ObjectPooler.Instance.SpawnFromPool("ExpGem", transform.position, Quaternion.identity);
                }
                else
                {
                    Instantiate(expGemPrefab, transform.position, Quaternion.identity);
                }
            }

            // Gold Coin Drop
            if (!isBoss && UnityEngine.Random.value < goldDropChance)
            {
                if (goldCoinPrefab != null)
                {
                    Instantiate(goldCoinPrefab, transform.position + new Vector3(UnityEngine.Random.Range(-0.3f, 0.3f), UnityEngine.Random.Range(-0.3f, 0.3f), 0), Quaternion.identity);
                }
                else
                {
                    // Fallback direct coin credit if prefab not linked
                    PlayerStats p = FindObjectOfType<PlayerStats>();
                    if (p != null) p.AddGold(UnityEngine.Random.Range(3, 8));
                }
            }

            // Boss Chest Drop
            if (isBoss && bossChestPrefab != null)
            {
                Instantiate(bossChestPrefab, transform.position, Quaternion.identity);
            }
        }

        private IEnumerator DestroyAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (Core.ObjectPooler.Instance != null && Core.ObjectPooler.Instance.poolDictionary.ContainsKey("Enemy"))
            {
                gameObject.SetActive(false);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
