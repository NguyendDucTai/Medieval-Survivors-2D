using System;
using UnityEngine;

namespace VampireSurvivors.Enemies
{
    public class EnemyStats : MonoBehaviour
    {
        [Header("Enemy Attributes")]
        public float maxHealth = 20f;
        public float currentHealth;
        public float moveSpeed = 2.5f;
        public float attackDamage = 10f;
        public float expValue = 10f;

        [Header("Drop Settings")]
        public GameObject expGemPrefab;

        public event Action OnEnemyDeath;

        private bool isDying = false;

        private void OnEnable()
        {
            currentHealth = maxHealth;
            isDying = false;

            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = true;

            EnemyAI ai = GetComponent<EnemyAI>();
            if (ai != null)
            {
                ai.enabled = true;
                ai.SetDead(false);
            }
        }

        public void TakeDamage(float amount)
        {
            if (isDying) return;

            currentHealth -= amount;
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            if (isDying) return;
            isDying = true;

            OnEnemyDeath?.Invoke();

            // Disable collider so enemy can't deal or receive damage while dying
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            // Stop AI movement
            EnemyAI ai = GetComponent<EnemyAI>();
            if (ai != null)
            {
                ai.SetDead(true);
            }

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;

            // Trigger Die animation
            Animator anim = GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger("Die");
            }

            // Spawn experience gem
            if (expGemPrefab != null)
            {
                if (Core.ObjectPooler.Instance != null && Core.ObjectPooler.Instance.poolDictionary.ContainsKey("ExpGem"))
                {
                    Core.ObjectPooler.Instance.SpawnFromPool("ExpGem", transform.position, Quaternion.identity);
                }
                else
                {
                    Instantiate(expGemPrefab, transform.position, Quaternion.identity);
                }
            }

            // Allow death animation (4 frames) to play before destroying
            StartCoroutine(DestroyAfterDelay(0.65f));
        }

        private System.Collections.IEnumerator DestroyAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (Core.ObjectPooler.Instance != null && Core.ObjectPooler.Instance.poolDictionary.ContainsKey("Enemy"))
            {
                gameObject.SetActive(false);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
