using System;

namespace VampireSurvivors.MetaProgression
{
    [Serializable]
    public enum AchievementType
    {
        TotalKills,
        SurvivalTime,
        LevelReached,
        WeaponEvolved,
        BossDefeated,
        GoldEarned
    }

    [Serializable]
    public class AchievementData
    {
        public string id;
        public string title;
        public string description;
        public AchievementType type;
        public float targetValue;
        public int goldReward;
        public string unlockHeroReward; // Optional hero unlock ID
    }
}
