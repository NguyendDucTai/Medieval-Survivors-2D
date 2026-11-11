using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using VampireSurvivors.Player;
using VampireSurvivors.UI;

namespace VampireSurvivors.Environment.Hazards
{
    /// <summary>
    /// Map 1 (Scorching Desert) Hazard:
    /// Periodically triggers a pixel-art Sandstorm that pushes PlayerController.externalVelocity
    /// along the wind direction and renders square pixel sand particles.
    /// Scaled dynamically across 5 stages based on survival time (0-20+ minutes).
    /// </summary>
    public class SandstormHazard : MonoBehaviour
    {
        [Header("Survival Progression")]
        public float survivalTimer = 0f;
        public int currentStage = 1;
        private int lastAnnouncedStage = 1;

        [Header("Storm Parameters")]
        public float stormInterval = 65f;
        public float firstStormDelay = 25f;
        public float warningDuration = 3.0f;
        public float stormDuration = 16.0f;
        public float windPushSpeed = 1.5f;

        private float timer = 0f;
        private bool firstStormTriggered = false;
        private bool isStormRunning = false;
        private Coroutine stormCoroutine;

        private GameObject overlayCanvasGO;
        private Image tintImage;
        private GameObject particleGO;
        private ParticleSystem sandParticles;

        private Vector2 GetRandomWindDirection()
        {
            // Pick any organic angle across 360 degrees
            float angleDeg = Random.Range(0f, 360f);
            float rad = angleDeg * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
        }

        private Vector2 GetShiftedWindDirection(Vector2 currentDir)
        {
            float currentAngle = Mathf.Atan2(currentDir.y, currentDir.x) * Mathf.Rad2Deg;
            // Shift by 60 to 150 degrees to mimic sudden desert wind shears
            float shift = Random.Range(60f, 150f) * (Random.value > 0.5f ? 1f : -1f);
            float newAngle = (currentAngle + shift) * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle)).normalized;
        }

        private void OnEnable()
        {
            timer = 0f;
            survivalTimer = 0f;
            currentStage = 1;
            lastAnnouncedStage = 1;
            firstStormTriggered = false;
            isStormRunning = false;
        }

        private void OnDisable()
        {
            ClearAll();
        }

        public void TickHazard(float deltaTime, Transform playerTransform)
        {
            if (playerTransform == null) return;

            survivalTimer += deltaTime;
            UpdateProgressionStage();

            if (particleGO != null)
            {
                particleGO.transform.position = playerTransform.position;
            }

            if (isStormRunning) return;

            timer += deltaTime;
            if (!firstStormTriggered && timer >= firstStormDelay)
            {
                firstStormTriggered = true;
                timer = 0f;
                TriggerStorm();
            }
            else if (firstStormTriggered && timer >= stormInterval)
            {
                timer = 0f;
                TriggerStorm();
            }
        }

        private void UpdateProgressionStage()
        {
            if (survivalTimer < 240f) currentStage = 1;       // 0 - 4 min
            else if (survivalTimer < 480f) currentStage = 2;  // 4 - 8 min
            else if (survivalTimer < 720f) currentStage = 3;  // 8 - 12 min
            else if (survivalTimer < 960f) currentStage = 4;  // 12 - 16 min
            else currentStage = 5;                           // 16 - 20+ min

            switch (currentStage)
            {
                case 1:
                    stormInterval = 65f;
                    stormDuration = 16f;
                    windPushSpeed = 1.70f;
                    break;
                case 2:
                    stormInterval = 55f;
                    stormDuration = 18f;
                    windPushSpeed = 2.20f;
                    break;
                case 3:
                    stormInterval = 45f;
                    stormDuration = 20f;
                    windPushSpeed = 2.70f;
                    break;
                case 4:
                    stormInterval = 38f;
                    stormDuration = 22f;
                    windPushSpeed = 3.25f;
                    break;
                default:
                    stormInterval = 30f;
                    stormDuration = 25f;
                    windPushSpeed = 3.90f;
                    break;
            }

            float hazMult = Core.DifficultyManager.GetHazardMultiplier();
            stormInterval = Mathf.Max(18f, stormInterval / hazMult);
            windPushSpeed *= hazMult;

            if (currentStage > lastAnnouncedStage)
            {
                lastAnnouncedStage = currentStage;
                AnnounceStageEscalation(currentStage);
            }
        }

        private void AnnounceStageEscalation(int stage)
        {
            string title = "🌪️ BÃO CÁT ĐANG MẠNH LÊN!";
            string sub = "Sức gió và tần suất bão cát tăng tiến!";

            switch (stage)
            {
                case 2:
                    title = "🌪️ BÃO CÁT TĂNG TỐC!";
                    sub = "Gió bão thổi mạnh hơn trên sa mạc!";
                    break;
                case 3:
                    title = "🌪️ BÃO CÁT ĐỔI CHIỀU BẤT NGỜ!";
                    sub = "Cơn lốc xoáy bắt đầu chuyển hướng đột ngột!";
                    break;
                case 4:
                    title = "🌪️ BÃO CÁT CUỒNG NỘ!";
                    sub = "Cát ma sát dữ dội, di chuyển liên tục để không bị dính sát thương!";
                    break;
                case 5:
                    title = "🌪️ BÃO CÁT TỬ THẦN!";
                    sub = "Gió giật cực đại và quái vật sa mạc cuồng nộ!";
                    break;
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowWaveBanner($"{title} - {sub}");
            }
        }

        public void ForceTrigger()
        {
            if (stormCoroutine != null)
            {
                StopCoroutine(stormCoroutine);
                ResetWindAndVisuals();
            }
            timer = 0f;
            TriggerStorm();
        }

        public void ClearAll()
        {
            if (stormCoroutine != null)
            {
                StopCoroutine(stormCoroutine);
                stormCoroutine = null;
            }
            ResetWindAndVisuals();
            if (overlayCanvasGO != null) Destroy(overlayCanvasGO);
            if (particleGO != null) Destroy(particleGO);
        }

        private void TriggerStorm()
        {
            EnsureVisualsCreated();
            stormCoroutine = StartCoroutine(SandstormRoutine());
        }

        private float GetMaxTintAlpha()
        {
            switch (currentStage)
            {
                case 1: return 0.25f;
                case 2: return 0.31f;
                case 3: return 0.37f;
                case 4: return 0.43f;
                default: return 0.49f;
            }
        }

        private IEnumerator SandstormRoutine()
        {
            isStormRunning = true;

            Vector2 windDir = GetRandomWindDirection();
            string dirName = GetDirectionVietnameseName(windDir);
            float maxAlpha = GetMaxTintAlpha();

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowWaveBanner($"🌪️ CẢNH BÁO: BÃO CÁT SA MẠC CẤP {currentStage} (HƯỚNG {dirName})!");
            }

            yield return new WaitForSeconds(warningDuration);

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowWaveBanner($"🏜️ BÃO CÁT HOÀNH HÀNH! GIÓ ĐẨY HƯỚNG {dirName} ({stormDuration:F0} GIÂY)!");
            }

            ConfigureParticleVelocity(windDir);
            if (sandParticles != null) sandParticles.Play();

            // Ramp up wind & screen tint over 1.0s
            float ramp = 0f;
            while (ramp < 1.0f)
            {
                ramp += Time.deltaTime;
                float p = Mathf.Clamp01(ramp / 1.0f);
                SetPlayerWind(windDir * (windPushSpeed * p));
                SetTintAlpha(maxAlpha * p);
                yield return null;
            }

            // Hold storm active with mid-storm direction shifts & standing-still friction
            float activeElapsed = 0f;
            float holdTime = Mathf.Max(1f, stormDuration - 2.0f);
            bool shiftedOnce = false;
            bool shiftedTwice = false;
            float frictionTimer = 0f;

            while (activeElapsed < holdTime)
            {
                float dt = Time.deltaTime;
                activeElapsed += dt;

                // Mid-storm wind direction shift for stage 2+
                if (currentStage >= 2 && !shiftedOnce && activeElapsed >= (holdTime * 0.5f))
                {
                    shiftedOnce = true;
                    windDir = GetShiftedWindDirection(windDir);
                    ConfigureParticleVelocity(windDir);
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ShowWaveBanner($"🌪️ GIÓ BÃO ĐỔI CHIỀU: CHUYỂN SANG HƯỚNG {GetDirectionVietnameseName(windDir)}!");
                    }
                }
                else if (currentStage >= 4 && !shiftedTwice && activeElapsed >= (holdTime * 0.75f))
                {
                    shiftedTwice = true;
                    windDir = GetShiftedWindDirection(windDir);
                    ConfigureParticleVelocity(windDir);
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ShowWaveBanner($"🌪️ LỐC XOÁY CHUYỂN HƯỚNG SANG {GetDirectionVietnameseName(windDir)}!");
                    }
                }

                SetPlayerWind(windDir * windPushSpeed);

                // Standing-still sand friction damage for Stage 4+ (standing still deals 1 dmg/sec)
                if (currentStage >= 4)
                {
                    frictionTimer += dt;
                    if (frictionTimer >= 1.0f)
                    {
                        frictionTimer = 0f;
                        var pRb = PlayerController.Instance != null ? PlayerController.Instance.GetComponent<Rigidbody2D>() : null;
                        if (pRb != null && pRb.linearVelocity.sqrMagnitude < 0.1f)
                        {
                            if (PlayerStats.Instance != null)
                            {
                                PlayerStats.Instance.TakeDamage(1f);
                            }
                        }
                    }
                }

                yield return null;
            }

            // Smoothly fade out wind & screen tint over 1.0s
            float fade = 0f;
            while (fade < 1.0f)
            {
                fade += Time.deltaTime;
                float p = 1f - Mathf.Clamp01(fade / 1.0f);
                SetPlayerWind(windDir * (windPushSpeed * p));
                SetTintAlpha(maxAlpha * p);
                yield return null;
            }

            ResetWindAndVisuals();
            Debug.Log("<color=#FFCC55>[DESERT SANDSTORM]</color> Sandstorm subsided.");
        }

        private void ResetWindAndVisuals()
        {
            isStormRunning = false;
            SetPlayerWind(Vector2.zero);
            SetTintAlpha(0f);
            if (sandParticles != null) sandParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        private void SetPlayerWind(Vector2 vel)
        {
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.externalVelocity = vel;
            }
        }

        private void SetTintAlpha(float a)
        {
            if (tintImage != null)
            {
                Color c = tintImage.color;
                c.a = a;
                tintImage.color = c;
            }
        }

        private void EnsureVisualsCreated()
        {
            if (overlayCanvasGO == null)
            {
                overlayCanvasGO = new GameObject("SandstormFX_WorldCanvas");
                Canvas c = overlayCanvasGO.AddComponent<Canvas>();
                c.renderMode = RenderMode.ScreenSpaceOverlay;
                c.sortingOrder = 15; // Below interactive modals, above world

                CanvasScaler scaler = overlayCanvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                GameObject imgGO = new GameObject("SandTint");
                imgGO.transform.SetParent(overlayCanvasGO.transform, false);
                RectTransform rt = imgGO.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                tintImage = imgGO.AddComponent<Image>();
                tintImage.color = new Color(0.88f, 0.65f, 0.28f, 0f);
                tintImage.raycastTarget = false;
            }

            if (particleGO == null)
            {
                particleGO = new GameObject("Sandstorm_PixelParticles");
                sandParticles = particleGO.AddComponent<ParticleSystem>();

                var main = sandParticles.main;
                main.playOnAwake = false;
                main.loop = true;
                main.startLifetime = new ParticleSystem.MinMaxCurve(1.6f, 2.4f);
                main.startSpeed = 0f; // Pure 2D motion driven 100% by velocityOverLifetime
                main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.24f); // Fine retro pixel sand grains as originally tuned
                main.startColor = new ParticleSystem.MinMaxGradient(
                    new Color(0.98f, 0.82f, 0.42f, 0.92f), // Golden desert sand
                    new Color(0.85f, 0.62f, 0.25f, 0.96f)  // Dark amber dust
                );
                main.maxParticles = 1200; // Rich dense particle count
                main.simulationSpace = ParticleSystemSimulationSpace.World;

                var emission = sandParticles.emission;
                emission.rateOverTime = 420f; // Dense sand stream

                var shape = sandParticles.shape;
                shape.shapeType = ParticleSystemShapeType.Box;
                shape.scale = new Vector3(30f, 18f, 1f);

                var renderer = particleGO.GetComponent<ParticleSystemRenderer>();
                renderer.sortingOrder = 25; // Above entities, below UI
                renderer.material = new Material(Shader.Find("Sprites/Default"));

                Sprite squareSpr = HazardSpriteLoader.GetPixelSquareSprite();
                if (squareSpr != null)
                {
                    renderer.material.mainTexture = squareSpr.texture;
                }
            }
        }

        private void ConfigureParticleVelocity(Vector2 windDir)
        {
            if (sandParticles == null) return;
            var vel = sandParticles.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            float speed = 18f + (currentStage * 2.5f);
            vel.x = new ParticleSystem.MinMaxCurve(windDir.x * speed);
            vel.y = new ParticleSystem.MinMaxCurve(windDir.y * speed);
            vel.z = new ParticleSystem.MinMaxCurve(0f);

            // Scale particle density with progression stage
            var emission = sandParticles.emission;
            emission.rateOverTime = 380f + (currentStage * 60f);
        }

        private string GetDirectionVietnameseName(Vector2 dir)
        {
            float deg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if (deg < 0f) deg += 360f;

            if (deg >= 337.5f || deg < 22.5f) return "ĐÔNG (➔)";
            if (deg >= 22.5f && deg < 67.5f) return "ĐÔNG BẮC (⬈)";
            if (deg >= 67.5f && deg < 112.5f) return "BẮC (⬆)";
            if (deg >= 112.5f && deg < 157.5f) return "TÂY BẮC (⬉)";
            if (deg >= 157.5f && deg < 202.5f) return "TÂY (⬅)";
            if (deg >= 202.5f && deg < 247.5f) return "TÂY NAM (⬋)";
            if (deg >= 247.5f && deg < 292.5f) return "NAM (⬇)";
            return "ĐÔNG NAM (⬊)";
        }
    }
}
