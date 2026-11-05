using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VampireSurvivors.Audio;
using VampireSurvivors.Enemies;
using VampireSurvivors.Player;
using VampireSurvivors.UI;
using VampireSurvivors.Wave;

namespace VampireSurvivors.Environment.Hazards
{
    /// <summary>
    /// Map 3 (Infernal Volcano) Hazard:
    /// Every 20 seconds, spawns 2-4 Lava Crack fissures that warn for 2.0s before erupting for 1.5s,
    /// dealing damage to both the Player and Enemies (allowing skillful players to kite hordes/bosses into the fire).
    /// </summary>
    public class LavaCrackHazard : MonoBehaviour
    {
        public float eruptionInterval = 20f;
        public float firstEruptionDelay = 8f;
        public int minCracks = 2;
        public int maxCracks = 4;
        public float warningDuration = 2.0f;
        public float fireDuration = 1.5f;
        public float playerDamagePerTick = 12f;
        public float enemyDamagePerTick = 28f;

        private float timer = 0f;
        private bool firstTriggered = false;
        private bool bannerShownOnce = false;
        private readonly List<GameObject> activeCracks = new List<GameObject>();

        private void OnEnable()
        {
            timer = 0f;
            firstTriggered = false;
            bannerShownOnce = false;
        }

        private void OnDisable()
        {
            ClearAll();
        }

        public void TickHazard(float deltaTime, Transform playerTransform)
        {
            if (playerTransform == null) return;

            activeCracks.RemoveAll(c => c == null);

            timer += deltaTime;
            if (!firstTriggered && timer >= firstEruptionDelay)
            {
                firstTriggered = true;
                timer = 0f;
                SpawnLavaWave(playerTransform.position);
            }
            else if (firstTriggered && timer >= eruptionInterval)
            {
                timer = 0f;
                SpawnLavaWave(playerTransform.position);
            }
        }

        public void ForceTrigger(Transform playerTransform)
        {
            if (playerTransform == null) return;
            activeCracks.RemoveAll(c => c == null);
            SpawnLavaWave(playerTransform.position, true);
        }

        public void ClearAll()
        {
            for (int i = activeCracks.Count - 1; i >= 0; i--)
            {
                if (activeCracks[i] != null) Destroy(activeCracks[i]);
            }
            activeCracks.Clear();
        }

        private void SpawnLavaWave(Vector3 playerPos, bool forceBanner = false)
        {
            int count = Random.Range(minCracks, maxCracks + 1);

            // Collect some nearby enemy positions so cracks frequently target monster clusters
            List<Vector3> enemyTargets = new List<Vector3>();
            foreach (var e in WaveManager.ActiveEnemies)
            {
                if (e != null && e.gameObject.activeInHierarchy && Vector2.Distance(e.transform.position, playerPos) <= 9.5f)
                {
                    enemyTargets.Add(e.transform.position);
                }
            }

            for (int i = 0; i < count; i++)
            {
                Vector3 pos;
                if (i > 0 && enemyTargets.Count > 0 && Random.value < 0.65f)
                {
                    Vector3 target = enemyTargets[Random.Range(0, enemyTargets.Count)];
                    pos = target + new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0f);
                }
                else
                {
                    float angle = Random.Range(0f, Mathf.PI * 2f);
                    float dist = Random.Range(1.8f, 6.2f);
                    pos = playerPos + new Vector3(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist, 0f);
                }

                float rotZ = Random.Range(0f, 360f);
                GameObject crackGO = CreateLavaCrack(pos, rotZ);
                activeCracks.Add(crackGO);
            }

            if (!bannerShownOnce || forceBanner)
            {
                bannerShownOnce = true;
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowWaveBanner("🌋 VỆT DUNG NHAM NỨT TOÁC (CẢNH BÁO 2 GIÂY)! DỤ QUÁI VÀO LỬA ĐỂ THIÊU RỤI!");
                }
            }
            Debug.Log($"<color=#FF5522>[VOLCANO HAZARD]</color> Spawned {count} Lava Cracks around battlefield!");
        }

        private GameObject CreateLavaCrack(Vector3 pos, float angleDeg)
        {
            GameObject go = new GameObject("VolcanoLavaCrack");
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);
            // 16x32 at PPU 16 is 1m x 2m. Scale (1.35, 2.6) => ~1.35m wide x 5.2m long
            go.transform.localScale = new Vector3(1.35f, 2.6f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = HazardSpriteLoader.GetLavaWarnSprite();
            sr.sortingOrder = -1; // Above map background (-10), below characters

            LavaCrackFissure fissure = go.AddComponent<LavaCrackFissure>();
            fissure.Initialize(warningDuration, fireDuration, playerDamagePerTick, enemyDamagePerTick, angleDeg);
            return go;
        }
    }

    public class LavaCrackFissure : MonoBehaviour
    {
        private float warnTime = 2.0f;
        private float eruptTime = 1.5f;
        private float playerDamage = 12f;
        private float enemyDamage = 28f;
        private float angleDeg = 0f;

        private SpriteRenderer sr;
        private Vector3 baseScale;

        public void Initialize(float warn, float erupt, float pDmg, float eDmg, float angle)
        {
            warnTime = warn;
            eruptTime = erupt;
            playerDamage = pDmg;
            enemyDamage = eDmg;
            angleDeg = angle;
            sr = GetComponent<SpriteRenderer>();
            baseScale = transform.localScale;
            StartCoroutine(FissureLifecycleRoutine());
        }

        private IEnumerator FissureLifecycleRoutine()
        {
            // Phase 1: 2.0s Warning with accelerating pulse
            float elapsed = 0f;
            while (elapsed < warnTime)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / warnTime);
                float freq = Mathf.Lerp(6f, 22f, progress);
                float alpha = 0.45f + Mathf.Abs(Mathf.Sin(elapsed * freq)) * 0.50f;

                if (sr != null)
                {
                    Color c = Color.Lerp(new Color(1f, 0.45f, 0.15f, alpha), new Color(1f, 0.85f, 0.25f, alpha), progress);
                    sr.color = c;
                }
                yield return null;
            }

            // Phase 2: 1.5s Eruption!
            if (sr != null)
            {
                sr.sprite = HazardSpriteLoader.GetLavaFireSprite();
                sr.color = Color.white;
                sr.sortingOrder = 1;
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfxByName("hit");
            }

            var camJuice = Camera.main != null ? Camera.main.GetComponent<Core.CameraJuice>() : null;
            if (camJuice != null) camJuice.Shake(0.08f, 0.12f);

            // World-space hitbox dimensions matching the fissure sprite
            Vector2 boxSize = new Vector2(1.05f, 4.8f);
            float fireElapsed = 0f;
            float tickTimer = 0.25f; // Deal first tick immediately upon eruption

            while (fireElapsed < eruptTime)
            {
                float dt = Time.deltaTime;
                fireElapsed += dt;
                tickTimer += dt;

                // Subtle width throb during eruption
                float throb = 1f + Mathf.Sin(fireElapsed * 25f) * 0.08f;
                transform.localScale = new Vector3(baseScale.x * throb, baseScale.y, 1f);

                if (tickTimer >= 0.25f)
                {
                    tickTimer -= 0.25f;
                    ApplyLavaDamageTick(boxSize);
                }

                yield return null;
            }

            // Quick fade out
            float fade = 0f;
            while (fade < 0.2f)
            {
                fade += Time.deltaTime;
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = 1f - (fade / 0.2f);
                    sr.color = c;
                }
                yield return null;
            }

            Destroy(gameObject);
        }

        private void ApplyLavaDamageTick(Vector2 boxSize)
        {
            Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, boxSize, angleDeg);
            HashSet<EnemyStats> damagedEnemiesThisTick = new HashSet<EnemyStats>();
            bool playerDamagedThisTick = false;

            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D col = hits[i];
                if (col == null) continue;

                if (!playerDamagedThisTick && col.CompareTag("Player"))
                {
                    PlayerStats ps = col.GetComponent<PlayerStats>();
                    if (ps != null && ps.currentHealth > 0)
                    {
                        playerDamagedThisTick = true;
                        ps.TakeDamage(playerDamage);
                    }
                }
                else if (col.CompareTag("Enemy"))
                {
                    EnemyStats es = col.GetComponent<EnemyStats>();
                    if (es == null) es = col.GetComponentInParent<EnemyStats>();
                    if (es != null && !damagedEnemiesThisTick.Contains(es) && es.currentHealth > 0)
                    {
                        damagedEnemiesThisTick.Add(es);
                        float dmg = es.isBoss ? enemyDamage * 0.5f : enemyDamage;
                        es.TakeDamage(dmg, false);
                    }
                }
            }
        }
    }
}
