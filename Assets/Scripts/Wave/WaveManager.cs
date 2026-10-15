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

        private Transform playerTransform;
        private float gameTimer = 0f;
        private float spawnTicker = 0f;

        private bool boss1Spawned = false;
        private bool boss2Spawned = false;
        private bool boss3Spawned = false;
        private bool swarm1Triggered = false;
        private bool swarmGhostTriggered = false;
        private bool swarm2Triggered = false;
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
            PlayerController p = FindObjectOfType<PlayerController>();
            if (p != null) playerTransform = p.transform;
        }

        private void LoadPrefabsIfNull()
        {
#if UNITY_EDITOR
            if (slimePrefab == null) slimePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Slime.prefab");
            if (batPrefab == null) batPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Bat.prefab");
            if (skeletonPrefab == null) skeletonPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Skeleton.prefab");
            if (goblinPrefab == null) goblinPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Goblin.prefab");
            if (ghostPrefab == null) ghostPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Ghost.prefab");
            if (bossGolemPrefab == null) bossGolemPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_BossGolem.prefab");
#endif
        }

        private void Update()
        {
            if (isVictory) return;
            if (Core.GameManager.Instance != null && Core.GameManager.Instance.isGameOver) return;
            if (UI.MainMenuManager.Instance != null && UI.MainMenuManager.Instance.isMenuOpen) return;

            gameTimer += Time.deltaTime;

            // Check for Milestones and Events
            CheckWaveEvents();

            // Dynamic Spawn Rate: starts at 1.5s per spawn, ramps down to 0.4s per spawn as time progresses
            float currentSpawnInterval = Mathf.Clamp(1.5f - (gameTimer / 450f), 0.35f, 1.5f);
            spawnTicker += Time.deltaTime;

            if (spawnTicker >= currentSpawnInterval)
            {
                spawnTicker = 0f;
                SpawnRegularEnemy();
            }
        }

        private void CheckWaveEvents()
        {
            // Minute 2.5: Bat Swarm
            if (gameTimer >= 150f && !swarm1Triggered)
            {
                swarm1Triggered = true;
                StartCoroutine(TriggerSwarm(batPrefab, 20, "CẢNH BÁO: BẦY DƠI ĐỘT KÍCH!"));
            }

            // Minute 3.5: Ancient Ghost Incursion
            if (gameTimer >= 210f && !swarmGhostTriggered && ghostPrefab != null)
            {
                swarmGhostTriggered = true;
                StartCoroutine(TriggerSwarm(ghostPrefab, 14, "CẢNH BÁO: HỒN MA CỔ XƯA XUẤT HIỆN!"));
            }

            // Minute 5.0: Mini-Boss King Slime
            if (gameTimer >= 300f && !boss1Spawned)
            {
                boss1Spawned = true;
                SpawnMiniBoss(slimePrefab, "King Slime (Vua Nhầy)", 400f, 1.8f, 2.2f);
            }

            // Minute 7.5: Goblin Rush
            if (gameTimer >= 450f && !swarm2Triggered)
            {
                swarm2Triggered = true;
                StartCoroutine(TriggerSwarm(goblinPrefab, 16, "CẢNH BÁO: QUÂN ĐOÀN YÊU TINH TIẾN CÔNG!"));
            }

            // Minute 10.0: Mini-Boss Skeleton Warlord
            if (gameTimer >= 600f && !boss2Spawned)
            {
                boss2Spawned = true;
                SpawnMiniBoss(skeletonPrefab, "Skeleton Warlord (Đại Tướng Bộ Xương)", 850f, 2.0f, 2.0f);
            }

            // Minute 15.0: World Boss Colossal Stone Golem
            if (gameTimer >= 900f && !boss3Spawned)
            {
                boss3Spawned = true;
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

            GameObject selectedPrefab = PickEnemyForCurrentTime();
            if (selectedPrefab == null) return;

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

        private GameObject PickEnemyForCurrentTime()
        {
            float minutes = gameTimer / 60f;
            float r = UnityEngine.Random.value;

            if (minutes < 2f)
            {
                // Mostly slimes, occasional bat
                return (r < 0.75f) ? slimePrefab : batPrefab;
            }
            else if (minutes < 5f)
            {
                // Bats, Skeletons, and Ghosts
                if (r < 0.35f) return slimePrefab;
                if (r < 0.65f) return batPrefab;
                if (r < 0.85f && ghostPrefab != null) return ghostPrefab;
                return skeletonPrefab;
            }
            else if (minutes < 10f)
            {
                // Skeletons, Goblins, Ghosts, and Bats
                if (r < 0.25f) return skeletonPrefab;
                if (r < 0.50f) return goblinPrefab;
                if (r < 0.70f && ghostPrefab != null) return ghostPrefab;
                if (r < 0.85f) return batPrefab;
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
            OnWaveBanner?.Invoke(bannerMessage);
            Debug.Log($"<color=red>[WAVE EVENT]</color> {bannerMessage}");

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

        private void SpawnMiniBoss(GameObject basePrefab, string title, float hp, float speed, float scale)
        {
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

            OnWaveBanner?.Invoke($"TRÙM XUẤT HIỆN: {title.ToUpper()}!");
            Debug.Log($"<color=orange>[BOSS SPAWNED]</color> {title} with {hp} HP!");
        }

        private void SpawnWorldBoss()
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

            OnWaveBanner?.Invoke("ĐẠI TRÙM THẾ GIỚI: GOLEM KHỔNG LỒ XUẤT HIỆN!");
            Debug.Log("<color=red>★★ [WORLD BOSS SPAWNED] ★★</color> Colossal Stone Golem (2500 HP)!");
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
            PlayerStats player = FindObjectOfType<PlayerStats>();
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
