using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using VampireSurvivors.Player;
using VampireSurvivors.Weapons;

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

        [Header("Arsenal HUD Elements (Top-Left Slots)")]
        private Image[] weaponSlotBgs = new Image[WeaponInventory.MAX_WEAPONS];
        private Outline[] weaponSlotOutlines = new Outline[WeaponInventory.MAX_WEAPONS];
        private Image[] weaponSlotIcons = new Image[WeaponInventory.MAX_WEAPONS];
        private Text[] weaponSlotLevels = new Text[WeaponInventory.MAX_WEAPONS];

        private Image[] passiveSlotBgs = new Image[WeaponInventory.MAX_PASSIVES];
        private Outline[] passiveSlotOutlines = new Outline[WeaponInventory.MAX_PASSIVES];
        private Image[] passiveSlotIcons = new Image[WeaponInventory.MAX_PASSIVES];
        private Text[] passiveSlotLevels = new Text[WeaponInventory.MAX_PASSIVES];

        private Dictionary<string, Sprite> hudSpriteCache = new Dictionary<string, Sprite>();
        private WeaponInventory subscribedInventory;

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

            UpdateArsenalHUD();

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
            if (playerStats == null)
            {
#if UNITY_6000_0_OR_NEWER
                playerStats = Object.FindAnyObjectByType<PlayerStats>();
#else
                playerStats = Object.FindObjectOfType<PlayerStats>();
#endif
            }

            if (!isSubscribed && playerStats != null)
            {
                playerStats.OnHealthChanged += UpdateHealthUI;
                playerStats.OnExpChanged += UpdateExpUI;
                playerStats.OnLevelUp += OnPlayerLevelUp;
                playerStats.OnGoldChanged += UpdateGoldUI;
                playerStats.OnPlayerDeath += OnPlayerDeath;
                isSubscribed = true;
            }

            WeaponInventory inv = WeaponInventory.Instance;
            if (inv == null && playerStats != null)
            {
                inv = playerStats.GetComponent<WeaponInventory>();
            }
            if (inv != null && subscribedInventory != inv)
            {
                if (subscribedInventory != null)
                {
                    subscribedInventory.OnInventoryChanged -= UpdateArsenalHUD;
                }
                subscribedInventory = inv;
                subscribedInventory.OnInventoryChanged += UpdateArsenalHUD;
                UpdateArsenalHUD();
            }
        }

        private void UnsubscribeEvents()
        {
            if (isSubscribed && playerStats != null)
            {
                playerStats.OnHealthChanged -= UpdateHealthUI;
                playerStats.OnExpChanged -= UpdateExpUI;
                playerStats.OnLevelUp -= OnPlayerLevelUp;
                playerStats.OnGoldChanged -= UpdateGoldUI;
                playerStats.OnPlayerDeath -= OnPlayerDeath;
                isSubscribed = false;
            }

            if (subscribedInventory != null)
            {
                subscribedInventory.OnInventoryChanged -= UpdateArsenalHUD;
                subscribedInventory = null;
            }
        }

        private void Update()
        {
            if ((playerStats == null || subscribedInventory == null) && Time.frameCount % 30 == 0)
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

        public void UpdateArsenalHUD()
        {
            WeaponInventory inv = subscribedInventory != null ? subscribedInventory : WeaponInventory.Instance;

            // 1. Update Weapon Slots (4 slots)
            for (int i = 0; i < WeaponInventory.MAX_WEAPONS; i++)
            {
                if (weaponSlotBgs[i] == null) continue;

                if (inv != null && i < inv.equippedWeapons.Count)
                {
                    WeaponData w = inv.equippedWeapons[i];
                    Sprite icon = GetEquipmentIcon(w.weaponID);
                    weaponSlotIcons[i].sprite = icon;
                    weaponSlotIcons[i].enabled = (icon != null);
                    weaponSlotIcons[i].color = Color.white;

                    bool readyToEvolve = !w.isEvolved && w.level >= w.maxLevel && inv.HasPassive(w.requiredPassiveID);

                    if (w.isEvolved)
                    {
                        weaponSlotBgs[i].color = new Color(0.28f, 0.10f, 0.36f, 0.92f);
                        weaponSlotOutlines[i].effectColor = new Color(1f, 0.84f, 0.2f, 1f);
                        weaponSlotLevels[i].text = "★MAX";
                        weaponSlotLevels[i].color = new Color(1f, 0.9f, 0.3f);
                    }
                    else if (readyToEvolve)
                    {
                        weaponSlotBgs[i].color = new Color(0.12f, 0.26f, 0.16f, 0.92f);
                        weaponSlotOutlines[i].effectColor = new Color(0.35f, 1f, 0.45f, 1f);
                        weaponSlotLevels[i].text = "⚡EVO";
                        weaponSlotLevels[i].color = new Color(0.4f, 1f, 0.5f);
                    }
                    else
                    {
                        weaponSlotBgs[i].color = new Color(0.12f, 0.15f, 0.22f, 0.88f);
                        weaponSlotOutlines[i].effectColor = (w.level >= w.maxLevel)
                            ? new Color(0.95f, 0.78f, 0.25f, 0.95f)
                            : new Color(0.55f, 0.65f, 0.82f, 0.9f);
                        weaponSlotLevels[i].text = w.level >= w.maxLevel ? "MAX" : $"Lv.{w.level}";
                        weaponSlotLevels[i].color = w.level >= w.maxLevel ? new Color(1f, 0.88f, 0.3f) : Color.white;
                    }
                }
                else
                {
                    weaponSlotIcons[i].sprite = null;
                    weaponSlotIcons[i].enabled = false;
                    weaponSlotBgs[i].color = new Color(0.06f, 0.08f, 0.12f, 0.52f);
                    weaponSlotOutlines[i].effectColor = new Color(0.25f, 0.30f, 0.38f, 0.55f);
                    weaponSlotLevels[i].text = "";
                }
            }

            // 2. Update Passive Relic Slots (4 slots)
            for (int i = 0; i < WeaponInventory.MAX_PASSIVES; i++)
            {
                if (passiveSlotBgs[i] == null) continue;

                if (inv != null && i < inv.equippedPassives.Count)
                {
                    PassiveItemData p = inv.equippedPassives[i];
                    Sprite icon = GetEquipmentIcon(p.passiveID);
                    passiveSlotIcons[i].sprite = icon;
                    passiveSlotIcons[i].enabled = (icon != null);
                    passiveSlotIcons[i].color = Color.white;

                    bool isMax = p.level >= p.maxLevel;
                    passiveSlotBgs[i].color = new Color(0.16f, 0.13f, 0.09f, 0.88f);
                    passiveSlotOutlines[i].effectColor = isMax
                        ? new Color(1f, 0.82f, 0.25f, 0.95f)
                        : new Color(0.72f, 0.58f, 0.35f, 0.9f);
                    passiveSlotLevels[i].text = isMax ? "MAX" : $"Lv.{p.level}";
                    passiveSlotLevels[i].color = isMax ? new Color(1f, 0.88f, 0.3f) : new Color(0.95f, 0.92f, 0.82f);
                }
                else
                {
                    passiveSlotIcons[i].sprite = null;
                    passiveSlotIcons[i].enabled = false;
                    passiveSlotBgs[i].color = new Color(0.06f, 0.08f, 0.12f, 0.52f);
                    passiveSlotOutlines[i].effectColor = new Color(0.25f, 0.30f, 0.38f, 0.55f);
                    passiveSlotLevels[i].text = "";
                }
            }
        }

        private Sprite GetEquipmentIcon(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            string path = "";
            switch (id)
            {
                case "HolySword":
                    path = "Assets/Sprites/Weapons/Weapon_HolySword.png";
                    break;
                case "Excalibur":
                    path = "Assets/Sprites/Weapons/VFX_ExcaliburSlash.png";
                    break;
                case "Fireball":
                    path = "Assets/Sprites/Weapons/Weapon_Fireball.png";
                    break;
                case "MeteorCalamity":
                case "Meteor":
                    path = "Assets/Sprites/Weapons/VFX_Meteor.png";
                    break;
                case "ArcaneStaff":
                    path = "Assets/Sprites/Weapons/Weapon_MagicStaff.png";
                    break;
                case "CosmicSingularity":
                case "Singularity":
                    path = "Assets/Sprites/Weapons/VFX_Singularity.png";
                    break;
                case "BasicBow":
                    path = "Assets/Sprites/Weapons/Weapon_Arrow.png";
                    break;
                case "TempestArrow":
                    path = "Assets/Sprites/Weapons/VFX_TempestArrow.png";
                    break;
                case "ShadowBlade":
                    path = "Assets/Sprites/Weapons/VFX_ShadowSlash.png";
                    break;
                case "CrimsonReaper":
                    path = "Assets/Sprites/Weapons/VFX_CrimsonReaper.png";
                    break;
                case "StormBow":
                    path = "Assets/Sprites/Weapons/Weapon_StormBow.png";
                    break;
                case "AstralBarrage":
                    path = "Assets/Sprites/Weapons/VFX_AstralBarrage.png";
                    break;
                case "WindQuiver":
                    path = "Assets/Sprites/Passives/Passive_WindQuiver.png";
                    break;
                case "IronArmor":
                    path = "Assets/Sprites/Passives/Passive_IronArmor.png";
                    break;
                case "SpellTome":
                    path = "Assets/Sprites/Passives/Passive_SpellTome.png";
                    break;
                case "MagicRing":
                    path = "Assets/Sprites/Passives/Passive_MagicRing.png";
                    break;
                case "MagnetTalisman":
                    path = "Assets/Sprites/Passives/Passive_MagnetTalisman.png";
                    break;
                case "BloodChalice":
                    path = "Assets/Sprites/Passives/Passive_BloodChalice.png";
                    break;
                case "CloverOfLuck":
                case "Clover":
                    path = "Assets/Sprites/Passives/Passive_Clover.png";
                    break;
                case "GreedCoin":
                    path = "Assets/Sprites/Passives/Passive_GreedCoin.png";
                    break;
                case "CrownOfKings":
                case "Crown":
                    path = "Assets/Sprites/Passives/Passive_Crown.png";
                    break;
                case "HolyCross":
                    path = "Assets/Sprites/Passives/Passive_HolyCross.png";
                    break;
                default:
                    path = "Assets/Sprites/Passives/Passive_GreedCoin.png";
                    break;
            }

            if (hudSpriteCache.TryGetValue(path, out Sprite cached) && cached != null)
            {
                return cached;
            }

#if UNITY_EDITOR
            Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s != null)
            {
                hudSpriteCache[path] = s;
                return s;
            }
#endif
            if (File.Exists(path))
            {
                try
                {
                    byte[] data = File.ReadAllBytes(path);
                    Texture2D tex = new Texture2D(2, 2);
                    if (tex.LoadImage(data))
                    {
                        tex.filterMode = FilterMode.Point;
                        Sprite created = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 16f);
                        hudSpriteCache[path] = created;
                        return created;
                    }
                }
                catch { }
            }
            return null;
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
            if (Core.GameManager.Instance != null && !Core.GameManager.Instance.isGameOver)
            {
                Core.GameManager.Instance.GameOver();
            }
            else
            {
                RunSummaryUI summary = runSummaryUI != null ? runSummaryUI : RunSummaryUI.Instance;
                if (summary == null)
                {
#if UNITY_6000_0_OR_NEWER
                    summary = FindAnyObjectByType<RunSummaryUI>();
#else
                    summary = FindObjectOfType<RunSummaryUI>();
#endif
                    if (summary == null)
                    {
                        GameObject go = new GameObject("RunSummaryUI");
                        summary = go.AddComponent<RunSummaryUI>();
                    }
                }
                if (summary != null)
                {
                    summary.ShowSummary(false);
                }
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
            if (healthBar != null && expBar != null && timerText != null && weaponSlotBgs[0] != null) return;

            // Find or create Overlay Canvas
            Canvas canvas = null;
#if UNITY_6000_0_OR_NEWER
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);
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

            // 3b. Arsenal Bar (4 Weapon Slots + 4 Passive Relic Slots below Health Bar)
            BuildArsenalBarHUD(hudRoot);

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
                bannerText = CreateProceduralText(hudRoot, "BannerText", "", 26, FontStyle.Bold, new Color(1f, 0.9f, 0.2f), TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 130f), new Vector2(1100f, 90f));
                bannerText.horizontalOverflow = HorizontalWrapMode.Wrap;
                bannerText.verticalOverflow = VerticalWrapMode.Overflow;
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

        private void BuildArsenalBarHUD(Transform hudRoot)
        {
            if (weaponSlotBgs[0] != null) return;

            Transform existing = hudRoot.Find("Arsenal_HUD_Container");
            if (existing != null) Destroy(existing.gameObject);

            GameObject containerGO = new GameObject("Arsenal_HUD_Container", typeof(RectTransform));
            containerGO.transform.SetParent(hudRoot, false);
            RectTransform containerRT = containerGO.GetComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0f, 1f);
            containerRT.anchorMax = new Vector2(0f, 1f);
            containerRT.pivot = new Vector2(0f, 1f);
            containerRT.anchoredPosition = new Vector2(30f, -86f);
            containerRT.sizeDelta = new Vector2(240f, 108f);

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Row 1: 4 Weapon Slots
            for (int i = 0; i < WeaponInventory.MAX_WEAPONS; i++)
            {
                CreateEquipmentSlotUI(
                    containerGO.transform,
                    $"WeaponSlot_{i}",
                    new Vector2(i * 58f, 0f),
                    font,
                    out weaponSlotBgs[i],
                    out weaponSlotOutlines[i],
                    out weaponSlotIcons[i],
                    out weaponSlotLevels[i]
                );
            }

            // Row 2: 4 Passive Relic Slots
            for (int i = 0; i < WeaponInventory.MAX_PASSIVES; i++)
            {
                CreateEquipmentSlotUI(
                    containerGO.transform,
                    $"PassiveSlot_{i}",
                    new Vector2(i * 58f, -52f),
                    font,
                    out passiveSlotBgs[i],
                    out passiveSlotOutlines[i],
                    out passiveSlotIcons[i],
                    out passiveSlotLevels[i]
                );
            }
        }

        private void CreateEquipmentSlotUI(
            Transform parent,
            string name,
            Vector2 anchoredPos,
            Font font,
            out Image slotBg,
            out Outline slotOutline,
            out Image slotIcon,
            out Text slotLevel)
        {
            GameObject slotGO = new GameObject(name, typeof(RectTransform));
            slotGO.transform.SetParent(parent, false);
            RectTransform rt = slotGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(48f, 46f);

            slotBg = slotGO.AddComponent<Image>();
            slotBg.color = new Color(0.06f, 0.08f, 0.12f, 0.52f);
            slotBg.raycastTarget = false;

            slotOutline = slotGO.AddComponent<Outline>();
            slotOutline.effectColor = new Color(0.25f, 0.30f, 0.38f, 0.55f);
            slotOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // Inner Icon
            GameObject iconGO = new GameObject("Icon", typeof(RectTransform));
            iconGO.transform.SetParent(slotGO.transform, false);
            RectTransform iconRT = iconGO.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.pivot = new Vector2(0.5f, 0.5f);
            iconRT.anchoredPosition = new Vector2(0f, 3f);
            iconRT.sizeDelta = new Vector2(32f, 32f);

            slotIcon = iconGO.AddComponent<Image>();
            slotIcon.preserveAspect = true;
            slotIcon.raycastTarget = false;
            slotIcon.enabled = false;

            // Level Badge at bottom-right of slot
            GameObject lvlGO = new GameObject("LevelText", typeof(RectTransform));
            lvlGO.transform.SetParent(slotGO.transform, false);
            RectTransform lvlRT = lvlGO.GetComponent<RectTransform>();
            lvlRT.anchorMin = new Vector2(0f, 0f);
            lvlRT.anchorMax = new Vector2(1f, 0f);
            lvlRT.pivot = new Vector2(0.5f, 0f);
            lvlRT.anchoredPosition = new Vector2(0f, 1f);
            lvlRT.sizeDelta = new Vector2(0f, 14f);

            slotLevel = lvlGO.AddComponent<Text>();
            slotLevel.font = font;
            slotLevel.fontSize = 10;
            slotLevel.fontStyle = FontStyle.Bold;
            slotLevel.alignment = TextAnchor.LowerCenter;
            slotLevel.color = Color.white;
            slotLevel.raycastTarget = false;
            slotLevel.text = "";

            Outline txtOutline = lvlGO.AddComponent<Outline>();
            txtOutline.effectColor = new Color(0f, 0f, 0f, 0.95f);
            txtOutline.effectDistance = new Vector2(1f, -1f);
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
