using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VampireSurvivors.Enemies;
using VampireSurvivors.Player;

namespace VampireSurvivors.Wave
{
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance;

        [Header("Enemy Prefab Catalog")]
        public GameObject slimePrefab;
        public GameObject batPrefab;
        public GameObject skeletonPrefab;
        public GameObject goblinPrefab;
        public GameObject ghostPrefab;
        public GameObject bossGolemPrefab;
        public GameObject bossChestPrefab;
        public GameObject goldCoinPrefab;

        [Header("Wave Tuning")]
        public float spawnRadius = 11f;
        public int maxEnemiesAlive = 60;
        public string currentWaveTitle = "⚔️ ĐỢT 1: KHỞI NGUYÊN (VƯƠNG QUỐC SLIME)";

        private Transform playerTransform;
        private float gameTimer = 0f;
        private float spawnTicker = 0f;

        private bool wave1Triggered = false;
        private bool wave2Triggered = false;
        private bool boss1Spawned = false;
        private bool wave3Triggered = false;
        private bool boss2Spawned = false;
        private bool swarm2Triggered = false;
        private bool boss3Spawned = false;
        private bool isVictory = false;

        public event Action<string> OnWaveBanner;
        public event Action OnVictory;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            LoadPrefabsIfNull();
        }

        private void Start()
        {
#if UNITY_6000_0_OR_NEWER
            PlayerController p = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
#else
            PlayerController p = UnityEngine.Object.FindObjectOfType<PlayerController>();
#endif
            if (p != null) playerTransform = p.transform;
        }

        public void LoadPrefabsIfNull()
        {
#if UNITY_EDITOR
            if (slimePrefab == null) slimePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Slime.prefab");
            if (batPrefab == null) batPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Bat.prefab");
            if (skeletonPrefab == null) skeletonPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Skeleton.prefab");
            if (goblinPrefab == null) goblinPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Goblin.prefab");
            if (ghostPrefab == null) ghostPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Ghost.prefab");
            if (bossGolemPrefab == null) bossGolemPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_BossGolem.prefab");
            if (bossChestPrefab == null) bossChestPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BossChest.prefab");
            if (goldCoinPrefab == null) goldCoinPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/GoldCoin.prefab");
#endif
        }

        private void Update()
        {
            if (isVictory) return;
            if (Core.GameManager.Instance != null && Core.GameManager.Instance.isGameOver) return;
            if (UI.MainMenuManager.Instance != null && UI.MainMenuManager.Instance.isMenuOpen) return;

            if (playerTransform == null)
            {
#if UNITY_6000_0_OR_NEWER
                PlayerController p = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
#else
                PlayerController p = UnityEngine.Object.FindObjectOfType<PlayerController>();
#endif
                if (p != null) playerTransform = p.transform;
            }

            gameTimer += Time.deltaTime;

            // Check for Milestones and Events
            CheckWaveEvents();

            // Dynamic Spawn Rate: starts at 1.4s per spawn, ramps down to 0.4s as game progresses
            float currentSpawnInterval = Mathf.Clamp(1.4f - (gameTimer / 400f), 0.35f, 1.4f);
            spawnTicker += Time.deltaTime;

            if (spawnTicker >= currentSpawnInterval)
            {
                spawnTicker = 0f;
                SpawnRegularEnemy();
            }
        }

        private void CheckWaveEvents()
        {
            // Second 1.0: Wave 1 Announcement & Initial Slime Swarm
            if (gameTimer >= 1.0f && !wave1Triggered)
            {
                wave1Triggered = true;
                currentWaveTitle = "⚔️ ĐỢT 1: KHỞI NGUYÊN (VƯƠNG QUỐC SLIME)";
                OnWaveBanner?.Invoke("⚔️ ĐỢT 1: VƯƠNG QUỐC SLIME BẮT ĐẦU!");
                StartCoroutine(TriggerSwarm(slimePrefab, 4, null));
            }

            // Second 60.0 (1:00): Wave 2 - Bat Swarm in Triangle Wedge Formation
            if (gameTimer >= 60f && !wave2Triggered)
            {
                wave2Triggered = true;
                currentWaveTitle = "🦇 ĐỢT 2: BẦY DƠI TAM GIÁC ĐỘT KÍCH";
                StartCoroutine(TriggerTriangleSwarm(batPrefab, 5, 1.35f, "🦇 CẢNH BÁO: BẦY DƠI ĐỘT KÍCH (ĐỘI HÌNH TAM GIÁC)!"));
            }

            // Second 90.0 (1:30): Mini-Boss King Slime (Early Boss encounter dropping Boss Chest Roulette!)
            if (gameTimer >= 90f && !boss1Spawned)
            {
                boss1Spawned = true;
                currentWaveTitle = "👑 TRÙM: VUA NHẦY KING SLIME";
                SpawnMiniBoss(slimePrefab, "King Slime (Vua Nhầy)", 350f, 1.8f, 2.2f);
            }

            // Second 180.0 (3:00): Wave 3 - Skeleton Army
            if (gameTimer >= 180f && !wave3Triggered)
            {
                wave3Triggered = true;
                currentWaveTitle = "💀 ĐỢT 3: QUÂN ĐOÀN BỘ XƯƠNG";
                OnWaveBanner?.Invoke("💀 ĐỢT 3: QUÂN ĐOÀN BỘ XƯƠNG TIẾN CÔNG!");
            }

            // Second 300.0 (5:00): Mini-Boss Skeleton Warlord
            if (gameTimer >= 300f && !boss2Spawned)
            {
                boss2Spawned = true;
                currentWaveTitle = "⚠️ TRÙM: ĐẠI TƯỚNG BỘ XƯƠNG";
                SpawnMiniBoss(skeletonPrefab, "Skeleton Warlord (Đại Tướng Xương)", 850f, 2.0f, 2.0f);
            }

            // Second 450.0 (7:30): Goblin Rush
            if (gameTimer >= 450f && !swarm2Triggered)
            {
                swarm2Triggered = true;
                currentWaveTitle = "👺 ĐỢT 4: QUÂN ĐOÀN YÊU TINH";
                StartCoroutine(TriggerSwarm(goblinPrefab, 16, "CẢNH BÁO: QUÂN ĐOÀN YÊU TINH TIẾN CÔNG!"));
            }

            // Second 600.0 (10:00): World Boss Colossal Stone Golem
            if (gameTimer >= 600f && !boss3Spawned)
            {
                boss3Spawned = true;
                currentWaveTitle = "🗿 SIÊU TRÙM: THẠCH QUÁI GOLEM";
                SpawnWorldBoss();
            }

            // Minute 20.0: VICTORY
            if (gameTimer >= 1200f && !isVictory)
            {
                isVictory = true;
                TriggerVictory();
            }
        }

        private void SpawnRegularEnemy()
        {
            int currentEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
            if (currentEnemies >= maxEnemiesAlive) return;

            // Group spawn: 2 to 3 enemies per burst for exciting cluster action
            int burstCount = UnityEngine.Random.Range(2, 4);
            for (int i = 0; i < burstCount; i++)
            {
                if (currentEnemies + i >= maxEnemiesAlive) break;

                GameObject selectedPrefab = PickEnemyForCurrentTime();
                if (selectedPrefab == null) continue;

                Vector3 spawnPos = GetRandomSpawnPosition();
                GameObject enemyGO = Instantiate(selectedPrefab, spawnPos, Quaternion.identity);

                // Scale stats by time (+4% HP per minute)
                float minutesPassed = gameTimer / 60f;
                EnemyStats stats = enemyGO.GetComponent<EnemyStats>();
                if (stats != null)
                {
                    stats.maxHealth *= (1f + (minutesPassed * 0.04f));
                    stats.currentHealth = stats.maxHealth;
                    stats.attackDamage *= (1f + (minutesPassed * 0.03f));
                    if (goldCoinPrefab != null) stats.goldCoinPrefab = goldCoinPrefab;
                }
            }
        }

        private GameObject PickEnemyForCurrentTime()
        {
            float minutes = gameTimer / 60f;
            float r = UnityEngine.Random.value;

            if (minutes < 1.5f)
            {
                // Mostly slimes, occasional bat
                return (r < 0.75f) ? slimePrefab : batPrefab;
            }
            else if (minutes < 3f)
            {
                // Bats, Skeletons, and Slimes
                if (r < 0.35f) return slimePrefab;
                if (r < 0.70f) return batPrefab;
                return skeletonPrefab;
            }
            else if (minutes < 6f)
            {
                // Skeletons, Goblins, Ghosts, and Bats
                if (r < 0.30f) return skeletonPrefab;
                if (r < 0.55f) return goblinPrefab;
                if (r < 0.75f && ghostPrefab != null) return ghostPrefab;
                if (r < 0.90f) return batPrefab;
                return slimePrefab;
            }
            else
            {
                // High tier mix
                if (r < 0.30f) return goblinPrefab;
                if (r < 0.55f) return skeletonPrefab;
                if (r < 0.75f && ghostPrefab != null) return ghostPrefab;
                if (r < 0.90f) return batPrefab;
                return slimePrefab;
            }
        }

        private IEnumerator TriggerSwarm(GameObject prefab, int count, string bannerMessage)
        {
            if (!string.IsNullOrEmpty(bannerMessage))
            {
                OnWaveBanner?.Invoke(bannerMessage);
                Debug.Log($"<color=red>[WAVE EVENT]</color> {bannerMessage}");
            }

            for (int i = 0; i < count; i++)
            {
                if (prefab != null)
                {
                    Vector3 pos = GetRandomSpawnPosition();
                    GameObject go = Instantiate(prefab, pos, Quaternion.identity);
                    EnemyStats stats = go.GetComponent<EnemyStats>();
                    if (stats != null)
                    {
                        stats.moveSpeed *= 1.25f; // Fast swarm
                    }
                }
                yield return new WaitForSeconds(0.12f);
            }
        }

        public IEnumerator TriggerTriangleSwarm(GameObject prefab, int rows, float spacing, string bannerMessage)
        {
            if (!string.IsNullOrEmpty(bannerMessage))
            {
                OnWaveBanner?.Invoke(bannerMessage);
                Debug.Log($"<color=red>[TRIANGLE SWARM]</color> {bannerMessage}");
            }

            if (prefab == null) yield break;

            Vector3 playerPos = playerTransform != null ? playerTransform.position : Vector3.zero;
            // Pick a random direction from which the triangle flock emerges
            Vector2 spawnDir = UnityEngine.Random.insideUnitCircle.normalized;
            if (spawnDir.sqrMagnitude < 0.1f) spawnDir = Vector2.right;

            float spawnDistance = spawnRadius + 3f;
            Vector2 apexPos = (Vector2)playerPos + (spawnDir * spawnDistance);

            // Forward vector is directed towards the player
            Vector2 forward = (-spawnDir).normalized;
            // Right vector is perpendicular to forward
            Vector2 right = new Vector2(-forward.y, forward.x);

            // Spawn rows forming a V / Triangle wedge
            for (int r = 0; r < rows; r++)
            {
                int countInRow = r + 1; // Row 0 has 1 (apex), Row 1 has 2, Row 2 has 3, etc.
                float rowDepth = r * spacing * 1.35f;
                Vector2 rowCenter = apexPos - (forward * rowDepth);

                float halfWidth = (countInRow - 1) * spacing * 0.5f;

                for (int c = 0; c < countInRow; c++)
                {
                    float lateralOffset = -halfWidth + (c * spacing);
                    Vector2 batPos = rowCenter + (right * lateralOffset);

                    GameObject batGO = Instantiate(prefab, new Vector3(batPos.x, batPos.y, 0f), Quaternion.identity);
                    EnemyStats stats = batGO.GetComponent<EnemyStats>();
                    if (stats != null)
                    {
                        // Faster bat speed to actively chase and challenge player
                        stats.moveSpeed = Mathf.Max(stats.moveSpeed, 4.2f);
                        stats.maxHealth = Mathf.Max(stats.maxHealth, 35f);
                        stats.currentHealth = stats.maxHealth;
                    }
                }

                // Slight delay between rows so the wedge flies forward in clear formation
                yield return new WaitForSeconds(0.08f);
            }
        }

        public void SpawnTestTriangleBatSwarm()
        {
            LoadPrefabsIfNull();
            if (batPrefab != null)
            {
                StartCoroutine(TriggerTriangleSwarm(batPrefab, 5, 1.35f, "🦇 THỬ NGHIỆM: ĐÀN DƠI ĐỘI HÌNH TAM GIÁC!"));
            }
        }

        public void SpawnMiniBoss(GameObject basePrefab, string title, float hp, float speed, float scale)
        {
            if (basePrefab == null) basePrefab = slimePrefab;
            if (basePrefab == null) return;

            Vector3 pos = GetRandomSpawnPosition();
            GameObject bossGO = Instantiate(basePrefab, pos, Quaternion.identity);
            bossGO.transform.localScale = Vector3.one * scale;

            EnemyStats stats = bossGO.GetComponent<EnemyStats>();
            if (stats != null)
            {
                stats.enemyName = title;
                stats.maxHealth = hp;
                stats.currentHealth = hp;
                stats.moveSpeed = speed;
                stats.attackDamage = 25f;
                stats.isBoss = true;
                stats.bossChestPrefab = bossChestPrefab;
            }

            BossController bc = bossGO.AddComponent<BossController>();
            bc.bossTitle = title;

            OnWaveBanner?.Invoke($"👑 TRÙM XUẤT HIỆN: {title.ToUpper()}!");
            Debug.Log($"<color=orange>[BOSS SPAWNED]</color> {title} with {hp} HP!");
        }

        public void SpawnWorldBoss()
        {
            GameObject prefab = bossGolemPrefab != null ? bossGolemPrefab : skeletonPrefab;
            Vector3 pos = GetRandomSpawnPosition();
            GameObject bossGO = Instantiate(prefab, pos, Quaternion.identity);
            bossGO.transform.localScale = Vector3.one * 2.5f;

            EnemyStats stats = bossGO.GetComponent<EnemyStats>();
            if (stats != null)
            {
                stats.enemyName = "Colossal Stone Golem";
                stats.maxHealth = 2500f;
                stats.currentHealth = 2500f;
                stats.moveSpeed = 1.6f;
                stats.attackDamage = 40f;
                stats.isBoss = true;
                stats.bossChestPrefab = bossChestPrefab;
            }

            BossController bc = bossGO.AddComponent<BossController>();
            bc.bossTitle = "Colossal Stone Golem (Đại Trùm Thần Thạch)";

            OnWaveBanner?.Invoke("🗿 ĐẠI TRÙM THẾ GIỚI: GOLEM KHỔNG LỒ XUẤT HIỆN!");
            Debug.Log("<color=red>★★ [WORLD BOSS SPAWNED] ★★</color> Colossal Stone Golem (2500 HP)!");
        }

        public void SpawnTestMiniBoss()
        {
            LoadPrefabsIfNull();
            GameObject prefab = slimePrefab != null ? slimePrefab : bossGolemPrefab;
            if (prefab != null)
            {
                Vector3 pos = playerTransform != null ? playerTransform.position + Vector3.right * 3.5f : Vector3.right * 3.5f;
                GameObject bossGO = Instantiate(prefab, pos, Quaternion.identity);
                bossGO.transform.localScale = Vector3.one * 2.2f;

                EnemyStats stats = bossGO.GetComponent<EnemyStats>();
                if (stats != null)
                {
                    stats.enemyName = "King Slime (Trùm Thử Nghiệm)";
                    stats.maxHealth = 120f; // Quick to defeat in test
                    stats.currentHealth = 120f;
                    stats.moveSpeed = 1.8f;
                    stats.attackDamage = 12f;
                    stats.isBoss = true;
                    stats.bossChestPrefab = bossChestPrefab;
                }
                OnWaveBanner?.Invoke("👑 TRÙM THỬ NGHIỆM: VUA NHẦY KING SLIME!");
            }
        }

        public void SpawnTestChest()
        {
            LoadPrefabsIfNull();
            Vector3 pos = playerTransform != null ? playerTransform.position + Vector3.right * 2f : Vector3.right * 2f;
            if (bossChestPrefab != null)
            {
                Instantiate(bossChestPrefab, pos, Quaternion.identity);
            }
            else
            {
                GameObject chestGO = new GameObject("BossChest");
                chestGO.transform.position = pos;
                var col = chestGO.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.8f;
                chestGO.AddComponent<Collectibles.BossChest>();
            }
            OnWaveBanner?.Invoke("🎁 RƯƠNG HOÀNG GIA ĐÃ XUẤT HIỆN GẦN BẠN!");
        }

        private void TriggerVictory()
        {
            Debug.Log("<color=yellow>★★★ [VICTORY ACHIEVED!] ★★★</color> Survived 20 Minutes!");
            OnVictory?.Invoke();

            if (MetaProgression.AchievementManager.Instance != null)
            {
                MetaProgression.AchievementManager.Instance.CheckProgress(MetaProgression.AchievementType.SurvivalTime, 1200f);
            }

            // Triple gold reward for victory
#if UNITY_6000_0_OR_NEWER
            PlayerStats player = UnityEngine.Object.FindAnyObjectByType<PlayerStats>();
#else
            PlayerStats player = UnityEngine.Object.FindObjectOfType<PlayerStats>();
#endif
            if (player != null)
            {
                player.AddGold(500);
            }

            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.GameOver();
            }
        }

        private Vector3 GetRandomSpawnPosition()
        {
            Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized * spawnRadius;
            Vector3 center = playerTransform != null ? playerTransform.position : Vector3.zero;
            return center + new Vector3(randomDir.x, randomDir.y, 0f);
        }
    }
}
