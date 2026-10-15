using System;
using UnityEngine;

namespace VampireSurvivors.MetaProgression
{
    [Serializable]
    public class UpgradeData
    {
        public string id;
        public string displayName;
        public string description;
        public int maxRank;
        public int baseCost;
        public float costMultiplier;
        public float valuePerRank;

        public int GetCostForRank(int currentRank)
        {
            if (currentRank >= maxRank) return -1;
            return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, currentRank));
        }
    }
}
