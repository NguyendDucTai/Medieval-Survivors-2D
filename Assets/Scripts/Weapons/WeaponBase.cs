using UnityEngine;
using VampireSurvivors.Player;

namespace VampireSurvivors.Weapons
{
    public class WeaponBase : MonoBehaviour
    {
        [Header("Weapon Config")]
        public string weaponName = "Auto Gun";
        public GameObject projectilePrefab;
        public float fireRate = 1.0f; // Shots per second
        public float attackRange = 8f;
        public int level = 1;

        private float fireTimer;
        private PlayerStats playerStats;

        private void Start()
        {
            playerStats = GetComponentInParent<PlayerStats>();
            // If WeaponInventory is present, it handles attacks and evolutions
            WeaponInventory inv = GetComponentInParent<WeaponInventory>();
            if (inv != null)
            {
                enabled = false;
            }
        }

        private void Update()
        {
            if (fireRate <= 0.2f) fireRate = 1.0f; // PhĂ²ng trÆ°á»ng há»£p Inspector bá»‹ 0

            fireTimer += Time.deltaTime;
            if (fireTimer >= (1f / fireRate))
            {
                fireTimer = 0f;
                Attack();
            }
        }

        protected virtual void Attack()
        {
            if (projectilePrefab == null) return;

            // Direct Player Aiming via movement buttons (WASD / Arrows)
            PlayerController pc = GetComponentInParent<PlayerController>();
            Vector2 attackDir = (pc != null) ? pc.CurrentAimDirection : Vector2.right;
            if (attackDir.sqrMagnitude < 0.001f)
            {
                SpriteRenderer sr = GetComponentInParent<SpriteRenderer>();
                float facingX = (sr != null && sr.flipX) ? -1f : 1f;
                attackDir = new Vector2(facingX, 0f);
            }

            bool isMelee = projectilePrefab.GetComponent<MeleeSlash>() != null;
            Vector3 spawnPos = transform.position;
            if (isMelee)
            {
                spawnPos += (Vector3)(attackDir * 0.85f);
            }

            GameObject spawnedGO = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            float mult = playerStats != null ? playerStats.damageMultiplier : 1f;

            Projectile proj = spawnedGO.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Setup(attackDir, mult);
            }

            MeleeSlash slash = spawnedGO.GetComponent<MeleeSlash>();
            if (slash != null)
            {
                slash.Setup(attackDir, mult);
            }
        }

        private Transform FindNearestEnemy()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            Transform nearest = null;
            float minDistance = attackRange;

            foreach (GameObject enemy in enemies)
            {
                float dist = Vector2.Distance(transform.position, enemy.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearest = enemy.transform;
                }
            }

            return nearest;
        }
    }
}
