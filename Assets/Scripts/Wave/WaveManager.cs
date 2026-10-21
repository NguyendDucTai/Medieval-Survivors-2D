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
        public GameObject plantPrefab;

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
        private bool plantTrapTriggered = false;
        private bool boss2Spawned = false;
        private bool marchingHordeTriggered = false;
        private bool swarm2Triggered = false;
        private bool boss3Spawned = false;
        private bool marchingHorde2Triggered = false;
        private bool wave5Triggered = false;
        private bool boss4Spawned = false;
        private bool wave6Triggered = false;
        private bool isVictory = false;

        public event Action<string> OnWaveBanner;
        public event Action OnVictory;

        private static readonly HashSet<EnemyStats> activeEnemySet = new HashSet<EnemyStats>();
        public static int ActiveEnemyCount => activeEnemySet.Count;

        public static void RegisterEnemy(EnemyStats enemy)
        {
            if (enemy != null) activeEnemySet.Add(enemy);
        }

        public static void UnregisterEnemy(EnemyStats enemy)
        {
            if (enemy != null) activeEnemySet.Remove(enemy);
        }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            activeEnemySet.Clear();
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
            if (plantPrefab == null) plantPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Plant.prefab");
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

            // Second 240.0 (4:00): Vampire Survivors Flower Ring Trap (360° Tightening Circle)
            if (gameTimer >= 240f && !plantTrapTriggered)
            {
                plantTrapTriggered = true;
                currentWaveTitle = "🥀 ĐỢT 3.5: VÒNG VÂY HOA QUỶ MA QUÁI";
                TriggerPlantCircleTrap(52, 13.5f, "🥀 CẢNH BÁO: VÒNG VÂY HOA QUỶ MA QUÁI KHÉP CHẶT (360 ĐỘ)!");
            }

            // Second 300.0 (5:00): Mini-Boss Skeleton Warlord
            if (gameTimer >= 300f && !boss2Spawned)
            {
                boss2Spawned = true;
                currentWaveTitle = "⚠️ TRÙM: ĐẠI TƯỚNG BỘ XƯƠNG";
                SpawnMiniBoss(skeletonPrefab, "Skeleton Warlord (Đại Tướng Xương)", 850f, 2.0f, 2.0f);
            }

            // Second 330.0 (5:30): Screen-Wide Continuous Skeleton Horde Wave 1 (Lasts 60 seconds / 1 full minute!)
            if (gameTimer >= 330f && !marchingHordeTriggered)
            {
                marchingHordeTriggered = true;
                currentWaveTitle = "💀 ĐỢT 3.8: THỦY TRIỀU BỘ XƯƠNG CÀN QUÉT (SINH TỒN 60 GIÂY)";
                TriggerContinuousHordeSwarm(60f, skeletonPrefab, "⚠️ BÁO ĐỘNG ĐỎ: THỦY TRIỀU BỘ XƯƠNG TRÀN VÀO (SINH TỒN 60 GIÂY)!", 240, 1.8f);
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

            // Second 720.0 (12:00): Wave 5 - Double Triangle Bat Swarms (Cross Charge)
            if (gameTimer >= 720f && !wave5Triggered)
            {
                wave5Triggered = true;
                currentWaveTitle = "🦇 ĐỢT 5: BÃO DƠI SONG ĐẠO ĐỘT KÍCH";
                StartCoroutine(TriggerTriangleSwarm(batPrefab, 5, 1.35f, "🦇 CẢNH BÁO: BÃO DƠI TỬ THẦN TIẾN CÔNG!"));
            }

            // Second 750.0 (12:30): Screen-Wide Continuous Goblin Horde Wave 2 (Lasts 75 seconds!)
            if (gameTimer >= 750f && !marchingHorde2Triggered)
            {
                marchingHorde2Triggered = true;
                currentWaveTitle = "👺 ĐỢT 5.5: THỦY TRIỀU YÊU TINH CUỒNG NỘ (SINH TỒN 75 GIÂY)";
                TriggerContinuousHordeSwarm(75f, goblinPrefab, "🔥 BÁO ĐỘNG TỐI CAO: THỦY TRIỀU YÊU TINH TRÀN VÀO (SINH TỒN 75 GIÂY)!", 260, 2.0f);
            }

            // Second 900.0 (15:00): Twin Titans Boss Encounter (King Slime + Skeleton Warlord)
            if (gameTimer >= 900f && !boss4Spawned)
            {
                boss4Spawned = true;
                currentWaveTitle = "👑 SONG TRÙM: BỘ ĐÔI TINH ANH";
                SpawnMiniBoss(slimePrefab, "Ancient King Slime", 650f, 1.9f, 2.4f);
                SpawnMiniBoss(skeletonPrefab, "Undead Warlord", 1200f, 2.1f, 2.2f);
            }

            // Second 1080.0 (18:00): Wave 6 - Final Goblin & Ghost Horde Rush (24 fast enemies)
            if (gameTimer >= 1080f && !wave6Triggered)
            {
                wave6Triggered = true;
                currentWaveTitle = "👺 ĐỢT 6: THỦY TRIỀU HẮC ÁM (VÂY HÃM)";
                StartCoroutine(TriggerSwarm(goblinPrefab, 24, "⚠️ BÁO ĐỘNG ĐỎ: THỦY TRIỀU HẮC ÁM TRÀN VÀO!"));
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
            if (ActiveEnemyCount >= maxEnemiesAlive) return;

            // Group spawn: 2 to 3 enemies per burst for exciting cluster action
            int burstCount = UnityEngine.Random.Range(2, 4);
            for (int i = 0; i < burstCount; i++)
            {
                if (ActiveEnemyCount >= maxEnemiesAlive) break;

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
                // Skeletons, Goblins, Bats, and Slimes (NO giant non-pixel ghosts!)
                if (r < 0.35f) return skeletonPrefab;
                if (r < 0.65f) return goblinPrefab;
                if (r < 0.85f) return batPrefab;
                return slimePrefab;
            }
            else
            {
                // High tier mix
                if (r < 0.40f) return goblinPrefab;
                if (r < 0.70f) return skeletonPrefab;
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

            if (playerTransform == null)
            {
#if UNITY_6000_0_OR_NEWER
                PlayerController p = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
#else
                PlayerController p = UnityEngine.Object.FindObjectOfType<PlayerController>();
#endif
                if (p != null) playerTransform = p.transform;
            }

            Vector3 playerPos = playerTransform != null ? playerTransform.position : Vector3.zero;

            // Audio alert cue
            if (VampireSurvivors.Audio.AudioManager.Instance != null)
            {
                VampireSurvivors.Audio.AudioManager.Instance.PlaySfxByName("shoot");
            }

            // Pick a clean approach direction just offscreen (8.5 to 9.5 meters away)
            Vector2[] approachAngles = new Vector2[]
            {
                Vector2.left, Vector2.right, Vector2.up, Vector2.down,
                new Vector2(1, 1).normalized, new Vector2(-1, 1).normalized,
                new Vector2(1, -1).normalized, new Vector2(-1, -1).normalized
            };
            Vector2 spawnDir = approachAngles[UnityEngine.Random.Range(0, approachAngles.Length)];

            float spawnDistance = 9.5f; // Just outside visible camera view
            Vector2 apexPos = (Vector2)playerPos + (spawnDir * spawnDistance);

            // Forward vector is directed in a locked straight line through the player's position at launch!
            Vector2 forward = ((Vector2)playerPos - apexPos).normalized;
            // Right vector is perpendicular to forward
            Vector2 right = new Vector2(-forward.y, forward.x);

            float swarmChargeSpeed = 9.2f;

            // Spawn rows forming a unified V / Triangle wedge that charges together
            for (int r = 0; r < rows; r++)
            {
                int countInRow = r + 1; // Row 0 has 1 (apex), Row 1 has 2, Row 2 has 3, etc.
                float rowDepth = r * spacing * 1.15f;
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
                        stats.maxHealth = Mathf.Max(stats.maxHealth, 35f);
                        stats.currentHealth = stats.maxHealth;
                        stats.attackDamage = 12f;
                        stats.moveSpeed = swarmChargeSpeed;
                    }

                    EnemyAI ai = batGO.GetComponent<EnemyAI>();
                    if (ai != null)
                    {
                        // Charge forward in a locked straight line - NO tracking! Allows skillful dodging!
                        ai.SetChargeFlyThrough(forward, swarmChargeSpeed, 5.0f, false);
                    }

                    // Visual polish: enraged eyes / flapping animation speed
                    var sr = batGO.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.color = new Color(1f, 0.4f, 0.4f);
                    var anim = batGO.GetComponent<Animator>();
                    if (anim != null) anim.speed = 1.8f;
                }
            }

            yield return null;
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

        public void TriggerPlantCircleTrap(int count = 52, float radius = 13.5f, string banner = null)
        {
            StartCoroutine(PlantCircleTrapRoutine(count, radius, banner));
        }

        private IEnumerator PlantCircleTrapRoutine(int count, float radius, string banner)
        {
            LoadPrefabsIfNull();
            if (plantPrefab == null) yield break;

            if (playerTransform == null)
            {
#if UNITY_6000_0_OR_NEWER
                PlayerController p = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
#else
                PlayerController p = UnityEngine.Object.FindObjectOfType<PlayerController>();
#endif
                if (p != null) playerTransform = p.transform;
            }
            if (playerTransform == null) yield break;

            if (!string.IsNullOrEmpty(banner))
            {
                OnWaveBanner?.Invoke(banner);
            }
            else
            {
                OnWaveBanner?.Invoke("🥀 CẢNH BÁO: HOA ĂN THỊT ĐANG MỌC THÀNH VÒNG QUANH BẠN!");
            }

            Vector2 center = playerTransform.position;
            List<PlantRingMember> ringMembers = new List<PlantRingMember>();

            float angleStep = 360f / count;

            // 1. Dần dần tạo thành 1 vòng tròn hoàn chỉnh quanh người chơi
            for (int i = 0; i < count; i += 2)
            {
                for (int sub = 0; sub < 2 && (i + sub) < count; sub++)
                {
                    int idx = i + sub;
                    float angle = idx * angleStep;
                    float rad = angle * Mathf.Deg2Rad;
                    Vector3 spawnPos = (Vector3)center + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * radius;

                    GameObject plantGO = Instantiate(plantPrefab, spawnPos, Quaternion.identity);

                    EnemyStats stats = plantGO.GetComponent<EnemyStats>();
                    if (stats != null)
                    {
                        stats.enemyName = "Living Plant (Hoa Ăn Thịt)";
                        stats.maxHealth = 1500f; // High HP: player must take damage to brute-force through!
                        stats.currentHealth = 1500f;
                        stats.attackDamage = 18f;
                        stats.moveSpeed = 0.5f;
                        stats.expValue = 40f;
                        if (goldCoinPrefab != null) stats.goldCoinPrefab = goldCoinPrefab;
                    }

                    PlantRingMember member = plantGO.AddComponent<PlantRingMember>();
                    member.Initialize(center, radius, angle);
                    ringMembers.Add(member);
                }

                yield return new WaitForSeconds(0.05f);
            }

            // 2. Khi đã tạo thành 1 vòng tròn, thông báo và bắt đầu thu dần lại
            OnWaveBanner?.Invoke("🥀 VÒNG VÂY HOA QUỶ ĐÃ KHÉP LẠI VÀ BẮT ĐẦU THU HẸP!");

            yield return new WaitForSeconds(0.8f);

            // 3. Thu dần vòng tròn lại với tốc độ rất chậm (0.18m/s)
            foreach (var m in ringMembers)
            {
                if (m != null)
                {
                    m.shrinkSpeed = 0.18f;
                    m.StartShrinking();
                }
            }

            Debug.Log($"<color=green>★★ [PLANT TRAP COMPLETED] ★★</color> {count} Plants formed 13.5m circle (1500 HP each) and are now slowly tightening!");
        }

        private Coroutine activeHordeSwarmCoroutine;

        public void TriggerContinuousHordeSwarm(float duration = 60f, GameObject customPrefab = null, string banner = null, int maxCapacity = 240, float moveSpeed = 1.8f)
        {
            if (activeHordeSwarmCoroutine != null)
            {
                StopCoroutine(activeHordeSwarmCoroutine);
            }
            activeHordeSwarmCoroutine = StartCoroutine(ContinuousHordeSwarmRoutine(duration, customPrefab, banner, maxCapacity, moveSpeed));
        }

        public void TriggerMarchingHordeWall(int totalRows = 12, float rowInterval = 2.4f, string banner = null, GameObject customPrefab = null, float marchSpeed = 2.4f)
        {
            // Backward-compatible redirect to continuous screen-filling flood swarm (no rigid rows)
            float duration = Mathf.Max(60f, totalRows * rowInterval);
            TriggerContinuousHordeSwarm(duration, customPrefab, banner, 240, marchSpeed > 0 ? marchSpeed : 1.8f);
        }

        private IEnumerator ContinuousHordeSwarmRoutine(float duration, GameObject customPrefab, string banner, int maxCapacity, float moveSpeed)
        {
            LoadPrefabsIfNull();
            GameObject prefabToUse = customPrefab != null ? customPrefab : (skeletonPrefab != null ? skeletonPrefab : goblinPrefab);
            if (prefabToUse == null) yield break;

            if (playerTransform == null)
            {
#if UNITY_6000_0_OR_NEWER
                PlayerController p = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
#else
                PlayerController p = UnityEngine.Object.FindObjectOfType<PlayerController>();
#endif
                if (p != null) playerTransform = p.transform;
            }
            if (playerTransform == null) yield break;

            if (!string.IsNullOrEmpty(banner))
            {
                OnWaveBanner?.Invoke(banner);
            }
            else
            {
                OnWaveBanner?.Invoke("⚠️ BÁO ĐỘNG ĐỎ: THỦY TRIỀU BỘ XƯƠNG TRÀN VÀO (SINH TỒN 60 GIÂY)!");
            }

            if (VampireSurvivors.Audio.AudioManager.Instance != null)
            {
                VampireSurvivors.Audio.AudioManager.Instance.PlaySfxByName("shoot");
            }

            float timer = 0f;
            float spawnTick = 0.12f;
            float nextCleanupTime = 1.0f;
            bool halfWayNotified = false;

            while (timer < duration)
            {
                if (playerTransform == null) yield break;

                timer += spawnTick;

                // Midpoint alert for extra tension
                if (!halfWayNotified && timer >= (duration * 0.5f))
                {
                    halfWayNotified = true;
                    OnWaveBanner?.Invoke($"⏱️ THỦY TRIỀU HẮC ÁM: CÒN {Mathf.CeilToInt(duration - timer)} GIÂY! HÃY TỰ TÌM KẼ HỞ ĐỂ THOÁT THÂN!");
                }

                Vector3 playerPos = playerTransform.position;

                // 1. Vampire Survivors Wrap-Around Teleport Recycling:
                // Instead of destroying and reinstantiating distant enemies, teleport them directly to the player's front advancing perimeter!
                if (timer >= nextCleanupTime)
                {
                    nextCleanupTime = timer + 0.8f;
                    Vector2 playerAim = PlayerController.Instance != null ? PlayerController.Instance.CurrentAimDirection : Vector2.right;
                    float baseAimAngle = Mathf.Atan2(playerAim.y, playerAim.x) * Mathf.Rad2Deg;

                    foreach (var es in activeEnemySet)
                    {
                        if (es != null && !es.isBoss)
                        {
                            if (Vector2.Distance(es.transform.position, playerPos) > 23f)
                            {
                                // Teleport to the perimeter in front of player's advance direction
                                float arcOffset = UnityEngine.Random.Range(-55f, 55f);
                                float teleportAngleRad = (baseAimAngle + arcOffset) * Mathf.Deg2Rad;
                                float dist = UnityEngine.Random.Range(11.5f, 14.5f);
                                float teleX = playerPos.x + (Mathf.Cos(teleportAngleRad) * dist * 1.35f);
                                float teleY = playerPos.y + (Mathf.Sin(teleportAngleRad) * dist);

                                es.transform.position = new Vector3(teleX, teleY, 0f);
                                es.currentHealth = es.maxHealth;
                            }
                        }
                    }
                }

                // 2. Continuous swarm spawning to keep screen densely populated ("kín map")
                int needed = maxCapacity - ActiveEnemyCount;

                if (needed > 0)
                {
                    // Spawn a cluster of 3 to 6 enemies per tick
                    int burst = Mathf.Min(needed, UnityEngine.Random.Range(3, 7));

                    // Pick a random base arc along the camera perimeter so enemies arrive in natural clusters
                    float baseAngle = UnityEngine.Random.Range(0f, 360f);

                    for (int b = 0; b < burst; b++)
                    {
                        float angleOffset = UnityEngine.Random.Range(-35f, 35f);
                        float angleRad = (baseAngle + angleOffset) * Mathf.Deg2Rad;

                        // Camera viewport is 16:9 with orthoSize 7.5, so elliptical perimeter just outside screen
                        float dist = UnityEngine.Random.Range(10.5f, 14.5f);
                        float spawnX = playerPos.x + (Mathf.Cos(angleRad) * dist * 1.35f);
                        float spawnY = playerPos.y + (Mathf.Sin(angleRad) * dist);
                        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);

                        GameObject enemyGO = Instantiate(prefabToUse, spawnPos, Quaternion.identity);

                        EnemyStats stats = enemyGO.GetComponent<EnemyStats>();
                        if (stats != null)
                        {
                            bool isGoblin = (customPrefab == goblinPrefab);
                            stats.enemyName = isGoblin ? "Horde Goblin (Yêu Tinh Thủy Triều)" : "Horde Skeleton (Bộ Xương Thủy Triều)";
                            stats.maxHealth = isGoblin ? 40f : 32f;
                            stats.currentHealth = stats.maxHealth;
                            stats.moveSpeed = moveSpeed;
                            stats.attackDamage = 8f;
                            stats.expValue = 12f;
                            stats.goldDropChance = 0.25f;
                            if (goldCoinPrefab != null) stats.goldCoinPrefab = goldCoinPrefab;
                        }

                        // Ensure enemy uses standard chasing AI (NO rigid locked charge)
                        EnemyAI ai = enemyGO.GetComponent<EnemyAI>();
                        if (ai != null)
                        {
                            ai.isChargeFlyThrough = false;
                        }

                        // Visual styling: light icy cyan tone for skeletons (matching screenshot) or enraged red for goblins
                        var sr = enemyGO.GetComponent<SpriteRenderer>();
                        if (sr != null)
                        {
                            sr.color = (customPrefab == goblinPrefab) ? new Color(1f, 0.65f, 0.65f) : new Color(0.72f, 0.88f, 1f);
                        }
                    }
                }

                yield return new WaitForSeconds(spawnTick);
            }

            OnWaveBanner?.Invoke("✨ BẠN ĐÃ SỐNG SÓT QUA ĐỢT THỦY TRIỀU THÀNH CÔNG!");
            activeHordeSwarmCoroutine = null;
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
