using System.Collections.Generic;
using UnityEngine;
using VampireSurvivors.Audio;
using VampireSurvivors.Collectibles;
using VampireSurvivors.Enemies;
using VampireSurvivors.Player;
using VampireSurvivors.UI;
using VampireSurvivors.Wave;

namespace VampireSurvivors.Environment.Hazards
{
    /// <summary>
    /// Map 2 (Castle Ruins) Hazard/Feature:
    /// Spawns an Arcane Summoning Circle on the ground. Standing inside for 10 seconds
    /// summons an Elite Skeleton/Goblin Warlord that drops a massive shower of Gold & ExpGems.
    /// </summary>
    public class SummoningCircleHazard : MonoBehaviour
    {
        public float spawnInterval = 150f;
        public float firstSpawnDelay = 25f;
        public float circleLifetime = 45f;
        public float requiredChannelTime = 10f;
        public float circleRadius = 2.15f;

        private float timer = 0f;
        private bool firstSpawned = false;
        private SummoningCircleInstance activeCircle;

        private void OnEnable()
        {
            timer = 0f;
            firstSpawned = false;
        }

        private void OnDisable()
        {
            ClearAll();
        }

        public void TickHazard(float deltaTime, Transform playerTransform)
        {
            if (playerTransform == null) return;

            if (activeCircle != null) return; // Max 1 circle at a time

            timer += deltaTime;
            float hazMult = Core.DifficultyManager.GetHazardMultiplier();
            float effectiveFirstDelay = firstSpawnDelay / hazMult;
            float effectiveInterval = spawnInterval / hazMult;

            if (!firstSpawned && timer >= effectiveFirstDelay)
            {
                firstSpawned = true;
                timer = 0f;
                SpawnCircle(playerTransform.position);
            }
            else if (firstSpawned && timer >= effectiveInterval)
            {
                timer = 0f;
                SpawnCircle(playerTransform.position);
            }
        }

        public void ForceTrigger(Transform playerTransform)
        {
            if (playerTransform == null) return;
            if (activeCircle != null)
            {
                Destroy(activeCircle.gameObject);
                activeCircle = null;
            }
            SpawnCircle(playerTransform.position);
        }

        public void ClearAll()
        {
            if (activeCircle != null)
            {
                Destroy(activeCircle.gameObject);
                activeCircle = null;
            }
        }

        private void SpawnCircle(Vector3 playerPos)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float dist = Random.Range(5.5f, 8.5f);
            Vector3 spawnPos = playerPos + new Vector3(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist, 0f);

            GameObject go = new GameObject("ArcaneSummoningCircle");
            go.transform.position = spawnPos;
            go.transform.localScale = Vector3.one * 2.15f; // 32x32 at PPU 16 = 2m * 2.15 = ~4.3m diameter (radius 2.15m)

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = HazardSpriteLoader.GetSummonRuneSprite();
            sr.sortingOrder = -2; // On ground beneath player & enemies

            activeCircle = go.AddComponent<SummoningCircleInstance>();
            activeCircle.Initialize(circleRadius, requiredChannelTime, circleLifetime);

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowWaveBanner("🔮 CỔ NGỮ TRIỆU HỒI XUẤT HIỆN! ĐỨNG TRONG VÒNG 10 GIÂY ĐỂ TRIỆU HỒI TINH ANH!");
            }
            Debug.Log($"<color=#CC88FF>[CASTLE RUINS HAZARD]</color> Arcane Summoning Circle spawned at {spawnPos}!");
        }
    }

    public class SummoningCircleInstance : MonoBehaviour
    {
        private float radius = 2.15f;
        private float requiredTime = 10f;
        private float maxLifetime = 45f;

        private float channelProgress = 0f;
        private float aliveTimer = 0f;
        private SpriteRenderer sr;
        private TextMesh labelMesh;
        private bool completed = false;

        public void Initialize(float rad, float reqTime, float life)
        {
            radius = rad;
            requiredTime = reqTime;
            maxLifetime = life;
            sr = GetComponent<SpriteRenderer>();

            // Create floating world-space progress indicator above the circle
            GameObject labelGO = new GameObject("RitualProgressLabel");
            labelGO.transform.SetParent(transform, false);
            labelGO.transform.localPosition = new Vector3(0f, 1.15f, 0f);
            labelGO.transform.localScale = Vector3.one * 0.065f;

            labelMesh = labelGO.AddComponent<TextMesh>();
            labelMesh.anchor = TextAnchor.MiddleCenter;
            labelMesh.alignment = TextAlignment.Center;
            labelMesh.fontSize = 36;
            labelMesh.fontStyle = FontStyle.Bold;
            labelMesh.color = new Color(0.85f, 0.70f, 1f, 1f);
            labelMesh.text = "CỔ NGỮ: 0%";

            MeshRenderer mr = labelGO.GetComponent<MeshRenderer>();
            if (mr != null) mr.sortingOrder = 12;
        }

        private void Update()
        {
            if (completed) return;

            aliveTimer += Time.deltaTime;
            if (aliveTimer >= maxLifetime && channelProgress <= 0.1f)
            {
                Destroy(gameObject);
                return;
            }

            // Rotate rune circle mystically
            transform.Rotate(0f, 0f, 28f * Time.deltaTime);

            Transform pTransform = PlayerController.Instance != null ? PlayerController.Instance.transform : null;
            bool playerInside = false;

            if (pTransform != null)
            {
                float dist = Vector2.Distance(transform.position, pTransform.position);
                playerInside = (dist <= radius);
            }

            if (playerInside)
            {
                channelProgress += Time.deltaTime;
            }
            else if (channelProgress > 0f)
            {
                // Decay slowly (0.45x speed) so stepping out briefly to dodge doesn't reset progress
                channelProgress = Mathf.Max(0f, channelProgress - Time.deltaTime * 0.45f);
            }

            float pct = Mathf.Clamp01(channelProgress / requiredTime);

            if (sr != null)
            {
                float pulse = 0.72f + Mathf.Sin(Time.time * (playerInside ? 8f : 3f)) * 0.25f;
                Color baseCol = playerInside
                    ? Color.Lerp(new Color(0.75f, 0.35f, 1f), new Color(0.35f, 1f, 0.85f), pct)
                    : new Color(0.65f, 0.30f, 0.95f);
                baseCol.a = pulse;
                sr.color = baseCol;
            }

            if (labelMesh != null)
            {
                labelMesh.transform.rotation = Quaternion.identity; // Keep text upright despite parent rotation
                int pctInt = Mathf.RoundToInt(pct * 100f);
                labelMesh.text = playerInside
                    ? $"⚡ ĐANG TRIỆU HỒI: {pctInt}%"
                    : $"🔮 CỔ NGỮ ({pctInt}%)";
                labelMesh.color = playerInside ? new Color(0.4f, 1f, 0.75f) : new Color(0.88f, 0.72f, 1f);
            }

            if (channelProgress >= requiredTime)
            {
                CompleteRitual();
            }
        }

        private void CompleteRitual()
        {
            if (completed) return;
            completed = true;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfxByName("levelup");
            }

            var camJuice = Camera.main != null ? Camera.main.GetComponent<Core.CameraJuice>() : null;
            if (camJuice != null) camJuice.Shake(0.18f, 0.22f);

            SpawnEliteMonster(transform.position);
            Destroy(gameObject);
        }

        private void SpawnEliteMonster(Vector3 spawnPos)
        {
            WaveManager wm = WaveManager.Instance;
            if (wm != null) wm.LoadPrefabsIfNull();

            bool spawnGoblin = Random.value > 0.5f;
            GameObject prefab = null;
            string eliteName = spawnGoblin ? "Tinh Anh Yêu Tinh Cuồng Nộ (Elite Goblin)" : "Tinh Anh Hài Cốt Cổ Đại (Elite Skeleton)";

            if (wm != null)
            {
                prefab = spawnGoblin
                    ? (wm.goblinPrefab != null ? wm.goblinPrefab : wm.skeletonPrefab)
                    : (wm.skeletonPrefab != null ? wm.skeletonPrefab : wm.goblinPrefab);
                if (prefab == null) prefab = wm.slimePrefab;
            }

            if (prefab != null)
            {
                GameObject eliteGO = Instantiate(prefab, spawnPos, Quaternion.identity);
                eliteGO.transform.localScale = Vector3.one * 1.65f;

                SpriteRenderer esr = eliteGO.GetComponent<SpriteRenderer>();
                if (esr != null) esr.color = new Color(1f, 0.75f, 0.35f, 1f); // Golden-purple elite aura

                EnemyStats stats = eliteGO.GetComponent<EnemyStats>();
                if (stats != null)
                {
                    float hpMult = Core.DifficultyManager.GetHealthMultiplier();
                    float speedMult = Core.DifficultyManager.GetSpeedMultiplier();
                    float dmgMult = Core.DifficultyManager.GetDamageMultiplier();
                    float hazMult = Core.DifficultyManager.GetHazardMultiplier();

                    stats.enemyName = eliteName;
                    stats.maxHealth = Mathf.Max(320f, stats.maxHealth * 6f) * hpMult;
                    stats.currentHealth = stats.maxHealth;
                    stats.moveSpeed *= 1.15f * speedMult;
                    stats.attackDamage = Mathf.Max(18f, stats.attackDamage * 1.4f) * dmgMult;
                    stats.expValue = Mathf.Max(120f, stats.expValue * 8f);
                    stats.goldDropChance = 1.0f;

                    EliteTreasureDropper dropper = eliteGO.AddComponent<EliteTreasureDropper>();
                    dropper.goldCoinsToDrop = Mathf.RoundToInt(Random.Range(6, 9) * hazMult);
                    dropper.bonusExpGems = Mathf.RoundToInt(5 * hazMult);
                    dropper.expPerBonusGem = 35f;
                    dropper.goldCoinPrefab = stats.goldCoinPrefab != null ? stats.goldCoinPrefab : (wm != null ? wm.goldCoinPrefab : null);
                    dropper.expGemPrefab = stats.expGemPrefab;
                }
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowWaveBanner($"👑 CỔ NGỮ HOÀN TẤT: {eliteName.ToUpper()} XUẤT HIỆN!");
            }
            Debug.Log($"<color=#FFD700>[SUMMONING COMPLETE]</color> Spawned {eliteName} with bonus Gold & ExpGem shower!");
        }
    }

    public class EliteTreasureDropper : MonoBehaviour
    {
        public int goldCoinsToDrop = 7;
        public int bonusExpGems = 5;
        public float expPerBonusGem = 35f;
        public GameObject goldCoinPrefab;
        public GameObject expGemPrefab;

        private EnemyStats stats;
        private bool dropped = false;

        private void Awake()
        {
            stats = GetComponent<EnemyStats>();
        }

        private void OnDisable()
        {
            TryDropTreasure();
        }

        private void OnDestroy()
        {
            TryDropTreasure();
        }

        private void TryDropTreasure()
        {
            if (dropped) return;
            if (stats == null || stats.currentHealth > 0f) return;
            if (!gameObject.scene.isLoaded) return;

            dropped = true;

            for (int i = 0; i < goldCoinsToDrop; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-0.85f, 0.85f), Random.Range(-0.85f, 0.85f), 0f);
                if (goldCoinPrefab != null)
                {
                    Instantiate(goldCoinPrefab, transform.position + offset, Quaternion.identity);
                }
                else if (PlayerStats.Instance != null)
                {
                    PlayerStats.Instance.AddGold(15);
                }
            }

            for (int i = 0; i < bonusExpGems; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-0.95f, 0.95f), Random.Range(-0.95f, 0.95f), 0f);
                ExpGem.DropExp(transform.position + offset, expPerBonusGem, expGemPrefab);
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowWaveBanner("💰 ĐÃ HẠ GỤC TINH ANH CỔ NGỮ! MƯA VÀNG & NGỌC KINH NGHIỆM RƠI RA!");
            }
        }
    }
}
