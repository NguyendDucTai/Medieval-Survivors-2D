using System;
using UnityEngine;
using VampireSurvivors.Player;

namespace VampireSurvivors.Weapons
{
    [Serializable]
    public class PassiveItemData
    {
        public string passiveID;
        public string passiveName;
        public string description;
        public int level = 1;
        public int maxLevel = 5;

        public void ApplyLevelBonus(PlayerStats stats)
        {
            if (stats == null) return;

            switch (passiveID)
            {
                case "IronArmor":
                    stats.armor += 1.5f;
                    break;
                case "SpellTome":
                    stats.damageMultiplier += 0.12f;
                    break;
                case "WindQuiver":
                    stats.cooldownReduction = Mathf.Clamp(stats.cooldownReduction + 0.05f, 0f, 0.5f);
                    stats.moveSpeed += 0.35f;
                    break;
                case "MagicRing":
                    stats.pickupRadius += 1.2f;
                    break;
                case "BloodChalice":
                    stats.hpRegen += 0.6f;
                    break;
                case "CloverOfLuck":
                    stats.critChance += 0.06f;
                    stats.critMultiplier += 0.25f;
                    break;
                case "GreedCoin":
                    stats.goldMultiplier += 0.20f;
                    break;
                case "CrownOfKings":
                    stats.expMultiplier += 0.15f;
                    break;
                case "MagnetTalisman":
                    stats.pickupRadius += 2.5f;
                    break;
                case "HolyCross":
                    stats.reviveCount += 1;
                    stats.maxHealth += 20f;
                    stats.Heal(20f);
                    break;
            }
        }
    }
}
