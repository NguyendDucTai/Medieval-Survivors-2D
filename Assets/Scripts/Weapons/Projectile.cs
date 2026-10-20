using System.Collections.Generic;
using UnityEngine;
using VampireSurvivors.Enemies;
using VampireSurvivors.Player;

namespace VampireSurvivors.Weapons
{
    public class Projectile : MonoBehaviour
    {
        [Header("Projectile Properties")]
        public float speed = 10f;
        public float damage = 15f;
        public float lifeTime = 3f;
        public int pierceCount = 1;
        public bool isCrit = false;

        private Vector2 moveDirection;
        private int currentPierce;
        private float timer;
        private HashSet<EnemyStats> hitEnemies = new HashSet<EnemyStats>();

        public void Setup(Vector2 direction, float damageMultiplier = 1f, bool criticalHit = false)
        {
            moveDirection = direction.normalized;
            damage *= damageMultiplier;
            isCrit = criticalHit;
            if (isCrit) damage *= 2.0f; // Critical hit multiplier

            currentPierce = pierceCount;
            timer = lifeTime;
            hitEnemies.Clear();

            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        protected virtual void Update()
        {
            transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Enemy"))
            {
                EnemyStats enemy = collision.GetComponent<EnemyStats>();
                if (enemy == null) enemy = collision.GetComponentInParent<EnemyStats>();
                if (enemy != null && !hitEnemies.Contains(enemy))
                {
                    hitEnemies.Add(enemy);
                    enemy.TakeDamage(damage, isCrit);
                    currentPierce--;
                    if (currentPierce <= 0)
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }
    }
}
