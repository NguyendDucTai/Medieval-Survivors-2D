using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using VampireSurvivors.Player;
using VampireSurvivors.Weapons;

namespace VampireSurvivors.UI
{
    public enum UpgradeOptionType
    {
        NewWeapon,
        UpgradeWeapon,
        EvolveWeapon,
        NewPassive,
        UpgradePassive,
        GoldBonus
    }

    [Serializable]
    public class UpgradeOption
    {
        public UpgradeOptionType type;
        public string id;
        public string title;
        public string description;
        public string badge;
        public int currentLevel;
        public int nextLevel;
    }

    public class LevelUpUI : MonoBehaviour
    {
        public static LevelUpUI Instance;

        [Header("UI Containers")]
        public GameObject panel;
        public Transform cardsContainer;
        public Button rerollButton;
        public Text rerollText;

        [Header("Card UI Elements (3 slots)")]
        public GameObject[] cardSlots;
        public Image[] iconImages;
        public Text[] titleTexts;
        public Text[] descTexts;
        public Text[] badgeTexts;
        public Button[] selectButtons;

        private List<UpgradeOption> currentOptions = new List<UpgradeOption>();
        private int remainingRerolls = 0;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            if (panel != null) panel.SetActive(false);
        }

        private void Start()
        {
            if (MetaProgression.UpgradeManager.Instance != null)
            {
                remainingRerolls = MetaProgression.UpgradeManager.Instance.GetRerollCount();
            }
            UpdateRerollButton();
        }

        public void OpenLevelUp()
        {
            if (panel == null)
            {
                BuildProceduralLevelUpUI();
            }

            if (panel == null) return;

            panel.SetActive(true);
            Time.timeScale = 0f; // Pause game

            GenerateOptions();
            DisplayOptions();
            UpdateRerollButton();

            // Play level up SFX
            if (Audio.AudioManager.Instance != null)
            {
                Audio.AudioManager.Instance.PlaySfxByName("levelup");
            }
        }

        public void RerollChoices()
        {
            if (remainingRerolls > 0)
            {
                remainingRerolls--;
                GenerateOptions();
                DisplayOptions();
                UpdateRerollButton();

                if (Audio.AudioManager.Instance != null)
                {
                    Audio.AudioManager.Instance.PlaySfxByName("click");
                }
            }
        }

        private void UpdateRerollButton()
        {
            if (rerollButton != null)
            {
                rerollButton.interactable = remainingRerolls > 0;
            }
            if (rerollText != null)
            {
                rerollText.text = $"Đổi Lại ({remainingRerolls})";
            }
        }

        public void GenerateOptions()
        {
            currentOptions.Clear();
            WeaponInventory inv = WeaponInventory.Instance;
            if (inv == null) return;

            List<UpgradeOption> pool = new List<UpgradeOption>();

            // 1. Upgrades for existing equipped weapons (Priority)
            foreach (var w in inv.equippedWeapons)
            {
                if (w.level < w.maxLevel && !w.isEvolved)
                {
                    string hint = GetWeaponRecipeHint(w.weaponID, inv);
                    pool.Add(new UpgradeOption
                    {
                        type = UpgradeOptionType.UpgradeWeapon,
                        id = w.weaponID,
                        title = $"{w.weaponName} (Lv {w.level + 1})",
                        description = $"+Sát thương, tốc độ bắn và số mục tiêu xuyên thấu.{hint}",
                        badge = "NÂNG CẤP VŨ KHÍ",
                        currentLevel = w.level,
                        nextLevel = w.level + 1
                    });
                }
            }

            // 2. New Weapons if slots available (Includes all 6 base heroes' weapons)
            if (inv.CanAddWeapon())
            {
                List<string> candidateWeapons = new List<string> { "HolySword", "Fireball", "ArcaneStaff", "BasicBow", "ShadowBlade", "StormBow" };
                foreach (var cid in candidateWeapons)
                {
                    if (!inv.HasWeapon(cid))
                    {
                        WeaponData sample = inv.CreateInitialWeapon(cid, null);
                        string hint = GetWeaponRecipeHint(cid, inv);
                        pool.Add(new UpgradeOption
                        {
                            type = UpgradeOptionType.NewWeapon,
                            id = cid,
                            title = sample.weaponName,
                            description = $"{sample.description}.{hint}",
                            badge = "VŨ KHÍ MỚI",
                            currentLevel = 0,
                            nextLevel = 1
                        });
                    }
                }
            }

            // 3. Upgrades for existing equipped passives
            foreach (var p in inv.equippedPassives)
            {
                if (p.level < p.maxLevel)
                {
                    string hint = GetPassiveRecipeHint(p.passiveID, inv);
                    pool.Add(new UpgradeOption
                    {
                        type = UpgradeOptionType.UpgradePassive,
                        id = p.passiveID,
                        title = $"{p.passiveName} (Lv {p.level + 1})",
                        description = $"{p.description}.{hint}",
                        badge = "CỔ VẬT BỊ ĐỘNG",
                        currentLevel = p.level,
                        nextLevel = p.level + 1
                    });
                }
            }

            // 4. New Passives if slots available
            if (inv.CanAddPassive())
            {
                var candidatePassives = GetAvailablePassiveDefinitions();
                foreach (var cp in candidatePassives)
                {
                    if (!inv.HasPassive(cp.passiveID))
                    {
                        string hint = GetPassiveRecipeHint(cp.passiveID, inv);
                        pool.Add(new UpgradeOption
                        {
                            type = UpgradeOptionType.NewPassive,
                            id = cp.passiveID,
                            title = cp.passiveName,
                            description = $"{cp.description}.{hint}",
                            badge = "CỔ VẬT MỚI",
                            currentLevel = 0,
                            nextLevel = 1
                        });
                    }
                }
            }

            // 5. Fallback: Bag of Gold if pool has less than 3
            while (pool.Count < 3)
            {
                pool.Add(new UpgradeOption
                {
                    type = UpgradeOptionType.GoldBonus,
                    id = "GoldBonus",
                    title = "Túi Tiền Hoàng Gia",
                    description = "Nhận ngay +100 Vàng cho quỹ nâng cấp vĩnh viễn",
                    badge = "TIỀN VÀNG",
                    currentLevel = 1,
                    nextLevel = 1
                });
            }

            // Pick 3 random distinct options
            Shuffle(pool);
            for (int i = 0; i < 3 && i < pool.Count; i++)
            {
                currentOptions.Add(pool[i]);
            }
        }

        private string GetWeaponRecipeHint(string weaponID, WeaponInventory inv)
        {
            string reqPassiveID = "";
            string passiveDisplayName = "";
            string evolvedName = "";

            switch (weaponID)
            {
                case "HolySword":
                    reqPassiveID = "IronArmor";
                    passiveDisplayName = "Giáp Thép Hiệp Sĩ";
                    evolvedName = "Excalibur (Thánh Kiếm)";
                    break;
                case "Fireball":
                    reqPassiveID = "SpellTome";
                    passiveDisplayName = "Sách Cổ Ma Thuật";
                    evolvedName = "Meteor Calamity (Thiên Thạch)";
                    break;
                case "ArcaneStaff":
                    reqPassiveID = "MagnetTalisman";
                    passiveDisplayName = "Nam Châm / Nhẫn Bí Thuật";
                    evolvedName = "Cosmic Singularity (Hố Đen)";
                    break;
                case "BasicBow":
                    reqPassiveID = "WindQuiver";
                    passiveDisplayName = "Ống Tên Thần Gió";
                    evolvedName = "Tempest Arrow (Bão Tố)";
                    break;
                case "ShadowBlade":
                    reqPassiveID = "BloodChalice";
                    passiveDisplayName = "Chén Máu Huyết Tộc";
                    evolvedName = "Crimson Reaper (Lưỡi Hái Máu)";
                    break;
                case "StormBow":
                    reqPassiveID = "CloverOfLuck";
                    passiveDisplayName = "Cỏ 4 Lá May Mắn";
                    evolvedName = "Astral Barrage (Mưa Tên Tinh Tú)";
                    break;
                default:
                    return "";
            }

            bool hasPassive = inv != null && inv.HasPassive(reqPassiveID);
            if (hasPassive)
            {
                return $"\n<color=#55FF55>✔ Đã có [{passiveDisplayName}]\n➔ Sẵn sàng: {evolvedName}</color>";
            }
            else
            {
                return $"\n<color=#FFD700>⚡ Ghép: [{passiveDisplayName}]\n➔ {evolvedName}</color>";
            }
        }

        private string GetPassiveRecipeHint(string passiveID, WeaponInventory inv)
        {
            string compWeaponID = "";
            string weaponDisplayName = "";
            string evolvedName = "";

            switch (passiveID)
            {
                case "IronArmor":
                    compWeaponID = "HolySword";
                    weaponDisplayName = "Holy Broadsword";
                    evolvedName = "Excalibur";
                    break;
                case "SpellTome":
                    compWeaponID = "Fireball";
                    weaponDisplayName = "Fireball";
                    evolvedName = "Meteor Calamity";
                    break;
                case "MagnetTalisman":
                case "MagicRing":
                    compWeaponID = "ArcaneStaff";
                    weaponDisplayName = "Lightning Staff";
                    evolvedName = "Cosmic Singularity";
                    break;
                case "WindQuiver":
                    compWeaponID = "BasicBow";
                    weaponDisplayName = "Wind Bow";
                    evolvedName = "Tempest Arrow";
                    break;
                case "BloodChalice":
                    compWeaponID = "ShadowBlade";
                    weaponDisplayName = "Dark Crescent";
                    evolvedName = "Crimson Reaper";
                    break;
                case "CloverOfLuck":
                    compWeaponID = "StormBow";
                    weaponDisplayName = "Storm Bow";
                    evolvedName = "Astral Barrage";
                    break;
                default:
                    return "";
            }

            bool hasWeapon = inv != null && inv.HasWeapon(compWeaponID);
            if (hasWeapon)
            {
                return $"\n<color=#55FF55>★ Sẵn sàng ghép: [{weaponDisplayName}]\n➔ {evolvedName}!</color>";
            }
            else
            {
                return $"\n<color=#FFCC00>⚡ Cần cho Thần Binh:\n[{weaponDisplayName}]</color>";
            }
        }

        private List<PassiveItemData> GetAvailablePassiveDefinitions()
        {
            return new List<PassiveItemData>
            {
                new PassiveItemData { passiveID = "WindQuiver", passiveName = "Ống Tên Thần Gió", description = "Tăng tốc độ di chuyển và giảm 5% hồi chiêu" },
                new PassiveItemData { passiveID = "IronArmor", passiveName = "Giáp Thép Hiệp Sĩ", description = "Giảm sát thương nhận vào (+1.5 Giáp)" },
                new PassiveItemData { passiveID = "SpellTome", passiveName = "Sách Cổ Ma Thuật", description = "Tăng +12% tổng sát thương vũ khí" },
                new PassiveItemData { passiveID = "MagicRing", passiveName = "Nhẫn Bí Thuật", description = "Tăng bán kính nhặt ngọc & vùng tác dụng" },
                new PassiveItemData { passiveID = "BloodChalice", passiveName = "Chén Máu Huyết Tộc", description = "Tự hồi phục +0.6 Máu mỗi giây" },
                new PassiveItemData { passiveID = "CloverOfLuck", passiveName = "Cỏ 4 Lá May Mắn", description = "Tăng +6% tỷ lệ ra đòn chí mạng" },
                new PassiveItemData { passiveID = "GreedCoin", passiveName = "Đồng Tiền Tham Lam", description = "Tăng +20% lượng vàng nhặt được" },
                new PassiveItemData { passiveID = "CrownOfKings", passiveName = "Vương Miện Trí Tuệ", description = "Tăng +15% kinh nghiệm nhận được" },
                new PassiveItemData { passiveID = "HolyCross", passiveName = "Thánh Giá Cứu Rỗi", description = "+1 Mạng hồi sinh và +20 Máu tối đa" }
            };
        }

        private static readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

        private Sprite LoadSpriteAtPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (spriteCache.TryGetValue(path, out Sprite cached) && cached != null)
                return cached;

#if UNITY_EDITOR
            Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s != null)
            {
                spriteCache[path] = s;
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
                        Sprite created = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                        spriteCache[path] = created;
                        return created;
                    }
                }
                catch { }
            }
            return null;
        }

        private Sprite GetOptionIcon(UpgradeOption opt)
        {
            if (opt == null) return null;

            string path = "";
            switch (opt.id)
            {
                case "HolySword":
                    path = "Assets/Sprites/Weapons/Weapon_HolySword.png";
                    break;
                case "Fireball":
                    path = "Assets/Sprites/Weapons/Weapon_Fireball.png";
                    break;
                case "ArcaneStaff":
                    path = "Assets/Sprites/Weapons/Weapon_MagicStaff.png";
                    break;
                case "BasicBow":
                    path = "Assets/Sprites/Weapons/Weapon_Arrow.png";
                    break;
                case "ShadowBlade":
                    path = "Assets/Sprites/Weapons/VFX_ShadowSlash.png";
                    break;
                case "StormBow":
                    path = "Assets/Sprites/Weapons/Weapon_StormBow.png";
                    break;
                case "Excalibur":
                    path = "Assets/Sprites/Weapons/VFX_ExcaliburSlash.png";
                    break;
                case "Meteor":
                    path = "Assets/Sprites/Weapons/VFX_Meteor.png";
                    break;
                case "Cosmic Singularity":
                case "Singularity":
                    path = "Assets/Sprites/Weapons/VFX_Singularity.png";
                    break;
                case "Tempest Arrow":
                    path = "Assets/Sprites/Weapons/VFX_TempestArrow.png";
                    break;
                case "Crimson Reaper":
                    path = "Assets/Sprites/Weapons/VFX_CrimsonReaper.png";
                    break;
                case "Astral Barrage":
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
                case "GoldBonus":
                    path = "Assets/Sprites/Passives/Passive_GreedCoin.png";
                    break;
                default:
                    path = "Assets/Sprites/Passives/Passive_GreedCoin.png";
                    break;
            }

            return LoadSpriteAtPath(path);
        }

        private void DisplayOptions()
        {
            for (int i = 0; i < 3; i++)
            {
                int index = i;
                if (i < currentOptions.Count)
                {
                    var opt = currentOptions[i];
                    if (cardSlots != null && i < cardSlots.Length && cardSlots[i] != null)
                        cardSlots[i].SetActive(true);

                    if (iconImages != null && i < iconImages.Length && iconImages[i] != null)
                    {
                        Sprite iconSpr = GetOptionIcon(opt);
                        iconImages[i].sprite = iconSpr;
                        iconImages[i].enabled = (iconSpr != null);
                    }

                    if (titleTexts != null && i < titleTexts.Length && titleTexts[i] != null)
                        titleTexts[i].text = opt.title;

                    if (descTexts != null && i < descTexts.Length && descTexts[i] != null)
                        descTexts[i].text = opt.description;

                    if (badgeTexts != null && i < badgeTexts.Length && badgeTexts[i] != null)
                        badgeTexts[i].text = opt.badge;

                    if (selectButtons != null && i < selectButtons.Length && selectButtons[i] != null)
                    {
                        selectButtons[i].onClick.RemoveAllListeners();
                        selectButtons[i].onClick.AddListener(() => OnOptionSelected(index));
                    }
                }
                else
                {
                    if (cardSlots != null && i < cardSlots.Length && cardSlots[i] != null)
                        cardSlots[i].SetActive(false);
                }
            }
        }

        public void OnOptionSelected(int index)
        {
            if (index < 0 || index >= currentOptions.Count) return;
            UpgradeOption opt = currentOptions[index];
            WeaponInventory inv = WeaponInventory.Instance;
            PlayerStats p = null;
#if UNITY_6000_0_OR_NEWER
            p = UnityEngine.Object.FindAnyObjectByType<PlayerStats>();
#else
            p = UnityEngine.Object.FindObjectOfType<PlayerStats>();
#endif

            switch (opt.type)
            {
                case UpgradeOptionType.NewWeapon:
                    if (inv != null) inv.AddWeapon(inv.CreateInitialWeapon(opt.id, null));
                    break;
                case UpgradeOptionType.UpgradeWeapon:
                    if (inv != null) inv.UpgradeWeapon(opt.id);
                    break;
                case UpgradeOptionType.EvolveWeapon:
                    if (inv != null) inv.EvolveWeapon(opt.id);
                    break;
                case UpgradeOptionType.NewPassive:
                    if (inv != null) inv.AddPassive(new PassiveItemData { passiveID = opt.id, passiveName = opt.title, description = opt.description, level = 1 });
                    break;
                case UpgradeOptionType.UpgradePassive:
                    if (inv != null) inv.UpgradePassive(opt.id);
                    break;
                case UpgradeOptionType.GoldBonus:
                    if (p != null) p.AddGold(100);
                    break;
            }

            if (Audio.AudioManager.Instance != null)
            {
                Audio.AudioManager.Instance.PlaySfxByName("pickup");
            }

            CloseLevelUp();
        }

        public void CloseLevelUp()
        {
            if (panel != null) panel.SetActive(false);
            Time.timeScale = 1f; // Resume game
        }

        private void Shuffle<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int r = UnityEngine.Random.Range(i, list.Count);
                T temp = list[i];
                list[i] = list[r];
                list[r] = temp;
            }
        }

        // =========================================================
        // PROCEDURAL UI BUILDER (Guarantees zero scene setup needed)
        // =========================================================
        private void BuildProceduralLevelUpUI()
        {
            Canvas targetCanvas = null;
#if UNITY_6000_0_OR_NEWER
            Canvas[] allCanvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
#else
            Canvas[] allCanvases = UnityEngine.Object.FindObjectsOfType<Canvas>();
#endif
            foreach (var c in allCanvases)
            {
                if (c.renderMode == RenderMode.ScreenSpaceOverlay && !c.gameObject.name.Contains("World"))
                {
                    targetCanvas = c;
                    break;
                }
            }

            if (targetCanvas == null)
            {
                GameObject canvasGO = new GameObject("LevelUp_Canvas");
                targetCanvas = canvasGO.AddComponent<Canvas>();
                targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                targetCanvas.sortingOrder = 550;

                CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                canvasGO.AddComponent<GraphicRaycaster>();
            }

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Main Modal Panel
            panel = new GameObject("LevelUp_ModalPanel");
            panel.transform.SetParent(targetCanvas.transform, false);
            RectTransform panelRT = panel.AddComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            Image bgOverlay = panel.AddComponent<Image>();
            bgOverlay.color = new Color(0.04f, 0.04f, 0.06f, 0.88f);

            // Modal Header
            GameObject headerGO = new GameObject("LevelUp_Header");
            headerGO.transform.SetParent(panel.transform, false);
            RectTransform headerRT = headerGO.AddComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0.5f, 1f);
            headerRT.anchorMax = new Vector2(0.5f, 1f);
            headerRT.pivot = new Vector2(0.5f, 1f);
            headerRT.anchoredPosition = new Vector2(0f, -50f);
            headerRT.sizeDelta = new Vector2(900f, 90f);

            Text headerTxt = headerGO.AddComponent<Text>();
            headerTxt.font = defaultFont;
            headerTxt.fontSize = 42;
            headerTxt.fontStyle = FontStyle.Bold;
            headerTxt.alignment = TextAnchor.MiddleCenter;
            headerTxt.color = new Color(1f, 0.85f, 0.25f);
            headerTxt.text = "★ CHỌN KỸ NĂNG THĂNG CẤP ★";

            Outline headerOutline = headerGO.AddComponent<Outline>();
            headerOutline.effectColor = new Color(0.15f, 0.08f, 0.02f, 1f);
            headerOutline.effectDistance = new Vector2(2f, -2f);

            // Subtitle
            GameObject subHeaderGO = new GameObject("LevelUp_SubHeader");
            subHeaderGO.transform.SetParent(panel.transform, false);
            RectTransform subRT = subHeaderGO.AddComponent<RectTransform>();
            subRT.anchorMin = new Vector2(0.5f, 1f);
            subRT.anchorMax = new Vector2(0.5f, 1f);
            subRT.pivot = new Vector2(0.5f, 1f);
            subRT.anchoredPosition = new Vector2(0f, -125f);
            subRT.sizeDelta = new Vector2(900f, 40f);

            Text subTxt = subHeaderGO.AddComponent<Text>();
            subTxt.font = defaultFont;
            subTxt.fontSize = 20;
            subTxt.alignment = TextAnchor.MiddleCenter;
            subTxt.color = new Color(0.85f, 0.85f, 0.88f);
            subTxt.text = "Lựa chọn 1 trong 3 nâng cấp để cường hóa sức mạnh sinh tồn";

            // Cards Container
            GameObject containerGO = new GameObject("Cards_Container");
            containerGO.transform.SetParent(panel.transform, false);
            RectTransform containerRT = containerGO.AddComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0.5f, 0.5f);
            containerRT.anchorMax = new Vector2(0.5f, 0.5f);
            containerRT.pivot = new Vector2(0.5f, 0.5f);
            containerRT.anchoredPosition = new Vector2(0f, -20f);
            containerRT.sizeDelta = new Vector2(1300f, 560f);
            cardsContainer = containerGO.transform;

            cardSlots = new GameObject[3];
            iconImages = new Image[3];
            titleTexts = new Text[3];
            descTexts = new Text[3];
            badgeTexts = new Text[3];
            selectButtons = new Button[3];

            float cardWidth = 380f;
            float cardHeight = 582f;
            float spacing = 415f;
            float startX = -spacing;

            Sprite cardFrameSprite = LoadSpriteAtPath("Assets/Sprites/UI/Frame_SkillCard.png");

            for (int i = 0; i < 3; i++)
            {
                int slotIndex = i;
                GameObject cardGO = new GameObject($"Card_Slot_{i}");
                cardGO.transform.SetParent(containerGO.transform, false);
                RectTransform cardRT = cardGO.AddComponent<RectTransform>();
                cardRT.anchorMin = new Vector2(0.5f, 0.5f);
                cardRT.anchorMax = new Vector2(0.5f, 0.5f);
                cardRT.pivot = new Vector2(0.5f, 0.5f);
                cardRT.anchoredPosition = new Vector2(startX + (i * spacing), 0f);
                cardRT.sizeDelta = new Vector2(cardWidth, cardHeight);

                // 1. Card Base Frame
                Image cardBg = cardGO.AddComponent<Image>();
                if (cardFrameSprite != null)
                {
                    cardBg.sprite = cardFrameSprite;
                    cardBg.color = Color.white;
                    cardBg.preserveAspect = true;
                }
                else
                {
                    cardBg.color = new Color(0.10f, 0.08f, 0.14f, 0.98f);
                    Outline cardOutline = cardGO.AddComponent<Outline>();
                    cardOutline.effectColor = new Color(0.92f, 0.76f, 0.25f, 0.95f);
                    cardOutline.effectDistance = new Vector2(3f, -3f);
                }

                // Make entire card clickable for instant intuitive feel
                Button cardBtn = cardGO.AddComponent<Button>();
                cardBtn.onClick.AddListener(() => OnOptionSelected(slotIndex));

                // Juice Hover & Press on Card
                var juice = cardGO.AddComponent<UIButtonJuice>();
                juice.hoverScale = 1.04f;
                juice.pressScale = 0.96f;

                // 2. Top Badge (Tag Plaque inside the ornate banner)
                GameObject badgeGO = new GameObject("Badge");
                badgeGO.transform.SetParent(cardGO.transform, false);
                RectTransform badgeRT = badgeGO.AddComponent<RectTransform>();
                badgeRT.anchorMin = new Vector2(0.5f, 1f);
                badgeRT.anchorMax = new Vector2(0.5f, 1f);
                badgeRT.pivot = new Vector2(0.5f, 1f);
                badgeRT.anchoredPosition = new Vector2(0f, -70f);
                badgeRT.sizeDelta = new Vector2(210f, 26f);

                GameObject badgeTxtGO = new GameObject("BadgeText");
                badgeTxtGO.transform.SetParent(badgeGO.transform, false);
                RectTransform bTxtRT = badgeTxtGO.AddComponent<RectTransform>();
                bTxtRT.anchorMin = Vector2.zero;
                bTxtRT.anchorMax = Vector2.one;
                bTxtRT.offsetMin = Vector2.zero;
                bTxtRT.offsetMax = Vector2.zero;

                Text bTxt = badgeTxtGO.AddComponent<Text>();
                bTxt.font = defaultFont;
                bTxt.fontSize = 14;
                bTxt.fontStyle = FontStyle.Bold;
                bTxt.alignment = TextAnchor.MiddleCenter;
                bTxt.color = new Color(1f, 0.88f, 0.35f);
                Outline bOutline = badgeTxtGO.AddComponent<Outline>();
                bOutline.effectColor = new Color(0.12f, 0.08f, 0.02f, 0.95f);
                bOutline.effectDistance = new Vector2(1f, -1f);
                badgeTexts[i] = bTxt;

                // 3. Upper Window: Icon Display (Animated floating weapon / relic)
                GameObject iconGO = new GameObject("Icon");
                iconGO.transform.SetParent(cardGO.transform, false);
                RectTransform iconRT = iconGO.AddComponent<RectTransform>();
                iconRT.anchorMin = new Vector2(0.5f, 0.5f);
                iconRT.anchorMax = new Vector2(0.5f, 0.5f);
                iconRT.pivot = new Vector2(0.5f, 0.5f);
                iconRT.anchoredPosition = new Vector2(0f, 75f);
                iconRT.sizeDelta = new Vector2(90f, 90f);

                Image iconImg = iconGO.AddComponent<Image>();
                iconImg.preserveAspect = true;
                iconImages[i] = iconImg;

                var iconJuice = iconGO.AddComponent<UIButtonJuice>();
                iconJuice.enableFloating = true;
                iconJuice.floatingDistance = 3f;
                iconJuice.floatingSpeed = 2f;

                // 4. Title (Placed neatly below icon inside upper window)
                GameObject titleGO = new GameObject("Title");
                titleGO.transform.SetParent(cardGO.transform, false);
                RectTransform titleRT = titleGO.AddComponent<RectTransform>();
                titleRT.anchorMin = new Vector2(0.5f, 0.5f);
                titleRT.anchorMax = new Vector2(0.5f, 0.5f);
                titleRT.pivot = new Vector2(0.5f, 0.5f);
                titleRT.anchoredPosition = new Vector2(0f, -22f);
                titleRT.sizeDelta = new Vector2(250f, 36f);

                Text tTxt = titleGO.AddComponent<Text>();
                tTxt.font = defaultFont;
                tTxt.fontSize = 18;
                tTxt.fontStyle = FontStyle.Bold;
                tTxt.alignment = TextAnchor.MiddleCenter;
                tTxt.color = Color.white;
                Outline titleOutline = titleGO.AddComponent<Outline>();
                titleOutline.effectColor = new Color(0f, 0f, 0f, 0.95f);
                titleOutline.effectDistance = new Vector2(1.5f, -1.5f);
                titleTexts[i] = tTxt;

                // 5. Lower Window: Description & Evolution Recipe
                GameObject descGO = new GameObject("Description");
                descGO.transform.SetParent(cardGO.transform, false);
                RectTransform descRT = descGO.AddComponent<RectTransform>();
                descRT.anchorMin = new Vector2(0.5f, 0.5f);
                descRT.anchorMax = new Vector2(0.5f, 0.5f);
                descRT.pivot = new Vector2(0.5f, 0.5f);
                descRT.anchoredPosition = new Vector2(0f, -174f);
                descRT.sizeDelta = new Vector2(245f, 115f);

                Text dTxt = descGO.AddComponent<Text>();
                dTxt.font = defaultFont;
                dTxt.fontSize = 13;
                dTxt.lineSpacing = 1.15f;
                dTxt.alignment = TextAnchor.MiddleCenter;
                dTxt.color = new Color(0.96f, 0.93f, 0.86f);
                dTxt.supportRichText = true;
                Outline descOutline = descGO.AddComponent<Outline>();
                descOutline.effectColor = new Color(0.08f, 0.06f, 0.04f, 0.95f);
                descOutline.effectDistance = new Vector2(1f, -1f);
                descTexts[i] = dTxt;

                // 6. Select Button
                GameObject btnGO = new GameObject("Btn_Select");
                btnGO.transform.SetParent(cardGO.transform, false);
                RectTransform btnRT = btnGO.AddComponent<RectTransform>();
                btnRT.anchorMin = new Vector2(0.5f, 0f);
                btnRT.anchorMax = new Vector2(0.5f, 0f);
                btnRT.pivot = new Vector2(0.5f, 0f);
                btnRT.anchoredPosition = new Vector2(0f, 22f);
                btnRT.sizeDelta = new Vector2(160f, 36f);

                Image btnImg = btnGO.AddComponent<Image>();
                btnImg.color = new Color(0.82f, 0.62f, 0.18f);

                Outline btnOutline = btnGO.AddComponent<Outline>();
                btnOutline.effectColor = new Color(0.25f, 0.16f, 0.06f, 0.95f);
                btnOutline.effectDistance = new Vector2(1.5f, -1.5f);

                Button btn = btnGO.AddComponent<Button>();
                var btnColors = btn.colors;
                btnColors.highlightedColor = new Color(1f, 0.88f, 0.35f);
                btnColors.pressedColor = new Color(0.65f, 0.45f, 0.10f);
                btn.colors = btnColors;
                btn.onClick.AddListener(() => OnOptionSelected(slotIndex));
                selectButtons[i] = btn;

                GameObject btnTxtGO = new GameObject("Btn_Text");
                btnTxtGO.transform.SetParent(btnGO.transform, false);
                RectTransform btnTxtRT = btnTxtGO.AddComponent<RectTransform>();
                btnTxtRT.anchorMin = Vector2.zero;
                btnTxtRT.anchorMax = Vector2.one;
                btnTxtRT.offsetMin = Vector2.zero;
                btnTxtRT.offsetMax = Vector2.zero;

                Text btnTxt = btnTxtGO.AddComponent<Text>();
                btnTxt.font = defaultFont;
                btnTxt.fontSize = 16;
                btnTxt.fontStyle = FontStyle.Bold;
                btnTxt.alignment = TextAnchor.MiddleCenter;
                btnTxt.color = new Color(0.12f, 0.08f, 0.02f);
                btnTxt.text = "CHỌN";

                Outline btnTxtOutline = btnTxtGO.AddComponent<Outline>();
                btnTxtOutline.effectColor = new Color(1f, 0.92f, 0.65f, 0.75f);
                btnTxtOutline.effectDistance = new Vector2(0.8f, -0.8f);

                cardSlots[i] = cardGO;
            }

            // Reroll Button
            GameObject rerollGO = new GameObject("Btn_Reroll");
            rerollGO.transform.SetParent(panel.transform, false);
            RectTransform rerollRT = rerollGO.AddComponent<RectTransform>();
            rerollRT.anchorMin = new Vector2(0.5f, 0f);
            rerollRT.anchorMax = new Vector2(0.5f, 0f);
            rerollRT.pivot = new Vector2(0.5f, 0f);
            rerollRT.anchoredPosition = new Vector2(0f, 40f);
            rerollRT.sizeDelta = new Vector2(260f, 55f);

            Image rerollImg = rerollGO.AddComponent<Image>();
            rerollImg.color = new Color(0.2f, 0.4f, 0.65f);

            rerollButton = rerollGO.AddComponent<Button>();
            rerollButton.onClick.AddListener(RerollChoices);

            GameObject rerollTxtGO = new GameObject("Reroll_Text");
            rerollTxtGO.transform.SetParent(rerollGO.transform, false);
            RectTransform rerollTxtRT = rerollTxtGO.AddComponent<RectTransform>();
            rerollTxtRT.anchorMin = Vector2.zero;
            rerollTxtRT.anchorMax = Vector2.one;
            rerollTxtRT.offsetMin = Vector2.zero;
            rerollTxtRT.offsetMax = Vector2.zero;

            rerollText = rerollTxtGO.AddComponent<Text>();
            rerollText.font = defaultFont;
            rerollText.fontSize = 20;
            rerollText.fontStyle = FontStyle.Bold;
            rerollText.alignment = TextAnchor.MiddleCenter;
            rerollText.color = Color.white;
            rerollText.text = "Đổi Lại (0)";

            var rerollJuice = rerollGO.AddComponent<UIButtonJuice>();
            rerollJuice.hoverScale = 1.05f;
            rerollJuice.pressScale = 0.95f;
        }
    }
}
