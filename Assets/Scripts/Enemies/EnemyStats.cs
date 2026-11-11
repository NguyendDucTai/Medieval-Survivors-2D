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
        private string cachedSpeciesKey = null;

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

            ApplyMapThemeTint();

            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = true;

            EnemyAI ai = GetComponent<EnemyAI>();
            if (ai != null)
            {
                ai.enabled = true;
                ai.SetDead(false);
            }

            Wave.WaveManager.RegisterEnemy(this);
        }

        private void OnDisable()
        {
            Wave.WaveManager.UnregisterEnemy(this);
        }

        public void ApplyMapThemeTint()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) return;

            // Prioritize live environment background as the single source of truth for the active map
            int mapIndex = 1;
            if (Environment.MapEnvironmentManager.Instance != null)
            {
                mapIndex = Environment.MapEnvironmentManager.Instance.currentMapIndex;
            }
            else if (UI.MainMenuManager.Instance != null)
            {
                mapIndex = UI.MainMenuManager.Instance.selectedMapIndex;
            }
            else if (Environment.Hazards.MapHazardManager.Instance != null)
            {
                mapIndex = Environment.Hazards.MapHazardManager.Instance.activeMapIndex;
            }
            else
            {
                mapIndex = PlayerPrefs.GetInt("SelectedMapIndex", 1);
            }

            // ONLY apply red magma/ember tints on Map 3 (Infernal Volcano)
            if (mapIndex == 3)
            {
                string species = GetSpeciesKey();
                if (isBoss) originalColor = new Color(1.0f, 0.40f, 0.12f, 1.0f); // Magma Golem
                else if (species == "Slime") originalColor = new Color(1.0f, 0.28f, 0.16f, 1.0f); // Magma Slime
                else if (species == "Bat") originalColor = new Color(0.92f, 0.15f, 0.22f, 1.0f); // Crimson Bat
                else if (species == "Skeleton") originalColor = new Color(1.0f, 0.58f, 0.45f, 1.0f); // Ash & Ember Skeleton
                else if (species == "Goblin") originalColor = new Color(0.95f, 0.35f, 0.20f, 1.0f); // Ember Goblin
                else originalColor = new Color(1.0f, 0.35f, 0.22f, 1.0f);
            }
            else
            {
                // All other maps (Emerald Meadow, Scorching Desert, Castle Ruins) retain 100% original sprite colors
                originalColor = Color.white;
            }

            spriteRenderer.color = originalColor;
        }

        public static void RefreshAllEnemyColors()
        {
#if UNITY_6000_0_OR_NEWER
            var allEnemies = UnityEngine.Object.FindObjectsByType<EnemyStats>(FindObjectsInactive.Include);
#else
            var allEnemies = UnityEngine.Object.FindObjectsOfType<EnemyStats>();
#endif
            for (int i = 0; i < allEnemies.Length; i++)
            {
                if (allEnemies[i] != null)
                {
                    allEnemies[i].ApplyMapThemeTint();
                }
            }
        }

        public string GetSpeciesKey()
        {
            if (isBoss) return "Boss";
            if (!string.IsNullOrEmpty(cachedSpeciesKey)) return cachedSpeciesKey;

            string combined = (enemyName ?? "") + "_" + gameObject.name;
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                combined += "_" + spriteRenderer.sprite.name;
            }

            if (combined.IndexOf("Bat", StringComparison.OrdinalIgnoreCase) >= 0) cachedSpeciesKey = "Bat";
            else if (combined.IndexOf("Skeleton", StringComparison.OrdinalIgnoreCase) >= 0) cachedSpeciesKey = "Skeleton";
            else if (combined.IndexOf("Goblin", StringComparison.OrdinalIgnoreCase) >= 0) cachedSpeciesKey = "Goblin";
            else if (combined.IndexOf("Plant", StringComparison.OrdinalIgnoreCase) >= 0 || combined.IndexOf("Flower", StringComparison.OrdinalIgnoreCase) >= 0) cachedSpeciesKey = "Plant";
            else cachedSpeciesKey = "Slime";

            return cachedSpeciesKey;
        }

        public void TakeDamage(float amount, bool isCrit = false)
        {
            if (isDying) return;

            // Apply UC-04 Grimoire Slayer Mastery Bonus (+2% / +5% / +10% damage against mastered species)
            if (Data.SaveManager.Instance != null)
            {
                float masteryMult = Data.SaveManager.Instance.GetSpeciesDamageMultiplier(GetSpeciesKey());
                amount *= masteryMult;
            }

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
            PlayerStats player = PlayerStats.Instance;
            if (player != null)
            {
                player.RecordEnemyKill(maxHealth);
            }

            // Record Bestiary Species Kill in Grimoire (UC-04)
            if (Data.SaveManager.Instance != null && !isBoss)
            {
                Data.SaveManager.Instance.RecordSpeciesKill(GetSpeciesKey());
            }

            // Check achievement progress (lifetime kills + current run kills)
            if (MetaProgression.AchievementManager.Instance != null)
            {
                int savedKills = (Data.SaveManager.Instance != null && Data.SaveManager.Instance.Data != null)
                    ? Data.SaveManager.Instance.Data.lifetimeKills
                    : 0;
                int totalKills = savedKills + (player != null ? player.inRunKills : 1);
                MetaProgression.AchievementManager.Instance.CheckProgress(MetaProgression.AchievementType.TotalKills, totalKills);
            }

            // Track Boss Kills for Boss Slayer Challenge & Archmage Unlock
            if (isBoss)
            {
                if (Data.SaveManager.Instance != null)
                {
                    Data.SaveManager.Instance.RecordBossKill();
                    if (!string.IsNullOrEmpty(enemyName) && enemyName.Contains("Golem") && Core.DifficultyManager.GetCurrentDifficulty() == Core.DifficultyMode.Hard)
                    {
                        Data.SaveManager.Instance.RecordHardGolemKill();
                    }
                }
                if (MetaProgression.AchievementManager.Instance != null && Data.SaveManager.Instance != null && Data.SaveManager.Instance.Data != null)
                {
                    MetaProgression.AchievementManager.Instance.CheckProgress(MetaProgression.AchievementType.BossDefeated, Data.SaveManager.Instance.Data.lifetimeBossKills);
                }
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
            // Exp Gem (with Vampire Survivors Gem Consolidation)
            if (expGemPrefab != null)
            {
                Collectibles.ExpGem.DropExp(transform.position, expValue, expGemPrefab);
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
                    PlayerStats p = PlayerStats.Instance;
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
