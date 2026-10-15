using System;
using UnityEngine;

namespace VampireSurvivors.Weapons
{
    [Serializable]
    public class WeaponData
    {
        public string weaponID;
        public string weaponName;
        public string description;
        public int level = 1;
        public int maxLevel = 5;

        public float baseDamage = 15f;
        public float baseFireRate = 1.0f;
        public float baseRange = 8f;
        public float baseSpeed = 10f;
        public int pierceCount = 1;
        public int projectileCount = 1;
        public float areaOfEffect = 1f;

        public bool isEvolved = false;
        public string evolvedWeaponID;
        public string requiredPassiveID;
        public string evolutionName;

        public GameObject projectilePrefab;

        public float GetDamage(float playerMultiplier = 1f)
        {
            float lvlBonus = (level - 1) * (baseDamage * 0.25f);
            return (baseDamage + lvlBonus) * playerMultiplier;
        }

        public float GetFireRate(float cooldownReduction = 0f)
        {
            float lvlBonus = (level - 1) * 0.15f;
            float rawRate = baseFireRate + lvlBonus;
            return rawRate / Mathf.Max(0.5f, 1f - cooldownReduction);
        }

        public int GetPierceCount()
        {
            return pierceCount + ((level - 1) / 2);
        }

        public int GetProjectileCount()
        {
            return projectileCount + (level >= 3 ? 1 : 0) + (level >= 5 ? 1 : 0);
        }
    }
}
