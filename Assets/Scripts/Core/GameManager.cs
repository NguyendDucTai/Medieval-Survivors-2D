using UnityEngine;
using VampireSurvivors.Data;
using VampireSurvivors.MetaProgression;
using VampireSurvivors.Player;
using VampireSurvivors.UI;
using VampireSurvivors.Wave;

namespace VampireSurvivors.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [Header("Game Timer")]
        public float survivalTime = 0f;
        public bool isGameOver = false;

        [Header("Legacy Spawning Config (Fallback if WaveManager absent)")]
        public GameObject enemyPrefab;
        public float spawnInterval = 2.0f;
        public float spawnRadius = 10.0f;
        public int maxEnemiesAlive = 50;
        private float spawnTimer;

        private Transform playerTransform;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            // Ensure Singletons are loaded
            if (SaveManager.Instance == null) { }
            if (UpgradeManager.Instance == null) { }
            if (AchievementManager.Instance == null) { }
        }

        private void Start()
        {
            Time.timeScale = 1f;

            if (enemyPrefab == null)
            {
#if UNITY_EDITOR
                enemyPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Slime.prefab");
#endif
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }

            // Ensure WaveManager exists
            if (WaveManager.Instance == null && FindObjectOfType<WaveManager>() == null)
            {
                GameObject wmGO = new GameObject("WaveManager");
                wmGO.AddComponent<WaveManager>();
            }

            // Ensure DamageNumberPool exists
            if (DamageNumberPool.Instance == null && FindObjectOfType<DamageNumberPool>() == null)
            {
                GameObject poolGO = new GameObject("DamageNumberPool");
                poolGO.AddComponent<DamageNumberPool>();
            }

            // Ensure CameraJuice exists on Main Camera
            if (Camera.main != null && Camera.main.GetComponent<CameraJuice>() == null)
            {
                Camera.main.gameObject.AddComponent<CameraJuice>();
            }

            // Ensure AudioManager exists
            if (Audio.AudioManager.Instance == null && FindObjectOfType<Audio.AudioManager>() == null)
            {
                GameObject audioGO = new GameObject("AudioManager");
                audioGO.AddComponent<Audio.AudioManager>();
            }

            // Ensure MainMenuManager exists
            if (MainMenuManager.Instance == null && FindObjectOfType<MainMenuManager>() == null)
            {
                GameObject menuGO = new GameObject("MainMenuManager");
                menuGO.AddComponent<MainMenuManager>();
            }

            // Ensure UIManager exists
            if (UIManager.Instance == null && FindObjectOfType<UIManager>() == null)
            {
                GameObject uiGO = new GameObject("UIManager");
                uiGO.AddComponent<UIManager>();
            }
        }

        private void Update()
        {
            if (isGameOver) return;
            if (MainMenuManager.Instance != null && MainMenuManager.Instance.isMenuOpen) return;

            survivalTime += Time.deltaTime;

            // Developer Testing Hotkeys:
            // Press U: Instantly level up and test LevelUp cards + recipe hints
            if (Input.GetKeyDown(KeyCode.U))
            {
                var stats = playerTransform != null ? playerTransform.GetComponent<PlayerStats>() : FindObjectOfType<PlayerStats>();
                if (stats != null)
                {
                    Debug.Log("<color=yellow>[DEV TEST]</color> Level Up triggered via Key [U]!");
                    stats.AddExperience(stats.expToNextLevel);
                }
            }

            // Press B: Instantly spawn King Slime / Mini-Boss
            if (Input.GetKeyDown(KeyCode.B))
            {
                if (WaveManager.Instance != null)
                {
                    Debug.Log("<color=yellow>[DEV TEST]</color> Mini-Boss spawned via Key [B]!");
                    WaveManager.Instance.SpawnTestMiniBoss();
                }
            }

            // Press C: Instantly spawn Royal Boss Chest directly at player's feet
            if (Input.GetKeyDown(KeyCode.C))
            {
                if (WaveManager.Instance != null)
                {
                    Debug.Log("<color=yellow>[DEV TEST]</color> Royal Boss Chest spawned via Key [C]!");
                    WaveManager.Instance.SpawnTestChest();
                }
            }

            // Press T: Instantly trigger Triangle Bat Swarm
            if (Input.GetKeyDown(KeyCode.T))
            {
                if (WaveManager.Instance != null)
                {
                    Debug.Log("<color=yellow>[DEV TEST]</color> Triangle Bat Swarm triggered via Key [T]!");
                    WaveManager.Instance.SpawnTestTriangleBatSwarm();
                }
            }

            // Only run legacy spawner if WaveManager is NOT handling spawning
            if (WaveManager.Instance == null)
            {
                if (spawnInterval < 1.0f) spawnInterval = 2.0f;

                int currentEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
                if (currentEnemies >= maxEnemiesAlive) return;

                spawnTimer += Time.deltaTime;
                if (spawnTimer >= spawnInterval)
                {
                    spawnTimer = 0f;
                    SpawnEnemyAroundPlayer();
                }
            }
        }

        private void SpawnEnemyAroundPlayer()
        {
            if (enemyPrefab == null) return;

            Vector3 spawnPos = Vector3.zero;
            if (playerTransform != null)
            {
                Vector2 randomCircle = Random.insideUnitCircle.normalized * spawnRadius;
                spawnPos = playerTransform.position + new Vector3(randomCircle.x, randomCircle.y, 0f);
            }
            else
            {
                Vector2 randomCircle = Random.insideUnitCircle.normalized * spawnRadius;
                spawnPos = new Vector3(randomCircle.x, randomCircle.y, 0f);
            }

            if (ObjectPooler.Instance != null && ObjectPooler.Instance.poolDictionary.ContainsKey("Enemy"))
            {
                ObjectPooler.Instance.SpawnFromPool("Enemy", spawnPos, Quaternion.identity);
            }
            else
            {
                Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            }
        }

        public void GameOver()
        {
            if (isGameOver) return;
            isGameOver = true;
            Debug.Log("Game Over! Total Survival Time: " + survivalTime.ToString("F1") + "s");

            if (RunSummaryUI.Instance != null)
            {
                RunSummaryUI.Instance.ShowSummary(survivalTime >= 1200f);
            }
        }
    }
}
