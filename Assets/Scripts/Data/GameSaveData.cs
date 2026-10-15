using System;
using System.Collections.Generic;

namespace VampireSurvivors.Data
{
    [Serializable]
    public class GameSaveData
    {
        // Currency
        public int gold = 0;

        // Statistics
        public int lifetimeKills = 0;
        public int lifetimeRuns = 0;
        public float lifetimeSurvivalSeconds = 0f;
        public float bestSurvivalSeconds = 0f;
        public int highestLevelReached = 1;

        // Selected & Unlocked Heroes
        public string selectedHero = "Archer";
        public List<string> unlockedHeroes = new List<string>() { "Knight", "Archer", "Mage" };

        // Meta-Progression Upgrade Levels (UpgradeID -> Level)
        // Supported: Vigor, IronSkin, Agility, Might, Quickness, Magnetism, Bounty, Scholar, CriticalStrike, VitalityRegen, SecondWind, Reroll
        public List<UpgradeSaveEntry> upgradeEntries = new List<UpgradeSaveEntry>();

        // Completed Achievements (IDs)
        public List<string> completedAchievements = new List<string>();

        public int GetUpgradeLevel(string upgradeId)
        {
            if (upgradeEntries == null) upgradeEntries = new List<UpgradeSaveEntry>();
            var entry = upgradeEntries.Find(e => e.id == upgradeId);
            return entry != null ? entry.level : 0;
        }

        public void SetUpgradeLevel(string upgradeId, int level)
        {
            if (upgradeEntries == null) upgradeEntries = new List<UpgradeSaveEntry>();
            var entry = upgradeEntries.Find(e => e.id == upgradeId);
            if (entry != null)
            {
                entry.level = level;
            }
            else
            {
                upgradeEntries.Add(new UpgradeSaveEntry { id = upgradeId, level = level });
            }
        }
    }

    [Serializable]
    public class UpgradeSaveEntry
    {
        public string id;
        public int level;
    }
}
