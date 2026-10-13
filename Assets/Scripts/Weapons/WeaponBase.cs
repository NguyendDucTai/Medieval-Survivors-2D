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
            Transform nearestEnemy = FindNearestEnemy();
            if (nearestEnemy == null) return; // Chá»‰ báº¯n khi phĂ¡t hiá»‡n quĂ¡i trong táº§m báº¯n!

            Vector2 attackDir = (nearestEnemy.position - transform.position).normalized;

            if (projectilePrefab != null)
            {
                GameObject projGO = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                Projectile proj = projGO.GetComponent<Projectile>();
                if (proj != null)
                {
                    float mult = playerStats != null ? playerStats.damageMultiplier : 1f;
                    proj.Setup(attackDir, mult);
                }
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
