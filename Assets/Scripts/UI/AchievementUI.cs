using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VampireSurvivors.Data;
using VampireSurvivors.MetaProgression;

namespace VampireSurvivors.UI
{
    /// <summary>
    /// UC-04: Sách Cổ Thư & Hệ Thống Thành Tựu (Grimoire & Milestones Modal).
    /// 3-Tab Interactive Compendium:
    ///   1. Sổ Tay Quái Vật (Bestiary & Slayer Mastery Bonuses +2% / +5% / +10% DMG)
    ///   2. Công Thức Thần Binh (6 Evolution Recipes & Discovery Status)
    ///   3. Thành Tựu Vương Quốc (Milestones & Gold/Hero Rewards)
    /// </summary>
    public class AchievementUI : MonoBehaviour
    {
        public static AchievementUI Instance;

        [Header("UI Containers")]
        public GameObject panel;
        public Button closeButton;
        public Transform itemsScrollContent;

        private Canvas grimoireCanvas;
        private Text summaryHeaderText;
        private Button[] tabButtons = new Button[3];
        private Image[] tabBgImages = new Image[3];
        private Text[] tabTexts = new Text[3];
        private int currentTabIndex = 0; // 0: Bestiary, 1: Evolutions, 2: Achievements

        private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

        public static AchievementUI GetOrCreate()
        {
            if (Instance != null) return Instance;
#if UNITY_6000_0_OR_NEWER
            var found = UnityEngine.Object.FindAnyObjectByType<AchievementUI>();
#else
            var found = UnityEngine.Object.FindObjectOfType<AchievementUI>();
#endif
            if (found != null)
            {
                Instance = found;
                return Instance;
            }

            GameObject go = new GameObject("Grimoire_AchievementUI_Manager");
            Instance = go.AddComponent<AchievementUI>();
            return Instance;
        }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            if (panel == null)
            {
                BuildProceduralGrimoireUI();
            }
            else
            {
                panel.SetActive(false);
            }
        }

        private void Update()
        {
            if (panel != null && panel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            {
                CloseAchievements();
            }
        }

        public void OpenAchievements(int defaultTab = 0)
        {
            if (panel == null || itemsScrollContent == null)
            {
                BuildProceduralGrimoireUI();
            }

            if (panel == null) return;

            if (Audio.AudioManager.Instance != null)
            {
                Audio.AudioManager.Instance.PlaySfxByName("click");
            }

            panel.SetActive(true);
            SwitchTab(Mathf.Clamp(defaultTab, 0, 2));
        }

        public void CloseAchievements()
        {
            if (Audio.AudioManager.Instance != null)
            {
                Audio.AudioManager.Instance.PlaySfxByName("click");
            }

            if (panel != null) panel.SetActive(false);

            // If Game Over summary was open before, keep it visible
            if (RunSummaryUI.Instance != null && RunSummaryUI.Instance.panel != null && Core.GameManager.Instance != null && Core.GameManager.Instance.isGameOver)
            {
                RunSummaryUI.Instance.panel.SetActive(true);
            }
        }

        public void SwitchTab(int tabIndex)
        {
            currentTabIndex = tabIndex;

            for (int i = 0; i < 3; i++)
            {
                if (tabBgImages[i] != null)
                {
                    bool active = (i == currentTabIndex);
                    tabBgImages[i].color = active
                        ? new Color(0.85f, 0.65f, 0.20f, 1f)
                        : new Color(0.22f, 0.14f, 0.08f, 0.92f);
                }
                if (tabTexts[i] != null)
                {
                    tabTexts[i].color = (i == currentTabIndex)
                        ? new Color(0.12f, 0.08f, 0.02f, 1f)
                        : new Color(0.90f, 0.90f, 0.95f, 1f);
                }
            }

            RefreshList();
        }

        public void RefreshList()
        {
            if (itemsScrollContent == null) return;

            foreach (Transform child in itemsScrollContent)
            {
                Destroy(child.gameObject);
            }

            UpdateSummaryHeader();

            if (currentTabIndex == 0)
            {
                PopulateBestiaryTab();
            }
            else if (currentTabIndex == 1)
            {
                PopulateEvolutionRecipesTab();
            }
            else
            {
                PopulateAchievementsTab();
            }
        }

        private void UpdateSummaryHeader()
        {
            if (summaryHeaderText == null) return;

            var data = SaveManager.Instance != null ? SaveManager.Instance.Data : null;
            int totalKills = data != null ? data.lifetimeKills : 0;
            int bossKills = data != null ? data.lifetimeBossKills : 0;
            int totalGold = data != null ? data.gold : 0;
            int achDone = (data != null && data.completedAchievements != null) ? data.completedAchievements.Count : 0;
            int achTotal = (AchievementManager.Instance != null && AchievementManager.Instance.allAchievements != null)
                ? AchievementManager.Instance.allAchievements.Count : 24;

            summaryHeaderText.text =
                $"💀 Tổng Diệt Quái: <color=#FF8888>{totalKills:N0}</color>   |   " +
                $"🗿 Đại Trùm Hạ Gục: <color=#FFAA44>{bossKills}</color>   |   " +
                $"🏆 Thành Tựu: <color=#55FF88>{achDone}/{achTotal}</color>   |   " +
                $"💰 Quỹ Vàng: <color=#FFD700>{totalGold:N0}</color>";
        }

        // =========================================================
        // TAB 0: BESTIARY & SLAYER MASTERY (SỔ TAY QUÁI VẬT)
        // =========================================================
        private struct BestiaryEntry
        {
            public string speciesKey;
            public string displayName;
            public string roleTitle;
            public string spritePath;
            public string lore;
            public int tier1;
            public int tier2;
            public int tier3;
            public string bonusDesc;
        }

        private void PopulateBestiaryTab()
        {
            BestiaryEntry[] entries = new BestiaryEntry[]
            {
                new BestiaryEntry
                {
                    speciesKey = "Slime",
                    displayName = "Slime Rừng Xanh (Forest Slime)",
                    roleTitle = "Quái Vật Khởi Nguyên • Xuất hiện: Phút 00:00+",
                    spritePath = "Assets/Sprites/Enemies/Slime_Green.png",
                    lore = "Sinh vật dịch nhầy ma thuật tràn ra từ Thảo Nguyên Lục Bảo. Di chuyển chậm theo bầy đàn nhưng số lượng cực đông.",
                    tier1 = 100, tier2 = 500, tier3 = 1500,
                    bonusDesc = "Nội tại Thợ Săn: +2% / +5% / +10% Sát thương lên loài Slime"
                },
                new BestiaryEntry
                {
                    speciesKey = "Bat",
                    displayName = "Dơi Huyết Nguyệt (Blood Bat)",
                    roleTitle = "Phi Đội Đột Kích • Xuất hiện: Phút 00:45+ & Đội Hình Tam Giác",
                    spritePath = "Assets/Sprites/Enemies/Bat_Fly.png",
                    lore = "Bay lượn tốc độ cao theo đội hình mũi tên xuyên qua màn hình. Máu giấy nhưng áp sát cực nhanh.",
                    tier1 = 100, tier2 = 500, tier3 = 1500,
                    bonusDesc = "Nội tại Thợ Săn: +2% / +5% / +10% Sát thương lên loài Dơi"
                },
                new BestiaryEntry
                {
                    speciesKey = "Skeleton",
                    displayName = "Chiến Binh Bộ Xương (Undead Skeleton)",
                    roleTitle = "Quân Đoàn Bất Tử • Xuất hiện: Phút 02:30+ & Thủy Triều Xương",
                    spritePath = "Assets/Sprites/Enemies/Skeleton_Walk.png",
                    lore = "Hài cốt của các binh sĩ cổ đại thức tỉnh tại Tàn Tích Lâu Đài. Lì đòn và thường tràn vào từ 4 phía màn hình.",
                    tier1 = 100, tier2 = 500, tier3 = 1500,
                    bonusDesc = "Nội tại Thợ Săn: +2% / +5% / +10% Sát thương lên Bộ Xương"
                },
                new BestiaryEntry
                {
                    speciesKey = "Plant",
                    displayName = "Hoa Quỷ Ăn Thịt (Cursed Man-Eating Flower)",
                    roleTitle = "Kết Giới Vòng Vây • Xuất hiện: Phút 04:00+ (Vòng Vây 360°)",
                    spritePath = "Assets/Sprites/Enemies/Enemy_Plant.png",
                    lore = "Thực vật biến dị mọc thành vòng tròn 360 độ khép chặt dần quanh con mồi. Cần tập trung hỏa lực phá vây để mở đường máu.",
                    tier1 = 100, tier2 = 500, tier3 = 1500,
                    bonusDesc = "Nội tại Thợ Săn: +2% / +5% / +10% Sát thương lên Hoa Quỷ"
                },
                new BestiaryEntry
                {
                    speciesKey = "Goblin",
                    displayName = "Yêu Tinh Cuồng Chiến (Goblin Raider)",
                    roleTitle = "Quân Đoàn Tốc Kích • Xuất hiện: Phút 03:30+ & Đợt Cuồng Nộ",
                    spritePath = "Assets/Sprites/Enemies/Goblin_Walk.png",
                    lore = "Bộ tộc Yêu Tinh hung hãn với tốc độ chạy nhanh và sức công phá lớn, săn lùng vàng bạc trên khắp chiến trường.",
                    tier1 = 100, tier2 = 500, tier3 = 1500,
                    bonusDesc = "Nội tại Thợ Săn: +2% / +5% / +10% Sát thương lên Yêu Tinh"
                },
                new BestiaryEntry
                {
                    speciesKey = "Boss",
                    displayName = "Thạch Quái Golem & Đại Trùm (Colossal Golem & Warlords)",
                    roleTitle = "Đại Trùm Thế Giới • Xuất hiện: Phút 01:30, 05:00, 10:00, 15:00",
                    spritePath = "Assets/Sprites/Enemies/Boss_Golem_Walk.png",
                    lore = "Những kẻ thống lĩnh tối cao mang theo Rương Kho Báu Hoàng Gia (Boss Chest) — chìa khóa duy nhất để thức tỉnh Thần Binh Tiến Hóa!",
                    tier1 = 3, tier2 = 10, tier3 = 25,
                    bonusDesc = "Nội tại Đồ Long: +4% / +8% / +15% Sát thương lên mọi Boss"
                }
            };

            foreach (var entry in entries)
            {
                int kills = SaveManager.Instance != null ? SaveManager.Instance.GetSpeciesKillCount(entry.speciesKey) : 0;
                float mult = SaveManager.Instance != null ? SaveManager.Instance.GetSpeciesDamageMultiplier(entry.speciesKey) : 1f;
                int bonusPct = Mathf.RoundToInt((mult - 1f) * 100f);

                string tierStatus;
                string badgeColor;
                if (kills >= entry.tier3)
                {
                    tierStatus = $"★ BẬC ĐẠI SƯ (+{bonusPct}% SÁT THƯƠNG) — Đã diệt: {kills:N0}";
                    badgeColor = "#FFD700";
                }
                else if (kills >= entry.tier2)
                {
                    tierStatus = $"⚔️ BẬC CAO THỦ (+{bonusPct}% SÁT THƯƠNG) — Tiến độ Bậc 3: {kills:N0}/{entry.tier3:N0}";
                    badgeColor = "#55FF77";
                }
                else if (kills >= entry.tier1)
                {
                    tierStatus = $"🛡️ BẬC THỢ SĂN (+{bonusPct}% SÁT THƯƠNG) — Tiến độ Bậc 2: {kills:N0}/{entry.tier2:N0}";
                    badgeColor = "#66D9FF";
                }
                else
                {
                    tierStatus = $"⏳ TIẾN ĐỘ BẬC 1 (+0% SÁT THƯƠNG): {kills:N0}/{entry.tier1:N0}";
                    badgeColor = "#E0E0E0";
                }

                Sprite icon = (entry.speciesKey == "Slime") ? LoadSpriteAtPath(entry.spritePath) : LoadFirstFrameSprite(entry.spritePath);
                string body =
                    $"<color=#66CCFF><b>{entry.roleTitle}</b></color>\n" +
                    $"<color=#F5F5FA>{entry.lore}</color>\n" +
                    $"<color={badgeColor}><b>{tierStatus}</b></color>  <color=#FFD54F>({entry.bonusDesc})</color>";

                CreateCompendiumCard(
                    "Bestiary_" + entry.speciesKey,
                    icon,
                    entry.displayName,
                    bonusPct > 0 ? $"+{bonusPct}% DMG KHẮC CHẾ" : $"ĐÃ DIỆT: {kills:N0}",
                    bonusPct > 0 ? new Color(0.18f, 0.58f, 0.28f, 0.95f) : new Color(0.24f, 0.28f, 0.38f, 0.95f),
                    body
                );
            }
        }

        // =========================================================
        // TAB 1: WEAPON EVOLUTION RECIPES (CÔNG THỨC THẦN BINH)
        // =========================================================
        private struct EvolutionRecipeCardData
        {
            public string evolvedID;
            public string title;
            public string baseWeaponName;
            public string passiveName;
            public string spritePath;
            public string effectDescription;
        }

        private void PopulateEvolutionRecipesTab()
        {
            EvolutionRecipeCardData[] recipes = new EvolutionRecipeCardData[]
            {
                new EvolutionRecipeCardData
                {
                    evolvedID = "Excalibur",
                    title = "1. Excalibur (Thần Kiếm Vạn Năng)",
                    baseWeaponName = "Holy Broadsword (Lv.5 - Hiệp sĩ Sir Roland)",
                    passiveName = "Iron Armor / Crown (Giáp Thép Hiệp Sĩ)",
                    spritePath = "Assets/Sprites/Weapons/VFX_ExcaliburSlash.png",
                    effectDescription = "Nhát chém Thánh Quang khổng lồ tăng +120% Sát thương, +40% Tốc đánh và quét sạch mọi kẻ địch chắn đường."
                },
                new EvolutionRecipeCardData
                {
                    evolvedID = "MeteorCalamity",
                    title = "2. Meteor Calamity (Hỏa Ngục Thiên Thạch)",
                    baseWeaponName = "Fireball (Lv.5 - Pháp sư Ignis)",
                    passiveName = "Spell Tome (Sách Cổ Ma Thuật)",
                    spritePath = "Assets/Sprites/Weapons/VFX_Meteor.png",
                    effectDescription = "Triệu hồi mưa thiên thạch rực lửa tự tìm mục tiêu, phát nổ diện rộng thiêu rụi cả bầy quái."
                },
                new EvolutionRecipeCardData
                {
                    evolvedID = "CosmicSingularity",
                    title = "3. Cosmic Singularity (Hố Đen Hư Vô)",
                    baseWeaponName = "Lightning Staff (Lv.5 - Đại Pháp sư Aurelius)",
                    passiveName = "Magnet Talisman / Magic Ring (Nam Châm Bí Thuật)",
                    spritePath = "Assets/Sprites/Weapons/VFX_Singularity.png",
                    effectDescription = "Phóng quả cầu lôi điện hư không xuyên thấu hàng loạt mục tiêu với sát thương bùng nổ."
                },
                new EvolutionRecipeCardData
                {
                    evolvedID = "TempestArrow",
                    title = "4. Tempest Arrow (Bão Tố Cuồng Phong)",
                    baseWeaponName = "Wind Bow (Lv.5 - Cung thủ Sylph)",
                    passiveName = "Wind Quiver (Ống Tên Thần Gió)",
                    spritePath = "Assets/Sprites/Weapons/VFX_TempestArrow.png",
                    effectDescription = "Bắn chùm tên cuồng phong siêu tốc xuyên qua nhiều lớp quái vật từ khoảng cách xa."
                },
                new EvolutionRecipeCardData
                {
                    evolvedID = "CrimsonReaper",
                    title = "5. Crimson Reaper (Lưỡi Hái Huyết Nguyệt)",
                    baseWeaponName = "Dark Crescent (Lv.5 - Sát thủ Valen)",
                    passiveName = "Blood Chalice (Chén Máu Huyết Tộc)",
                    spritePath = "Assets/Sprites/Weapons/VFX_CrimsonReaper.png",
                    effectDescription = "Lưỡi hái tử thần chém vòng cung cực rộng, tự động HÚT MÁU hồi phục HP cho người chơi mỗi khi trúng đích."
                },
                new EvolutionRecipeCardData
                {
                    evolvedID = "AstralBarrage",
                    title = "6. Astral Barrage (Mưa Tên Tinh Tú)",
                    baseWeaponName = "Storm Bow (Lv.5 - Xạ thủ Aeris)",
                    passiveName = "Clover of Luck (Cỏ 4 Lá May Mắn)",
                    spritePath = "Assets/Sprites/Weapons/VFX_AstralBarrage.png",
                    effectDescription = "Mưa tên ánh sáng tinh tú tỏa rộng nhiều hướng với tỷ lệ chí mạng và tốc độ bắn tối đa."
                }
            };

            var discovered = (SaveManager.Instance != null && SaveManager.Instance.Data != null && SaveManager.Instance.Data.discoveredEvolutions != null)
                ? SaveManager.Instance.Data.discoveredEvolutions
                : new List<string>();

            foreach (var r in recipes)
            {
                bool isDiscovered = discovered.Contains(r.evolvedID);
                Sprite icon = LoadSpriteAtPath(r.spritePath);

                string body =
                    $"<color=#FFD54F><b>Công thức ghép:</b> [{r.baseWeaponName}]  +  [{r.passiveName}]  +  [Mở Rương Boss Golem]</color>\n" +
                    $"<color=#F5F5FA>{r.effectDescription}</color>";

                CreateCompendiumCard(
                    "Recipe_" + r.evolvedID,
                    icon,
                    r.title,
                    isDiscovered ? "★ ĐÃ KHAI MỞ" : "⚡ CÔNG THỨC TIẾN HÓA",
                    isDiscovered ? new Color(0.60f, 0.25f, 0.75f, 0.95f) : new Color(0.75f, 0.50f, 0.15f, 0.95f),
                    body
                );
            }
        }

        // =========================================================
        // TAB 2: ACHIEVEMENTS & MILESTONES (THÀNH TỰU VƯƠNG QUỐC)
        // =========================================================
        private void PopulateAchievementsTab()
        {
            if (AchievementManager.Instance == null) return;

            var list = AchievementManager.Instance.allAchievements;
            foreach (var ach in list)
            {
                bool isDone = AchievementManager.Instance.IsCompleted(ach.id);
                string rewardHero = !string.IsNullOrEmpty(ach.unlockHeroReward)
                    ? $"  |  🎖️ Mở khóa Tướng: <color=#FFE066>{ach.unlockHeroReward}</color>"
                    : "";

                string progressStr = GetAchievementProgressLabel(ach, isDone);

                string body =
                    $"<color=#F5F5FA>{ach.description}</color>\n" +
                    $"<color=#FFD54F>💰 Phần thưởng: +{ach.goldReward:N0} Vàng{rewardHero}</color>   •   {progressStr}";

                Sprite icon = GetAchievementCategoryIcon(ach.type);

                CreateCompendiumCard(
                    "Ach_" + ach.id,
                    icon,
                    ach.title,
                    isDone ? "✔ ĐÃ HOÀN THÀNH" : "⏳ ĐANG TIẾN HÀNH",
                    isDone ? new Color(0.18f, 0.58f, 0.28f, 0.95f) : new Color(0.32f, 0.25f, 0.25f, 0.95f),
                    body
                );
            }
        }

        private string GetAchievementProgressLabel(AchievementData ach, bool isDone)
        {
            if (isDone) return "<color=#55FF88><b>Đã nhận thưởng!</b></color>";
            var data = SaveManager.Instance != null ? SaveManager.Instance.Data : null;
            if (data == null) return "";

            switch (ach.type)
            {
                case AchievementType.TotalKills:
                    return $"<color=#66CCFF>Tiến độ: {data.lifetimeKills:N0} / {ach.targetValue:N0}</color>";
                case AchievementType.SurvivalTime:
                    int bm = Mathf.FloorToInt(data.bestSurvivalSeconds / 60f);
                    int bs = Mathf.FloorToInt(data.bestSurvivalSeconds % 60f);
                    int tm = Mathf.FloorToInt(ach.targetValue / 60f);
                    return $"<color=#66CCFF>Kỷ lục: {bm:00}:{bs:00} / {tm:00}:00</color>";
                case AchievementType.LevelReached:
                    return $"<color=#66CCFF>Cấp cao nhất: Lv.{data.highestLevelReached} / Lv.{ach.targetValue:F0}</color>";
                case AchievementType.BossDefeated:
                    return $"<color=#66CCFF>Tiến độ: {data.lifetimeBossKills} / {ach.targetValue:F0} Trùm</color>";
                case AchievementType.GoldEarned:
                    int bestGold = Mathf.Max(data.gold, data.lifetimeGoldEarned);
                    return $"<color=#66CCFF>Tiến độ: {bestGold:N0} / {ach.targetValue:N0} Vàng</color>";
                case AchievementType.WeaponEvolved:
                    int count = data.discoveredEvolutions != null ? data.discoveredEvolutions.Count : 0;
                    return $"<color=#66CCFF>Đã khám phá: {count} / {ach.targetValue:F0}</color>";
                default:
                    return "";
            }
        }

        private Sprite GetAchievementCategoryIcon(AchievementType type)
        {
            switch (type)
            {
                case AchievementType.TotalKills:
                    return LoadSpriteAtPath("Assets/Sprites/Weapons/Weapon_HolySword.png");
                case AchievementType.SurvivalTime:
                    return LoadSpriteAtPath("Assets/Sprites/Passives/Passive_IronArmor.png");
                case AchievementType.LevelReached:
                    return LoadSpriteAtPath("Assets/Sprites/Passives/Passive_SpellTome.png");
                case AchievementType.WeaponEvolved:
                    return LoadSpriteAtPath("Assets/Sprites/Weapons/VFX_ExcaliburSlash.png");
                case AchievementType.BossDefeated:
                    return LoadFirstFrameSprite("Assets/Sprites/Enemies/Boss_Golem_Walk.png");
                case AchievementType.GoldEarned:
                default:
                    return LoadSpriteAtPath("Assets/Sprites/Passives/Passive_GreedCoin.png");
            }
        }

        // =========================================================
        // CARD ROW BUILDER (Crystal Clear Typography & High Contrast)
        // =========================================================
        private void CreateCompendiumCard(string name, Sprite icon, string title, string badgeText, Color badgeColor, string bodyRichText)
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GameObject cardGO = new GameObject(name, typeof(RectTransform));
            cardGO.transform.SetParent(itemsScrollContent, false);

            LayoutElement le = cardGO.AddComponent<LayoutElement>();
            le.minHeight = 124f;
            le.preferredHeight = 124f;

            Image cardBg = cardGO.AddComponent<Image>();
            cardBg.color = new Color(0.18f, 0.12f, 0.07f, 0.88f);

            Outline cardOutline = cardGO.AddComponent<Outline>();
            cardOutline.effectColor = new Color(0.65f, 0.52f, 0.25f, 0.9f);
            cardOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // Left Icon Box
            GameObject iconBoxGO = new GameObject("IconBox", typeof(RectTransform));
            iconBoxGO.transform.SetParent(cardGO.transform, false);
            RectTransform iconBoxRT = iconBoxGO.GetComponent<RectTransform>();
            iconBoxRT.anchorMin = new Vector2(0f, 0.5f);
            iconBoxRT.anchorMax = new Vector2(0f, 0.5f);
            iconBoxRT.pivot = new Vector2(0f, 0.5f);
            iconBoxRT.anchoredPosition = new Vector2(14f, 0f);
            iconBoxRT.sizeDelta = new Vector2(84f, 84f);

            Image iconBoxBg = iconBoxGO.AddComponent<Image>();
            iconBoxBg.color = new Color(0.11f, 0.07f, 0.04f, 0.95f);

            Outline iconBoxOutline = iconBoxGO.AddComponent<Outline>();
            iconBoxOutline.effectColor = new Color(0.85f, 0.70f, 0.25f, 0.95f);
            iconBoxOutline.effectDistance = new Vector2(1.5f, -1.5f);

            if (icon != null)
            {
                GameObject iconGO = new GameObject("Icon", typeof(RectTransform));
                iconGO.transform.SetParent(iconBoxGO.transform, false);
                RectTransform iconRT = iconGO.GetComponent<RectTransform>();
                iconRT.anchorMin = new Vector2(0.5f, 0.5f);
                iconRT.anchorMax = new Vector2(0.5f, 0.5f);
                iconRT.pivot = new Vector2(0.5f, 0.5f);
                iconRT.anchoredPosition = Vector2.zero;
                iconRT.sizeDelta = new Vector2(64f, 64f);

                Image iconImg = iconGO.AddComponent<Image>();
                iconImg.sprite = icon;
                iconImg.preserveAspect = true;
                if (icon != null && icon.texture != null) icon.texture.filterMode = FilterMode.Point;
            }

            // Title Text (Top Left next to icon)
            GameObject titleGO = new GameObject("Title", typeof(RectTransform));
            titleGO.transform.SetParent(cardGO.transform, false);
            RectTransform titleRT = titleGO.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0f, 1f);
            titleRT.anchorMax = new Vector2(1f, 1f);
            titleRT.pivot = new Vector2(0f, 1f);
            titleRT.offsetMin = new Vector2(112f, -38f);
            titleRT.offsetMax = new Vector2(-235f, -8f);

            Text titleTxt = titleGO.AddComponent<Text>();
            titleTxt.font = font;
            titleTxt.fontSize = 20;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.alignment = TextAnchor.MiddleLeft;
            titleTxt.color = new Color(1f, 0.92f, 0.45f);
            titleTxt.text = title;

            // Subtle dark shadow instead of heavy outline to keep text crisp
            Shadow titleShadow = titleGO.AddComponent<Shadow>();
            titleShadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
            titleShadow.effectDistance = new Vector2(1.2f, -1.2f);

            // Status Badge (Top Right)
            GameObject badgeGO = new GameObject("Badge", typeof(RectTransform));
            badgeGO.transform.SetParent(cardGO.transform, false);
            RectTransform badgeRT = badgeGO.GetComponent<RectTransform>();
            badgeRT.anchorMin = new Vector2(1f, 1f);
            badgeRT.anchorMax = new Vector2(1f, 1f);
            badgeRT.pivot = new Vector2(1f, 1f);
            badgeRT.anchoredPosition = new Vector2(-14f, -10f);
            badgeRT.sizeDelta = new Vector2(215f, 28f);

            Image badgeImg = badgeGO.AddComponent<Image>();
            badgeImg.color = badgeColor;

            Outline badgeOutline = badgeGO.AddComponent<Outline>();
            badgeOutline.effectColor = new Color(0f, 0f, 0f, 0.6f);
            badgeOutline.effectDistance = new Vector2(1f, -1f);

            GameObject badgeTxtGO = new GameObject("BadgeText", typeof(RectTransform));
            badgeTxtGO.transform.SetParent(badgeGO.transform, false);
            RectTransform badgeTxtRT = badgeTxtGO.GetComponent<RectTransform>();
            badgeTxtRT.anchorMin = Vector2.zero;
            badgeTxtRT.anchorMax = Vector2.one;
            badgeTxtRT.sizeDelta = Vector2.zero;

            Text bTxt = badgeTxtGO.AddComponent<Text>();
            bTxt.font = font;
            bTxt.fontSize = 13;
            bTxt.fontStyle = FontStyle.Bold;
            bTxt.alignment = TextAnchor.MiddleCenter;
            bTxt.color = Color.white;
            bTxt.text = badgeText;

            // Body Description Text (Larger, Crystal Clear Typography)
            GameObject bodyGO = new GameObject("Body", typeof(RectTransform));
            bodyGO.transform.SetParent(cardGO.transform, false);
            RectTransform bodyRT = bodyGO.GetComponent<RectTransform>();
            bodyRT.anchorMin = new Vector2(0f, 0f);
            bodyRT.anchorMax = new Vector2(1f, 1f);
            bodyRT.offsetMin = new Vector2(112f, 10f);
            bodyRT.offsetMax = new Vector2(-16f, -42f);

            Text bodyTxt = bodyGO.AddComponent<Text>();
            bodyTxt.font = font;
            bodyTxt.fontSize = 15;
            bodyTxt.lineSpacing = 1.25f;
            bodyTxt.alignment = TextAnchor.MiddleLeft;
            bodyTxt.color = new Color(0.96f, 0.96f, 0.98f);
            bodyTxt.supportRichText = true;
            bodyTxt.text = bodyRichText;

            Shadow bodyShadow = bodyGO.AddComponent<Shadow>();
            bodyShadow.effectColor = new Color(0f, 0f, 0f, 0.75f);
            bodyShadow.effectDistance = new Vector2(1f, -1f);
        }

        // =========================================================
        // PROCEDURAL GRIMOIRE MODAL BUILDER (Custom Frame Integration)
        // =========================================================
        private void BuildProceduralGrimoireUI()
        {
            if (panel != null && itemsScrollContent != null) return;

            GameObject canvasGO = new GameObject("Grimoire_Canvas");
            grimoireCanvas = canvasGO.AddComponent<Canvas>();
            grimoireCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            grimoireCanvas.sortingOrder = 860; // Above MainMenu (500) and Summary (700)

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            scaler.dynamicPixelsPerUnit = 2.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Dark Vignette Backdrop
            panel = new GameObject("Grimoire_Modal_Panel", typeof(RectTransform));
            panel.transform.SetParent(canvasGO.transform, false);
            RectTransform panelRT = panel.GetComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.sizeDelta = Vector2.zero;

            Image backdropImg = panel.AddComponent<Image>();
            backdropImg.color = new Color(0.02f, 0.02f, 0.04f, 0.88f);

            // Main Book Window Frame (Exact 1024:765 aspect ratio of Frame_Grimoire.png)
            GameObject windowGO = new GameObject("Grimoire_Window", typeof(RectTransform));
            windowGO.transform.SetParent(panel.transform, false);
            RectTransform winRT = windowGO.GetComponent<RectTransform>();
            winRT.anchorMin = new Vector2(0.5f, 0.5f);
            winRT.anchorMax = new Vector2(0.5f, 0.5f);
            winRT.pivot = new Vector2(0.5f, 0.5f);
            winRT.anchoredPosition = Vector2.zero;
            winRT.sizeDelta = new Vector2(1380f, 1030f);

            Image winBg = windowGO.AddComponent<Image>();
            Sprite customFrame = LoadSpriteAtPath("Assets/Sprites/UI/Frame_Grimoire.png");
            if (customFrame == null)
            {
                customFrame = LoadSpriteAtPath("Assets/Sprites/UI/Frame_Options.png");
            }

            if (customFrame != null)
            {
                winBg.sprite = customFrame;
                winBg.color = Color.white;
                winBg.type = Image.Type.Simple;
                winBg.preserveAspect = true;
            }
            else
            {
                winBg.color = new Color(0.14f, 0.10f, 0.07f, 0.98f);
                Outline winOutline = windowGO.AddComponent<Outline>();
                winOutline.effectColor = new Color(0.92f, 0.74f, 0.24f, 1f);
                winOutline.effectDistance = new Vector2(3.5f, -3.5f);
            }

            // Title Header (Positioned neatly at top of parchment pages, below top glowing runes)
            GameObject titleGO = new GameObject("Title", typeof(RectTransform));
            titleGO.transform.SetParent(windowGO.transform, false);
            RectTransform titleRT = titleGO.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 1f);
            titleRT.anchorMax = new Vector2(0.5f, 1f);
            titleRT.pivot = new Vector2(0.5f, 1f);
            titleRT.anchoredPosition = new Vector2(0f, -144f);
            titleRT.sizeDelta = new Vector2(880f, 38f);

            Text titleTxt = titleGO.AddComponent<Text>();
            titleTxt.font = font;
            titleTxt.fontSize = 27;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.color = new Color(1f, 0.90f, 0.35f);
            titleTxt.text = "\uD83D\uDCD6 S\u00C1CH C\u1ED4 TH\u01AF & B\u1EA2NG TH\u00C0NH T\u1EF0U (ROYAL GRIMOIRE)";

            Outline titleOutline = titleGO.AddComponent<Outline>();
            titleOutline.effectColor = new Color(0.12f, 0.06f, 0.02f, 0.98f);
            titleOutline.effectDistance = new Vector2(2f, -2f);

            // Summary Stats Bar
            GameObject summaryGO = new GameObject("SummaryStats", typeof(RectTransform));
            summaryGO.transform.SetParent(windowGO.transform, false);
            RectTransform summaryRT = summaryGO.GetComponent<RectTransform>();
            summaryRT.anchorMin = new Vector2(0.5f, 1f);
            summaryRT.anchorMax = new Vector2(0.5f, 1f);
            summaryRT.pivot = new Vector2(0.5f, 1f);
            summaryRT.anchoredPosition = new Vector2(0f, -184f);
            summaryRT.sizeDelta = new Vector2(880f, 28f);

            summaryHeaderText = summaryGO.AddComponent<Text>();
            summaryHeaderText.font = font;
            summaryHeaderText.fontSize = 16;
            summaryHeaderText.fontStyle = FontStyle.Bold;
            summaryHeaderText.alignment = TextAnchor.MiddleCenter;
            summaryHeaderText.color = Color.white;
            summaryHeaderText.supportRichText = true;

            Outline sumOutline = summaryGO.AddComponent<Outline>();
            sumOutline.effectColor = new Color(0.10f, 0.05f, 0.02f, 0.95f);
            sumOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // 3 Navigation Tabs (Fit strictly within the 884px parchment width)
            string[] tabNames =
            {
                "\uD83D\uDC09 S\u1ED4 TAY QU\u00C1I V\u1EACT (BESTIARY)",
                "\u26A1 C\u00D4NG TH\u1EE8C TH\u1EA6N BINH (RECIPES)",
                "\uD83C\uDFC6 TH\u00C0NH T\u1EF0U & TH\u01AF\u1EDENG (MILESTONES)"
            };

            for (int i = 0; i < 3; i++)
            {
                int tabIdx = i;
                GameObject tabGO = new GameObject($"Tab_{i}", typeof(RectTransform));
                tabGO.transform.SetParent(windowGO.transform, false);
                RectTransform tabRT = tabGO.GetComponent<RectTransform>();
                tabRT.anchorMin = new Vector2(0.5f, 1f);
                tabRT.anchorMax = new Vector2(0.5f, 1f);
                tabRT.pivot = new Vector2(0.5f, 1f);
                tabRT.anchoredPosition = new Vector2((i - 1) * 296f, -216f);
                tabRT.sizeDelta = new Vector2(284f, 40f);

                Image tabImg = tabGO.AddComponent<Image>();
                tabBgImages[i] = tabImg;

                Outline tabOutline = tabGO.AddComponent<Outline>();
                tabOutline.effectColor = new Color(0.85f, 0.68f, 0.22f, 0.9f);
                tabOutline.effectDistance = new Vector2(1.5f, -1.5f);

                Button tBtn = tabGO.AddComponent<Button>();
                tBtn.onClick.AddListener(() =>
                {
                    if (Audio.AudioManager.Instance != null) Audio.AudioManager.Instance.PlaySfxByName("click");
                    SwitchTab(tabIdx);
                });
                tabButtons[i] = tBtn;

                var juice = tabGO.AddComponent<UIButtonJuice>();
                juice.hoverScale = 1.04f;
                juice.pressScale = 0.96f;

                GameObject tabTxtGO = new GameObject("Text", typeof(RectTransform));
                tabTxtGO.transform.SetParent(tabGO.transform, false);
                RectTransform tabTxtRT = tabTxtGO.GetComponent<RectTransform>();
                tabTxtRT.anchorMin = Vector2.zero;
                tabTxtRT.anchorMax = Vector2.one;
                tabTxtRT.sizeDelta = Vector2.zero;

                Text tTxt = tabTxtGO.AddComponent<Text>();
                tTxt.font = font;
                tTxt.fontSize = 14;
                tTxt.fontStyle = FontStyle.Bold;
                tTxt.alignment = TextAnchor.MiddleCenter;
                tTxt.text = tabNames[i];
                tabTexts[i] = tTxt;
            }

            // ScrollView Area (Nested inside parchment pages so glowing runes & parchment texture remain visible!)
            GameObject scrollGO = new GameObject("ScrollView", typeof(RectTransform));
            scrollGO.transform.SetParent(windowGO.transform, false);
            RectTransform scrollRT = scrollGO.GetComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0.5f, 1f);
            scrollRT.anchorMax = new Vector2(0.5f, 1f);
            scrollRT.pivot = new Vector2(0.5f, 1f);
            scrollRT.anchoredPosition = new Vector2(0f, -264f);
            scrollRT.sizeDelta = new Vector2(884f, 566f);

            Image scrollBg = scrollGO.AddComponent<Image>();
            scrollBg.color = new Color(0.10f, 0.06f, 0.03f, 0.12f); // Almost transparent so open book pages show through

            ScrollRect sr = scrollGO.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Clamped;
            sr.scrollSensitivity = 45f;

            // Viewport
            GameObject viewportGO = new GameObject("Viewport", typeof(RectTransform));
            viewportGO.transform.SetParent(scrollGO.transform, false);
            RectTransform vpRT = viewportGO.GetComponent<RectTransform>();
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.offsetMin = new Vector2(4f, 4f);
            vpRT.offsetMax = new Vector2(-4f, -4f);

            Image vpMaskImg = viewportGO.AddComponent<Image>();
            vpMaskImg.color = Color.white;
            Mask vpMask = viewportGO.AddComponent<Mask>();
            vpMask.showMaskGraphic = false;

            sr.viewport = vpRT;

            // Content
            GameObject contentGO = new GameObject("Content", typeof(RectTransform));
            contentGO.transform.SetParent(viewportGO.transform, false);
            RectTransform contentRT = contentGO.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0f, 1f);
            contentRT.anchorMax = new Vector2(1f, 1f);
            contentRT.pivot = new Vector2(0.5f, 1f);
            contentRT.anchoredPosition = Vector2.zero;
            contentRT.sizeDelta = new Vector2(0f, 600f);

            VerticalLayoutGroup vlg = contentGO.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(6, 6, 6, 6);
            vlg.spacing = 10f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            ContentSizeFitter csf = contentGO.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.content = contentRT;
            itemsScrollContent = contentGO.transform;

            // Built-in [X] Close Button (Positioned on top-right brass corner of the book)
            GameObject closeBtnX = new GameObject("Btn_Close_X", typeof(RectTransform));
            closeBtnX.transform.SetParent(windowGO.transform, false);
            RectTransform closeXRT = closeBtnX.GetComponent<RectTransform>();
            closeXRT.anchorMin = new Vector2(1f, 1f);
            closeXRT.anchorMax = new Vector2(1f, 1f);
            closeXRT.pivot = new Vector2(0.5f, 0.5f);
            closeXRT.anchoredPosition = new Vector2(-118f, -92f);
            closeXRT.sizeDelta = new Vector2(56f, 56f);

            Image closeXImg = closeBtnX.AddComponent<Image>();
            Sprite closeSprite = LoadSpriteAtPath("Assets/Sprites/UI/Btn_Close.png");
            if (closeSprite != null)
            {
                closeXImg.sprite = closeSprite;
                closeXImg.preserveAspect = true;
            }
            else
            {
                closeXImg.color = new Color(0.75f, 0.2f, 0.2f, 1f);
            }

            Button btnX = closeBtnX.AddComponent<Button>();
            btnX.onClick.AddListener(CloseAchievements);

            var closeXJuice = closeBtnX.AddComponent<UIButtonJuice>();
            closeXJuice.hoverScale = 1.1f;
            closeXJuice.pressScale = 0.9f;

            // Bottom Close Plaque Button (Positioned on bottom center book clasp)
            GameObject closeGO = new GameObject("Btn_Close", typeof(RectTransform));
            closeGO.transform.SetParent(windowGO.transform, false);
            RectTransform closeRT = closeGO.GetComponent<RectTransform>();
            closeRT.anchorMin = new Vector2(0.5f, 0f);
            closeRT.anchorMax = new Vector2(0.5f, 0f);
            closeRT.pivot = new Vector2(0.5f, 0f);
            closeRT.anchoredPosition = new Vector2(0f, 76f);
            closeRT.sizeDelta = new Vector2(260f, 48f);

            Image closeImg = closeGO.AddComponent<Image>();
            closeImg.color = new Color(0.68f, 0.18f, 0.16f, 0.98f);

            Outline closeOutline = closeGO.AddComponent<Outline>();
            closeOutline.effectColor = new Color(0.95f, 0.80f, 0.30f, 1f);
            closeOutline.effectDistance = new Vector2(2f, -2f);

            closeButton = closeGO.AddComponent<Button>();
            closeButton.onClick.AddListener(CloseAchievements);

            var closeJuice = closeGO.AddComponent<UIButtonJuice>();
            closeJuice.hoverScale = 1.06f;
            closeJuice.pressScale = 0.94f;

            GameObject closeTxtGO = new GameObject("Text", typeof(RectTransform));
            closeTxtGO.transform.SetParent(closeGO.transform, false);
            RectTransform closeTxtRT = closeTxtGO.GetComponent<RectTransform>();
            closeTxtRT.anchorMin = Vector2.zero;
            closeTxtRT.anchorMax = Vector2.one;
            closeTxtRT.sizeDelta = Vector2.zero;

            Text closeTxt = closeTxtGO.AddComponent<Text>();
            closeTxt.font = font;
            closeTxt.fontSize = 18;
            closeTxt.fontStyle = FontStyle.Bold;
            closeTxt.alignment = TextAnchor.MiddleCenter;
            closeTxt.color = Color.white;
            closeTxt.text = "\u0110\u00D3NG C\u1ED4 TH\u01AF [ESC]";

            panel.SetActive(false);
        }

        private Sprite LoadSpriteAtPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (spriteCache.TryGetValue(path, out Sprite cached) && cached != null) return cached;

            // Load fresh disk bytes first for custom Grimoire frame to bypass stale Unity AssetDatabase cache
            if (path.Contains("Grimoire") && File.Exists(path))
            {
                try
                {
                    byte[] freshData = File.ReadAllBytes(path);
                    Texture2D freshTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (freshTex.LoadImage(freshData))
                    {
                        freshTex.filterMode = FilterMode.Bilinear;
                        freshTex.wrapMode = TextureWrapMode.Clamp;
                        Sprite freshSprite = Sprite.Create(freshTex, new Rect(0, 0, freshTex.width, freshTex.height), new Vector2(0.5f, 0.5f), 100f);
                        spriteCache[path] = freshSprite;
                        return freshSprite;
                    }
                }
                catch { }
            }

#if UNITY_EDITOR
            Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s != null)
            {
                spriteCache[path] = s;
                return s;
            }

            Texture2D editorTex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (editorTex != null)
            {
                editorTex.filterMode = FilterMode.Bilinear;
                Sprite editorCreated = Sprite.Create(editorTex, new Rect(0, 0, editorTex.width, editorTex.height), new Vector2(0.5f, 0.5f), 100f);
                spriteCache[path] = editorCreated;
                return editorCreated;
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
                        tex.filterMode = FilterMode.Bilinear;
                        Sprite created = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                        spriteCache[path] = created;
                        return created;
                    }
                }
                catch { }
            }
            return null;
        }

        private Sprite LoadFirstFrameSprite(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            string cacheKey = path + "#frame0";
            if (spriteCache.TryGetValue(cacheKey, out Sprite cached) && cached != null) return cached;

#if UNITY_EDITOR
            var allAssets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
            if (allAssets != null)
            {
                Sprite first = allAssets.OfType<Sprite>().FirstOrDefault();
                if (first != null)
                {
                    spriteCache[cacheKey] = first;
                    return first;
                }
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
                        int frames = Mathf.Max(1, Mathf.RoundToInt((float)tex.width / tex.height));
                        int frameWidth = tex.width / frames;
                        Sprite created = Sprite.Create(tex, new Rect(0, 0, frameWidth, tex.height), new Vector2(0.5f, 0.5f), 16f);
                        spriteCache[cacheKey] = created;
                        return created;
                    }
                }
                catch { }
            }
            return null;
        }
    }
}
