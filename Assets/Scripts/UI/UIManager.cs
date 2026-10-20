using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using VampireSurvivors.Player;

namespace VampireSurvivors.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        [Header("References")]
        public PlayerStats playerStats;

        [Header("Primary HUD Elements")]
        public Slider healthBar;
        public Text hpNumText;
        public Slider expBar;
        public Text levelText;
        public Text timerText;
        public Text waveText;
        public Text killCountText;
        public Text goldText;
        public Text bannerText;

        [Header("UI Panels")]
        public GameObject levelUpPanel;
        public LevelUpUI levelUpUI;
        public RunSummaryUI runSummaryUI;
        public ShopUI shopUI;
        public AchievementUI achievementUI;
        public BossHealthBarUI bossHealthBarUI;

        private Canvas hudCanvas;
        private bool isSubscribed = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            EnsureHUDExists();
        }

        private void Start()
        {
            if (playerStats == null)
            {
#if UNITY_6000_0_OR_NEWER
                playerStats = Object.FindAnyObjectByType<PlayerStats>();
#else
                playerStats = Object.FindObjectOfType<PlayerStats>();
#endif
            }

            SubscribeEvents();

            if (playerStats != null)
            {
                UpdateHealthUI(playerStats.currentHealth, playerStats.maxHealth);
                UpdateExpUI(playerStats.currentExp, playerStats.expToNextLevel);
                UpdateGoldUI(playerStats.inRunGold);
            }

            if (Wave.WaveManager.Instance != null)
            {
                Wave.WaveManager.Instance.OnWaveBanner += ShowWaveBanner;
            }
        }

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();

            if (Wave.WaveManager.Instance != null)
            {
                Wave.WaveManager.Instance.OnWaveBanner -= ShowWaveBanner;
            }
        }

        private void SubscribeEvents()
        {
            if (isSubscribed) return;
            if (playerStats == null)
            {
#if UNITY_6000_0_OR_NEWER
                playerStats = Object.FindAnyObjectByType<PlayerStats>();
#else
                playerStats = Object.FindObjectOfType<PlayerStats>();
#endif
            }

            if (playerStats != null)
            {
                playerStats.OnHealthChanged += UpdateHealthUI;
                playerStats.OnExpChanged += UpdateExpUI;
                playerStats.OnLevelUp += OnPlayerLevelUp;
                playerStats.OnGoldChanged += UpdateGoldUI;
                playerStats.OnPlayerDeath += OnPlayerDeath;
                isSubscribed = true;
            }
        }

        private void UnsubscribeEvents()
        {
            if (!isSubscribed || playerStats == null) return;
            playerStats.OnHealthChanged -= UpdateHealthUI;
            playerStats.OnExpChanged -= UpdateExpUI;
            playerStats.OnLevelUp -= OnPlayerLevelUp;
            playerStats.OnGoldChanged -= UpdateGoldUI;
            playerStats.OnPlayerDeath -= OnPlayerDeath;
            isSubscribed = false;
        }

        private void Update()
        {
            if (playerStats == null && Time.frameCount % 30 == 0)
            {
                SubscribeEvents();
            }

            // Update Survival Timer HUD
            if (timerText != null && Core.GameManager.Instance != null)
            {
                float t = Core.GameManager.Instance.survivalTime;
                int mins = Mathf.FloorToInt(t / 60f);
                int secs = Mathf.FloorToInt(t % 60f);
                timerText.text = $"{mins:00}:{secs:00}";
            }

            // Update Kill Count HUD
            if (killCountText != null && playerStats != null)
            {
                killCountText.text = "💀 " + playerStats.inRunKills;
            }

            // Update Wave Title
            if (waveText != null && Wave.WaveManager.Instance != null)
            {
                waveText.text = Wave.WaveManager.Instance.currentWaveTitle;
            }
        }

        public void UpdateHealthUI(float current, float max)
        {
            if (healthBar != null)
            {
                healthBar.maxValue = max;
                healthBar.value = current;
            }
            if (hpNumText != null)
            {
                hpNumText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
            }
        }

        public void UpdateExpUI(float current, float max)
        {
            if (expBar != null)
            {
                expBar.maxValue = max;
                expBar.value = current;
            }
        }

        public void UpdateGoldUI(int gold)
        {
            if (goldText != null)
            {
                goldText.text = "💰 " + gold;
            }
        }

        private void OnPlayerLevelUp(int newLevel)
        {
            if (levelText != null)
            {
                levelText.text = "LV " + newLevel;
            }

            // Use interactive LevelUpUI with 3 cards if available
            if (levelUpUI != null)
            {
                levelUpUI.OpenLevelUp();
            }
            else if (LevelUpUI.Instance != null)
            {
                LevelUpUI.Instance.OpenLevelUp();
            }
            else
            {
#if UNITY_6000_0_OR_NEWER
                var lvl = Object.FindAnyObjectByType<LevelUpUI>();
#else
                var lvl = Object.FindObjectOfType<LevelUpUI>();
#endif
                if (lvl == null)
                {
                    GameObject go = new GameObject("LevelUpUI");
                    lvl = go.AddComponent<LevelUpUI>();
                }
                lvl.OpenLevelUp();
            }
        }

        public void CloseLevelUpPanel()
        {
            if (levelUpUI != null)
            {
                levelUpUI.CloseLevelUp();
            }
            else if (levelUpPanel != null)
            {
                levelUpPanel.SetActive(false);
                Time.timeScale = 1f;
            }
        }

        public void OnPlayerDeath()
        {
            if (runSummaryUI != null)
            {
                runSummaryUI.ShowSummary(false);
            }
            else if (RunSummaryUI.Instance != null)
            {
                RunSummaryUI.Instance.ShowSummary(false);
            }
        }

        public void ShowWaveBanner(string message)
        {
            if (bannerText != null)
            {
                StopCoroutine("AnimateBanner");
                StartCoroutine(AnimateBanner(message));
            }
        }

        private IEnumerator AnimateBanner(string message)
        {
            bannerText.text = message;
            bannerText.gameObject.SetActive(true);
            bannerText.transform.localScale = Vector3.one * 0.7f;

            // Punch animation
            float elapsed = 0f;
            while (elapsed < 0.25f)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / 0.25f;
                bannerText.transform.localScale = Vector3.Lerp(Vector3.one * 0.7f, Vector3.one * 1.15f, progress);
                yield return null;
            }
            bannerText.transform.localScale = Vector3.one;

            yield return new WaitForSecondsRealtime(3.2f);
            bannerText.gameObject.SetActive(false);
        }

        // =========================================================
        // PROCEDURAL HUD BUILDER (Ensures zero missing scene UI)
        // =========================================================
        public void EnsureHUDExists()
        {
            if (healthBar != null && expBar != null && timerText != null) return;

            // Find or create Overlay Canvas
            Canvas canvas = null;
#if UNITY_6000_0_OR_NEWER
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            var canvases = Object.FindObjectsOfType<Canvas>();
#endif
            foreach (var c in canvases)
            {
                if (c.renderMode == RenderMode.ScreenSpaceOverlay && !c.name.Contains("World") && !c.name.Contains("MainMenu"))
                {
                    canvas = c;
                    break;
                }
            }

            if (canvas == null)
            {
                GameObject canvasGO = new GameObject("InGame_HUD_Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;

                CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                canvasGO.AddComponent<GraphicRaycaster>();
            }
            hudCanvas = canvas;

            Transform hudRoot = canvas.transform.Find("HUD_Root");
            if (hudRoot == null)
            {
                GameObject rootGO = new GameObject("HUD_Root", typeof(RectTransform));
                rootGO.transform.SetParent(canvas.transform, false);
                RectTransform rootRT = rootGO.GetComponent<RectTransform>();
                rootRT.anchorMin = Vector2.zero;
                rootRT.anchorMax = Vector2.one;
                rootRT.sizeDelta = Vector2.zero;
                hudRoot = rootGO.transform;
            }

            // 1. EXP Bar (Across top edge)
            if (expBar == null)
            {
                expBar = CreateProceduralSlider(hudRoot, "ExpBar", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -10f), new Vector2(-40f, 16f), new Color(0.08f, 0.12f, 0.2f, 0.85f), new Color(0.2f, 0.75f, 1f, 1f));
            }

            // 2. Level Text (Top left above exp bar)
            if (levelText == null)
            {
                levelText = CreateProceduralText(hudRoot, "LevelText", "LV 1", 18, FontStyle.Bold, new Color(1f, 0.95f, 0.4f), TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(70f, -32f), new Vector2(120f, 30f));
            }

            // 3. Health Bar (Top Left)
            if (healthBar == null)
            {
                healthBar = CreateProceduralSlider(hudRoot, "HealthBar", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(150f, -62f), new Vector2(240f, 26f), new Color(0.25f, 0.05f, 0.05f, 0.85f), new Color(0.9f, 0.2f, 0.2f, 1f));
                hpNumText = CreateProceduralText(healthBar.transform, "HP_Text", "100 / 100", 14, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            }

            // 4. Timer Text (Top Center)
            if (timerText == null)
            {
                timerText = CreateProceduralText(hudRoot, "TimerText", "00:00", 34, FontStyle.Bold, new Color(1f, 0.88f, 0.3f), TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(200f, 40f));
            }

            // 5. Wave Text (Below Timer)
            if (waveText == null)
            {
                waveText = CreateProceduralText(hudRoot, "WaveText", "⚔️ ĐỢT 1: KHỞI NGUYÊN (SLIME)", 17, FontStyle.Bold, new Color(0.35f, 0.95f, 1f), TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -72f), new Vector2(450f, 28f));
            }

            // 6. Kills & Gold (Top Right)
            if (killCountText == null)
            {
                killCountText = CreateProceduralText(hudRoot, "KillCountText", "💀 0", 22, FontStyle.Bold, Color.white, TextAnchor.MiddleRight, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-190f, -50f), new Vector2(140f, 30f));
            }
            if (goldText == null)
            {
                goldText = CreateProceduralText(hudRoot, "GoldText", "💰 0", 22, FontStyle.Bold, new Color(1f, 0.85f, 0.2f), TextAnchor.MiddleRight, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-60f, -50f), new Vector2(120f, 30f));
            }

            // 7. Banner Text (Center)
            if (bannerText == null)
            {
                bannerText = CreateProceduralText(hudRoot, "BannerText", "", 32, FontStyle.Bold, new Color(1f, 0.9f, 0.2f), TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 130f), new Vector2(900f, 60f));
                bannerText.gameObject.SetActive(false);
            }

            // 8. Ensure LevelUpUI exists
            if (levelUpUI == null)
            {
#if UNITY_6000_0_OR_NEWER
                levelUpUI = Object.FindAnyObjectByType<LevelUpUI>();
#else
                levelUpUI = Object.FindObjectOfType<LevelUpUI>();
#endif
                if (levelUpUI == null)
                {
                    GameObject lvlGO = new GameObject("LevelUpUI");
                    levelUpUI = lvlGO.AddComponent<LevelUpUI>();
                }
            }
        }

        private Slider CreateProceduralSlider(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size, Color bgColor, Color fillColor)
        {
            GameObject sliderGO = new GameObject(name, typeof(RectTransform));
            sliderGO.transform.SetParent(parent, false);
            RectTransform rt = sliderGO.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            // Background
            GameObject bgGO = new GameObject("Background", typeof(RectTransform));
            bgGO.transform.SetParent(sliderGO.transform, false);
            RectTransform bgRT = bgGO.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;
            Image bgImg = bgGO.AddComponent<Image>();
            bgImg.color = bgColor;

            // Fill Area
            GameObject fillAreaGO = new GameObject("Fill Area", typeof(RectTransform));
            fillAreaGO.transform.SetParent(sliderGO.transform, false);
            RectTransform fillAreaRT = fillAreaGO.GetComponent<RectTransform>();
            fillAreaRT.anchorMin = Vector2.zero;
            fillAreaRT.anchorMax = Vector2.one;
            fillAreaRT.sizeDelta = Vector2.zero;

            // Fill
            GameObject fillGO = new GameObject("Fill", typeof(RectTransform));
            fillGO.transform.SetParent(fillAreaGO.transform, false);
            RectTransform fillRT = fillGO.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.sizeDelta = Vector2.zero;
            Image fillImg = fillGO.AddComponent<Image>();
            fillImg.color = fillColor;

            Slider slider = sliderGO.AddComponent<Slider>();
            slider.fillRect = fillRT;
            slider.targetGraphic = fillImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 100f;
            slider.value = 100f;

            // Add thin dark outline for retro pixel style
            Outline outline = sliderGO.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(2f, -2f);

            return slider;
        }

        private Text CreateProceduralText(Transform parent, string name, string initialText, int fontSize, FontStyle fontStyle, Color color, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
        {
            GameObject txtGO = new GameObject(name, typeof(RectTransform));
            txtGO.transform.SetParent(parent, false);
            RectTransform rt = txtGO.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Text txt = txtGO.AddComponent<Text>();
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.font = font;
            txt.fontSize = fontSize;
            txt.fontStyle = fontStyle;
            txt.color = color;
            txt.alignment = alignment;
            txt.text = initialText;

            Outline outline = txtGO.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.95f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            return txt;
        }
    }
}
