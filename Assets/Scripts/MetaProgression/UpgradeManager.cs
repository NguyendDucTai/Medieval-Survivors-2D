using System;
using System.Collections.Generic;
using UnityEngine;
using VampireSurvivors.Data;

namespace VampireSurvivors.MetaProgression
{
    public class UpgradeManager : MonoBehaviour
    {
        private static UpgradeManager instance;
        public static UpgradeManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("UpgradeManager");
                    instance = go.AddComponent<UpgradeManager>();
                    DontDestroyOnLoad(go);
                    instance.InitializeUpgrades();
                }
                return instance;
            }
        }

        public List<UpgradeData> allUpgrades = new List<UpgradeData>();

        public event Action<string, int> OnUpgradePurchased;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeUpgrades();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void InitializeUpgrades()
        {
            if (allUpgrades != null && allUpgrades.Count > 0) return;

            allUpgrades = new List<UpgradeData>
            {
                new UpgradeData { id = "Vigor", displayName = "Sinh Lực (Vigor)", description = "+10 Máu tối đa mỗi cấp", maxRank = 10, baseCost = 100, costMultiplier = 1.4f, valuePerRank = 10f },
                new UpgradeData { id = "IronSkin", displayName = "Giáp Thép (Iron Skin)", description = "-1 Sát thương nhận vào mỗi cấp", maxRank = 5, baseCost = 250, costMultiplier = 1.7f, valuePerRank = 1f },
                new UpgradeData { id = "Agility", displayName = "Tốc Hành (Agility)", description = "+4% Tốc độ di chuyển mỗi cấp", maxRank = 10, baseCost = 120, costMultiplier = 1.4f, valuePerRank = 0.04f },
                new UpgradeData { id = "Might", displayName = "Uy Lực (Might)", description = "+5% Sát thương toàn vũ khí", maxRank = 10, baseCost = 150, costMultiplier = 1.5f, valuePerRank = 0.05f },
                new UpgradeData { id = "Quickness", displayName = "Thần Tốc (Quickness)", description = "-3% Thời gian hồi chiêu", maxRank = 8, baseCost = 200, costMultiplier = 1.6f, valuePerRank = 0.03f },
                new UpgradeData { id = "Magnetism", displayName = "Hút Ngọc (Magnetism)", description = "+12% Bán kính thu thập ngọc & vàng", maxRank = 10, baseCost = 100, costMultiplier = 1.35f, valuePerRank = 0.12f },
                new UpgradeData { id = "Bounty", displayName = "Thần Tài (Bounty)", description = "+10% Lượng vàng thu thập", maxRank = 10, baseCost = 180, costMultiplier = 1.5f, valuePerRank = 0.10f },
                new UpgradeData { id = "Scholar", displayName = "Học Giả (Scholar)", description = "+8% Điểm kinh nghiệm nhận được", maxRank = 10, baseCost = 150, costMultiplier = 1.45f, valuePerRank = 0.08f },
                new UpgradeData { id = "CriticalStrike", displayName = "Chí Mạng (Crit Strike)", description = "+3% Tỷ lệ chí mạng (x2 sát thương)", maxRank = 10, baseCost = 250, costMultiplier = 1.6f, valuePerRank = 0.03f },
                new UpgradeData { id = "VitalityRegen", displayName = "Hồi Máu (Vitality Regen)", description = "+0.5 Máu hồi phục mỗi giây", maxRank = 5, baseCost = 350, costMultiplier = 1.8f, valuePerRank = 0.5f },
                new UpgradeData { id = "SecondWind", displayName = "Hồi Sinh (Second Wind)", description = "+1 Lần hồi sinh sống lại khi cạn máu", maxRank = 2, baseCost = 1000, costMultiplier = 2.5f, valuePerRank = 1f },
                new UpgradeData { id = "Reroll", displayName = "Cầu May (Reroll)", description = "+1 Lượt đổi thẻ kỹ năng khi lên cấp", maxRank = 3, baseCost = 500, costMultiplier = 2.0f, valuePerRank = 1f }
            };
        }

        public int GetRank(string id)
        {
            if (SaveManager.Instance.Data == null) SaveManager.Instance.Load();
            return SaveManager.Instance.Data.GetUpgradeLevel(id);
        }

        public UpgradeData GetUpgrade(string id)
        {
            InitializeUpgrades();
            return allUpgrades.Find(u => u.id == id);
        }

        public bool CanPurchase(string id)
        {
            UpgradeData up = GetUpgrade(id);
            if (up == null) return false;
            int currentRank = GetRank(id);
            if (currentRank >= up.maxRank) return false;
            int cost = up.GetCostForRank(currentRank);
            return SaveManager.Instance.Data.gold >= cost;
        }

        public bool PurchaseUpgrade(string id)
        {
            UpgradeData up = GetUpgrade(id);
            if (up == null) return false;
            int currentRank = GetRank(id);
            if (currentRank >= up.maxRank) return false;

            int cost = up.GetCostForRank(currentRank);
            if (SaveManager.Instance.SpendGold(cost))
            {
                int newRank = currentRank + 1;
                SaveManager.Instance.Data.SetUpgradeLevel(id, newRank);
                SaveManager.Instance.Save();
                OnUpgradePurchased?.Invoke(id, newRank);
                Debug.Log($"<color=green>[Shop Purchase]</color> {up.displayName} upgraded to Rank {newRank}!");
                return true;
            }
            return false;
        }

        // Calculation Helpers for in-game stats
        public float GetMaxHealthBonus() => GetRank("Vigor") * 10f;
        public float GetArmorBonus() => GetRank("IronSkin") * 1f;
        public float GetMoveSpeedMultiplier() => 1f + (GetRank("Agility") * 0.04f);
        public float GetDamageMultiplier() => 1f + (GetRank("Might") * 0.05f);
        public float GetCooldownReduction() => Mathf.Clamp(GetRank("Quickness") * 0.03f, 0f, 0.4f);
        public float GetPickupRadiusMultiplier() => 1f + (GetRank("Magnetism") * 0.12f);
        public float GetGoldGainMultiplier() => 1f + (GetRank("Bounty") * 0.10f);
        public float GetExpGainMultiplier() => 1f + (GetRank("Scholar") * 0.08f);
        public float GetCritChanceBonus() => GetRank("CriticalStrike") * 0.03f;
        public float GetRegenPerSecond() => GetRank("VitalityRegen") * 0.5f;
        public int GetReviveCount() => GetRank("SecondWind");
        public int GetRerollCount() => GetRank("Reroll");
    }
}
