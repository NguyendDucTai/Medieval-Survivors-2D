using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VampireSurvivors.Data;
using VampireSurvivors.MetaProgression;

namespace VampireSurvivors.UI
{
    public class ShopUI : MonoBehaviour
    {
        public static ShopUI Instance;

        [Header("UI Containers")]
        public GameObject panel;
        public Text goldBalanceText;
        public Button closeButton;
        public Button playAgainButton;
        public Transform itemsScrollContent;

        [Header("Shop Item Template")]
        public GameObject shopItemPrefab;

        private List<GameObject> activeCards = new List<GameObject>();
        private Canvas shopCanvas;

        public static ShopUI GetOrCreate()
        {
            if (Instance != null) return Instance;
#if UNITY_6000_0_OR_NEWER
            var found = UnityEngine.Object.FindAnyObjectByType<ShopUI>();
#else
            var found = UnityEngine.Object.FindObjectOfType<ShopUI>();
#endif
            if (found != null)
            {
                Instance = found;
                return Instance;
            }

            GameObject go = new GameObject("ShopUI_Manager");
            Instance = go.AddComponent<ShopUI>();
            return Instance;
        }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            if (panel == null)
            {
                BuildProceduralShopUI();
            }
            else
            {
                panel.SetActive(false);
            }

            if (closeButton != null) closeButton.onClick.AddListener(CloseShop);
            if (playAgainButton != null) playAgainButton.onClick.AddListener(PlayAgainFromShop);

            if (PlayerPrefs.GetInt("ShopAutoResetV1", 0) == 0)
            {
                PlayerPrefs.SetInt("ShopAutoResetV1", 1);
                PlayerPrefs.Save();
                if (UpgradeManager.Instance != null)
                {
                    UpgradeManager.Instance.RefundAllUpgrades();
                }
            }
        }

        public void OpenShop()
        {
            if (panel == null)
            {
                BuildProceduralShopUI();
            }

            if (panel == null) return;
            panel.SetActive(true);
            RefreshShop();
        }

        public void CloseShop()
        {
            if (Audio.AudioManager.Instance != null)
            {
                Audio.AudioManager.Instance.PlaySfxByName("click");
            }

            if (panel != null) panel.SetActive(false);

            // If Game Over panel was open before, return to it
            if (RunSummaryUI.Instance != null && RunSummaryUI.Instance.panel != null && Core.GameManager.Instance != null && Core.GameManager.Instance.isGameOver)
            {
                RunSummaryUI.Instance.panel.SetActive(true);
            }
        }

        public void PlayAgainFromShop()
        {
            Time.timeScale = 1f;
            if (Audio.AudioManager.Instance != null)
            {
                Audio.AudioManager.Instance.PlaySfxByName("click");
            }

            if (panel != null) panel.SetActive(false);

            PlayerPrefs.SetInt("AutoStartBattle", 1);
            PlayerPrefs.Save();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void RefreshShop()
        {
            if (SaveManager.Instance != null && SaveManager.Instance.Data == null)
            {
                SaveManager.Instance.Load();
            }
            if (UpgradeManager.Instance == null) return;

            int currentGold = SaveManager.Instance != null && SaveManager.Instance.Data != null ? SaveManager.Instance.Data.gold : 0;

            if (goldBalanceText != null)
            {
                goldBalanceText.text = $"💰 QUỸ VÀNG: <color=#ffd700>{currentGold:N0}</color> VÀNG";
            }

            // Clear old cards
            foreach (var c in activeCards)
            {
                if (c != null) Destroy(c);
            }
            activeCards.Clear();

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var upgrades = UpgradeManager.Instance.allUpgrades;

            foreach (var up in upgrades)
            {
                int currentRank = UpgradeManager.Instance.GetRank(up.id);
                int cost = up.GetCostForRank(currentRank);
                bool isMax = currentRank >= up.maxRank;
                bool canAfford = !isMax && currentGold >= cost;

                GameObject card = CreateUpgradeCard(up, currentRank, cost, isMax, canAfford, font);
                if (card != null)
                {
                    activeCards.Add(card);
                }
            }
        }

        private GameObject CreateUpgradeCard(UpgradeData up, int rank, int cost, bool isMax, bool canAfford, Font font)
        {
            if (itemsScrollContent == null) return null;

            GameObject cardGO = new GameObject("Card_" + up.id, typeof(RectTransform));
            cardGO.transform.SetParent(itemsScrollContent, false);
            RectTransform cardRT = cardGO.GetComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(405f, 95f);

            Image cardImg = cardGO.AddComponent<Image>();
            cardImg.color = new Color(0.14f, 0.12f, 0.19f, 0.94f);

            Outline cardOutline = cardGO.AddComponent<Outline>();
            cardOutline.effectColor = isMax ? new Color(0.3f, 0.8f, 0.5f, 0.7f) : new Color(0.35f, 0.3f, 0.45f, 0.7f);
            cardOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // Left info container
            GameObject infoGO = new GameObject("Info", typeof(RectTransform));
            infoGO.transform.SetParent(cardGO.transform, false);
            RectTransform infoRT = infoGO.GetComponent<RectTransform>();
            infoRT.anchorMin = Vector2.zero;
            infoRT.anchorMax = Vector2.one;
            infoRT.offsetMin = new Vector2(12f, 8f);
            infoRT.offsetMax = new Vector2(-115f, -8f);

            // Title + Rank Text
            GameObject titleGO = new GameObject("Title", typeof(RectTransform));
            titleGO.transform.SetParent(infoGO.transform, false);
            RectTransform titleRT = titleGO.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0f, 0.52f);
            titleRT.anchorMax = new Vector2(1f, 1f);
            titleRT.offsetMin = Vector2.zero;
            titleRT.offsetMax = Vector2.zero;

            Text titleTxt = titleGO.AddComponent<Text>();
            titleTxt.font = font;
            titleTxt.fontSize = 15;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.alignment = TextAnchor.MiddleLeft;
            titleTxt.color = Color.white;
            titleTxt.supportRichText = true;

            string rankTag = isMax ? "<color=#55ff99>[TỐI ĐA]</color>" : $"<color=#ffd700>[{rank}/{up.maxRank}]</color>";
            titleTxt.text = $"{up.displayName} {rankTag}";

            // Desc Text
            GameObject descGO = new GameObject("Desc", typeof(RectTransform));
            descGO.transform.SetParent(infoGO.transform, false);
            RectTransform descRT = descGO.GetComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0f, 0f);
            descRT.anchorMax = new Vector2(1f, 0.52f);
            descRT.offsetMin = Vector2.zero;
            descRT.offsetMax = Vector2.zero;

            Text descTxt = descGO.AddComponent<Text>();
            descTxt.font = font;
            descTxt.fontSize = 12;
            descTxt.alignment = TextAnchor.MiddleLeft;
            descTxt.color = new Color(0.85f, 0.85f, 0.9f);
            descTxt.supportRichText = true;

            string costInfo = isMax ? "<color=#55ff99>★ HOÀN TẤT</color>" : $"Giá: <color=#ffd700>{cost:N0} Vàng</color>";
            descTxt.text = $"{up.description}\n{costInfo}";

            // Buy Button
            GameObject btnGO = new GameObject("Btn_Buy", typeof(RectTransform));
            btnGO.transform.SetParent(cardGO.transform, false);
            RectTransform btnRT = btnGO.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(1f, 0.5f);
            btnRT.anchorMax = new Vector2(1f, 0.5f);
            btnRT.pivot = new Vector2(1f, 0.5f);
            btnRT.anchoredPosition = new Vector2(-10f, 0f);
            btnRT.sizeDelta = new Vector2(100f, 44f);

            Image btnImg = btnGO.AddComponent<Image>();
            Color btnBgColor;
            if (isMax) btnBgColor = new Color(0.28f, 0.28f, 0.32f);
            else if (canAfford) btnBgColor = new Color(0.18f, 0.65f, 0.32f);
            else btnBgColor = new Color(0.45f, 0.18f, 0.18f);
            btnImg.color = btnBgColor;

            Outline btnOutline = btnGO.AddComponent<Outline>();
            btnOutline.effectColor = new Color(0f, 0f, 0f, 0.8f);
            btnOutline.effectDistance = new Vector2(1.5f, -1.5f);

            Button btn = btnGO.AddComponent<Button>();
            btn.interactable = canAfford;
            string upId = up.id;
            btn.onClick.AddListener(() => OnPurchaseClicked(upId));

            if (canAfford)
            {
                var juice = btnGO.AddComponent<UIButtonJuice>();
                juice.hoverScale = 1.07f;
                juice.pressScale = 0.93f;
            }

            GameObject btnTxtGO = new GameObject("Text", typeof(RectTransform));
            btnTxtGO.transform.SetParent(btnGO.transform, false);
            RectTransform btnTxtRT = btnTxtGO.GetComponent<RectTransform>();
            btnTxtRT.anchorMin = Vector2.zero;
            btnTxtRT.anchorMax = Vector2.one;
            btnTxtRT.offsetMin = Vector2.zero;
            btnTxtRT.offsetMax = Vector2.zero;

            Text btnTxt = btnTxtGO.AddComponent<Text>();
            btnTxt.font = font;
            btnTxt.fontSize = 14;
            btnTxt.fontStyle = FontStyle.Bold;
            btnTxt.alignment = TextAnchor.MiddleCenter;
            btnTxt.color = Color.white;

            if (isMax) btnTxt.text = "MAX";
            else if (canAfford) btnTxt.text = "NÂNG CẤP";
            else btnTxt.text = "THIẾU VÀNG";

            return cardGO;
        }

        private void OnPurchaseClicked(string upgradeId)
        {
            if (UpgradeManager.Instance != null && UpgradeManager.Instance.PurchaseUpgrade(upgradeId))
            {
                if (Audio.AudioManager.Instance != null)
                {
                    Audio.AudioManager.Instance.PlaySfxByName("pickup");
                }
                RefreshShop();
            }
        }

        // =========================================================
        // PROCEDURAL SHOP MODAL BUILDER (Custom Frame_Shop.png integrated)
        // =========================================================
        public void BuildProceduralShopUI()
        {
            if (panel != null) return;

            Canvas canvas = null;
#if UNITY_6000_0_OR_NEWER
            var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);
#else
            var canvases = UnityEngine.Object.FindObjectsOfType<Canvas>();
#endif
            foreach (var c in canvases)
            {
                if (c.renderMode == RenderMode.ScreenSpaceOverlay && c.name.Contains("Shop"))
                {
                    canvas = c;
                    break;
                }
            }

            if (canvas == null)
            {
                GameObject canvasGO = new GameObject("Shop_Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 600;

                CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                canvasGO.AddComponent<GraphicRaycaster>();
            }

            shopCanvas = canvas;
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Fullscreen backdrop overlay
            panel = new GameObject("ShopModal_Panel", typeof(RectTransform));
            panel.transform.SetParent(shopCanvas.transform, false);
            RectTransform panelRT = panel.GetComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            Image bgOverlay = panel.AddComponent<Image>();
            bgOverlay.color = new Color(0f, 0f, 0f, 0.88f);

            // Centered Modal Window Box
            GameObject boxGO = new GameObject("Box", typeof(RectTransform));
            boxGO.transform.SetParent(panel.transform, false);
            RectTransform boxRT = boxGO.GetComponent<RectTransform>();
            boxRT.anchorMin = new Vector2(0.5f, 0.5f);
            boxRT.anchorMax = new Vector2(0.5f, 0.5f);
            boxRT.anchoredPosition = Vector2.zero;
            boxRT.sizeDelta = new Vector2(1160f, 866f);

            Image boxImg = boxGO.AddComponent<Image>();
            Sprite customFrame = LoadSpriteAtPath("Assets/Sprites/UI/Frame_Shop.png");
            if (customFrame != null)
            {
                boxImg.sprite = customFrame;
                boxImg.type = Image.Type.Simple;
                boxImg.preserveAspect = true;
            }
            else
            {
                boxImg.color = new Color(0.11f, 0.09f, 0.15f, 0.98f);
                Outline boxOutline = boxGO.AddComponent<Outline>();
                boxOutline.effectColor = new Color(0.85f, 0.68f, 0.2f, 0.9f);
                boxOutline.effectDistance = new Vector2(3f, -3f);
            }

            // Built-in [X] Close Button (Placed right on the pre-drawn iron X at top-right corner)
            GameObject closeBtnGO = new GameObject("Btn_Close_X", typeof(RectTransform));
            closeBtnGO.transform.SetParent(boxGO.transform, false);
            RectTransform closeRT = closeBtnGO.GetComponent<RectTransform>();
            closeRT.anchorMin = new Vector2(0.5f, 0.5f);
            closeRT.anchorMax = new Vector2(0.5f, 0.5f);
            closeRT.anchoredPosition = new Vector2(460f, 338f);
            closeRT.sizeDelta = new Vector2(56f, 56f);

            Sprite closeSprite = null;
#if UNITY_EDITOR
            closeSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Btn_Close.png");
            if (closeSprite == null)
            {
                Texture2D tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/UI/Btn_Close.png");
                if (tex != null)
                {
                    closeSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                }
            }
#endif

            Image closeImg = closeBtnGO.AddComponent<Image>();
            if (closeSprite != null)
            {
                closeImg.sprite = closeSprite;
                closeImg.color = Color.white;
                closeImg.preserveAspect = true;
            }
            else
            {
                closeImg.color = new Color(1f, 1f, 1f, 0.002f);
            }
            closeImg.raycastTarget = true;

            closeButton = closeBtnGO.AddComponent<Button>();
            closeButton.targetGraphic = closeImg;
            ColorBlock cb = closeButton.colors;
            if (closeSprite != null)
            {
                cb.normalColor = Color.white;
                cb.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
                cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            }
            else
            {
                cb.normalColor = Color.clear;
                cb.highlightedColor = new Color(1f, 0.9f, 0.4f, 0.25f);
                cb.pressedColor = new Color(1f, 0.8f, 0.2f, 0.4f);
            }
            closeButton.colors = cb;
            closeButton.onClick.AddListener(CloseShop);

            var closeJuice = closeBtnGO.AddComponent<UIButtonJuice>();
            closeJuice.onClickAction = null;
            closeJuice.hoverScale = 1.25f;
            closeJuice.pressScale = 0.85f;

            // Gold Balance Text (Under top SHOP header divider)
            GameObject goldGO = new GameObject("GoldBalance", typeof(RectTransform));
            goldGO.transform.SetParent(boxGO.transform, false);
            RectTransform goldRT = goldGO.GetComponent<RectTransform>();
            goldRT.anchorMin = new Vector2(0.5f, 0.5f);
            goldRT.anchorMax = new Vector2(0.5f, 0.5f);
            goldRT.anchoredPosition = new Vector2(0f, 255f);
            goldRT.sizeDelta = new Vector2(500f, 32f);

            goldBalanceText = goldGO.AddComponent<Text>();
            goldBalanceText.font = font;
            goldBalanceText.fontSize = 20;
            goldBalanceText.fontStyle = FontStyle.Bold;
            goldBalanceText.alignment = TextAnchor.MiddleCenter;
            goldBalanceText.color = new Color(1f, 0.92f, 0.35f);
            goldBalanceText.supportRichText = true;
            goldBalanceText.text = "💰 QUỸ VÀNG: 0 VÀNG";
            Outline goldOutline = goldGO.AddComponent<Outline>();
            goldOutline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            goldOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // Refund / Reset Button
            GameObject resetGO = new GameObject("Btn_ResetUpgrades", typeof(RectTransform));
            resetGO.transform.SetParent(boxGO.transform, false);
            RectTransform resetRT = resetGO.GetComponent<RectTransform>();
            resetRT.anchorMin = new Vector2(0.5f, 0.5f);
            resetRT.anchorMax = new Vector2(0.5f, 0.5f);
            resetRT.anchoredPosition = new Vector2(330f, 255f);
            resetRT.sizeDelta = new Vector2(160f, 34f);

            Image resetImg = resetGO.AddComponent<Image>();
            resetImg.color = new Color(0.48f, 0.16f, 0.16f, 0.95f);

            Outline resetOutline = resetGO.AddComponent<Outline>();
            resetOutline.effectColor = new Color(0.85f, 0.65f, 0.2f, 0.9f);
            resetOutline.effectDistance = new Vector2(1.5f, -1.5f);

            Button resetBtn = resetGO.AddComponent<Button>();
            resetBtn.onClick.AddListener(() =>
            {
                if (UpgradeManager.Instance != null)
                {
                    UpgradeManager.Instance.RefundAllUpgrades();
                    if (Audio.AudioManager.Instance != null)
                    {
                        Audio.AudioManager.Instance.PlaySfxByName("pickup");
                    }
                    RefreshShop();
                }
            });

            var resetJuice = resetGO.AddComponent<UIButtonJuice>();
            resetJuice.hoverScale = 1.07f;
            resetJuice.pressScale = 0.93f;

            GameObject resetTxtGO = new GameObject("Text", typeof(RectTransform));
            resetTxtGO.transform.SetParent(resetGO.transform, false);
            RectTransform resetTxtRT = resetTxtGO.GetComponent<RectTransform>();
            resetTxtRT.anchorMin = Vector2.zero;
            resetTxtRT.anchorMax = Vector2.one;
            resetTxtRT.offsetMin = Vector2.zero;
            resetTxtRT.offsetMax = Vector2.zero;

            Text resetTxt = resetTxtGO.AddComponent<Text>();
            resetTxt.font = font;
            resetTxt.fontSize = 13;
            resetTxt.fontStyle = FontStyle.Bold;
            resetTxt.alignment = TextAnchor.MiddleCenter;
            resetTxt.color = new Color(1f, 0.9f, 0.85f);
            resetTxt.text = "🔄 HOÀN ĐIỂM";

            // ScrollView for Upgrades (Centered inside parchment interior)
            GameObject scrollGO = new GameObject("ScrollView", typeof(RectTransform));
            scrollGO.transform.SetParent(boxGO.transform, false);
            RectTransform scrollRT = scrollGO.GetComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0.5f, 0.5f);
            scrollRT.anchorMax = new Vector2(0.5f, 0.5f);
            scrollRT.anchoredPosition = new Vector2(0f, -20f);
            scrollRT.sizeDelta = new Vector2(850f, 500f);

            ScrollRect scrollRect = scrollGO.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 28f;

            // Viewport
            GameObject viewportGO = new GameObject("Viewport", typeof(RectTransform));
            viewportGO.transform.SetParent(scrollGO.transform, false);
            RectTransform vpRT = viewportGO.GetComponent<RectTransform>();
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.offsetMin = Vector2.zero;
            vpRT.offsetMax = Vector2.zero;

            Image vpImg = viewportGO.AddComponent<Image>();
            vpImg.color = new Color(0f, 0f, 0f, 0.2f);
            viewportGO.AddComponent<RectMask2D>();
            scrollRect.viewport = vpRT;

            // Content
            GameObject contentGO = new GameObject("Content", typeof(RectTransform));
            contentGO.transform.SetParent(viewportGO.transform, false);
            RectTransform contentRT = contentGO.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0f, 1f);
            contentRT.anchorMax = new Vector2(1f, 1f);
            contentRT.pivot = new Vector2(0.5f, 1f);
            contentRT.anchoredPosition = Vector2.zero;
            contentRT.sizeDelta = new Vector2(0f, 650f);

            GridLayoutGroup glg = contentGO.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(405f, 95f);
            glg.spacing = new Vector2(16f, 10f);
            glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
            glg.startAxis = GridLayoutGroup.Axis.Horizontal;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 2;
            glg.childAlignment = TextAnchor.UpperCenter;

            ContentSizeFitter csf = contentGO.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRT;
            itemsScrollContent = contentRT;

            // Frame bottom is kept clean to showcase the ancient runic artwork on Frame_Shop.png
            // Players exit naturally using the [X] button or Escape/B keys
            playAgainButton = null;

            panel.SetActive(false);
        }

        private Button CreateFooterButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, Color bgColor, Color textColor, Font font, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnGO = new GameObject(name, typeof(RectTransform));
            btnGO.transform.SetParent(parent, false);
            RectTransform rt = btnGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = btnGO.AddComponent<Image>();
            img.color = bgColor;

            Outline o = btnGO.AddComponent<Outline>();
            o.effectColor = new Color(0f, 0f, 0f, 0.85f);
            o.effectDistance = new Vector2(2f, -2f);

            Button btn = btnGO.AddComponent<Button>();
            if (onClick != null) btn.onClick.AddListener(onClick);

            var juice = btnGO.AddComponent<UIButtonJuice>();
            juice.hoverScale = 1.06f;
            juice.pressScale = 0.94f;

            GameObject txtGO = new GameObject("Text", typeof(RectTransform));
            txtGO.transform.SetParent(btnGO.transform, false);
            RectTransform txtRT = txtGO.GetComponent<RectTransform>();
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.offsetMin = Vector2.zero;
            txtRT.offsetMax = Vector2.zero;

            Text txt = txtGO.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = 16;
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
                        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                    }
                }
                catch { }
            }
            return null;
        }
    }

    public class ShopItemRow : MonoBehaviour
    {
        public Text titleText;
        public Text descText;
        public Text costText;
        public Button buyButton;

        public void Setup(UpgradeData up, int rank, int cost, bool isMax, bool canAfford, UnityEngine.Events.UnityAction onBuy)
        {
            if (titleText != null)
            {
                string rankStr = isMax ? "[CẤP TỐI ĐA]" : $"[Cấp {rank}/{up.maxRank}]";
                titleText.text = $"{up.displayName} {rankStr}";
            }
            if (descText != null) descText.text = up.description;
            if (costText != null) costText.text = isMax ? "HOÀN TẤT" : $"{cost} VÀNG";
            if (buyButton != null)
            {
                buyButton.interactable = canAfford;
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(onBuy);
            }
        }
    }
}
