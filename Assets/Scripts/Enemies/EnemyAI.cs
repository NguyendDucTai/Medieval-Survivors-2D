using UnityEngine;
using VampireSurvivors.Player;

namespace VampireSurvivors.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(EnemyStats))]
    public class EnemyAI : MonoBehaviour
    {
        private Rigidbody2D rb;
        private EnemyStats stats;
        private SpriteRenderer spriteRenderer;
        private Animator animator;
        private Transform playerTarget;
        private float lastAttackTime = -10f;
        public float attackCooldown = 0.8f;
        private bool isDead = false;

        public void SetDead(bool dead)
        {
            isDead = dead;
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            stats = GetComponent<EnemyStats>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        private void Start()
        {
            FindPlayer();
        }

        private void OnEnable()
        {
            isDead = false;
            FindPlayer();
        }

        private void FindPlayer()
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                playerTarget = player.transform;
            }
        }

        private void FixedUpdate()
        {
            if (isDead || playerTarget == null) return;

            Vector2 direction = (playerTarget.position - transform.position).normalized;
            rb.linearVelocity = direction * stats.moveSpeed;

            if (spriteRenderer != null && direction.x != 0)
            {
                spriteRenderer.flipX = direction.x < 0;
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (isDead) return;

            if (collision.gameObject.CompareTag("Player"))
            {
                if (animator != null && Time.time - lastAttackTime >= attackCooldown)
                {
                    animator.SetTrigger("Attack");
                    lastAttackTime = Time.time;
                }

                PlayerStats playerStats = collision.gameObject.GetComponent<PlayerStats>();
                if (playerStats != null)
                {
                    playerStats.TakeDamage(stats.attackDamage * Time.deltaTime);
                }
            }
        }
    }
}
