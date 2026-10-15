using System;
using System.Collections.Generic;
using UnityEngine;
using VampireSurvivors.Player;

namespace VampireSurvivors.Weapons
{
    public class WeaponInventory : MonoBehaviour
    {
        public static WeaponInventory Instance;

        public const int MAX_WEAPONS = 4;
        public const int MAX_PASSIVES = 4;

        [Header("Equipped Arsenal")]
        public List<WeaponData> equippedWeapons = new List<WeaponData>();
        public List<PassiveItemData> equippedPassives = new List<PassiveItemData>();

        private Dictionary<string, float> weaponTimers = new Dictionary<string, float>();
        private PlayerStats playerStats;
        private SpriteRenderer spriteRenderer;

        public event Action OnInventoryChanged;
        public event Action<WeaponData> OnWeaponEvolved;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            playerStats = GetComponent<PlayerStats>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            if (playerStats == null) playerStats = GetComponent<PlayerStats>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            // Setup default weapon if empty
            if (equippedWeapons.Count == 0)
            {
                WeaponBase existing = GetComponent<WeaponBase>();
                if (existing != null)
                {
                    WeaponData initial = CreateInitialWeapon(existing.weaponName, existing.projectilePrefab);
                    AddWeapon(initial);
                }
                else
                {
                    // Fallback to Storm Bow for Archer
                    WeaponData defaultWeapon = CreateInitialWeapon("Storm Bow", null);
                    AddWeapon(defaultWeapon);
                }
            }
        }

        public WeaponData CreateInitialWeapon(string name, GameObject prefab)
        {
            WeaponData w = new WeaponData();
            w.weaponName = name;
            w.level = 1;
            w.projectilePrefab = prefab;

            if (name.Contains("Storm"))
            {
                // Cung thủ 2 (Archer): Cung Thần Phong
                w.weaponID = "StormBow";
                w.weaponName = "Storm Bow";
                w.description = "Bắn mũi tên thần phong bão tố ma thuật xuyên thấu mục tiêu";
                w.baseDamage = 25f;
                w.baseFireRate = 1.8f;
                w.baseRange = 11f;
                w.baseSpeed = 15f;
                w.pierceCount = 2;
                w.projectileCount = 1;
                w.requiredPassiveID = "WindQuiver";
                w.evolvedWeaponID = "TempestBarrage";
                w.evolutionName = "Tempest Barrage (Bão Tố Cuồng Phong)";
#if UNITY_EDITOR
                if (w.projectilePrefab == null)
                {
                    w.projectilePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile_StormArrow.prefab");
                }
#endif
            }
            else if (name.Contains("Bow") || name.Contains("Arrow") || name.Contains("Ranger") || name.Contains("Hunter"))
            {
                // Cung thủ 1 (Ranger): Cung Săn Bắn cổ điển
                w.weaponID = "BasicBow";
                w.weaponName = "Bow & Arrow";
                w.description = "Bắn mũi tên gỗ lông đỏ cổ điển chính xác và đẩy lùi mục tiêu";
                w.baseDamage = 16f;
                w.baseFireRate = 1.4f;
                w.baseRange = 9.5f;
                w.baseSpeed = 11f;
                w.pierceCount = 1;
                w.projectileCount = 1;
                w.requiredPassiveID = "WindQuiver";
                w.evolvedWeaponID = "StormBow";
                w.evolutionName = "Storm Bow (Cung Thần Phong)";
#if UNITY_EDITOR
                if (w.projectilePrefab == null)
                {
                    w.projectilePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile_Arrow.prefab");
                }
#endif
            }
            else if (name.Contains("Sword") || name.Contains("Holy"))
            {
                w.weaponID = "HolySword";
                w.weaponName = "Holy Sword";
                w.description = "Chém cận chiến thánh quang đẩy lùi quái vật";
                w.baseDamage = 30f;
                w.baseFireRate = 1.3f;
                w.baseRange = 3.5f;
                w.baseSpeed = 0f;
                w.pierceCount = 999;
                w.projectileCount = 1;
                w.requiredPassiveID = "IronArmor";
                w.evolvedWeaponID = "Excalibur";
                w.evolutionName = "Excalibur (Thần Kiếm Vạn Năng)";
            }
            else if (name.Contains("Fireball"))
            {
                w.weaponID = "Fireball";
                w.weaponName = "Fireball";
                w.description = "Bắn cầu lửa ma thuật thiêu đốt mục tiêu";
                w.baseDamage = 25f;
                w.baseFireRate = 1.1f;
                w.baseRange = 9f;
                w.baseSpeed = 9f;
                w.pierceCount = 1;
                w.projectileCount = 1;
                w.requiredPassiveID = "SpellTome";
                w.evolvedWeaponID = "MeteorCalamity";
                w.evolutionName = "Meteor Calamity (Hỏa Ngục Thiên Thạch)";
            }
            else if (name.Contains("Arcane") || name.Contains("Staff"))
            {
                w.weaponID = "ArcaneStaff";
                w.weaponName = "Arcane Staff";
                w.description = "Bắn quả cầu ma pháp bí thuật cực đại";
                w.baseDamage = 28f;
                w.baseFireRate = 1.4f;
                w.baseRange = 10f;
                w.baseSpeed = 8f;
                w.pierceCount = 3;
                w.projectileCount = 1;
                w.requiredPassiveID = "MagicRing";
                w.evolvedWeaponID = "CosmicSingularity";
                w.evolutionName = "Cosmic Singularity (Hố Đen Hư Vô)";
            }
            else if (name.Contains("Dark") || name.Contains("Crescent") || name.Contains("Shadow") || name.Contains("Rogue") || name.Contains("Blade"))
            {
                w.weaponID = "ShadowBlade";
                w.weaponName = "Dark Crescent Blade";
                w.description = "Nhát chém lưỡi kiếm trăng khuyết bóng tối màu tím sắc bén xuyên phá kẻ địch";
                w.baseDamage = 45f;
                w.baseFireRate = 1.8f;
                w.baseRange = 4.5f;
                w.baseSpeed = 0f;
                w.pierceCount = 999;
                w.projectileCount = 1;
                w.requiredPassiveID = "IronArmor";
                w.evolvedWeaponID = "ShadowExecutioner";
                w.evolutionName = "Shadow Executioner (Lưỡi Hái Hắc Ám)";
#if UNITY_EDITOR
                if (w.projectilePrefab == null)
                {
                    w.projectilePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX_ShadowSlash.prefab");
                }
#endif
            }
            else
            {
                w.weaponID = "HolySword";
                w.weaponName = "Holy Sword";
                w.requiredPassiveID = "IronArmor";
                w.evolvedWeaponID = "Excalibur";
                w.evolutionName = "Excalibur (Thần Kiếm Vạn Năng)";
            }

            return w;
        }

        private void Update()
        {
            if (playerStats != null && playerStats.currentHealth <= 0) return;

            // Update independent cooldown timers for all equipped weapons
            for (int i = 0; i < equippedWeapons.Count; i++)
            {
                WeaponData weapon = equippedWeapons[i];
                if (!weaponTimers.ContainsKey(weapon.weaponID))
                {
                    weaponTimers[weapon.weaponID] = 0f;
                }

                weaponTimers[weapon.weaponID] += Time.deltaTime;
                float cooldown = 1f / weapon.GetFireRate(playerStats != null ? playerStats.cooldownReduction : 0f);

                if (weaponTimers[weapon.weaponID] >= cooldown)
                {
                    weaponTimers[weapon.weaponID] = 0f;
                    FireWeapon(weapon);
                }
            }
        }

        private void FireWeapon(WeaponData weapon)
        {
            if (weapon.projectilePrefab == null)
            {
                // Load default prefab if missing
                ResolveWeaponPrefab(weapon);
                if (weapon.projectilePrefab == null) return;
            }

            Transform nearestEnemy = FindNearestEnemy(weapon.baseRange);
            Vector2 baseDir;

            bool isMelee = weapon.projectilePrefab.GetComponent<MeleeSlash>() != null;

            if (nearestEnemy != null)
            {
                baseDir = (nearestEnemy.position - transform.position).normalized;
            }
            else
            {
                float facingX = (spriteRenderer != null && spriteRenderer.flipX) ? -1f : 1f;
                baseDir = new Vector2(facingX, 0f);
            }

            int count = weapon.GetProjectileCount();
            float spreadAngle = count > 1 ? 25f : 0f;
            float startAngle = -((count - 1) * spreadAngle) * 0.5f;

            for (int i = 0; i < count; i++)
            {
                float currentAngle = startAngle + (i * spreadAngle);
                Vector2 attackDir = Quaternion.Euler(0, 0, currentAngle) * baseDir;

                Vector3 spawnPos = transform.position;
                if (isMelee)
                {
                    spawnPos += (Vector3)(attackDir * 0.85f);
                }

                GameObject go = Instantiate(weapon.projectilePrefab, spawnPos, Quaternion.identity);

                // Roll for critical strike
                bool isCrit = playerStats != null && (UnityEngine.Random.value < playerStats.critChance);
                float mult = playerStats != null ? playerStats.damageMultiplier : 1f;

                Projectile proj = go.GetComponent<Projectile>();
                if (proj != null)
                {
                    proj.damage = weapon.GetDamage(mult);
                    proj.speed = weapon.baseSpeed;
                    proj.pierceCount = weapon.GetPierceCount();
                    proj.Setup(attackDir, 1f, isCrit);
                }

                MeleeSlash slash = go.GetComponent<MeleeSlash>();
                if (slash != null)
                {
                    slash.damage = weapon.GetDamage(mult);
                    slash.Setup(attackDir, 1f, isCrit);
                }
            }
        }

        private void ResolveWeaponPrefab(WeaponData weapon)
        {
#if UNITY_EDITOR
            if (weapon.weaponID == "BasicBow")
            {
                weapon.projectilePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile_Arrow.prefab");
            }
            else if (weapon.weaponID == "StormBow" || weapon.weaponID == "TempestBarrage")
            {
                weapon.projectilePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile_StormArrow.prefab");
            }
            else if (weapon.weaponID == "HolySword" || weapon.weaponID == "Excalibur")
            {
                weapon.projectilePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX_SwordSlash.prefab");
            }
            else if (weapon.weaponID == "ShadowBlade" || weapon.weaponID == "ShadowExecutioner")
            {
                weapon.projectilePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX_ShadowSlash.prefab");
            }
            else if (weapon.weaponID == "Fireball" || weapon.weaponID == "MeteorCalamity")
            {
                weapon.projectilePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile_Fireball.prefab");
            }
            else if (weapon.weaponID == "ArcaneStaff" || weapon.weaponID == "CosmicSingularity")
            {
                weapon.projectilePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile_ArcaneOrb.prefab");
            }
            else
            {
                weapon.projectilePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile_StormArrow.prefab");
            }
#endif
        }

        private Transform FindNearestEnemy(float range)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            Transform nearest = null;
            float minDist = range;

            foreach (var e in enemies)
            {
                float d = Vector2.Distance(transform.position, e.transform.position);
                if (d < minDist)
                {
                    minDist = d;
                    nearest = e.transform;
                }
            }
            return nearest;
        }

        // --- Inventory Management ---
        public bool HasWeapon(string weaponID) => equippedWeapons.Exists(w => w.weaponID == weaponID);
        public bool HasPassive(string passiveID) => equippedPassives.Exists(p => p.passiveID == passiveID);

        public bool CanAddWeapon() => equippedWeapons.Count < MAX_WEAPONS;
        public bool CanAddPassive() => equippedPassives.Count < MAX_PASSIVES;

        public void AddWeapon(WeaponData weapon)
        {
            if (equippedWeapons.Count >= MAX_WEAPONS) return;
            equippedWeapons.Add(weapon);
            weaponTimers[weapon.weaponID] = 0f;
            ResolveWeaponPrefab(weapon);
            OnInventoryChanged?.Invoke();
            Debug.Log($"<color=green>[Arsenal]</color> Equipped Weapon: {weapon.weaponName} (Lv {weapon.level})");
        }

        public void UpgradeWeapon(string weaponID)
        {
            WeaponData w = equippedWeapons.Find(x => x.weaponID == weaponID);
            if (w != null && w.level < w.maxLevel)
            {
                w.level++;
                OnInventoryChanged?.Invoke();
                Debug.Log($"<color=green>[Arsenal]</color> Upgraded Weapon: {w.weaponName} to Lv {w.level}!");
            }
        }

        public void AddPassive(PassiveItemData passive)
        {
            if (equippedPassives.Count >= MAX_PASSIVES) return;
            equippedPassives.Add(passive);
            passive.ApplyLevelBonus(playerStats);
            OnInventoryChanged?.Invoke();
            Debug.Log($"<color=yellow>[Relic]</color> Equipped Passive: {passive.passiveName} (Lv {passive.level})");
        }

        public void UpgradePassive(string passiveID)
        {
            PassiveItemData p = equippedPassives.Find(x => x.passiveID == passiveID);
            if (p != null && p.level < p.maxLevel)
            {
                p.level++;
                p.ApplyLevelBonus(playerStats);
                OnInventoryChanged?.Invoke();
                Debug.Log($"<color=yellow>[Relic]</color> Upgraded Passive: {p.passiveName} to Lv {p.level}!");
            }
        }

        public bool CanEvolve(WeaponData weapon)
        {
            if (weapon == null || weapon.isEvolved) return false;
            if (weapon.level < weapon.maxLevel) return false;
            return HasPassive(weapon.requiredPassiveID);
        }

        public bool EvolveWeapon(string weaponID)
        {
            WeaponData w = equippedWeapons.Find(x => x.weaponID == weaponID);
            if (w == null || !CanEvolve(w)) return false;

            w.isEvolved = true;
            w.weaponID = w.evolvedWeaponID;
            w.weaponName = w.evolutionName;
            w.baseDamage *= 2.2f;
            w.baseFireRate *= 1.4f;
            w.projectileCount += 2;
            w.pierceCount += 3;

            OnWeaponEvolved?.Invoke(w);
            OnInventoryChanged?.Invoke();

            if (MetaProgression.AchievementManager.Instance != null)
            {
                MetaProgression.AchievementManager.Instance.CheckProgress(MetaProgression.AchievementType.WeaponEvolved, 1);
            }

            Debug.Log($"<color=magenta>★ [WEAPON EVOLUTION COMPLETED] ★</color> {w.evolutionName} has awakened!");
            return true;
        }
    }
}
