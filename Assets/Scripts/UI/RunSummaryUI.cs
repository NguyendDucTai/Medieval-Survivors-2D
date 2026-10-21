using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VampireSurvivors.Data;
using VampireSurvivors.Player;

namespace VampireSurvivors.UI
{
    public class RunSummaryUI : MonoBehaviour
    {
        public static RunSummaryUI Instance;

        [Header("Panel Container")]
        public GameObject panel;

        [Header("Labels")]
        public Text resultTitleText;
        public Text survivalTimeText;
        public Text levelText;
        public Text killsText;
        public Text damageText;
        public Text runGoldText;
        public Text bankGoldText;

        [Header("Buttons")]
        public Button playAgainButton;
        public Button mainMenuButton;
        public Button shopButton;
        public Button achievementsButton;

        private Canvas summaryCanvas;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            BuildProceduralSummaryUI();

            if (playAgainButton != null) playAgainButton.onClick.AddListener(PlayAgain);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            if (shopButton != null) shopButton.onClick.AddListener(OpenShop);
            if (achievementsButton != null) achievementsButton.onClick.AddListener(OpenAchievements);
        }

        public void ShowSummary(bool isVictory)
        {
            if (panel == null)
            {
                BuildProceduralSummaryUI();
            }

            if (panel == null) return;

#if UNITY_6000_0_OR_NEWER
            PlayerStats player = UnityEngine.Object.FindAnyObjectByType<PlayerStats>();
#else
            PlayerStats player = UnityEngine.Object.FindObjectOfType<PlayerStats>();
#endif
            float time = Core.GameManager.Instance != null ? Core.GameManager.Instance.survivalTime : 0f;
            int level = player != null ? player.currentLevel : 1;
            int kills = player != null ? player.inRunKills : 0;
            float damage = player != null ? player.inRunDamageDealt : 0f;
            int runGold = player != null ? player.inRunGold : 0;

            // Save to persistent storage
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.RecordRunEnd(time, level, kills, runGold);
            }

            // Check survival achievements
            if (MetaProgression.AchievementManager.Instance != null && SaveManager.Instance != null)
            {
                MetaProgression.AchievementManager.Instance.CheckProgress(MetaProgression.AchievementType.SurvivalTime, time);
                MetaProgression.AchievementManager.Instance.CheckProgress(MetaProgression.AchievementType.GoldEarned, SaveManager.Instance.Data.gold);
            }

            if (resultTitleText != null)
            {
                resultTitleText.text = isVictory ? "★ CHIẾN THẮNG HUY HOÀNG ★" : "💀 KẾT THÚC TRẬN ĐẤU (BẠN ĐÃ HY SINH) 💀";
                resultTitleText.color = isVictory ? new Color(1f, 0.88f, 0.2f) : new Color(0.95f, 0.25f, 0.25f);
            }

            int mins = Mathf.FloorToInt(time / 60f);
            int secs = Mathf.FloorToInt(time % 60f);
            if (survivalTimeText != null) survivalTimeText.text = $"⏱️ Thời Gian Sinh Tồn: <color=#FFE066>{mins:00}:{secs:00}</color>";
            if (levelText != null) levelText.text = $"🎖️ Cấp Độ Đạt Được: <color=#66D9FF>Lv {level}</color>";
            if (killsText != null) killsText.text = $"💀 Quái Vật Hạ Gục: <color=#FF6666>{kills:N0} con</color>";
            if (damageText != null) damageText.text = $"💥 Tổng Sát Thương: <color=#FF9933>{damage:N0}</color>";
            if (runGoldText != null) runGoldText.text = $"💰 Vàng Thu Thập: <color=#FFD700>+{runGold:N0} Vàng</color>";

            int totalBankGold = (SaveManager.Instance != null && SaveManager.Instance.Data != null) ? SaveManager.Instance.Data.gold : runGold;
            if (bankGoldText != null) bankGoldText.text = $"🏦 Tổng Quỹ Vàng: <color=#FFCC00>{totalBankGold:N0} Vàng</color>";

            panel.SetActive(true);
            Time.timeScale = 0f; // Pause game

            if (Audio.AudioManager.Instance != null)
            {
                Audio.AudioManager.Instance.PlaySfxByName("gameover");
            }
        }

        public void PlayAgain()
        {
            Time.timeScale = 1f;
            if (Audio.AudioManager.Instance != null)
            {
                Audio.AudioManager.Instance.PlaySfxByName("click");
            }
            PlayerPrefs.SetInt("AutoStartBattle", 1);
            PlayerPrefs.Save();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            if (Audio.AudioManager.Instance != null)
            {
                Audio.AudioManager.Instance.PlaySfxByName("click");
            }
            PlayerPrefs.SetInt("AutoStartBattle", 0);
            PlayerPrefs.Save();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void OpenShop()
        {
            if (Audio.AudioManager.Instance != null)
            {
                Audio.AudioManager.Instance.PlaySfxByName("click");
            }

            if (panel != null) panel.SetActive(false);
            ShopUI.GetOrCreate().OpenShop();
        }

        public void OpenAchievements()
        {
            Time.timeScale = 1f;
            if (Audio.AudioManager.Instance != null)
            {
                Audio.AudioManager.Instance.PlaySfxByName("click");
            }

            if (AchievementUI.Instance != null)
            {
                AchievementUI.Instance.OpenAchievements();
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        // =========================================================
        // PROCEDURAL GAME OVER & SUMMARY MODAL BUILDER
        // =========================================================
        public void BuildProceduralSummaryUI()
        {
            if (panel != null) return;

            // Find or create Overlay Canvas
            Canvas canvas = null;
#if UNITY_6000_0_OR_NEWER
            var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);
#else
            var canvases = UnityEngine.Object.FindObjectsOfType<Canvas>();
#endif
            foreach (var c in canvases)
            {
                if (c.renderMode == RenderMode.ScreenSpaceOverlay && c.name.Contains("Summary"))
                {
                    canvas = c;
                    break;
                }
            }

            if (canvas == null)
            {
                GameObject canvasGO = new GameObject("RunSummary_Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 700; // Above HUD and LevelUp

                CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                canvasGO.AddComponent<GraphicRaycaster>();
            }
            summaryCanvas = canvas;

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Modal Background Overlay
            GameObject modalGO = new GameObject("Summary_Panel");
            modalGO.transform.SetParent(canvas.transform, false);
            RectTransform modalRT = modalGO.AddComponent<RectTransform>();
            modalRT.anchorMin = Vector2.zero;
            modalRT.anchorMax = Vector2.one;
            modalRT.sizeDelta = Vector2.zero;

            Image bgImg = modalGO.AddComponent<Image>();
            bgImg.color = new Color(0.02f, 0.03f, 0.06f, 0.92f); // Deep dark vignette
            panel = modalGO;

            // Dialog Box Frame
            GameObject boxGO = new GameObject("DialogBox");
            boxGO.transform.SetParent(modalGO.transform, false);
            RectTransform boxRT = boxGO.AddComponent<RectTransform>();
            boxRT.anchorMin = new Vector2(0.5f, 0.5f);
            boxRT.anchorMax = new Vector2(0.5f, 0.5f);
            boxRT.pivot = new Vector2(0.5f, 0.5f);
            boxRT.anchoredPosition = Vector2.zero;
            boxRT.sizeDelta = new Vector2(800f, 720f);

            Image boxImg = boxGO.AddComponent<Image>();
            Sprite customFrame = LoadSpriteAtPath("Assets/Sprites/UI/Frame_GameOver.png");
            if (customFrame != null)
            {
                boxImg.sprite = customFrame;
                boxImg.type = Image.Type.Simple;
                boxImg.preserveAspect = true;
            }
            else
            {
                // Dark Medieval Stone Box
                boxImg.color = new Color(0.10f, 0.12f, 0.16f, 0.98f);
                Outline boxOutline = boxGO.AddComponent<Outline>();
                boxOutline.effectColor = new Color(0.85f, 0.70f, 0.25f, 1f); // Rich gold border
                boxOutline.effectDistance = new Vector2(4f, -4f);
            }

            // Title Banner
            GameObject titleGO = new GameObject("ResultTitle");
            titleGO.transform.SetParent(boxGO.transform, false);
            RectTransform titleRT = titleGO.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 1f);
            titleRT.anchorMax = new Vector2(0.5f, 1f);
            titleRT.pivot = new Vector2(0.5f, 1f);
            titleRT.anchoredPosition = new Vector2(0f, -40f);
            titleRT.sizeDelta = new Vector2(740f, 60f);

            resultTitleText = titleGO.AddComponent<Text>();
            resultTitleText.font = font;
            resultTitleText.fontSize = 32;
            resultTitleText.fontStyle = FontStyle.Bold;
            resultTitleText.alignment = TextAnchor.MiddleCenter;
            resultTitleText.color = new Color(0.95f, 0.25f, 0.25f);
            resultTitleText.text = "💀 KẾT THÚC TRẬN ĐẤU 💀";

            Outline titleOutline = titleGO.AddComponent<Outline>();
            titleOutline.effectColor = Color.black;
            titleOutline.effectDistance = new Vector2(2f, -2f);

            // Stats Container (Parchment Inner Box)
            GameObject statsBoxGO = new GameObject("StatsBox");
            statsBoxGO.transform.SetParent(boxGO.transform, false);
            RectTransform statsBoxRT = statsBoxGO.AddComponent<RectTransform>();
            statsBoxRT.anchorMin = new Vector2(0.5f, 0.5f);
            statsBoxRT.anchorMax = new Vector2(0.5f, 0.5f);
            statsBoxRT.pivot = new Vector2(0.5f, 0.5f);
            statsBoxRT.anchoredPosition = new Vector2(0f, 25f);
            statsBoxRT.sizeDelta = new Vector2(680f, 400f);

            Image statsBg = statsBoxGO.AddComponent<Image>();
            statsBg.color = new Color(0.06f, 0.08f, 0.10f, 0.92f);
            Outline statsOutline = statsBoxGO.AddComponent<Outline>();
            statsOutline.effectColor = new Color(0.35f, 0.45f, 0.6f, 0.8f);
            statsOutline.effectDistance = new Vector2(2f, -2f);

            // 6 Stat Labels
            survivalTimeText = CreateStatLine(statsBoxGO.transform, "Stat_Time", "⏱️ Thời Gian Sinh Tồn: 00:00", 0, font);
            levelText = CreateStatLine(statsBoxGO.transform, "Stat_Level", "🎖️ Cấp Độ Đạt Được: Lv 1", 1, font);
            killsText = CreateStatLine(statsBoxGO.transform, "Stat_Kills", "💀 Quái Vật Hạ Gục: 0 con", 2, font);
            damageText = CreateStatLine(statsBoxGO.transform, "Stat_Damage", "💥 Tổng Sát Thương: 0", 3, font);
            runGoldText = CreateStatLine(statsBoxGO.transform, "Stat_RunGold", "💰 Vàng Thu Thập: +0 Vàng", 4, font);
            bankGoldText = CreateStatLine(statsBoxGO.transform, "Stat_BankGold", "🏦 Tổng Quỹ Vàng: 0 Vàng", 5, font);

            // Buttons Container
            GameObject btnRow = new GameObject("ButtonRow");
            btnRow.transform.SetParent(boxGO.transform, false);
            RectTransform btnRowRT = btnRow.AddComponent<RectTransform>();
            btnRowRT.anchorMin = new Vector2(0.5f, 0f);
            btnRowRT.anchorMax = new Vector2(0.5f, 0f);
            btnRowRT.pivot = new Vector2(0.5f, 0f);
            btnRowRT.anchoredPosition = new Vector2(0f, 40f);
            btnRowRT.sizeDelta = new Vector2(720f, 75f);

            HorizontalLayoutGroup hlg = btnRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            // Button 1: Chơi Lại (Play Again)
            playAgainButton = CreateButton(btnRow.transform, "Btn_PlayAgain", "CHƠI LẠI", new Color(0.9f, 0.72f, 0.18f), new Color(0.12f, 0.08f, 0.02f), font, PlayAgain);

            // Button 2: Về Menu Chính (Main Menu)
            mainMenuButton = CreateButton(btnRow.transform, "Btn_MainMenu", "MENU CHÍNH", new Color(0.25f, 0.35f, 0.5f), Color.white, font, ReturnToMainMenu);

            // Button 3: Nâng Cấp (Upgrades Shop)
            shopButton = CreateButton(btnRow.transform, "Btn_Shop", "NÂNG CẤP SHOP", new Color(0.2f, 0.65f, 0.45f), Color.white, font, OpenShop);

            panel.SetActive(false);
        }

        private Text CreateStatLine(Transform parent, string name, string text, int index, Font font)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -25f - (index * 60f));
            rt.sizeDelta = new Vector2(620f, 48f);

            Text t = go.AddComponent<Text>();
            t.font = font;
            t.fontSize = 22;
            t.fontStyle = FontStyle.Bold;
            t.alignment = TextAnchor.MiddleLeft;
            t.color = Color.white;
            t.supportRichText = true;
            t.text = text;

            Outline o = go.AddComponent<Outline>();
            o.effectColor = new Color(0f, 0f, 0f, 0.9f);
            o.effectDistance = new Vector2(1.5f, -1.5f);

            return t;
        }

        private Button CreateButton(Transform parent, string name, string label, Color bgColor, Color textColor, Font font, UnityEngine.Events.UnityAction action)
        {
            GameObject btnGO = new GameObject(name, typeof(RectTransform));
            btnGO.transform.SetParent(parent, false);
            RectTransform rt = btnGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(215f, 60f);

            Image img = btnGO.AddComponent<Image>();
            img.color = bgColor;

            Outline outline = btnGO.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
            outline.effectDistance = new Vector2(2f, -2f);

            Button btn = btnGO.AddComponent<Button>();
            if (action != null) btn.onClick.AddListener(action);

            var juice = btnGO.AddComponent<UIButtonJuice>();
            juice.hoverScale = 1.06f;
            juice.pressScale = 0.94f;

            GameObject txtGO = new GameObject("Text", typeof(RectTransform));
            txtGO.transform.SetParent(btnGO.transform, false);
            RectTransform txtRT = txtGO.GetComponent<RectTransform>();
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.sizeDelta = Vector2.zero;

            Text txt = txtGO.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = 18;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = textColor;
            txt.text = label;

            return btn;
        }

        private Sprite LoadSpriteAtPath(string path)
        {
#if UNITY_EDITOR
            Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s != null) return s;
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
                        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 16f);
                    }
                }
                catch { }
            }
            return null;
        }
    }
}
