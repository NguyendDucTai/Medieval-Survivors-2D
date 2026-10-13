using UnityEngine;

namespace VampireSurvivors.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [Header("Game Timer")]
        public float survivalTime = 0f;
        public bool isGameOver = false;

        [Header("Enemy Spawning Config")]
        public GameObject enemyPrefab;
        public float spawnInterval = 2.0f;
        public float spawnRadius = 10.0f;
        public int maxEnemiesAlive = 15;
        private float spawnTimer;

        private Transform playerTransform;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
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
        }

        private void Update()
        {
            if (isGameOver) return;

            // PhĂ²ng trÆ°á»ng há»£p giĂ¡ trá»‹ trong Inspector bá»‹ chá»‰nh vá» 0 hoáº·c quĂ¡ nhá»
            if (spawnInterval < 1.0f) spawnInterval = 2.0f;

            survivalTime += Time.deltaTime;

            // Giá»›i háº¡n sá»‘ lÆ°á»£ng quĂ¡i cĂ¹ng lĂºc trĂªn mĂ n hĂ¬nh (tá»‘i Ä‘a 15 quĂ¡i Ä‘á»ƒ trĂ¡nh giáº­t lag)
            int currentEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
            if (currentEnemies >= maxEnemiesAlive) return;

            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;
                SpawnEnemyAroundPlayer();
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
            isGameOver = true;
            Debug.Log("Game Over! Total Survival Time: " + survivalTime.ToString("F1") + "s");
        }
    }
}
