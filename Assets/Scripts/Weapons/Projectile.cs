using UnityEngine;
using VampireSurvivors.Enemies;

namespace VampireSurvivors.Weapons
{
    public class Projectile : MonoBehaviour
    {
        [Header("Projectile Properties")]
        public float speed = 10f;
        public float damage = 15f;
        public float lifeTime = 3f;
        public int pierceCount = 1;

        private Vector2 moveDirection;
        private int currentPierce;
        private float timer;

        public void Setup(Vector2 direction, float damageMultiplier = 1f)
        {
            moveDirection = direction.normalized;
            damage *= damageMultiplier;
            currentPierce = pierceCount;
            timer = lifeTime;

            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private void Update()
        {
            transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Enemy"))
            {
                EnemyStats enemy = collision.GetComponent<EnemyStats>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
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
