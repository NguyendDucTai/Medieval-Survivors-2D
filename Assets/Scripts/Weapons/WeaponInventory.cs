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

            // Normalize weapon name
            string n = name ?? "";

            if (n.Contains("Storm") || n.Contains("Aeris") || n.Equals("StormBow", StringComparison.OrdinalIgnoreCase))
            {
                // 1. Xạ thủ Aeris: Cung Thần Phong -> Astral Barrage (Mưa Tên Tinh Tú)
                w.weaponID = "StormBow";
                w.weaponName = "Storm Bow";
                w.description = "Bắn mũi tên thần phong bão tố ma thuật xuyên thấu kẻ địch";
                w.baseDamage = 26f;
                w.baseFireRate = 1.9f;
                w.baseRange = 11.5f;
                w.baseSpeed = 15f;
                w.pierceCount = 2;
                w.projectileCount = 1;
                w.requiredPassiveID = "CloverOfLuck";
                w.evolvedWeaponID = "AstralBarrage";
                w.evolutionName = "Astral Barrage (Mưa Tên Tinh Tú)";
#if UNITY_EDITOR
                if (w.projectilePrefab == null)
                {
                    w.projectilePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile_StormArrow.prefab");
                }
#endif
            }
            else if (n.Contains("Wind") || n.Contains("BasicBow") || n.Contains("Ranger") || n.Contains("Sylph") || (n.Contains("Bow") && !n.Contains("Storm")))
            {
                // 2. Cung thủ Sylph: Cung Gió -> Tempest Arrow (Bão Tố Cuồng Phong)
                w.weaponID = "BasicBow";
                w.weaponName = "Wind Bow";
                w.description = "Bắn mũi tên gió chính xác tầm xa và đẩy lùi mục tiêu";
                w.baseDamage = 18f;
                w.baseFireRate = 1.5f;
                w.baseRange = 10f;
                w.baseSpeed = 12f;
                w.pierceCount = 1;
                w.projectileCount = 1;
                w.requiredPassiveID = "WindQuiver";
                w.evolvedWeaponID = "TempestArrow";
                w.evolutionName = "Tempest Arrow (Bão Tố Cuồng Phong)";
#if UNITY_EDITOR
                if (w.projectilePrefab == null)
                {
                    w.projectilePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile_Arrow.prefab");
                }
#endif
            }
            else if (n.Contains("Sword") || n.Contains("Holy") || n.Contains("Roland") || n.Contains("Knight"))
            {
                // 3. Hiệp sĩ Sir Roland: Thánh Kiếm -> Excalibur (Thần Kiếm Vạn Năng)
                w.weaponID = "HolySword";
                w.weaponName = "Holy Broadsword";
                w.description = "Chém cận chiến thánh quang đẩy lùi quái vật mạnh mẽ";
                w.baseDamage = 32f;
                w.baseFireRate = 1.35f;
                w.baseRange = 3.6f;
                w.baseSpeed = 0f;
                w.pierceCount = 999;
                w.projectileCount = 1;
                w.requiredPassiveID = "IronArmor";
                w.evolvedWeaponID = "Excalibur";
                w.evolutionName = "Excalibur (Thần Kiếm Vạn Năng)";
#if UNITY_EDITOR
                if (w.projectilePrefab == null)
                {
                    w.projectilePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX_SwordSlash.prefab");
                }
#endif
            }
            else if (n.Contains("Fireball") || n.Contains("Ignis") || n.Contains("Mage"))
            {
                // 4. Pháp sư Ignis: Cầu Lửa -> Meteor Calamity (Hỏa Ngục Thiên Thạch)
                w.weaponID = "Fireball";
                w.weaponName = "Fireball";
                w.description = "Bắn cầu lửa ma thuật thiêu đốt mục tiêu và phát nổ";
                w.baseDamage = 28f;
                w.baseFireRate = 1.15f;
                w.baseRange = 9.5f;
                w.baseSpeed = 9.5f;
                w.pierceCount = 1;
                w.projectileCount = 1;
                w.requiredPassiveID = "SpellTome";
                w.evolvedWeaponID = "MeteorCalamity";
                w.evolutionName = "Meteor Calamity (Hỏa Ngục Thiên Thạch)";
#if UNITY_EDITOR
                if (w.projectilePrefab == null)
                {
                    w.projectilePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile_Fireball.prefab");
                }
#endif
            }
            else if (n.Contains("Staff") || n.Contains("Arcane") || n.Contains("Lightning") || n.Contains("Aurelius") || n.Contains("Archmage"))
            {
                // 5. Đại Pháp sư Aurelius: Gậy Thần Bão Tố -> Cosmic Singularity (Hố Đen Hư Vô)
                w.weaponID = "ArcaneStaff";
                w.weaponName = "Lightning Staff";
                w.description = "Phóng quả cầu lôi điện ma pháp bí thuật cực đại";
                w.baseDamage = 30f;
                w.baseFireRate = 1.45f;
                w.baseRange = 10.5f;
                w.baseSpeed = 8.5f;
                w.pierceCount = 3;
                w.projectileCount = 1;
                w.requiredPassiveID = "MagnetTalisman";
                w.evolvedWeaponID = "CosmicSingularity";
                w.evolutionName = "Cosmic Singularity (Hố Đen Hư Vô)";
#if UNITY_EDITOR
                if (w.projectilePrefab == null)
                {
                    w.projectilePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile_ArcaneOrb.prefab");
                }
#endif
            }
            else if (n.Contains("Dark") || n.Contains("Crescent") || n.Contains("Shadow") || n.Contains("Valen") || n.Contains("Rogue") || n.Contains("Blade"))
            {
                // 6. Sát thủ Valen: Lưỡi Đao Hắc Ám -> Crimson Reaper (Lưỡi Hái Huyết Nguyệt)
                w.weaponID = "ShadowBlade";
                w.weaponName = "Dark Crescent";
                w.description = "Nhát chém trăng khuyết bóng tối màu tím sắc bén xuyên phá kẻ địch";
                w.baseDamage = 45f;
                w.baseFireRate = 1.8f;
                w.baseRange = 4.5f;
                w.baseSpeed = 0f;
                w.pierceCount = 999;
                w.projectileCount = 1;
                w.requiredPassiveID = "BloodChalice";
                w.evolvedWeaponID = "CrimsonReaper";
                w.evolutionName = "Crimson Reaper (Lưỡi Hái Huyết Nguyệt)";
#if UNITY_EDITOR
                if (w.projectilePrefab == null)
                {
                    w.projectilePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX_ShadowSlash.prefab");
                }
#endif
            }
            else
            {
                // Fallback default
                w.weaponID = "HolySword";
                w.weaponName = "Holy Broadsword";
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
#if UNITY_EDITOR
                if (w.projectilePrefab == null)
                {
                    w.projectilePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX_SwordSlash.prefab");
                }
#endif
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
            if (weapon == null) return;

#if UNITY_EDITOR
            if (weapon.weaponID == "BasicBow")
            {
                weapon.projectilePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile_Arrow.prefab");
            }
            else if (weapon.weaponID == "StormBow" || weapon.weaponID == "TempestArrow" || weapon.weaponID == "TempestBarrage")
            {
                weapon.projectilePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile_StormArrow.prefab");
            }
            else if (weapon.weaponID == "AstralBarrage")
            {
                weapon.projectilePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile_AstralArrow.prefab");
            }
            else if (weapon.weaponID == "HolySword" || weapon.weaponID == "Excalibur")
            {
                weapon.projectilePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX_SwordSlash.prefab");
            }
            else if (weapon.weaponID == "ShadowBlade" || weapon.weaponID == "ShadowExecutioner")
            {
                weapon.projectilePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX_ShadowSlash.prefab");
            }
            else if (weapon.weaponID == "CrimsonReaper")
            {
                weapon.projectilePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX_CrimsonReaper.prefab");
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
        public bool HasPassive(string passiveID)
        {
            if (string.IsNullOrEmpty(passiveID)) return false;
            return equippedPassives.Exists(p =>
                p.passiveID.Equals(passiveID, StringComparison.OrdinalIgnoreCase) ||
                (passiveID.Contains("Clover") && p.passiveID.Contains("Clover")) ||
                (passiveID.Contains("Magnet") && (p.passiveID.Contains("Magnet") || p.passiveID.Contains("Ring"))) ||
                (passiveID.Contains("Ring") && (p.passiveID.Contains("Magnet") || p.passiveID.Contains("Ring"))) ||
                (passiveID.Contains("Armor") && (p.passiveID.Contains("Armor") || p.passiveID.Contains("Crown")))
            );
        }

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

            // Refresh projectile prefab to match newly evolved weapon
            w.projectilePrefab = null;
            ResolveWeaponPrefab(w);

            OnWeaponEvolved?.Invoke(w);
            OnInventoryChanged?.Invoke();

            if (MetaProgression.AchievementManager.Instance != null)
            {
                MetaProgression.AchievementManager.Instance.CheckProgress(MetaProgression.AchievementType.WeaponEvolved, 1);
            }

            var cameraJuice = Camera.main != null ? Camera.main.GetComponent<Core.CameraJuice>() : null;
            if (cameraJuice != null) cameraJuice.Shake(0.4f, 0.4f);

            Debug.Log($"<color=magenta>★ [WEAPON EVOLUTION COMPLETED] ★</color> {w.evolutionName} has awakened!");
            return true;
        }
    }
}

