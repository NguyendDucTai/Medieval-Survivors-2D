using System;
using System.Collections.Generic;
using UnityEngine;

namespace VampireSurvivors.Data
{
    public class SaveManager : MonoBehaviour
    {
        private static SaveManager instance;
        public static SaveManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("SaveManager");
                    instance = go.AddComponent<SaveManager>();
                    DontDestroyOnLoad(go);
                    instance.Load();
                }
                return instance;
            }
        }

        private const string SAVE_KEY = "PRU213_MEDIEVAL_SURVIVORS_SAVE";

        public GameSaveData Data { get; private set; }

        public event Action<int> OnGoldChanged;
        public event Action OnDataSaved;
        public event Action<string> OnHeroUnlocked;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                Load();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void Load()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                try
                {
                    string json = PlayerPrefs.GetString(SAVE_KEY);
                    Data = JsonUtility.FromJson<GameSaveData>(json);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[SaveManager] Failed to parse save json: " + e.Message);
                    Data = new GameSaveData();
                }
            }

            if (Data == null)
            {
                Data = new GameSaveData();
                // Bonus starting gold for fresh player
                Data.gold = 150;
                Data.lifetimeGoldEarned = 150;
                Save();
            }

            if (Data.discoveredEvolutions == null) Data.discoveredEvolutions = new List<string>();
            if (Data.completedAchievements == null) Data.completedAchievements = new List<string>();

            SanitizeHeroUnlocks();
        }

        private void SanitizeHeroUnlocks()
        {
            if (Data.unlockedHeroes == null) Data.unlockedHeroes = new List<string>();

            // Always ensure the 3 starters are unlocked
            if (!Data.unlockedHeroes.Contains("Knight")) Data.unlockedHeroes.Add("Knight");
            if (!Data.unlockedHeroes.Contains("Mage")) Data.unlockedHeroes.Add("Mage");
            if (!Data.unlockedHeroes.Contains("Ranger")) Data.unlockedHeroes.Add("Ranger");

            // Migration check V2: enforce lock on Valen, Aurelius, and Aeris unless criteria are met
            if (PlayerPrefs.GetInt("HERO_UNLOCK_SYSTEM_MIGRATION_V2", 0) == 0)
            {
                PlayerPrefs.SetInt("HERO_UNLOCK_SYSTEM_MIGRATION_V2", 1);
                PlayerPrefs.Save();

                // If player hadn't survived 10 min, lock Valen
                if (Data.bestSurvivalSeconds < 600f)
                {
                    Data.unlockedHeroes.Remove("ShadowKnight");
                    Data.unlockedHeroes.Remove("Rogue");
                }

                // If player hadn't killed 5 bosses, lock Aurelius
                if (Data.lifetimeBossKills < 5)
                {
                    Data.unlockedHeroes.Remove("Archmage");
                }

                // If player hadn't reached 50,000 gold, lock Aeris
                if (Data.gold < 50000 && Data.lifetimeGoldEarned < 50000)
                {
                    Data.unlockedHeroes.Remove("Archer");
                }

                Save();
            }

            // Continuous lock integrity: ensure non-starter heroes aren't unlocked prematurely
            if (Data.bestSurvivalSeconds < 600f)
            {
                Data.unlockedHeroes.Remove("ShadowKnight");
                Data.unlockedHeroes.Remove("Rogue");
            }
            if (Data.lifetimeBossKills < 5)
            {
                Data.unlockedHeroes.Remove("Archmage");
            }
            // Aeris: requires direct purchase with gold
            bool isAerisPurchased = (Data.purchasedHeroes != null && Data.purchasedHeroes.Contains("Archer"));
            if (!isAerisPurchased)
            {
                Data.unlockedHeroes.Remove("Archer");
            }

            // In case criteria were achieved, unlock them
            CheckHeroUnlocks();
        }

        public void Save()
        {
            if (Data == null) return;
            string json = JsonUtility.ToJson(Data, false);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
            OnDataSaved?.Invoke();
        }

        public void AddGold(int amount)
        {
            if (Data == null) Load();
            Data.gold += amount;
            if (amount > 0) Data.lifetimeGoldEarned += amount;
            if (Data.gold < 0) Data.gold = 0;
            Save();
            OnGoldChanged?.Invoke(Data.gold);
            CheckHeroUnlocks();
        }

        public bool SpendGold(int amount)
        {
            if (Data == null) Load();
            if (Data.gold >= amount)
            {
                Data.gold -= amount;
                Save();
                OnGoldChanged?.Invoke(Data.gold);
                return true;
            }
            return false;
        }

        public void RecordBossKill()
        {
            if (Data == null) Load();
            Data.lifetimeBossKills++;
            Save();
            CheckHeroUnlocks();
        }

        public void RecordSpeciesKill(string speciesKey)
        {
            if (Data == null) Load();
            switch (speciesKey)
            {
                case "Slime":
                    Data.slimeKills++;
                    break;
                case "Bat":
                    Data.batKills++;
                    break;
                case "Skeleton":
                    Data.skeletonKills++;
                    break;
                case "Goblin":
                    Data.goblinKills++;
                    break;
                case "Plant":
                    Data.plantKills++;
                    break;
            }
        }

        public int GetSpeciesKillCount(string speciesKey)
        {
            if (Data == null) Load();
            switch (speciesKey)
            {
                case "Slime": return Data.slimeKills;
                case "Bat": return Data.batKills;
                case "Skeleton": return Data.skeletonKills;
                case "Goblin": return Data.goblinKills;
                case "Plant": return Data.plantKills;
                case "Boss": return Data.lifetimeBossKills;
                default: return 0;
            }
        }

        /// <summary>
        /// UC-04 Grimoire Slayer Mastery Bonus:
        /// Tier 1 (100 kills): +2% damage
        /// Tier 2 (500 kills): +5% damage
        /// Tier 3 (1,500 kills): +10% damage
        /// For Bosses: Tier 1 (3 kills): +4%, Tier 2 (10 kills): +8%, Tier 3 (25 kills): +15%
        /// </summary>
        public float GetSpeciesDamageMultiplier(string speciesKey)
        {
            int kills = GetSpeciesKillCount(speciesKey);
            if (speciesKey == "Boss")
            {
                if (kills >= 25) return 1.15f;
                if (kills >= 10) return 1.08f;
                if (kills >= 3) return 1.04f;
                return 1.0f;
            }

            if (kills >= 1500) return 1.10f;
            if (kills >= 500) return 1.05f;
            if (kills >= 100) return 1.02f;
            return 1.0f;
        }

        public void RecordEvolutionDiscovered(string evolvedWeaponID)
        {
            if (string.IsNullOrEmpty(evolvedWeaponID)) return;
            if (Data == null) Load();
            if (Data.discoveredEvolutions == null) Data.discoveredEvolutions = new List<string>();
            if (!Data.discoveredEvolutions.Contains(evolvedWeaponID))
            {
                Data.discoveredEvolutions.Add(evolvedWeaponID);
                Save();
            }
        }

        public void RecordRunEnd(float survivalTime, int level, int kills, int runGold, bool alreadyBankedGold = false)
        {
            if (Data == null) Load();

            Data.lifetimeRuns++;
            Data.lifetimeSurvivalSeconds += survivalTime;
            Data.lifetimeKills += kills;

            if (survivalTime > Data.bestSurvivalSeconds)
            {
                Data.bestSurvivalSeconds = survivalTime;
            }

            if (level > Data.highestLevelReached)
            {
                Data.highestLevelReached = level;
            }

            if (!alreadyBankedGold && runGold > 0)
            {
                AddGold(runGold);
            }
            else
            {
                Save();
                CheckHeroUnlocks();
            }
        }

        public bool IsHeroUnlocked(string heroKey)
        {
            if (Data == null) Load();
            if (Data.unlockedHeroes == null) return false;

            // Starters are always unlocked
            if (heroKey == "Knight" || heroKey == "Mage" || heroKey == "Ranger") return true;

            // ShadowKnight alias check
            if (heroKey == "ShadowKnight" && Data.unlockedHeroes.Contains("Rogue")) return true;

            return Data.unlockedHeroes.Contains(heroKey);
        }

        public List<string> recentUnlocks = new List<string>();

        public void UnlockHero(string heroName)
        {
            if (Data == null) Load();
            if (Data.unlockedHeroes == null) Data.unlockedHeroes = new List<string>();

            if (!Data.unlockedHeroes.Contains(heroName))
            {
                Data.unlockedHeroes.Add(heroName);
                if (!recentUnlocks.Contains(heroName)) recentUnlocks.Add(heroName);
                Save();
                OnHeroUnlocked?.Invoke(heroName);
                Debug.Log($"<color=#FFD700><b>[HERO UNLOCKED]</b> Chúc mừng! Đã mở khóa anh hùng: {heroName}!</color>");
            }
        }

        public List<string> CheckHeroUnlocks()
        {
            if (Data == null) Load();
            List<string> newlyUnlocked = new List<string>();

            // 1. Valen (ShadowKnight): Sống sót tối thiểu 10 phút (600 giây)
            if (!IsHeroUnlocked("ShadowKnight") && Data.bestSurvivalSeconds >= 600f)
            {
                UnlockHero("ShadowKnight");
                newlyUnlocked.Add("ShadowKnight");
            }

            // 2. Aurelius (Archmage): Tiêu diệt tổng cộng 5 Đại Trùm (Boss)
            if (!IsHeroUnlocked("Archmage") && Data.lifetimeBossKills >= 5)
            {
                UnlockHero("Archmage");
                newlyUnlocked.Add("Archmage");
            }

            // 3. Aeris (Archer): Được mua trực tiếp bằng 10.000 Vàng trong bảng chọn tướng
            if (Data.purchasedHeroes != null && Data.purchasedHeroes.Contains("Archer") && !IsHeroUnlocked("Archer"))
            {
                UnlockHero("Archer");
                newlyUnlocked.Add("Archer");
            }

            return newlyUnlocked;
        }

        public bool PurchaseHero(string heroKey, int price)
        {
            if (Data == null) Load();
            if (IsHeroUnlocked(heroKey)) return true;
            if (Data.gold < price) return false;

            if (SpendGold(price))
            {
                if (Data.purchasedHeroes == null) Data.purchasedHeroes = new List<string>();
                if (!Data.purchasedHeroes.Contains(heroKey)) Data.purchasedHeroes.Add(heroKey);

                UnlockHero(heroKey);
                Save();
                Debug.Log($"<color=#FFD700><b>[HERO PURCHASED]</b> Mua thành công anh hùng {heroKey} với {price:N0} Vàng!</color>");
                return true;
            }
            return false;
        }
    }
}
