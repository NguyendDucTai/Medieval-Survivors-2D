using System;
using System.Collections.Generic;
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
            if (panel == null) return;

            panel.SetActive(true);
            Time.timeScale = 0f; // Pause game

            GenerateOptions();
            DisplayOptions();
            UpdateRerollButton();
        }

        public void RerollChoices()
        {
            if (remainingRerolls > 0)
            {
                remainingRerolls--;
                GenerateOptions();
                DisplayOptions();
                UpdateRerollButton();
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

            // 1. Check for Evolutions first (Highest Priority & Excitement!)
            foreach (var w in inv.equippedWeapons)
            {
                if (inv.CanEvolve(w))
                {
                    pool.Add(new UpgradeOption
                    {
                        type = UpgradeOptionType.EvolveWeapon,
                        id = w.weaponID,
                        title = $"★ THẦN BINH: {w.evolutionName} ★",
                        description = "Tiến hoá vũ khí tối thượng! Sức mạnh hủy diệt và hiệu ứng đặc biệt!",
                        badge = "TIẾN HOÁ TỐI THƯỢNG",
                        currentLevel = w.level,
                        nextLevel = w.level
                    });
                }
            }

            // 2. Upgrades for existing weapons
            foreach (var w in inv.equippedWeapons)
            {
                if (w.level < w.maxLevel && !w.isEvolved)
                {
                    pool.Add(new UpgradeOption
                    {
                        type = UpgradeOptionType.UpgradeWeapon,
                        id = w.weaponID,
                        title = $"{w.weaponName} (Lv {w.level + 1})",
                        description = $"+Sát thương, tốc độ bắn và số lượng mục tiêu",
                        badge = "NÂNG CẤP VŨ KHÍ",
                        currentLevel = w.level,
                        nextLevel = w.level + 1
                    });
                }
            }

            // 3. New Weapons if slots available
            if (inv.CanAddWeapon())
            {
                List<string> candidateWeapons = new List<string> { "StormBow", "HolySword", "Fireball", "ArcaneStaff" };
                foreach (var cid in candidateWeapons)
                {
                    if (!inv.HasWeapon(cid))
                    {
                        WeaponData sample = inv.CreateInitialWeapon(cid, null);
                        pool.Add(new UpgradeOption
                        {
                            type = UpgradeOptionType.NewWeapon,
                            id = cid,
                            title = $"Mới: {sample.weaponName}",
                            description = sample.description,
                            badge = "VŨ KHÍ MỚI",
                            currentLevel = 0,
                            nextLevel = 1
                        });
                    }
                }
            }

            // 4. Upgrades for existing passives
            foreach (var p in inv.equippedPassives)
            {
                if (p.level < p.maxLevel)
                {
                    pool.Add(new UpgradeOption
                    {
                        type = UpgradeOptionType.UpgradePassive,
                        id = p.passiveID,
                        title = $"{p.passiveName} (Lv {p.level + 1})",
                        description = p.description,
                        badge = "CỔ VẬT BỊ ĐỘNG",
                        currentLevel = p.level,
                        nextLevel = p.level + 1
                    });
                }
            }

            // 5. New Passives if slots available
            if (inv.CanAddPassive())
            {
                var candidatePassives = GetAvailablePassiveDefinitions();
                foreach (var cp in candidatePassives)
                {
                    if (!inv.HasPassive(cp.passiveID))
                    {
                        pool.Add(new UpgradeOption
                        {
                            type = UpgradeOptionType.NewPassive,
                            id = cp.passiveID,
                            title = $"Cổ vật: {cp.passiveName}",
                            description = cp.description,
                            badge = "CỔ VẬT MỚI",
                            currentLevel = 0,
                            nextLevel = 1
                        });
                    }
                }
            }

            // Fallback: Bag of Gold if pool has less than 3
            while (pool.Count < 3)
            {
                pool.Add(new UpgradeOption
                {
                    type = UpgradeOptionType.GoldBonus,
                    id = "GoldBonus",
                    title = "Túi Tiền Hoàng Gia",
                    description = "Nhận ngay +75 Vàng cho quỹ nâng cấp vĩnh viễn",
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
            PlayerStats p = FindObjectOfType<PlayerStats>();

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
                    if (p != null) p.AddGold(75);
                    break;
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
    }
}
