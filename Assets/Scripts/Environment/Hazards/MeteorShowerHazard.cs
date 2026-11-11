using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VampireSurvivors.Audio;
using VampireSurvivors.Core;
using VampireSurvivors.Enemies;
using VampireSurvivors.Player;
using VampireSurvivors.UI;

namespace VampireSurvivors.Environment.Hazards
{
    /// <summary>
    /// Map 3 (Infernal Volcano) Hazard:
    /// Periodically unleashes a devastating Volcanic Meteor Shower.
    /// Scaled dynamically across 5 stages based on survival time (0-20+ minutes).
    /// Meteors damage BOTH the Player and Enemies (enabling high-skill kiting of hordes/bosses into the blast).
    /// </summary>
    public class MeteorShowerHazard : MonoBehaviour
    {
        [Header("Survival Progression")]
        public float survivalTimer = 0f;
        public int currentStage = 1;

        private float waveTimer = 0f;
        private bool firstWaveTriggered = false;
        private int lastAnnouncedStage = 1;

        private readonly List<GameObject> activeVisuals = new List<GameObject>();
        private readonly Collider2D[] hitBuffer = new Collider2D[32];

        private void OnEnable()
        {
            survivalTimer = 0f;
            waveTimer = 0f;
            firstWaveTriggered = false;
            currentStage = 1;
            lastAnnouncedStage = 1;
        }

        private void OnDisable()
        {
            ClearAll();
        }

        public void ClearAll()
        {
            StopAllCoroutines();
            foreach (var go in activeVisuals)
            {
                if (go != null) Destroy(go);
            }
            activeVisuals.Clear();
        }

        public void TickHazard(float deltaTime, Transform playerTransform)
        {
            if (playerTransform == null) return;

            survivalTimer += deltaTime;
            UpdateProgressionStage();

            waveTimer += deltaTime;
            float targetInterval = GetIntervalForStage();

            if (!firstWaveTriggered && waveTimer >= 6f)
            {
                firstWaveTriggered = true;
                waveTimer = 0f;
                StartCoroutine(SpawnMeteorWave(playerTransform.position));
            }
            else if (firstWaveTriggered && waveTimer >= targetInterval)
            {
                waveTimer = 0f;
                StartCoroutine(SpawnMeteorWave(playerTransform.position));
            }
        }

        public void ForceTrigger(Transform playerTransform)
        {
            if (playerTransform == null) return;
            StartCoroutine(SpawnMeteorWave(playerTransform.position, true));
        }

        private void UpdateProgressionStage()
        {
            int prev = currentStage;
            if (survivalTimer < 180f) currentStage = 1;       // 0 - 3 min
            else if (survivalTimer < 420f) currentStage = 2;  // 3 - 7 min
            else if (survivalTimer < 720f) currentStage = 3;  // 7 - 12 min
            else if (survivalTimer < 1020f) currentStage = 4; // 12 - 17 min
            else currentStage = 5;                           // 17 - 20+ min

            if (currentStage > lastAnnouncedStage)
            {
                lastAnnouncedStage = currentStage;
                AnnounceStageEscalation(currentStage);
            }
        }

        private void AnnounceStageEscalation(int stage)
        {
            string title = "☄️ MƯA THIÊN THẠCH TĂNG TỐC!";
            string sub = "Độ dày và quy mô thiên thạch tăng lên!";

            switch (stage)
            {
                case 2:
                    title = "☄️ ĐỊA CHẤN BÙNG NỔ!";
                    sub = "Thiên thạch rơi dày hơn, nhắm vào chiến trường!";
                    break;
                case 3:
                    title = "☄️ MƯA THIÊN THẠCH DỮ DỘI!";
                    sub = "Cảnh báo: Thiên thạch bắt đầu định vị người chơi!";
                    break;
                case 4:
                    title = "🔥 HỎA NGỤC GIÁNG LÂM!";
                    sub = "Mưa thiên thạch liên hoàn rơi cấp tốc!";
                    break;
                case 5:
                    title = "💥 THIÊN THẠCH KHỔNG LỒ!";
                    sub = "Đại thiên thạch 32x32 hủy diệt diện rộng xuất hiện!";
                    break;
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowWaveBanner($"{title} - {sub}");
            }
        }

        private float GetIntervalForStage()
        {
            float baseInt;
            switch (currentStage)
            {
                case 1: baseInt = 18f; break;
                case 2: baseInt = 14f; break;
                case 3: baseInt = 10f; break;
                case 4: baseInt = 8f; break;
                default: baseInt = 6f; break;
            }
            return Mathf.Max(3.5f, baseInt / DifficultyManager.GetHazardMultiplier());
        }

        private int GetMeteorCountForStage()
        {
            int baseCount;
            switch (currentStage)
            {
                case 1: baseCount = Random.Range(2, 4); break;
                case 2: baseCount = Random.Range(4, 6); break;
                case 3: baseCount = Random.Range(6, 9); break;
                case 4: baseCount = Random.Range(8, 11); break;
                default: baseCount = Random.Range(10, 15); break;
            }
            return Mathf.Max(2, Mathf.RoundToInt(baseCount * DifficultyManager.GetHazardMultiplier()));
        }

        private float GetWarningDurationForStage()
        {
            switch (currentStage)
            {
                case 1: return 1.8f;
                case 2: return 1.6f;
                case 3: return 1.4f;
                case 4: return 1.2f;
                default: return 1.0f;
            }
        }

        private float GetPlayerDamageForStage(bool isGiant)
        {
            float dmgMult = DifficultyManager.GetDamageMultiplier();
            if (isGiant) return 38f * dmgMult;
            switch (currentStage)
            {
                case 1: return 10f * dmgMult;
                case 2: return 14f * dmgMult;
                case 3: return 18f * dmgMult;
                case 4: return 22f * dmgMult;
                default: return 26f * dmgMult;
            }
        }

        private float GetEnemyDamageForStage(bool isGiant)
        {
            float hazMult = DifficultyManager.GetHazardMultiplier();
            if (isGiant) return 110f * hazMult;
            switch (currentStage)
            {
                case 1: return 28f * hazMult;
                case 2: return 38f * hazMult;
                case 3: return 50f * hazMult;
                case 4: return 65f * hazMult;
                default: return 80f * hazMult;
            }
        }

        private IEnumerator SpawnMeteorWave(Vector3 playerPos, bool forced = false)
        {
            int count = forced ? 6 : GetMeteorCountForStage();
            float warnDuration = GetWarningDurationForStage();

            for (int i = 0; i < count; i++)
            {
                Vector3 targetPos;
                bool isGiant = (currentStage >= 5 && i == 0); // At stage 5, the first meteor of each wave is Giant

                if (i == 0 && currentStage >= 3)
                {
                    // Target directly near the player
                    targetPos = playerPos + (Vector3)(Random.insideUnitCircle * 1.5f);
                }
                else
                {
                    // Spread around the player
                    targetPos = playerPos + (Vector3)(Random.insideUnitCircle * Random.Range(2.5f, 9.5f));
                }

                StartCoroutine(SingleMeteorRoutine(targetPos, warnDuration, isGiant));

                // Stagger spawn between meteors
                float delay = (currentStage >= 4) ? 0.12f : 0.22f;
                yield return new WaitForSeconds(delay);
            }
        }

        private IEnumerator SingleMeteorRoutine(Vector3 impactPos, float warnDuration, bool isGiant)
        {
            // 1. Warning Reticle on ground
            GameObject warnGO = new GameObject("Meteor_Warn");
            activeVisuals.Add(warnGO);
            warnGO.transform.position = impactPos;

            SpriteRenderer warnSR = warnGO.AddComponent<SpriteRenderer>();
            warnSR.sprite = HazardSpriteLoader.GetMeteorWarnSprite();
            warnSR.sortingOrder = -8; // Below player and enemies, above terrain
            float baseWarnScale = isGiant ? 2.0f : 1.25f;
            warnGO.transform.localScale = Vector3.one * baseWarnScale;

            float elapsed = 0f;
            while (elapsed < warnDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / warnDuration;
                // Pulsate scale and alpha
                float pulse = 0.85f + 0.25f * Mathf.Sin(progress * Mathf.PI * 8f);
                warnGO.transform.localScale = Vector3.one * (baseWarnScale * pulse);
                warnSR.color = Color.Lerp(new Color(1f, 0.4f, 0.1f, 0.6f), new Color(1f, 0.1f, 0.05f, 0.95f), progress);
                yield return null;
            }

            Destroy(warnGO);
            activeVisuals.Remove(warnGO);

            // 2. Falling Meteor from top-right down to targetPos
            GameObject meteorGO = new GameObject(isGiant ? "Meteor_Giant" : "Meteor_Rock");
            activeVisuals.Add(meteorGO);

            SpriteRenderer meteorSR = meteorGO.AddComponent<SpriteRenderer>();
            meteorSR.sprite = isGiant ? HazardSpriteLoader.GetMeteorGiantSprite() : HazardSpriteLoader.GetMeteorSprite();
            meteorSR.sortingOrder = 18; // In front of entities while falling
            float meteorScale = isGiant ? 1.5f : 1.2f;
            meteorGO.transform.localScale = Vector3.one * meteorScale;

            Vector3 startPos = impactPos + new Vector3(4.5f, 8.0f, 0f);
            meteorGO.transform.position = startPos;

            // Rotate along trajectory
            float angle = Mathf.Atan2(-8.0f, -4.5f) * Mathf.Rad2Deg;
            meteorGO.transform.rotation = Quaternion.Euler(0f, 0f, angle + 45f);

            float fallDuration = isGiant ? 0.38f : 0.30f;
            float fallElapsed = 0f;
            while (fallElapsed < fallDuration)
            {
                fallElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(fallElapsed / fallDuration);
                // Accelerated fall curve
                float curvedT = t * t;
                meteorGO.transform.position = Vector3.Lerp(startPos, impactPos, curvedT);
                yield return null;
            }

            Destroy(meteorGO);
            activeVisuals.Remove(meteorGO);

            // 3. Impact Blast, Scorch Decal, Damage & Camera Trauma
            ExecuteImpact(impactPos, isGiant);
        }

        private void ExecuteImpact(Vector3 impactPos, bool isGiant)
        {
            float blastRadius = isGiant ? 2.6f : 1.5f;
            float playerDamage = GetPlayerDamageForStage(isGiant);
            float enemyDamage = GetEnemyDamageForStage(isGiant);

            // Audio & Camera Trauma
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfxByName("explosion");
            }

            if (CameraJuice.Instance != null)
            {
                CameraJuice.Instance.Shake(isGiant ? 0.45f : 0.25f);
            }

            // Ground Scorch Decal (fades out after 2.5s)
            StartCoroutine(ScorchDecalRoutine(impactPos, isGiant));

            // Deal Damage to Player & Enemies in Radius
            Collider2D[] cols = Physics2D.OverlapCircleAll(impactPos, blastRadius);
            bool playerHit = false;

            for (int i = 0; i < cols.Length; i++)
            {
                Collider2D col = cols[i];
                if (col == null) continue;

                // Hit Player
                if (!playerHit && (col.CompareTag("Player") || col.GetComponent<PlayerStats>() != null || col.GetComponent<PlayerController>() != null))
                {
                    playerHit = true;
                    var pStats = col.GetComponent<PlayerStats>() ?? PlayerStats.Instance;
                    if (pStats != null)
                    {
                        pStats.TakeDamage(playerDamage);
                    }
                }

                // Hit Enemies (tactical kiting)
                var enemyStats = col.GetComponent<EnemyStats>();
                if (enemyStats != null)
                {
                    enemyStats.TakeDamage(enemyDamage);
                }
            }
        }

        private IEnumerator ScorchDecalRoutine(Vector3 impactPos, bool isGiant)
        {
            GameObject scorchGO = new GameObject("Meteor_Scorch");
            activeVisuals.Add(scorchGO);
            scorchGO.transform.position = impactPos;

            SpriteRenderer scorchSR = scorchGO.AddComponent<SpriteRenderer>();
            scorchSR.sprite = HazardSpriteLoader.GetMeteorScorchSprite();
            scorchSR.sortingOrder = -9; // On top of terrain, below reticle and entities
            scorchGO.transform.localScale = Vector3.one * (isGiant ? 2.0f : 1.3f);

            float lifetime = 2.5f;
            float elapsed = 0f;
            Color baseCol = Color.white;

            while (elapsed < lifetime)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / lifetime;
                float alpha = Mathf.Clamp01(1f - progress);
                scorchSR.color = new Color(baseCol.r, baseCol.g, baseCol.b, alpha);
                yield return null;
            }

            Destroy(scorchGO);
            activeVisuals.Remove(scorchGO);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 1.5f);
        }
    }
}
