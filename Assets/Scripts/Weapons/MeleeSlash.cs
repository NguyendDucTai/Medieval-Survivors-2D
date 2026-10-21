using System.Collections.Generic;
using UnityEngine;
using VampireSurvivors.Enemies;

namespace VampireSurvivors.Weapons
{
    [RequireComponent(typeof(Collider2D))]
    public class MeleeSlash : MonoBehaviour
    {
        [Header("Slash Settings")]
        public float damage = 30f;
        public float duration = 0.22f;
        public float initialScale = 0.8f;
        public float finalScale = 1.6f;
        public float knockbackForce = 2.5f;
        public bool isCrit = false;

        private float timer;
        private Vector2 slashDirection = Vector2.right;
        private HashSet<EnemyStats> hitEnemies = new HashSet<EnemyStats>();
        private SpriteRenderer spriteRenderer;

        public void Setup(Vector2 direction, float damageMultiplier = 1f, bool criticalHit = false)
        {
            if (direction.sqrMagnitude > 0.001f)
            {
                slashDirection = direction.normalized;
            }

            damage *= damageMultiplier;
            isCrit = criticalHit;
            if (isCrit) damage *= 2.0f;

            timer = duration;
            hitEnemies.Clear();

            // VFX_Slash crest points up (+Y in sprite space). Offset by -90 degrees so crest faces attackDir.
            float angle = Mathf.Atan2(slashDirection.y, slashDirection.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
            transform.localScale = Vector3.one * (isCrit ? initialScale * 1.3f : initialScale);

            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        protected virtual void Update()
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            // Smoothly expand scale during slash swing
            float progress = 1f - (timer / duration);
            float baseTargetScale = isCrit ? finalScale * 1.3f : finalScale;
            float currentScale = Mathf.Lerp(isCrit ? initialScale * 1.3f : initialScale, baseTargetScale, progress);
            transform.localScale = Vector3.one * currentScale;

            // Fade out slightly near end of swing
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = Mathf.Clamp01(timer / (duration * 0.6f));
                spriteRenderer.color = c;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            HandleHit(collision);
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            HandleHit(collision);
        }

        protected virtual void HandleHit(Collider2D collision)
        {
            if (collision.CompareTag("Enemy"))
            {
                EnemyStats enemy = collision.GetComponent<EnemyStats>();
                if (enemy == null) enemy = collision.GetComponentInParent<EnemyStats>();

                if (enemy != null && !hitEnemies.Contains(enemy))
                {
                    hitEnemies.Add(enemy);
                    enemy.TakeDamage(damage, isCrit);

                    OnEnemyHit(enemy, damage);
                }
            }
        }

        protected virtual void OnEnemyHit(EnemyStats enemy, float finalDamage)
        {
        }
    }
}
