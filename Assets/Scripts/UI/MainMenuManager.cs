using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VampireSurvivors.Audio;
using VampireSurvivors.Core;
using VampireSurvivors.Player;

namespace VampireSurvivors.UI
{
    /// <summary>
    /// Master UI Manager for the Title Screen, Hero Selection, Map Selection, and Options menu.
    /// Uses invisible click hitboxes over pre-rendered artwork buttons to preserve art integrity.
    /// Equipped with UI Juice micro-animations: hover scale, click bounce, idle breathing, and modal pop-ins.
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        public static MainMenuManager Instance;

        [System.Serializable]
        public class HeroData
        {
            public string heroKey;
            public string heroName;
            public string weaponName;
            public int hp;
            public float speed;
            public int mightPercent;
            public int armor;
            public string passiveDescription;
            public Sprite portraitSprite;
        }

        [System.Serializable]
        public class MapData
        {
            public string mapName;
            public string difficultyStars;
            public string goldMultiplier;
            public string monstersDescription;
            public string hazardDescription;
            public Sprite mapPreviewSprite;
            public float goldBonusValue;
        }

        [Header("State")]
        public bool isMenuOpen = true;
        public int selectedHeroIndex = 0;
        public int selectedMapIndex = 0;

        [Header("Hero Catalog")]
        public List<HeroData> heroes = new List<HeroData>();

        [Header("Map Catalog")]
        public List<MapData> maps = new List<MapData>();

        // Internal UI references
        private Canvas menuCanvas;
        private GameObject titlePanel;
        private GameObject heroSelectModal;
        private GameObject mapSelectModal;
        private GameObject optionsModal;

        private RectTransform heroFrameRect;
        private RectTransform mapFrameRect;
        private RectTransform optionsFrameRect;

        // Hero Select UI references
        private GameObject portraitBoxGO;
        private GameObject infoBoxGO;
        private Image imgHeroPortrait;
        private Text txtHeroName;
        private Text txtHeroWeapon;
        private Text txtHeroStats;
        private Text txtHeroPassive;
        private List<GameObject> heroSlotIndicators = new List<GameObject>();
        private List<GameObject> heroSlotButtons = new List<GameObject>();

        // Map Select UI references
        private GameObject mapPreviewBoxGO;
        private GameObject detailsBoxGO;
        private Image imgMapPreview;
        private Text txtMapTitle;
        private Text txtMapDifficulty;
        private Text txtMapDescription;

        // Options UI references
        private Slider sliderMusic;
        private Slider sliderSfx;
        private Text txtMusicValue;
        private Text txtSfxValue;
        private Toggle toggleMute;

        // Cached UI Button Sprites
        private Sprite sprBtnStart;
        private Sprite sprBtnHeroes;
        private Sprite sprBtnMaps;
        private Sprite sprBtnOptions;
        private Sprite sprBtnConfirm;
        private Sprite sprBtnClose;
        private Sprite sprArrowLeft;
        private Sprite sprArrowRight;
        private Sprite sprBtnShop;

        private Dictionary<GameObject, Coroutine> runningModalAnims = new Dictionary<GameObject, Coroutine>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            EnsureEventSystem();
            LoadUISprites();
            SetupHeroData();
            SetupMapData();

#if UNITY_EDITOR
            // Automatically turn off Gizmos in Game View so the white wireframe sphere/crosshair doesn't cover UI
            try
            {
                var gameViewType = System.Type.GetType("UnityEditor.GameView, UnityEditor");
                if (gameViewType != null)
                {
                    var showGizmosProp = gameViewType.GetProperty("showGizmos", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var activeGameView = UnityEditor.EditorWindow.GetWindow(gameViewType, false, null, false);
                    if (activeGameView != null && showGizmosProp != null)
                    {
                        showGizmosProp.SetValue(activeGameView, false);
                    }
                }
            }
            catch { }
#endif
        }

        private void LoadUISprites()
        {
            sprBtnStart = LoadSpriteAtPath("Assets/Sprites/UI/Btn_Start.png");
            sprBtnHeroes = LoadSpriteAtPath("Assets/Sprites/UI/Btn_Heroes.png");
            sprBtnMaps = LoadSpriteAtPath("Assets/Sprites/UI/Btn_Maps.png");
            sprBtnOptions = LoadSpriteAtPath("Assets/Sprites/UI/Btn_Options.png");
            sprBtnConfirm = LoadSpriteAtPath("Assets/Sprites/UI/Btn_Confirm.png");
            sprBtnClose = LoadSpriteAtPath("Assets/Sprites/UI/Btn_Close.png");
            sprArrowLeft = LoadSpriteAtPath("Assets/Sprites/UI/Arrow_Left.png");
            sprArrowRight = LoadSpriteAtPath("Assets/Sprites/UI/Arrow_Right.png");
            sprBtnShop = LoadSpriteAtPath("Assets/Sprites/UI/Btn_Shop.png");
        }

        private void Start()
        {
            EnsureEventSystem();
            selectedHeroIndex = Mathf.Clamp(PlayerPrefs.GetInt("SelectedHeroIndex", 0), 0, heroes.Count - 1);
            selectedMapIndex = Mathf.Clamp(PlayerPrefs.GetInt("SelectedMapIndex", 0), 0, maps.Count - 1);

            BuildMenuUI();

            // Setup in-game background terrain with the currently selected map
            ApplySelectedMapToEnvironment();

            if (PlayerPrefs.GetInt("AutoStartBattle", 0) == 1)
            {
                PlayerPrefs.SetInt("AutoStartBattle", 0);
                PlayerPrefs.Save();
                StartBattle();
            }
            else
            {
                Time.timeScale = 0f;
                isMenuOpen = true;
            }
        }

        private void Update()
        {
            if (!isMenuOpen) return;

            // Keyboard navigation shortcuts
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                if ((heroSelectModal == null || !heroSelectModal.activeSelf) &&
                    (mapSelectModal == null || !mapSelectModal.activeSelf) &&
                    (optionsModal == null || !optionsModal.activeSelf))
                {
                    StartBattle();
                }
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                ShopUI.GetOrCreate().OpenShop();
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                if (heroSelectModal != null) OpenHeroSelect(!heroSelectModal.activeSelf);
            }
            if (Input.GetKeyDown(KeyCode.M))
            {
                if (mapSelectModal != null) OpenMapSelect(!mapSelectModal.activeSelf);
            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                if (optionsModal != null) OpenOptions(!optionsModal.activeSelf);
            }
            if (heroSelectModal != null && heroSelectModal.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) PrevHero();
                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) NextHero();
                for (int k = 0; k < heroes.Count && k < 9; k++)
                {
                    if (Input.GetKeyDown(KeyCode.Alpha1 + k) || Input.GetKeyDown(KeyCode.Keypad1 + k))
                    {
                        SelectHero(k);
                    }
                }
            }
            else if (mapSelectModal != null && mapSelectModal.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) PrevMap();
                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) NextMap();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (heroSelectModal != null && heroSelectModal.activeSelf) OpenHeroSelect(false);
                else if (mapSelectModal != null && mapSelectModal.activeSelf) OpenMapSelect(false);
                else if (optionsModal != null && optionsModal.activeSelf) OpenOptions(false);
            }
        }

        /// <summary>
        /// Guarantees that an EventSystem with a functional InputModule exists in the scene so clicks and hovers work.
        /// </summary>
        private void EnsureEventSystem()
        {
            UnityEngine.EventSystems.EventSystem es = FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (es == null)
            {
                GameObject esGO = new GameObject("EventSystem");
                es = esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();

                bool moduleAdded = false;
#if ENABLE_INPUT_SYSTEM
                try
                {
                    System.Type inputSystemUIModuleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                    if (inputSystemUIModuleType != null)
                    {
                        var module = esGO.AddComponent(inputSystemUIModuleType);
                        var assignMethod = inputSystemUIModuleType.GetMethod("AssignDefaultActions");
                        if (assignMethod != null) assignMethod.Invoke(module, null);
                        moduleAdded = true;
                        Debug.Log("<color=green>[MainMenuManager]</color> Created EventSystem with InputSystemUIInputModule.");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("[MainMenuManager] InputSystemUIInputModule init failed: " + ex.Message);
                }
#endif
                if (!moduleAdded)
                {
                    esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                    Debug.Log("<color=green>[MainMenuManager]</color> Created EventSystem with StandaloneInputModule.");
                }

                DontDestroyOnLoad(esGO);
            }
            else
            {
                UnityEngine.EventSystems.BaseInputModule bim = es.GetComponent<UnityEngine.EventSystems.BaseInputModule>();
                if (bim == null)
                {
                    bool moduleAdded = false;
#if ENABLE_INPUT_SYSTEM
                    try
                    {
                        System.Type inputSystemUIModuleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                        if (inputSystemUIModuleType != null)
                        {
                            var module = es.gameObject.AddComponent(inputSystemUIModuleType);
                            var assignMethod = inputSystemUIModuleType.GetMethod("AssignDefaultActions");
                            if (assignMethod != null) assignMethod.Invoke(module, null);
                            moduleAdded = true;
                        }
                    }
                    catch { }
#endif
                    if (!moduleAdded)
                    {
                        es.gameObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                    }
                }
            }
        }

        private void SetupHeroData()
        {
            heroes.Clear();

            heroes.Add(new HeroData
            {
                heroKey = "Knight",
                heroName = "SIR ROLAND - HIỆP SĨ THÁNH",
                weaponName = "Thánh Kiếm (Holy Broadsword)",
                hp = 120,
                speed = 5.0f,
                mightPercent = 20,
                armor = 2,
                passiveDescription = "Lưỡi kiếm hình bán nguyệt quét sạch quái phía trước. Tăng 20% phạm vi sát thương và khả năng chống chịu cực cao."
            });

            heroes.Add(new HeroData
            {
                heroKey = "ShadowKnight",
                heroName = "VALEN - SÁT THỦ BÓNG ĐÊM",
                weaponName = "Lưỡi Đao Hắc Ám (Dark Crescent)",
                hp = 90,
                speed = 6.2f,
                mightPercent = 30,
                armor = 0,
                passiveDescription = "Vung lưỡi đao bóng đêm tím xuyên giáp cực nhanh. Tốc độ di chuyển phi phàm giúp né đòn quái vật linh hoạt."
            });

            heroes.Add(new HeroData
            {
                heroKey = "Mage",
                heroName = "IGNIS - PHÁP SƯ NGUYÊN TỐ",
                weaponName = "Cầu Lửa Hỏa Ngục (Fireball)",
                hp = 80,
                speed = 4.8f,
                mightPercent = 40,
                armor = 0,
                passiveDescription = "Phóng cầu lửa tự tìm mục tiêu và phát nổ diện rộng, thiêu rụi toàn bộ bầy quái vật xung quanh."
            });

            heroes.Add(new HeroData
            {
                heroKey = "Archmage",
                heroName = "AURELIUS - ĐẠI PHÁP SƯ SÉT",
                weaponName = "Gậy Thần Bão Tố (Lightning Staff)",
                hp = 85,
                speed = 5.0f,
                mightPercent = 35,
                armor = 1,
                passiveDescription = "Triệu hồi các tia sấm sét uy lực từ trên trời giáng thẳng xuống đầu kẻ địch nguy hiểm nhất."
            });

            heroes.Add(new HeroData
            {
                heroKey = "Ranger",
                heroName = "SYLPH - CUNG THỦ BÃO GIÓ",
                weaponName = "Cung Tên Bắn Tỉa (Wind Bow)",
                hp = 95,
                speed = 5.6f,
                mightPercent = 25,
                armor = 1,
                passiveDescription = "Bắn liên hoàn các mũi tên tầm cực xa xuyên thủng hàng ngũ quái vật, tốc độ bắn siêu nhanh."
            });

            heroes.Add(new HeroData
            {
                heroKey = "Archer",
                heroName = "AERIS - XẠ THỦ THẦN PHONG",
                weaponName = "Cung Thần Phong (Storm Bow)",
                hp = 90,
                speed = 5.8f,
                mightPercent = 35,
                armor = 1,
                passiveDescription = "Bắn liên hoàn các mũi tên ma thuật bão tố cuồng phong xuyên thấu kẻ địch với tốc độ bắn cực nhanh."
            });

            heroes[0].portraitSprite = LoadHeroPortrait("Assets/Sprites/Characters/Hero_Knight_Idle.png");
            heroes[1].portraitSprite = LoadHeroPortrait("Assets/Sprites/Characters/Hero_Rogue_Idle.png");
            heroes[2].portraitSprite = LoadHeroPortrait("Assets/Sprites/Characters/Hero_Mage_Idle.png");
            heroes[3].portraitSprite = LoadHeroPortrait("Assets/Sprites/Characters/Hero_Archmage_Idle.png");
            heroes[4].portraitSprite = LoadHeroPortrait("Assets/Sprites/Characters/Hero_Ranger_Idle.png");
            heroes[5].portraitSprite = LoadHeroPortrait("Assets/Sprites/Characters/Hero_Archer_Idle.png");
        }

        private void SetupMapData()
        {
            maps.Clear();

            maps.Add(new MapData
            {
                mapName = "EMERALD MEADOW (RỪNG CỎ XANH)",
                difficultyStars = "⭐⭐☆☆☆ (Tiêu Chuẩn)",
                goldMultiplier = "VÀNG x1.0 | EXP x1.0",
                monstersDescription = "Quái vật: Slime Xanh, Dơi Rừng, Hồn Ma Cổ Xưa",
                hazardDescription = "Địa hình rộng rãi, bằng phẳng. Rất thích hợp để làm quen bảng kỹ năng và thử nghiệm build.",
                goldBonusValue = 1.0f
            });

            maps.Add(new MapData
            {
                mapName = "SCORCHING DESERT (SA MẠC BÃO CÁT)",
                difficultyStars = "⭐⭐⭐☆☆ (Thử Thách Vừa)",
                goldMultiplier = "VÀNG x1.3 | EXP x1.2",
                monstersDescription = "Quái vật: Bọ Cạp Sa Mạc, Xác Ướp Lăng Mộ, Giun Cát Cổ Đại",
                hazardDescription = "Bão cát cuộn xoáy định kỳ làm giảm 25% tầm nhìn, bãi cát lún làm giảm tốc độ di chuyển nhưng rơi nhiều Vàng hơn 30%!",
                goldBonusValue = 1.3f
            });

            maps.Add(new MapData
            {
                mapName = "CASTLE RUINS (TÀN TÍCH LÂU ĐÀI)",
                difficultyStars = "⭐⭐⭐⭐☆ (Thử Thách / Khó)",
                goldMultiplier = "VÀNG x1.5 | EXP x1.3",
                monstersDescription = "Quái vật: Binh Đoàn Bộ Xương, Đội Quân Yêu Tinh Goblin",
                hazardDescription = "Hành lang đá cổ hẹp, quái vật kéo đến với số lượng đông hơn 30%. Tăng 50% tiền vàng thu được!",
                goldBonusValue = 1.5f
            });

            maps.Add(new MapData
            {
                mapName = "INFERNAL VOLCANO (NÚI LỬA ĐỊA NGỤC)",
                difficultyStars = "⭐⭐⭐⭐⭐ (Ác Mộng / Ác Chiến)",
                goldMultiplier = "VÀNG x2.5 | EXP x2.0",
                monstersDescription = "Quái vật: Boss Golem Nham Thạch, Quái Hỏa Ngục Hung Tợn",
                hazardDescription = "Vùng đất nham thạch rực lửa. Quái vật trâu hơn và hung hãn gấp đôi, nhưng rơi gấp 2.5 lần Vàng!",
                goldBonusValue = 2.5f
            });

            maps[0].mapPreviewSprite = LoadSpriteAtPath("Assets/Sprites/Maps/Map_EmeraldMeadow.png");
            maps[1].mapPreviewSprite = LoadSpriteAtPath("Assets/Sprites/Maps/Map_ScorchingDesert.png") ?? maps[0].mapPreviewSprite;
            maps[2].mapPreviewSprite = LoadSpriteAtPath("Assets/Sprites/Maps/Map_CastleRuins.png") ?? maps[0].mapPreviewSprite;
            maps[3].mapPreviewSprite = LoadSpriteAtPath("Assets/Sprites/Maps/Map_InfernalVolcano.png") ?? maps[0].mapPreviewSprite;
        }

        private Sprite LoadHeroPortrait(string idlePath)
        {
            if (string.IsNullOrEmpty(idlePath)) return null;

#if UNITY_EDITOR
            var allAssets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(idlePath);
            foreach (var asset in allAssets)
            {
                if (asset is Sprite s)
                {
                    return s;
                }
            }
#endif
            return LoadSingleFrameSprite(idlePath);
        }

        private Sprite LoadSingleFrameSprite(string path)
        {
#if UNITY_EDITOR
            var allAssets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var asset in allAssets)
            {
                if (asset is Sprite s)
                {
                    return s;
                }
            }
#endif
            if (System.IO.File.Exists(path))
            {
                try
                {
                    byte[] data = System.IO.File.ReadAllBytes(path);
                    Texture2D fileTex = new Texture2D(2, 2);
                    if (fileTex.LoadImage(data))
                    {
                        fileTex.filterMode = FilterMode.Point;
                        int frameCount = Mathf.Max(1, Mathf.RoundToInt((float)fileTex.width / fileTex.height));
                        int frameWidth = fileTex.width / frameCount;
                        return Sprite.Create(fileTex, new Rect(0, 0, frameWidth, fileTex.height), new Vector2(0.5f, 0.5f), 16f);
                    }
                }
                catch { }
            }
            return null;
        }

        private Sprite LoadSpriteAtPath(string path)
        {
#if UNITY_EDITOR
            Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s != null) return s;

            Texture2D tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null)
            {
                tex.filterMode = FilterMode.Bilinear;
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            }
#endif
            if (System.IO.File.Exists(path))
            {
                try
                {
                    byte[] data = System.IO.File.ReadAllBytes(path);
                    Texture2D fileTex = new Texture2D(2, 2);
                    if (fileTex.LoadImage(data))
                    {
                        fileTex.filterMode = FilterMode.Bilinear;
                        return Sprite.Create(fileTex, new Rect(0, 0, fileTex.width, fileTex.height), new Vector2(0.5f, 0.5f), 100f);
                    }
                }
                catch { }
            }
            return null;
        }

        // =========================================================
        // UI CONSTRUCTION & LAYOUT
        // =========================================================
        private void BuildMenuUI()
        {
            GameObject canvasGO = new GameObject("MainMenu_Canvas");
            menuCanvas = canvasGO.AddComponent<Canvas>();
            menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            menuCanvas.sortingOrder = 500;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 1. TITLE PANEL
            titlePanel = CreateUIObject("TitleScreen_Panel", canvasGO.transform);
            RectTransform titleRect = titlePanel.GetComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            Image bgImg = titlePanel.AddComponent<Image>();
#if UNITY_EDITOR
            Sprite titleBg = LoadSpriteAtPath("Assets/Sprites/UI/TitleScreen_Background.png");
            if (titleBg != null) bgImg.sprite = titleBg;
            else bgImg.color = new Color(0.08f, 0.08f, 0.12f, 1f);
#else
            bgImg.color = new Color(0.08f, 0.08f, 0.12f, 1f);
#endif
            bgImg.type = Image.Type.Simple;
            bgImg.raycastTarget = false; // Do NOT block button hitboxes!

            // INTERACTIVE BUTTONS WITH REAL SPRITES AND JUICE ANIMATIONS
            // 1. START Button: Breathing Pulse + Subtle Floating Motion + Hover Pop-up!
            GameObject btnStart = CreateJuicyButton("Btn_StartBattle", titlePanel.transform, new Vector2(0, -90), new Vector2(480, 240), StartBattle, sprBtnStart, 1.07f, 0.93f, true);
            UIButtonJuice startJuice = btnStart.GetComponent<UIButtonJuice>();
            startJuice.enableFloating = true;
            startJuice.floatingDistance = 4f;
            startJuice.floatingSpeed = 2.0f;

            // 2. Large Bottom Plaques: HEROES, MAPS, OPTIONS -> Hover scale + Click compression
            CreateJuicyButton("Btn_Heroes", titlePanel.transform, new Vector2(-480, -380), new Vector2(400, 190), () => OpenHeroSelect(true), sprBtnHeroes, 1.07f, 0.93f);
            CreateJuicyButton("Btn_Maps", titlePanel.transform, new Vector2(0, -380), new Vector2(400, 190), () => OpenMapSelect(true), sprBtnMaps, 1.07f, 0.93f);
            CreateJuicyButton("Btn_Options", titlePanel.transform, new Vector2(480, -380), new Vector2(400, 175), () => OpenOptions(true), sprBtnOptions, 1.07f, 0.93f);

            // 3. Shop Button on Title Screen (Top right corner, uses custom wooden SHOP sign or royal gold plaque)
            GameObject btnShop = CreateUIObject("Btn_ShopTitle", titlePanel.transform);
            RectTransform shopRT = btnShop.GetComponent<RectTransform>();
            shopRT.anchoredPosition = new Vector2(720, 440);

            Image shopImg = btnShop.AddComponent<Image>();
            if (sprBtnShop != null)
            {
                shopRT.sizeDelta = new Vector2(300, 92);
                shopImg.sprite = sprBtnShop;
                shopImg.color = Color.white;
                shopImg.preserveAspect = true;
            }
            else
            {
                shopRT.sizeDelta = new Vector2(280, 68);
                shopImg.color = new Color(0.12f, 0.09f, 0.16f, 0.96f);

                Outline shopOutline = btnShop.AddComponent<Outline>();
                shopOutline.effectColor = new Color(0.95f, 0.78f, 0.25f, 1f);
                shopOutline.effectDistance = new Vector2(2.5f, -2.5f);

                // Inner Bronze Inset Frame
                GameObject innerPlaque = CreateUIObject("InnerPlaque", btnShop.transform);
                RectTransform innerRT = innerPlaque.GetComponent<RectTransform>();
                innerRT.anchorMin = Vector2.zero;
                innerRT.anchorMax = Vector2.one;
                innerRT.offsetMin = new Vector2(4f, 4f);
                innerRT.offsetMax = new Vector2(-4f, -4f);

                Image innerImg = innerPlaque.AddComponent<Image>();
                innerImg.color = new Color(0.18f, 0.14f, 0.24f, 0.95f);
                innerImg.raycastTarget = false;

                Outline innerOutline = innerPlaque.AddComponent<Outline>();
                innerOutline.effectColor = new Color(0.55f, 0.40f, 0.15f, 0.85f);
                innerOutline.effectDistance = new Vector2(1.5f, -1.5f);

                GameObject shopTxtGO = CreateUIObject("Text", btnShop.transform);
                RectTransform shopTxtRT = shopTxtGO.GetComponent<RectTransform>();
                shopTxtRT.anchorMin = Vector2.zero;
                shopTxtRT.anchorMax = Vector2.one;
                shopTxtRT.offsetMin = Vector2.zero;
                shopTxtRT.offsetMax = Vector2.zero;

                Text shopTxt = shopTxtGO.AddComponent<Text>();
                shopTxt.font = defaultFont;
                shopTxt.fontSize = 20;
                shopTxt.fontStyle = FontStyle.Bold;
                shopTxt.alignment = TextAnchor.MiddleCenter;
                shopTxt.color = new Color(1f, 0.92f, 0.35f);
                shopTxt.text = "💰 CỬA HÀNG [B]";

                Outline txtOutline = shopTxtGO.AddComponent<Outline>();
                txtOutline.effectColor = new Color(0f, 0f, 0f, 0.9f);
                txtOutline.effectDistance = new Vector2(1.5f, -1.5f);
            }

            Button shopBtn = btnShop.AddComponent<Button>();
            shopBtn.onClick.AddListener(() => ShopUI.GetOrCreate().OpenShop());

            UIButtonJuice shopJuice = btnShop.AddComponent<UIButtonJuice>();
            shopJuice.hoverScale = 1.08f;
            shopJuice.pressScale = 0.92f;

            // 4. HERO SELECT MODAL
            BuildHeroSelectModal(canvasGO.transform, defaultFont);

            // 3. MAP SELECT MODAL
            BuildMapSelectModal(canvasGO.transform, defaultFont);

            // 4. OPTIONS MODAL
            BuildOptionsModal(canvasGO.transform, defaultFont);
        }

        private GameObject CreateJuicyButton(string name, Transform parent, Vector2 anchoredPos, Vector2 size, UnityEngine.Events.UnityAction onClick, float hoverScale = 1.07f, float pressScale = 0.93f, bool breathing = false)
        {
            return CreateJuicyButton(name, parent, anchoredPos, size, onClick, null, hoverScale, pressScale, breathing);
        }

        private GameObject CreateJuicyButton(string name, Transform parent, Vector2 anchoredPos, Vector2 size, UnityEngine.Events.UnityAction onClick, Sprite sprite, float hoverScale = 1.07f, float pressScale = 0.93f, bool breathing = false)
        {
            GameObject go = CreateUIObject(name, parent);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Image img = go.AddComponent<Image>();
            if (sprite != null)
            {
                img.sprite = sprite;
                img.color = Color.white;
                img.preserveAspect = true;
            }
            else
            {
                // Use tiny non-zero alpha so CanvasRenderer registers it as active and raycastable, but visually 100% transparent
                img.color = new Color(1f, 1f, 1f, 0.002f);
            }
            img.raycastTarget = true;

            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            ColorBlock cb = btn.colors;
            if (sprite != null)
            {
                cb.normalColor = Color.white;
                cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
                cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            }
            else
            {
                cb.normalColor = Color.clear;
                cb.highlightedColor = new Color(1f, 0.95f, 0.7f, 0.12f); // Subtle glow on hover
                cb.pressedColor = new Color(1f, 0.9f, 0.5f, 0.22f);
            }
            btn.colors = cb;

            btn.onClick.AddListener(onClick);

            // Attach juice component without double-firing clicks
            UIButtonJuice juice = go.AddComponent<UIButtonJuice>();
            juice.onClickAction = null; // Handled exclusively by Button.onClick to prevent double-click jumps
            juice.hoverScale = hoverScale;
            juice.pressScale = pressScale;
            juice.enableBreathing = breathing;
            if (breathing)
            {
                juice.breathingSpeed = 2.4f;
                juice.breathingAmount = 0.035f;
            }

            return go;
        }

        // =========================================================
        // HERO SELECTION WINDOW
        // =========================================================
        private void BuildHeroSelectModal(Transform parent, Font font)
        {
            heroSelectModal = CreateUIObject("Modal_HeroSelect", parent);
            RectTransform modalRect = heroSelectModal.GetComponent<RectTransform>();
            modalRect.anchorMin = Vector2.zero;
            modalRect.anchorMax = Vector2.one;
            modalRect.offsetMin = Vector2.zero;
            modalRect.offsetMax = Vector2.zero;

            Image backdrop = heroSelectModal.AddComponent<Image>();
            backdrop.color = new Color(0f, 0f, 0f, 0.75f);

            // Frame Image
            GameObject frameGO = CreateUIObject("HeroFrame", heroSelectModal.transform);
            heroFrameRect = frameGO.GetComponent<RectTransform>();
            heroFrameRect.anchoredPosition = Vector2.zero;
            heroFrameRect.sizeDelta = new Vector2(1200, 670);

            Image frameImg = frameGO.AddComponent<Image>();
            Sprite heroFrameSprite = LoadSpriteAtPath("Assets/Sprites/UI/Frame_HeroSelection.png");
            if (heroFrameSprite != null) frameImg.sprite = heroFrameSprite;
            else frameImg.color = new Color(0.12f, 0.1f, 0.08f, 0.95f);

            // Left Box: Hero Portrait Preview (Stable, centered inside left carved frame, no breathing to preserve pixel art)
            portraitBoxGO = CreateUIObject("PortraitBox", frameGO.transform);
            RectTransform pBoxRect = portraitBoxGO.GetComponent<RectTransform>();
            pBoxRect.anchoredPosition = new Vector2(-252, 22);
            pBoxRect.sizeDelta = new Vector2(240, 260);

            // Aura glow behind portrait
            GameObject glowGO = CreateUIObject("PortraitGlow", portraitBoxGO.transform);
            RectTransform glowRect = glowGO.GetComponent<RectTransform>();
            glowRect.anchorMin = Vector2.zero;
            glowRect.anchorMax = Vector2.one;
            glowRect.sizeDelta = new Vector2(20, 20);
            Outline glowOl = glowGO.AddComponent<Outline>();
            glowOl.effectDistance = new Vector2(4f, 4f);
            UIButtonJuice glowJuice = glowGO.AddComponent<UIButtonJuice>();
            glowJuice.enableColorPulse = true;
            glowJuice.pulseColorA = new Color(1f, 0.85f, 0.2f, 0.25f);
            glowJuice.pulseColorB = new Color(1f, 0.98f, 0.5f, 0.75f);
            glowJuice.pulseSpeed = 2.6f;

            imgHeroPortrait = portraitBoxGO.AddComponent<Image>();
            imgHeroPortrait.preserveAspect = true;
            imgHeroPortrait.raycastTarget = false;

            // Gentle floating on portrait (Breathing disabled to avoid blurring pixel art)
            UIButtonJuice portraitJuice = portraitBoxGO.AddComponent<UIButtonJuice>();
            portraitJuice.enableFloating = true;
            portraitJuice.floatingDistance = 3.5f;
            portraitJuice.floatingSpeed = 2.0f;
            portraitJuice.enableBreathing = false;
            portraitJuice.hoverScale = 1.05f;

            // Right Box: Dynamic Text Form inside parchment area - Centered & Aligned with Golden Frame
            infoBoxGO = CreateUIObject("InfoBox", frameGO.transform);
            RectTransform infoRect = infoBoxGO.GetComponent<RectTransform>();
            infoRect.anchoredPosition = new Vector2(248, 30);
            infoRect.sizeDelta = new Vector2(450, 300);

            UIButtonJuice infoJuice = infoBoxGO.AddComponent<UIButtonJuice>();
            infoJuice.hoverScale = 1.0f; // Punches on hero switch

            VerticalLayoutGroup vlg = infoBoxGO.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8f;
            vlg.padding = new RectOffset(18, 18, 16, 14);
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            txtHeroName = CreateText("TxtHeroName", infoBoxGO.transform, "SIR ROLAND - HIỆP SĨ THÁNH", 23, FontStyle.Bold, new Color(1f, 0.88f, 0.35f), TextAnchor.UpperLeft, font);
            txtHeroName.GetComponent<RectTransform>().sizeDelta = new Vector2(414, 30);
            AddOutline(txtHeroName.gameObject, new Color(0.12f, 0.06f, 0.02f, 1f), 2f);

            txtHeroWeapon = CreateText("TxtHeroWeapon", infoBoxGO.transform, "Vũ khí khởi đầu: Thánh Kiếm (Holy Broadsword)", 16, FontStyle.Bold, new Color(0.42f, 0.78f, 1f), TextAnchor.UpperLeft, font);
            txtHeroWeapon.GetComponent<RectTransform>().sizeDelta = new Vector2(414, 24);
            AddOutline(txtHeroWeapon.gameObject, new Color(0.04f, 0.08f, 0.16f, 1f), 1.5f);

            txtHeroStats = CreateText("TxtHeroStats", infoBoxGO.transform, "Máu: 120       •   Tốc độ: 5.0\nSát thương: +20%   •   Giáp: +2", 15, FontStyle.Bold, new Color(0.96f, 0.94f, 0.90f), TextAnchor.UpperLeft, font);
            txtHeroStats.GetComponent<RectTransform>().sizeDelta = new Vector2(414, 46);
            AddOutline(txtHeroStats.gameObject, Color.black, 1.5f);

            txtHeroPassive = CreateText("TxtHeroPassive", infoBoxGO.transform, "Đặc tính: Lưỡi kiếm hình bán nguyệt quét sạch quái phía trước. Tăng 20% phạm vi sát thương và khả năng chống chịu cực cao.", 15, FontStyle.Italic, new Color(0.92f, 0.88f, 0.80f), TextAnchor.UpperLeft, font);
            txtHeroPassive.horizontalOverflow = HorizontalWrapMode.Wrap;
            txtHeroPassive.verticalOverflow = VerticalWrapMode.Truncate;
            txtHeroPassive.GetComponent<RectTransform>().sizeDelta = new Vector2(414, 90);
            AddOutline(txtHeroPassive.gameObject, new Color(0.10f, 0.06f, 0.03f, 1f), 1.2f);

            // =========================================================
            // BOTTOM HERO NAVIGATION & 6 HERO SLOTS
            // [ ◀ ]  [Slot 0] [Slot 1] [Slot 2] [Slot 3] [Slot 4] [Slot 5]  [ ▶ ]
            // =========================================================
            // Arrow Navigation Button ◀ (to the left of slot row)
            GameObject btnPrevHero = CreateJuicyButton("Btn_PrevHero", frameGO.transform, new Vector2(-468, -210), new Vector2(38, 52), PrevHero, sprArrowLeft, 1.25f, 0.85f);
            UIButtonJuice phj = btnPrevHero.GetComponent<UIButtonJuice>();
            if (phj != null)
            {
                phj.enableHorizontalNudge = true;
                phj.nudgeDirection = -1f;
                phj.nudgeDistance = 4f;
                phj.nudgeSpeed = 3.0f;
            }

            // Arrow Navigation Button ▶ (to the right of slot row)
            GameObject btnNextHero = CreateJuicyButton("Btn_NextHero", frameGO.transform, new Vector2(-32, -210), new Vector2(38, 52), NextHero, sprArrowRight, 1.25f, 0.85f);
            UIButtonJuice nhj = btnNextHero.GetComponent<UIButtonJuice>();
            if (nhj != null)
            {
                nhj.enableHorizontalNudge = true;
                nhj.nudgeDirection = 1f;
                nhj.nudgeDistance = 4f;
                nhj.nudgeSpeed = 3.0f;
            }

            // 6 Hero Slot Buttons neatly spaced at the bottom
            float startX = -415f;
            float stepX = 66f;
            heroSlotIndicators.Clear();
            heroSlotButtons.Clear();

            for (int i = 0; i < heroes.Count; i++)
            {
                int index = i;
                Vector2 slotPos = new Vector2(startX + (i * stepX), -210);

                GameObject btnSlot = CreateJuicyButton("Slot_" + i, frameGO.transform, slotPos, new Vector2(60, 60), () => SelectHero(index), 1.14f, 0.88f);
                heroSlotButtons.Add(btnSlot);

                // Carved medieval slot background tile
                GameObject slotBg = CreateUIObject("SlotBg", btnSlot.transform);
                RectTransform bgRect = slotBg.GetComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.sizeDelta = Vector2.zero;
                Image bgImg = slotBg.AddComponent<Image>();
                bgImg.color = new Color(0.12f, 0.09f, 0.06f, 0.92f);
                bgImg.raycastTarget = false;
                AddOutline(slotBg, new Color(0.35f, 0.25f, 0.15f, 0.95f), 1.5f);

                // Mini portrait icon inside slot (100% visible, sharp pixel art)
                GameObject iconGO = CreateUIObject("Icon", btnSlot.transform);
                RectTransform iconRect = iconGO.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.08f, 0.08f);
                iconRect.anchorMax = new Vector2(0.92f, 0.92f);
                iconRect.offsetMin = Vector2.zero;
                iconRect.offsetMax = Vector2.zero;
                Image iconImg = iconGO.AddComponent<Image>();
                iconImg.sprite = heroes[i].portraitSprite;
                iconImg.preserveAspect = true;
                iconImg.raycastTarget = false;

                // HOLLOW Glowing Golden Highlight Frame (Does NOT cover the character inside!)
                GameObject indicator = CreateUIObject("Indicator", btnSlot.transform);
                RectTransform indRect = indicator.GetComponent<RectTransform>();
                indRect.anchorMin = Vector2.zero;
                indRect.anchorMax = Vector2.one;
                indRect.offsetMin = new Vector2(-4, -4);
                indRect.offsetMax = new Vector2(4, 4);

                Color goldColor = new Color(1f, 0.88f, 0.25f, 1f);
                Color glowAmber = new Color(1f, 0.55f, 0.05f, 0.9f);

                // 4 Hollow Border Bars: Top, Bottom, Left, Right
                CreateBorderLine("BorderT", indicator.transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 2), new Vector2(4, 3), goldColor, glowAmber);
                CreateBorderLine("BorderB", indicator.transform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, -2), new Vector2(4, 3), goldColor, glowAmber);
                CreateBorderLine("BorderL", indicator.transform, new Vector2(0, 0), new Vector2(0, 1), new Vector2(-2, 0), new Vector2(3, 4), goldColor, glowAmber);
                CreateBorderLine("BorderR", indicator.transform, new Vector2(1, 0), new Vector2(1, 1), new Vector2(2, 0), new Vector2(3, 4), goldColor, glowAmber);

                // 4 Golden Corner Accents
                CreateCornerGlow("CornerTL", indicator.transform, new Vector2(0, 1), new Vector2(-2, 2), goldColor);
                CreateCornerGlow("CornerTR", indicator.transform, new Vector2(1, 1), new Vector2(2, 2), goldColor);
                CreateCornerGlow("CornerBL", indicator.transform, new Vector2(0, 0), new Vector2(-2, -2), goldColor);
                CreateCornerGlow("CornerBR", indicator.transform, new Vector2(1, 0), new Vector2(2, -2), goldColor);

                // Subtle warm backlight aura placed behind icon
                GameObject backAura = CreateUIObject("BackAura", indicator.transform);
                backAura.transform.SetAsFirstSibling();
                RectTransform baRect = backAura.GetComponent<RectTransform>();
                baRect.anchorMin = Vector2.zero;
                baRect.anchorMax = Vector2.one;
                baRect.offsetMin = Vector2.zero;
                baRect.offsetMax = Vector2.zero;
                Image baImg = backAura.AddComponent<Image>();
                baImg.color = new Color(1f, 0.88f, 0.2f, 0.16f);
                baImg.raycastTarget = false;

                indicator.SetActive(i == selectedHeroIndex);
                heroSlotIndicators.Add(indicator);
            }

            // CONFIRM button: responsive interactive button over painted button at (285, -210)
            GameObject confirmBtn = CreateJuicyButton("Btn_ConfirmHero", frameGO.transform, new Vector2(285, -210), new Vector2(200, 80), () => OpenHeroSelect(false), 1.08f, 0.92f);
            Image cbImg = confirmBtn.GetComponent<Image>();
            if (cbImg != null) cbImg.color = new Color(0f, 0f, 0f, 0.01f);

            // Close (X) button at Top Right
            CreateJuicyButton("Btn_Close", frameGO.transform, new Vector2(560, 300), new Vector2(54, 54), () => OpenHeroSelect(false), sprBtnClose, 1.25f, 0.85f);

            UpdateHeroDisplay();
            heroSelectModal.SetActive(false);
        }

        // =========================================================
        // MAP SELECTION WINDOW
        // =========================================================
        private void BuildMapSelectModal(Transform parent, Font font)
        {
            mapSelectModal = CreateUIObject("Modal_MapSelect", parent);
            RectTransform modalRect = mapSelectModal.GetComponent<RectTransform>();
            modalRect.anchorMin = Vector2.zero;
            modalRect.anchorMax = Vector2.one;
            modalRect.offsetMin = Vector2.zero;
            modalRect.offsetMax = Vector2.zero;

            Image backdrop = mapSelectModal.AddComponent<Image>();
            backdrop.color = new Color(0f, 0f, 0f, 0.75f);

            // Frame Image
            GameObject frameGO = CreateUIObject("MapFrame", mapSelectModal.transform);
            mapFrameRect = frameGO.GetComponent<RectTransform>();
            mapFrameRect.anchoredPosition = Vector2.zero;
            mapFrameRect.sizeDelta = new Vector2(1180, 660);

            Image frameImg = frameGO.AddComponent<Image>();
            Sprite mapFrameSprite = LoadSpriteAtPath("Assets/Sprites/UI/Frame_MapSelection.png");
            if (mapFrameSprite != null) frameImg.sprite = mapFrameSprite;
            else frameImg.color = new Color(0.12f, 0.1f, 0.08f, 0.95f);

            // Map Preview Center Image - Crisp and stable, NO breathing or shaking
            mapPreviewBoxGO = CreateUIObject("MapPreviewBox", frameGO.transform);
            RectTransform prevRect = mapPreviewBoxGO.GetComponent<RectTransform>();
            prevRect.anchoredPosition = new Vector2(0, 72);
            prevRect.sizeDelta = new Vector2(680, 210);

            imgMapPreview = mapPreviewBoxGO.AddComponent<Image>();
            imgMapPreview.preserveAspect = true;
            imgMapPreview.raycastTarget = false;

            // Navigation Arrow Buttons ◀ and ▶
            GameObject btnPrev = CreateJuicyButton("Btn_PrevMap", frameGO.transform, new Vector2(-400, 72), new Vector2(65, 80), PrevMap, sprArrowLeft, 1.25f, 0.85f);
            UIButtonJuice pj = btnPrev.GetComponent<UIButtonJuice>();
            pj.enableHorizontalNudge = true;
            pj.nudgeDirection = -1f;
            pj.nudgeDistance = 5f;
            pj.nudgeSpeed = 3.0f;

            GameObject btnNext = CreateJuicyButton("Btn_NextMap", frameGO.transform, new Vector2(400, 72), new Vector2(65, 80), NextMap, sprArrowRight, 1.25f, 0.85f);
            UIButtonJuice nj = btnNext.GetComponent<UIButtonJuice>();
            nj.enableHorizontalNudge = true;
            nj.nudgeDirection = 1f;
            nj.nudgeDistance = 5f;
            nj.nudgeSpeed = 3.0f;

            // Map Details Text Panel on Parchment - Centered dark medieval lettering
            detailsBoxGO = CreateUIObject("MapDetailsBox", frameGO.transform);
            RectTransform dRect = detailsBoxGO.GetComponent<RectTransform>();
            dRect.anchoredPosition = new Vector2(0, -95);
            dRect.sizeDelta = new Vector2(680, 95);

            UIButtonJuice dj = detailsBoxGO.AddComponent<UIButtonJuice>();
            dj.hoverScale = 1.0f; // Soft punch on map switch

            VerticalLayoutGroup vlg = detailsBoxGO.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 4f;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            txtMapTitle = CreateText("TxtMapTitle", detailsBoxGO.transform, "EMERALD MEADOW", 22, FontStyle.Bold, new Color(0.20f, 0.12f, 0.05f), TextAnchor.MiddleCenter, font);
            txtMapTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(660, 28);

            txtMapDifficulty = CreateText("TxtDifficulty", detailsBoxGO.transform, "Độ khó: ⭐⭐☆☆☆   |   Thưởng: VÀNG x1.0", 16, FontStyle.Bold, new Color(0.55f, 0.15f, 0.10f), TextAnchor.MiddleCenter, font);
            txtMapDifficulty.GetComponent<RectTransform>().sizeDelta = new Vector2(660, 22);

            txtMapDescription = CreateText("TxtDesc", detailsBoxGO.transform, "Mô tả map...", 14, FontStyle.Italic, new Color(0.22f, 0.20f, 0.18f), TextAnchor.MiddleCenter, font);
            txtMapDescription.GetComponent<RectTransform>().sizeDelta = new Vector2(660, 36);

            // CONFIRM button at bottom center
            CreateJuicyButton("Btn_ConfirmMap", frameGO.transform, new Vector2(0, -225), new Vector2(240, 75), () => OpenMapSelect(false), sprBtnConfirm, 1.08f, 0.92f, breathing: true);

            // Close (X) button at Top Right
            CreateJuicyButton("Btn_Close", frameGO.transform, new Vector2(550, 295), new Vector2(54, 54), () => OpenMapSelect(false), sprBtnClose, 1.25f, 0.85f);

            UpdateMapDisplay();
            mapSelectModal.SetActive(false);
        }

        // =========================================================
        // OPTIONS WINDOW (AUDIO & CONTROLS)
        // =========================================================
        private void BuildOptionsModal(Transform parent, Font font)
        {
            optionsModal = CreateUIObject("Modal_Options", parent);
            RectTransform modalRect = optionsModal.GetComponent<RectTransform>();
            modalRect.anchorMin = Vector2.zero;
            modalRect.anchorMax = Vector2.one;
            modalRect.offsetMin = Vector2.zero;
            modalRect.offsetMax = Vector2.zero;

            Image backdrop = optionsModal.AddComponent<Image>();
            backdrop.color = new Color(0f, 0f, 0f, 0.75f);

            // Frame Image
            GameObject frameGO = CreateUIObject("OptionsFrame", optionsModal.transform);
            optionsFrameRect = frameGO.GetComponent<RectTransform>();
            optionsFrameRect.anchoredPosition = Vector2.zero;
            optionsFrameRect.sizeDelta = new Vector2(1100, 640);

            Image frameImg = frameGO.AddComponent<Image>();
            Sprite optFrameSprite = LoadSpriteAtPath("Assets/Sprites/UI/Frame_Options.png");
            if (optFrameSprite != null) frameImg.sprite = optFrameSprite;
            else frameImg.color = new Color(0.12f, 0.1f, 0.08f, 0.95f);

            // Controls Container
            GameObject panelBox = CreateUIObject("OptionsContent", frameGO.transform);
            RectTransform pRect = panelBox.GetComponent<RectTransform>();
            pRect.anchoredPosition = new Vector2(0, 25);
            pRect.sizeDelta = new Vector2(760, 380);

            VerticalLayoutGroup vlg = panelBox.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 20f;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            // Slider 1: BGM Music Volume with Value Pop Animation
            float initialMusic = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
            sliderMusic = CreateSliderRow(panelBox.transform, "Nhạc Nền (Music):", initialMusic, (val) =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(val);
            }, out txtMusicValue, font, isSfx: false);

            // Slider 2: SFX Sound Effects Volume with Live Audio Preview
            float initialSfx = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
            sliderSfx = CreateSliderRow(panelBox.transform, "Hiệu Ứng (SFX):", initialSfx, (val) =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.SetSfxVolume(val);
            }, out txtSfxValue, font, isSfx: true);

            // Toggle: Mute All with Punch Bounce
            bool initialMute = PlayerPrefs.GetInt("IsMuted", 0) == 1;
            toggleMute = CreateToggleRow(panelBox.transform, "Tắt Toàn Bộ Âm Thanh (Mute)", initialMute, (isMuted) =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.SetMute(isMuted);
            }, font);

            // Controls Hint with Breathing Pulse
            Text txtControls = CreateText("TxtControls", panelBox.transform, "ĐIỀU KHIỂN:  [WASD / Mũi tên] Di chuyển   |   [ESC] Tạm dừng   |   [Chuột] Chọn Kỹ Năng", 19, FontStyle.Bold, new Color(1f, 0.94f, 0.72f), TextAnchor.MiddleCenter, font);
            txtControls.GetComponent<RectTransform>().sizeDelta = new Vector2(760, 42);
            AddOutline(txtControls.gameObject, new Color(0.08f, 0.05f, 0.02f, 1f), 2.0f);

            UIButtonJuice ctrlJuice = txtControls.gameObject.AddComponent<UIButtonJuice>();
            ctrlJuice.enableBreathing = true;
            ctrlJuice.breathingSpeed = 1.8f;
            ctrlJuice.breathingAmount = 0.02f;

            // CONFIRM button at bottom center
            CreateJuicyButton("Btn_ConfirmOptions", frameGO.transform, new Vector2(0, -225), new Vector2(240, 75), () => OpenOptions(false), sprBtnConfirm, 1.08f, 0.92f, breathing: true);

            // Close (X) button at Top Right
            CreateJuicyButton("Btn_Close", frameGO.transform, new Vector2(490, 275), new Vector2(50, 50), () => OpenOptions(false), sprBtnClose, 1.25f, 0.85f);

            optionsModal.SetActive(false);
        }

        private Slider CreateSliderRow(Transform parent, string label, float initialValue, System.Action<float> onValueChangedCallback, out Text valueText, Font font, bool isSfx = false)
        {
            GameObject row = CreateUIObject("SliderRow", parent);
            RectTransform r = row.GetComponent<RectTransform>();
            r.sizeDelta = new Vector2(720, 52);

            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            // Hover juice on the row itself
            UIButtonJuice rowJuice = row.AddComponent<UIButtonJuice>();
            rowJuice.hoverScale = 1.02f;
            rowJuice.pressScale = 1.0f;
            rowJuice.playSoundOnHover = true;

            Text lbl = CreateText("Lbl", row.transform, label, 22, FontStyle.Bold, new Color(1f, 0.97f, 0.9f), TextAnchor.MiddleLeft, font);
            lbl.GetComponent<RectTransform>().sizeDelta = new Vector2(270, 42);
            AddOutline(lbl.gameObject, new Color(0.08f, 0.05f, 0.02f, 1f), 2.0f);

            GameObject sliderGO = CreateUIObject("Slider", row.transform);
            RectTransform sRect = sliderGO.GetComponent<RectTransform>();
            sRect.sizeDelta = new Vector2(300, 26);

            Slider slider = sliderGO.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = initialValue;

            Image bg = sliderGO.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.09f, 0.06f, 1f);
            AddOutline(sliderGO, new Color(0.7f, 0.55f, 0.25f, 0.9f), 1.5f);

            GameObject fillArea = CreateUIObject("Fill Area", sliderGO.transform);
            RectTransform faRect = fillArea.GetComponent<RectTransform>();
            faRect.anchorMin = Vector2.zero;
            faRect.anchorMax = Vector2.one;
            faRect.sizeDelta = Vector2.zero;

            GameObject fill = CreateUIObject("Fill", fillArea.transform);
            RectTransform fRect = fill.GetComponent<RectTransform>();
            fRect.sizeDelta = Vector2.zero;
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(1f, 0.82f, 0.2f, 1f);

            slider.fillRect = fRect;

            valueText = CreateText("ValText", row.transform, Mathf.RoundToInt(initialValue * 100f) + "%", 22, FontStyle.Bold, new Color(1f, 0.88f, 0.25f), TextAnchor.MiddleRight, font);
            valueText.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 42);
            AddOutline(valueText.gameObject, new Color(0.08f, 0.05f, 0.02f, 1f), 2.0f);

            UIButtonJuice valJuice = valueText.gameObject.AddComponent<UIButtonJuice>();

            Text capturedValText = valueText;
            float lastSfxTestTime = 0f;

            slider.onValueChanged.AddListener((val) =>
            {
                if (onValueChangedCallback != null) onValueChangedCallback(val);
                if (capturedValText != null)
                {
                    capturedValText.text = Mathf.RoundToInt(val * 100f) + "%";
                    if (valJuice != null) valJuice.Punch(1.22f);
                }
                if (isSfx && Time.unscaledTime - lastSfxTestTime > 0.12f)
                {
                    lastSfxTestTime = Time.unscaledTime;
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxByName("pickup");
                }
            });

            return slider;
        }

        private Toggle CreateToggleRow(Transform parent, string label, bool initialValue, UnityEngine.Events.UnityAction<bool> onValueChanged, Font font)
        {
            GameObject row = CreateUIObject("ToggleRow", parent);
            RectTransform r = row.GetComponent<RectTransform>();
            r.sizeDelta = new Vector2(720, 48);

            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            UIButtonJuice rowJuice = row.AddComponent<UIButtonJuice>();
            rowJuice.hoverScale = 1.02f;
            rowJuice.pressScale = 1.0f;

            GameObject toggleGO = CreateUIObject("Toggle", row.transform);
            RectTransform tRect = toggleGO.GetComponent<RectTransform>();
            tRect.sizeDelta = new Vector2(34, 34);

            Toggle toggle = toggleGO.AddComponent<Toggle>();
            Image bg = toggleGO.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.09f, 0.06f, 1f);
            AddOutline(toggleGO, new Color(0.85f, 0.7f, 0.3f, 1f), 2.0f);

            UIButtonJuice toggleJuice = toggleGO.AddComponent<UIButtonJuice>();
            toggleJuice.hoverScale = 1.15f;
            toggleJuice.pressScale = 0.88f;

            GameObject check = CreateUIObject("Checkmark", toggleGO.transform);
            RectTransform cRect = check.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0.15f, 0.15f);
            cRect.anchorMax = new Vector2(0.85f, 0.85f);
            cRect.sizeDelta = Vector2.zero;
            Image checkImg = check.AddComponent<Image>();
            checkImg.color = new Color(1f, 0.85f, 0.2f);

            UIButtonJuice checkJuice = check.AddComponent<UIButtonJuice>();

            toggle.graphic = checkImg;
            toggle.isOn = initialValue;
            toggle.onValueChanged.AddListener((val) =>
            {
                if (checkJuice != null) checkJuice.Punch(1.40f);
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxByName("click");
                if (onValueChanged != null) onValueChanged(val);
            });

            Text lbl = CreateText("Lbl", row.transform, label, 22, FontStyle.Bold, new Color(1f, 0.95f, 0.8f), TextAnchor.MiddleLeft, font);
            lbl.GetComponent<RectTransform>().sizeDelta = new Vector2(460, 42);
            AddOutline(lbl.gameObject, new Color(0.08f, 0.05f, 0.02f, 1f), 2.0f);

            return toggle;
        }

        // =========================================================
        // SELECTION & LOGIC WITH JUICE
        // =========================================================
        public void SelectHero(int index)
        {
            if (index < 0 || index >= heroes.Count) return;
            selectedHeroIndex = index;
            PlayerPrefs.SetInt("SelectedHeroIndex", selectedHeroIndex);
            PlayerPrefs.Save();

            UpdateHeroDisplay();

            // Punch scale on portrait to give feedback!
            if (portraitBoxGO != null)
            {
                UIButtonJuice j = portraitBoxGO.GetComponent<UIButtonJuice>();
                if (j != null) j.Punch(1.18f);
            }

            // Punch scale on info box to emphasize stats change!
            if (infoBoxGO != null)
            {
                UIButtonJuice ij = infoBoxGO.GetComponent<UIButtonJuice>();
                if (ij != null) ij.Punch(1.06f);
            }

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxByName("click");
        }

        private void UpdateHeroDisplay()
        {
            if (selectedHeroIndex < 0 || selectedHeroIndex >= heroes.Count) return;
            HeroData h = heroes[selectedHeroIndex];

            if (txtHeroName != null) txtHeroName.text = h.heroName;
            if (txtHeroWeapon != null) txtHeroWeapon.text = "Vũ khí khởi đầu: " + h.weaponName;
            if (txtHeroStats != null) txtHeroStats.text = $"Máu: {h.hp}       •   Tốc độ: {h.speed:F1}\nSát thương: +{h.mightPercent}%   •   Giáp: +{h.armor}";
            if (txtHeroPassive != null) txtHeroPassive.text = "Đặc tính: " + h.passiveDescription;

            if (imgHeroPortrait != null && h.portraitSprite != null)
            {
                imgHeroPortrait.sprite = h.portraitSprite;
            }

            for (int i = 0; i < heroSlotIndicators.Count; i++)
            {
                bool isSelected = (i == selectedHeroIndex);
                if (heroSlotIndicators[i] != null) heroSlotIndicators[i].SetActive(isSelected);
                if (i < heroSlotButtons.Count && heroSlotButtons[i] != null)
                {
                    heroSlotButtons[i].transform.localScale = isSelected ? new Vector3(1.12f, 1.12f, 1f) : Vector3.one;
                }
            }
        }

        public void PrevHero()
        {
            selectedHeroIndex = (selectedHeroIndex - 1 + heroes.Count) % heroes.Count;
            SelectHero(selectedHeroIndex);
        }

        public void NextHero()
        {
            selectedHeroIndex = (selectedHeroIndex + 1) % heroes.Count;
            SelectHero(selectedHeroIndex);
        }

        public void PrevMap()
        {
            selectedMapIndex = (selectedMapIndex - 1 + maps.Count) % maps.Count;
            SaveAndRefreshMap();
        }

        public void NextMap()
        {
            selectedMapIndex = (selectedMapIndex + 1) % maps.Count;
            SaveAndRefreshMap();
        }

        private void SaveAndRefreshMap()
        {
            PlayerPrefs.SetInt("SelectedMapIndex", selectedMapIndex);
            PlayerPrefs.Save();
            UpdateMapDisplay();
            ApplySelectedMapToEnvironment();

            // Map preview stays perfectly stable without punch/shaking to preserve crisp pixel art

            // Punch scale on map details text
            if (detailsBoxGO != null)
            {
                UIButtonJuice dj = detailsBoxGO.GetComponent<UIButtonJuice>();
                if (dj != null) dj.Punch(1.08f);
            }

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxByName("click");
        }

        private void UpdateMapDisplay()
        {
            if (selectedMapIndex < 0 || selectedMapIndex >= maps.Count) return;
            MapData m = maps[selectedMapIndex];

            if (txtMapTitle != null) txtMapTitle.text = m.mapName;
            if (txtMapDifficulty != null) txtMapDifficulty.text = $"Độ khó: {m.difficultyStars}   |   Thưởng: {m.goldMultiplier}";
            if (txtMapDescription != null) txtMapDescription.text = $"{m.monstersDescription}\n{m.hazardDescription}";

            if (imgMapPreview != null && m.mapPreviewSprite != null)
            {
                imgMapPreview.sprite = m.mapPreviewSprite;
            }
        }

        public void OpenHeroSelect(bool open)
        {
            if (open)
            {
                UpdateHeroDisplay();
                AnimateModalOpen(heroSelectModal, heroFrameRect);
            }
            else
            {
                AnimateModalClose(heroSelectModal, heroFrameRect);
            }
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxByName("click");
        }

        public void OpenMapSelect(bool open)
        {
            if (open)
            {
                UpdateMapDisplay();
                AnimateModalOpen(mapSelectModal, mapFrameRect);
            }
            else
            {
                AnimateModalClose(mapSelectModal, mapFrameRect);
            }
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxByName("click");
        }

        public void OpenOptions(bool open)
        {
            if (open)
            {
                AnimateModalOpen(optionsModal, optionsFrameRect);
            }
            else
            {
                AnimateModalClose(optionsModal, optionsFrameRect);
            }
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxByName("click");
        }

        private void AnimateModalOpen(GameObject modal, RectTransform frame)
        {
            if (modal == null) return;
            modal.SetActive(true);
            if (runningModalAnims.ContainsKey(modal) && runningModalAnims[modal] != null)
            {
                StopCoroutine(runningModalAnims[modal]);
            }
            runningModalAnims[modal] = StartCoroutine(CoModalOpen(modal, frame));
        }

        private void AnimateModalClose(GameObject modal, RectTransform frame)
        {
            if (modal == null || !modal.activeSelf) return;
            if (runningModalAnims.ContainsKey(modal) && runningModalAnims[modal] != null)
            {
                StopCoroutine(runningModalAnims[modal]);
            }
            runningModalAnims[modal] = StartCoroutine(CoModalClose(modal, frame));
        }

        private IEnumerator CoModalOpen(GameObject modal, RectTransform frame)
        {
            Image bg = modal.GetComponent<Image>();
            float elapsed = 0f;
            float duration = 0.22f;

            if (frame != null) frame.localScale = new Vector3(0.82f, 0.82f, 1f);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                if (bg != null)
                {
                    bg.color = new Color(0f, 0f, 0f, Mathf.Lerp(0f, 0.75f, t));
                }

                if (frame != null)
                {
                    // Elastic spring curve
                    float scale = 0.82f + 0.18f * Mathf.Sin(t * Mathf.PI * 0.5f) + Mathf.Sin(t * Mathf.PI) * 0.05f;
                    frame.localScale = new Vector3(scale, scale, 1f);
                }

                yield return null;
            }

            if (frame != null) frame.localScale = Vector3.one;
            if (bg != null) bg.color = new Color(0f, 0f, 0f, 0.75f);
        }

        private IEnumerator CoModalClose(GameObject modal, RectTransform frame)
        {
            Image bg = modal.GetComponent<Image>();
            float elapsed = 0f;
            float duration = 0.14f;
            Vector3 startScale = frame != null ? frame.localScale : Vector3.one;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                if (bg != null)
                {
                    bg.color = new Color(0f, 0f, 0f, Mathf.Lerp(0.75f, 0f, t));
                }

                if (frame != null)
                {
                    float scale = Mathf.Lerp(startScale.x, 0.85f, t);
                    frame.localScale = new Vector3(scale, scale, 1f);
                }

                yield return null;
            }

            if (frame != null) frame.localScale = Vector3.one;
            modal.SetActive(false);
        }

        // =========================================================
        // START BATTLE
        // =========================================================
        public void ApplySelectedMapToEnvironment()
        {
            if (selectedMapIndex < 0 || selectedMapIndex >= maps.Count) return;
            Sprite mapSpr = maps[selectedMapIndex].mapPreviewSprite;
            if (mapSpr == null) return;

            GameObject bg = GameObject.Find("Environment_Background");
            if (bg == null)
            {
                bg = new GameObject("Environment_Background");
                bg.transform.position = Vector3.zero;
            }

            var env = bg.GetComponent<VampireSurvivors.Environment.MapEnvironmentManager>();
            if (env == null) env = bg.AddComponent<VampireSurvivors.Environment.MapEnvironmentManager>();
            env.ApplyMapIndex(selectedMapIndex, mapSpr);
        }

        public void StartBattle()
        {
            Debug.Log($"<color=green>[START BATTLE]</color> Hero: {heroes[selectedHeroIndex].heroName}, Map: {maps[selectedMapIndex].mapName}");

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfxByName("click");
            }

            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player != null && selectedHeroIndex < heroes.Count)
            {
                player.SwitchHero(heroes[selectedHeroIndex].heroKey);
            }

            ApplySelectedMapToEnvironment();

            if (menuCanvas != null)
            {
                menuCanvas.gameObject.SetActive(false);
            }

            isMenuOpen = false;
            Time.timeScale = 1f;

            if (selectedMapIndex < maps.Count)
            {
                PlayerPrefs.SetFloat("SelectedMapGoldBonus", maps[selectedMapIndex].goldBonusValue);
                PlayerPrefs.SetInt("SelectedMapIndex", selectedMapIndex);
                PlayerPrefs.Save();
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.survivalTime = 0f;
                GameManager.Instance.isGameOver = false;
            }
        }

        // =========================================================
        // HELPER FACTORIES
        // =========================================================
        private GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private Text CreateText(string name, Transform parent, string text, int fontSize, FontStyle style, Color color, TextAnchor alignment, Font font)
        {
            GameObject go = CreateUIObject(name, parent);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300, 40);

            Text t = go.AddComponent<Text>();
            t.text = text;
            t.fontSize = fontSize;
            t.fontStyle = style;
            t.color = color;
            t.alignment = alignment;
            t.raycastTarget = false;
            if (font != null) t.font = font;

            return t;
        }

        private void AddOutline(GameObject go, Color color, float distance)
        {
            Outline ol = go.AddComponent<Outline>();
            ol.effectColor = color;
            ol.effectDistance = new Vector2(distance, -distance);
        }

        private GameObject CreateBorderLine(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta, Color color, Color outlineColor)
        {
            GameObject line = CreateUIObject(name, parent);
            RectTransform rt = line.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            Image img = line.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;

            Outline ol = line.AddComponent<Outline>();
            ol.effectColor = outlineColor;
            ol.effectDistance = new Vector2(1.5f, -1.5f);

            return line;
        }

        private GameObject CreateCornerGlow(string name, Transform parent, Vector2 anchor, Vector2 anchoredPos, Color color)
        {
            GameObject dot = CreateUIObject(name, parent);
            RectTransform rt = dot.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(6, 6);

            Image img = dot.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;

            Outline ol = dot.AddComponent<Outline>();
            ol.effectColor = new Color(1f, 1f, 0.7f, 1f);
            ol.effectDistance = new Vector2(1f, -1f);

            return dot;
        }
    }
}
