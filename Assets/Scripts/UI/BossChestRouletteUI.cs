using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VampireSurvivors.Audio;
using VampireSurvivors.Collectibles;
using VampireSurvivors.Player;
using VampireSurvivors.Weapons;

namespace VampireSurvivors.UI
{
    public class BossChestRouletteUI : MonoBehaviour
    {
        public static BossChestRouletteUI Instance;

        public class RouletteReward
        {
            public string title;
            public string subTitle;
            public string badge;
            public Color badgeColor;
            public bool isEvolution;
        }

        private Canvas rouletteCanvas;
        private GameObject modalPanel;
        private RectTransform chestImageRT;
        private Image chestImage;
        private Text titleText;
        private Text subTitleText;
        private GameObject rewardsContainer;
        private Button claimButton;
        private Text claimButtonText;

        private PlayerStats currentPlayer;
        private BossChest currentChest;
        private List<RouletteReward> awardedList = new List<RouletteReward>();
        private int totalGoldAwarded = 0;
        private bool isAnimating = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            BuildRouletteCanvas();
        }

        public void OpenRoulette(PlayerStats player, BossChest chest)
        {
            currentPlayer = player;
            currentChest = chest;

            if (modalPanel == null)
            {
                BuildRouletteCanvas();
            }

            modalPanel.SetActive(true);
            Time.timeScale = 0f; // Pause game during roulette celebration

            StartCoroutine(ExecuteRouletteSequence());
        }

        private IEnumerator ExecuteRouletteSequence()
        {
            isAnimating = true;
            awardedList.Clear();
            totalGoldAwarded = 0;

            if (claimButton != null) claimButton.gameObject.SetActive(false);
            if (rewardsContainer != null)
            {
                foreach (Transform child in rewardsContainer.transform)
                {
                    Destroy(child.gameObject);
                }
            }

            // Play Opening SFX
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfxByName("levelup");
            }

            if (titleText != null)
            {
                titleText.text = "★ CHIẾN LỢI PHẨM BOSS GOLEM ★";
            }
            if (subTitleText != null)
            {
                subTitleText.text = "Đang khai mở Rương Kho Báu Hoàng Gia...";
            }

            // Determine Number of Rewards based on Luck (critChance and CloverOfLuck)
            float playerLuck = 1f;
            if (currentPlayer != null)
            {
                playerLuck += currentPlayer.critChance * 2f;
            }
            if (WeaponInventory.Instance != null && WeaponInventory.Instance.HasPassive("Clover"))
            {
                playerLuck += 0.5f;
            }
            int rewardSlots = 1;

            float roll = UnityEngine.Random.value * 100f;
            float jackpotChance = 12f * playerLuck;
            float tripleChance = 35f * playerLuck;

            if (roll < jackpotChance)
            {
                rewardSlots = 5; // JACKPOT!
            }
            else if (roll < (jackpotChance + tripleChance))
            {
                rewardSlots = 3; // Rare triple reward
            }
            else
            {
                rewardSlots = 1; // Standard reward
            }

            // 1. Check for Guaranteed Weapon Evolution first (Exclusive Gate)
            WeaponInventory inv = WeaponInventory.Instance;
            bool didEvolve = false;
            if (inv != null)
            {
                foreach (var w in inv.equippedWeapons)
                {
                    if (inv.CanEvolve(w))
                    {
                        inv.EvolveWeapon(w.weaponID);
                        awardedList.Add(new RouletteReward
                        {
                            title = w.evolutionName,
                            subTitle = "Sức mạnh tối thượng đã thức tỉnh! Sát thương x2.2 & đòn đánh đặc biệt!",
                            badge = "★ THẦN BINH THỨC TỈNH ★",
                            badgeColor = new Color(1f, 0.25f, 0.85f),
                            isEvolution = true
                        });
                        didEvolve = true;
                        break;
                    }
                }
            }

            // 2. Fill remaining slots with Upgrades or Gold
            int remainingToFill = rewardSlots - awardedList.Count;
            for (int s = 0; s < remainingToFill; s++)
            {
                bool upgraded = false;
                if (inv != null && inv.equippedWeapons.Count > 0)
                {
                    var upgradableWeapons = inv.equippedWeapons.FindAll(x => x.level < x.maxLevel && !x.isEvolved);
                    if (upgradableWeapons.Count > 0)
                    {
                        var target = upgradableWeapons[UnityEngine.Random.Range(0, upgradableWeapons.Count)];
                        inv.UpgradeWeapon(target.weaponID);
                        awardedList.Add(new RouletteReward
                        {
                            title = $"{target.weaponName} (Lv {target.level})",
                            subTitle = "+Sát thương, tốc độ bắn và phạm vi xuyên thấu",
                            badge = "NÂNG CẤP VŨ KHÍ",
                            badgeColor = new Color(0.2f, 0.8f, 1f),
                            isEvolution = false
                        });
                        upgraded = true;
                    }
                }

                if (!upgraded && inv != null && inv.equippedPassives.Count > 0)
                {
                    var upgradablePassives = inv.equippedPassives.FindAll(x => x.level < x.maxLevel);
                    if (upgradablePassives.Count > 0)
                    {
                        var target = upgradablePassives[UnityEngine.Random.Range(0, upgradablePassives.Count)];
                        inv.UpgradePassive(target.passiveID);
                        awardedList.Add(new RouletteReward
                        {
                            title = $"{target.passiveName} (Lv {target.level})",
                            subTitle = target.description,
                            badge = "CỔ VẬT BỊ ĐỘNG",
                            badgeColor = new Color(0.3f, 1f, 0.4f),
                            isEvolution = false
                        });
                        upgraded = true;
                    }
                }

                // If fully maxed, award Royal Gold Sack
                if (!upgraded)
                {
                    int goldDrop = UnityEngine.Random.Range(200, 350);
                    totalGoldAwarded += goldDrop;
                    awardedList.Add(new RouletteReward
                    {
                        title = $"Túi Vàng Hoàng Gia (+{goldDrop} 💰)",
                        subTitle = "Cổ vật và vũ khí đã đạt cảnh giới tối đa, quy đổi thành vàng vĩnh cửu!",
                        badge = "KHO BÁU TIỀN VÀNG",
                        badgeColor = new Color(1f, 0.85f, 0.2f),
                        isEvolution = false
                    });
                }
            }

            // Add Base Boss Gold
            int baseGold = (rewardSlots == 5) ? UnityEngine.Random.Range(800, 1500) :
                           (rewardSlots == 3) ? UnityEngine.Random.Range(350, 600) :
                                                UnityEngine.Random.Range(150, 300);
            totalGoldAwarded += baseGold;
            if (currentPlayer != null)
            {
                currentPlayer.AddGold(totalGoldAwarded);
                currentPlayer.Heal(30f * rewardSlots);
            }

            // Animate Chest Shaking / Floating
            float elapsed = 0f;
            Vector3 startPos = chestImageRT.anchoredPosition;
            while (elapsed < 0.8f)
            {
                elapsed += 0.02f;
                float shake = Mathf.Sin(elapsed * 45f) * 12f * (1f - elapsed / 0.8f);
                chestImageRT.anchoredPosition = startPos + new Vector3(shake, 0f, 0f);
                yield return new WaitForSecondsRealtime(0.02f);
            }
            chestImageRT.anchoredPosition = startPos;

            // Camera shake on Jackpot
            if (rewardSlots == 5 || didEvolve)
            {
                var cameraJuice = Camera.main != null ? Camera.main.GetComponent<Core.CameraJuice>() : null;
                if (cameraJuice != null) cameraJuice.Shake(0.45f, 0.45f);
            }

            // Update UI Header
            if (didEvolve)
            {
                titleText.text = "★ THẦN BINH TỐI THƯỢNG THỨC TỈNH ★";
                titleText.color = new Color(1f, 0.3f, 0.9f);
            }
            else if (rewardSlots == 5)
            {
                titleText.text = "★★★ JACKPOT ĐẠI THẮNG (5 PHẦN THƯỞNG) ★★★";
                titleText.color = new Color(1f, 0.85f, 0.2f);
            }
            else if (rewardSlots == 3)
            {
                titleText.text = "★★ CHIẾN LỢI PHẨM QUÝ HIẾM (3 PHẦN THƯỞNG) ★★";
                titleText.color = new Color(0.3f, 0.9f, 1f);
            }
            else
            {
                titleText.text = "★ CHIẾN LỢI PHẨM BOSS GOLEM ★";
                titleText.color = new Color(1f, 0.85f, 0.25f);
            }

            subTitleText.text = $"Tổng vàng nhận được: +{totalGoldAwarded} 💰 | Hồi phục: +{30 * rewardSlots} HP";

            // Spawn Reward Cards Sequentially with sound ticks
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            for (int i = 0; i < awardedList.Count; i++)
            {
                var r = awardedList[i];
                CreateRewardCardUI(r, defaultFont);

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySfxByName("pickup");
                }
                yield return new WaitForSecondsRealtime(0.18f);
            }

            // Show Claim Button
            if (claimButton != null)
            {
                claimButton.gameObject.SetActive(true);
            }
            isAnimating = false;
        }

        private void CreateRewardCardUI(RouletteReward r, Font font)
        {
            if (rewardsContainer == null) return;

            GameObject cardGO = new GameObject("Reward_Card");
            cardGO.transform.SetParent(rewardsContainer.transform, false);

            RectTransform cardRT = cardGO.AddComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(230f, 260f);

            Image cardBg = cardGO.AddComponent<Image>();
            cardBg.color = r.isEvolution ? new Color(0.22f, 0.08f, 0.25f, 0.95f) : new Color(0.1f, 0.12f, 0.16f, 0.95f);

            Outline cardOutline = cardGO.AddComponent<Outline>();
            cardOutline.effectColor = r.badgeColor;
            cardOutline.effectDistance = new Vector2(2.5f, -2.5f);

            // Badge
            GameObject badgeGO = new GameObject("Badge");
            badgeGO.transform.SetParent(cardGO.transform, false);
            RectTransform badgeRT = badgeGO.AddComponent<RectTransform>();
            badgeRT.anchorMin = new Vector2(0.5f, 1f);
            badgeRT.anchorMax = new Vector2(0.5f, 1f);
            badgeRT.pivot = new Vector2(0.5f, 1f);
            badgeRT.anchoredPosition = new Vector2(0f, -10f);
            badgeRT.sizeDelta = new Vector2(215f, 30f);

            Text badgeTxt = badgeGO.AddComponent<Text>();
            badgeTxt.font = font;
            badgeTxt.fontSize = 15;
            badgeTxt.fontStyle = FontStyle.Bold;
            badgeTxt.alignment = TextAnchor.MiddleCenter;
            badgeTxt.color = r.badgeColor;
            badgeTxt.text = r.badge;

            // Title
            GameObject titleGO = new GameObject("Title");
            titleGO.transform.SetParent(cardGO.transform, false);
            RectTransform titleRT = titleGO.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 1f);
            titleRT.anchorMax = new Vector2(0.5f, 1f);
            titleRT.pivot = new Vector2(0.5f, 1f);
            titleRT.anchoredPosition = new Vector2(0f, -48f);
            titleRT.sizeDelta = new Vector2(215f, 65f);

            Text titleTxt = titleGO.AddComponent<Text>();
            titleTxt.font = font;
            titleTxt.fontSize = 18;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.color = Color.white;
            titleTxt.text = r.title;

            // Description
            GameObject descGO = new GameObject("Desc");
            descGO.transform.SetParent(cardGO.transform, false);
            RectTransform descRT = descGO.AddComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0.5f, 0f);
            descRT.anchorMax = new Vector2(0.5f, 0f);
            descRT.pivot = new Vector2(0.5f, 0f);
            descRT.anchoredPosition = new Vector2(0f, 15f);
            descRT.sizeDelta = new Vector2(215f, 120f);

            Text descTxt = descGO.AddComponent<Text>();
            descTxt.font = font;
            descTxt.fontSize = 14;
            descTxt.alignment = TextAnchor.MiddleCenter;
            descTxt.color = new Color(0.85f, 0.85f, 0.88f);
            descTxt.text = r.subTitle;
        }

        public void ClaimAndClose()
        {
            if (isAnimating) return;

            if (modalPanel != null)
            {
                modalPanel.SetActive(false);
            }

            Time.timeScale = 1f; // Resume gameplay

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfxByName("click");
            }

            if (currentChest != null)
            {
                Destroy(currentChest.gameObject);
                currentChest = null;
            }
        }

        // =========================================================
        // PROCEDURAL CANVAS BUILDER
        // =========================================================
        private void BuildRouletteCanvas()
        {
            if (modalPanel != null) return;

            GameObject canvasGO = new GameObject("BossChestRoulette_Canvas");
            rouletteCanvas = canvasGO.AddComponent<Canvas>();
            rouletteCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            rouletteCanvas.sortingOrder = 600; // Above LevelUp UI

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Modal Background Overlay
            modalPanel = new GameObject("Modal_Panel");
            modalPanel.transform.SetParent(canvasGO.transform, false);
            RectTransform panelRT = modalPanel.AddComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            Image bgImg = modalPanel.AddComponent<Image>();
            bgImg.color = new Color(0.04f, 0.04f, 0.07f, 0.94f);

            // Header Title
            GameObject titleGO = new GameObject("Title");
            titleGO.transform.SetParent(modalPanel.transform, false);
            RectTransform titleRT = titleGO.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 1f);
            titleRT.anchorMax = new Vector2(0.5f, 1f);
            titleRT.pivot = new Vector2(0.5f, 1f);
            titleRT.anchoredPosition = new Vector2(0f, -40f);
            titleRT.sizeDelta = new Vector2(1200f, 75f);

            titleText = titleGO.AddComponent<Text>();
            titleText.font = defaultFont;
            titleText.fontSize = 42;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color(1f, 0.85f, 0.25f);
            titleText.text = "★ CHIẾN LỢI PHẨM BOSS GOLEM ★";

            Outline titleOutline = titleGO.AddComponent<Outline>();
            titleOutline.effectColor = new Color(0.12f, 0.06f, 0.02f, 1f);
            titleOutline.effectDistance = new Vector2(3f, -3f);

            // Subtitle
            GameObject subGO = new GameObject("SubTitle");
            subGO.transform.SetParent(modalPanel.transform, false);
            RectTransform subRT = subGO.AddComponent<RectTransform>();
            subRT.anchorMin = new Vector2(0.5f, 1f);
            subRT.anchorMax = new Vector2(0.5f, 1f);
            subRT.pivot = new Vector2(0.5f, 1f);
            subRT.anchoredPosition = new Vector2(0f, -115f);
            subRT.sizeDelta = new Vector2(1000f, 40f);

            subTitleText = subGO.AddComponent<Text>();
            subTitleText.font = defaultFont;
            subTitleText.fontSize = 22;
            subTitleText.alignment = TextAnchor.MiddleCenter;
            subTitleText.color = new Color(0.85f, 0.85f, 0.9f);
            subTitleText.text = "Tổng vàng: +0 💰";

            // Big Chest Image Centerpiece
            GameObject chestGO = new GameObject("Chest_Image");
            chestGO.transform.SetParent(modalPanel.transform, false);
            chestImageRT = chestGO.AddComponent<RectTransform>();
            chestImageRT.anchorMin = new Vector2(0.5f, 0.5f);
            chestImageRT.anchorMax = new Vector2(0.5f, 0.5f);
            chestImageRT.pivot = new Vector2(0.5f, 0.5f);
            chestImageRT.anchoredPosition = new Vector2(0f, 160f);
            chestImageRT.sizeDelta = new Vector2(380f, 320f);

            chestImage = chestGO.AddComponent<Image>();
            chestImage.sprite = LoadSpriteAtPath("Assets/Sprites/UI/Roulette_Chest.png");
            chestImage.preserveAspect = true;

            var chestJuice = chestGO.AddComponent<UIButtonJuice>();
            chestJuice.enableFloating = true;
            chestJuice.floatingDistance = 8f;
            chestJuice.floatingSpeed = 2f;

            // Rewards Horizontal Container
            GameObject contGO = new GameObject("Rewards_Container");
            contGO.transform.SetParent(modalPanel.transform, false);
            RectTransform contRT = contGO.AddComponent<RectTransform>();
            contRT.anchorMin = new Vector2(0.5f, 0.5f);
            contRT.anchorMax = new Vector2(0.5f, 0.5f);
            contRT.pivot = new Vector2(0.5f, 0.5f);
            contRT.anchoredPosition = new Vector2(0f, -120f);
            contRT.sizeDelta = new Vector2(1250f, 270f);

            HorizontalLayoutGroup hlg = contGO.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            rewardsContainer = contGO;

            // Claim Button
            GameObject btnGO = new GameObject("Btn_Claim");
            btnGO.transform.SetParent(modalPanel.transform, false);
            RectTransform btnRT = btnGO.AddComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.5f, 0f);
            btnRT.anchorMax = new Vector2(0.5f, 0f);
            btnRT.pivot = new Vector2(0.5f, 0f);
            btnRT.anchoredPosition = new Vector2(0f, 40f);
            btnRT.sizeDelta = new Vector2(340f, 65f);

            Image btnImg = btnGO.AddComponent<Image>();
            btnImg.color = new Color(0.9f, 0.72f, 0.18f);

            claimButton = btnGO.AddComponent<Button>();
            claimButton.onClick.AddListener(ClaimAndClose);

            GameObject btnTxtGO = new GameObject("Btn_Text");
            btnTxtGO.transform.SetParent(btnGO.transform, false);
            RectTransform btnTxtRT = btnTxtGO.AddComponent<RectTransform>();
            btnTxtRT.anchorMin = Vector2.zero;
            btnTxtRT.anchorMax = Vector2.one;
            btnTxtRT.offsetMin = Vector2.zero;
            btnTxtRT.offsetMax = Vector2.zero;

            claimButtonText = btnTxtGO.AddComponent<Text>();
            claimButtonText.font = defaultFont;
            claimButtonText.fontSize = 24;
            claimButtonText.fontStyle = FontStyle.Bold;
            claimButtonText.alignment = TextAnchor.MiddleCenter;
            claimButtonText.color = new Color(0.12f, 0.08f, 0.02f);
            claimButtonText.text = "NHẬN CHIẾN LỢI PHẨM";

            var btnJuice = btnGO.AddComponent<UIButtonJuice>();
            btnJuice.hoverScale = 1.06f;
            btnJuice.pressScale = 0.94f;

            modalPanel.SetActive(false);
        }

        private Sprite LoadSpriteAtPath(string path)
        {
#if UNITY_EDITOR
            Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s != null) return s;

            Texture2D tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null)
            {
                tex.filterMode = FilterMode.Point;
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 16f);
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
                        return Sprite.Create(fileTex, new Rect(0, 0, fileTex.width, fileTex.height), new Vector2(0.5f, 0.5f), 16f);
                    }
                }
                catch { }
            }
            return null;
        }
    }
}
