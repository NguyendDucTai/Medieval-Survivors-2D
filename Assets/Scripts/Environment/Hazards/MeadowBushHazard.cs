using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VampireSurvivors.Audio;
using VampireSurvivors.Collectibles;
using VampireSurvivors.Player;
using VampireSurvivors.UI;

namespace VampireSurvivors.Environment.Hazards
{
    /// <summary>
    /// Map 0 (Emerald Meadow) Hazard/Feature:
    /// Periodically spawns destructible 16x16 Ancient Bushes around the player.
    /// Destroying a bush drops a Healing Potion (70%) or a Global ExpGem Magnet (30%).
    /// </summary>
    public class MeadowBushHazard : MonoBehaviour
    {
        public float spawnInterval = 180f; // Every 3 minutes per UC-05 spec
        public float firstSpawnDelay = 20f; // Early first spawn so players experience it early in the run
        public int minBushesPerWave = 3;
        public int maxBushesPerWave = 5;
        public int maxActiveBushes = 8;
        public float bushLifetime = 60f;

        private float timer = 0f;
        private bool firstWaveSpawned = false;
        private readonly List<AncientBush> activeBushes = new List<AncientBush>();
        private readonly List<GameObject> activePickups = new List<GameObject>();

        private void OnEnable()
        {
            timer = 0f;
            firstWaveSpawned = false;
        }

        private void OnDisable()
        {
            ClearAll();
        }

        public void TickHazard(float deltaTime, Transform playerTransform)
        {
            if (playerTransform == null) return;

            CleanupNulls();

            timer += deltaTime;
            float hazMult = Core.DifficultyManager.GetHazardMultiplier();
            float effectiveFirstDelay = firstSpawnDelay / hazMult;
            float effectiveInterval = spawnInterval / hazMult;

            if (!firstWaveSpawned && timer >= effectiveFirstDelay)
            {
                firstWaveSpawned = true;
                timer = 0f;
                SpawnBushCluster(playerTransform.position);
            }
            else if (firstWaveSpawned && timer >= effectiveInterval)
            {
                timer = 0f;
                SpawnBushCluster(playerTransform.position);
            }
        }

        public void ForceTrigger(Transform playerTransform)
        {
            if (playerTransform == null) return;
            CleanupNulls();
            SpawnBushCluster(playerTransform.position);
        }

        public void ClearAll()
        {
            for (int i = activeBushes.Count - 1; i >= 0; i--)
            {
                if (activeBushes[i] != null) Destroy(activeBushes[i].gameObject);
            }
            activeBushes.Clear();

            for (int i = activePickups.Count - 1; i >= 0; i--)
            {
                if (activePickups[i] != null) Destroy(activePickups[i]);
            }
            activePickups.Clear();
        }

        private void CleanupNulls()
        {
            activeBushes.RemoveAll(b => b == null);
            activePickups.RemoveAll(p => p == null);
        }

        private void SpawnBushCluster(Vector3 centerPos)
        {
            int availableSlots = maxActiveBushes - activeBushes.Count;
            if (availableSlots <= 0) return;

            int count = Mathf.Min(availableSlots, Random.Range(minBushesPerWave, maxBushesPerWave + 1));
            int spawned = 0;

            for (int i = 0; i < count; i++)
            {
                Vector3 candidate = centerPos;
                bool valid = false;

                for (int attempt = 0; attempt < 12; attempt++)
                {
                    float angle = Random.Range(0f, Mathf.PI * 2f);
                    float dist = Random.Range(4.8f, 9.2f);
                    candidate = centerPos + new Vector3(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist, 0f);

                    bool tooClose = false;
                    foreach (var b in activeBushes)
                    {
                        if (b != null && Vector2.Distance(b.transform.position, candidate) < 2.0f)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    if (!tooClose)
                    {
                        valid = true;
                        break;
                    }
                }

                if (valid)
                {
                    AncientBush bush = CreateBush(candidate);
                    activeBushes.Add(bush);
                    spawned++;
                }
            }

            if (spawned > 0)
            {
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowWaveBanner("🌿 BỤI CÂY CỔ THỤ MỌC LÊN! PHÁ HỦY ĐỂ NHẬN BÌNH MÁU / NAM CHÂM!");
                }
                Debug.Log($"<color=#66FF88>[EMERALD MEADOW HAZARD]</color> Spawned {spawned} Ancient Bushes around player!");
            }
        }

        private AncientBush CreateBush(Vector3 pos)
        {
            GameObject go = new GameObject("AncientBush_16x16");
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 1.35f;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = HazardSpriteLoader.GetBushSprite();
            sr.sortingOrder = 2;

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.58f;

            AncientBush bush = go.AddComponent<AncientBush>();
            float bushHp = 30f * Core.DifficultyManager.GetHazardMultiplier();
            bush.Initialize(this, bushHp, bushLifetime);
            return bush;
        }

        public void RegisterPickup(GameObject pickupGO)
        {
            if (pickupGO != null) activePickups.Add(pickupGO);
        }
    }

    public class AncientBush : MonoBehaviour, IBreakable
    {
        public float maxHp = 30f;
        public float currentHp = 30f;
        public float lifetime = 60f;
        public bool IsDestroyed { get; private set; }

        private MeadowBushHazard owner;
        private SpriteRenderer sr;
        private Vector3 baseScale;
        private float spawnTime;

        public void Initialize(MeadowBushHazard hazardOwner, float hp, float life)
        {
            owner = hazardOwner;
            maxHp = hp;
            currentHp = hp;
            lifetime = life;
            spawnTime = Time.time;
            sr = GetComponent<SpriteRenderer>();
            baseScale = transform.localScale;
            StartCoroutine(SpawnPopRoutine());
        }

        private IEnumerator SpawnPopRoutine()
        {
            float t = 0f;
            transform.localScale = Vector3.zero;
            while (t < 0.22f)
            {
                t += Time.deltaTime;
                float p = t / 0.22f;
                transform.localScale = Vector3.Lerp(Vector3.zero, baseScale * 1.15f, p);
                yield return null;
            }
            transform.localScale = baseScale;
        }

        private void Update()
        {
            if (IsDestroyed) return;
            if (Time.time - spawnTime >= lifetime)
            {
                IsDestroyed = true;
                Destroy(gameObject);
            }
        }

        public void TakeBreakableDamage(float amount)
        {
            if (IsDestroyed) return;

            currentHp -= amount;
            if (DamageNumberPool.Instance != null)
            {
                DamageNumberPool.Instance.SpawnDamage(amount, transform.position + Vector3.up * 0.35f, false);
            }

            if (currentHp <= 0f)
            {
                BreakBush();
            }
            else if (gameObject.activeInHierarchy)
            {
                StartCoroutine(HitFlashRoutine());
            }
        }

        private IEnumerator HitFlashRoutine()
        {
            if (sr != null) sr.color = new Color(1f, 0.55f, 0.55f, 1f);
            transform.localScale = baseScale * 1.18f;
            yield return new WaitForSeconds(0.08f);
            if (sr != null) sr.color = Color.white;
            transform.localScale = baseScale;
        }

        private void BreakBush()
        {
            if (IsDestroyed) return;
            IsDestroyed = true;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfxByName("hit");
            }

            // 70% Health Potion, 30% Magnet
            bool isMagnet = Random.value < 0.30f;
            SpawnRewardPickup(isMagnet);

            Destroy(gameObject);
        }

        private void SpawnRewardPickup(bool isMagnet)
        {
            GameObject pickup = new GameObject(isMagnet ? "HazardPickup_Magnet" : "HazardPickup_HealthPotion");
            pickup.transform.position = transform.position;
            pickup.transform.localScale = Vector3.one * 1.25f;

            SpriteRenderer psr = pickup.AddComponent<SpriteRenderer>();
            psr.sprite = isMagnet ? HazardSpriteLoader.GetMagnetSprite() : HazardSpriteLoader.GetHealthPotionSprite();
            psr.sortingOrder = 3;

            CircleCollider2D col = pickup.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.55f;

            MeadowBushPickup comp = pickup.AddComponent<MeadowBushPickup>();
            comp.isMagnet = isMagnet;

            if (owner != null) owner.RegisterPickup(pickup);
        }
    }

    public class MeadowBushPickup : MonoBehaviour
    {
        public bool isMagnet = false;
        public float healFraction = 0.25f; // Restores 25% Max HP
        public float lifetime = 45f;

        private Vector3 startPos;
        private float birthTime;
        private bool collected = false;

        private void Start()
        {
            startPos = transform.position;
            birthTime = Time.time;
        }

        private void Update()
        {
            if (collected) return;

            if (Time.time - birthTime >= lifetime)
            {
                Destroy(gameObject);
                return;
            }

            // Gentle retro pixel bobbing
            float bob = Mathf.Sin((Time.time - birthTime) * 4.5f) * 0.08f;
            transform.position = startPos + new Vector3(0f, bob, 0f);

            // Check distance to player for smooth pickup
            PlayerStats player = PlayerStats.Instance;
            if (player == null && PlayerController.Instance != null)
            {
                player = PlayerController.Instance.GetComponent<PlayerStats>();
            }

            if (player != null && player.currentHealth > 0)
            {
                float dist = Vector2.Distance(transform.position, player.transform.position);
                if (dist <= 1.15f)
                {
                    Consume(player);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collected) return;
            if (collision.CompareTag("Player"))
            {
                PlayerStats ps = collision.GetComponent<PlayerStats>();
                if (ps != null) Consume(ps);
            }
        }

        private void Consume(PlayerStats player)
        {
            if (collected || player == null) return;
            collected = true;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfxByName("pickup");
            }

            if (isMagnet)
            {
                int pulled = ExpGem.MagnetAllActiveGems(player);
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowWaveBanner($"🧲 NAM CHÂM CỔ THỤ: HÚT TOÀN BỘ {pulled} NGỌC KINH NGHIỆM!");
                }
                Debug.Log($"<color=#55CCFF>[MEADOW MAGNET]</color> Pulled {pulled} active ExpGems to player!");
            }
            else
            {
                float healAmt = Mathf.Max(15f, Mathf.Round(player.maxHealth * healFraction));
                player.Heal(healAmt);
                if (DamageNumberPool.Instance != null)
                {
                    DamageNumberPool.Instance.SpawnHeal(healAmt, player.transform.position + Vector3.up * 0.5f);
                }
                Debug.Log($"<color=#55FF88>[MEADOW POTION]</color> Restored +{healAmt} HP to player!");
            }

            Destroy(gameObject);
        }
    }
}
