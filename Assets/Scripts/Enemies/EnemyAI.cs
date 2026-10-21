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
        public float attackCooldown = 0.35f; // Responsive contact damage tick
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
            if (PlayerController.Instance != null)
            {
                playerTarget = PlayerController.Instance.transform;
                IgnorePlayerPhysicalCollision(PlayerController.Instance.gameObject);
                return;
            }

#if UNITY_6000_0_OR_NEWER
            PlayerController player = FindAnyObjectByType<PlayerController>();
#else
            PlayerController player = FindObjectOfType<PlayerController>();
#endif
            if (player != null)
            {
                playerTarget = player.transform;
                IgnorePlayerPhysicalCollision(player.gameObject);
            }
        }

        public void IgnorePlayerPhysicalCollision(GameObject playerGO)
        {
            if (playerGO == null) return;
            Collider2D myCol = GetComponent<Collider2D>();
            Collider2D playerCol = playerGO.GetComponent<Collider2D>();
            if (myCol != null && playerCol != null)
            {
                // Vampire Survivors mechanic: Player can move THROUGH enemies instead of being physically blocked
                Physics2D.IgnoreCollision(myCol, playerCol, true);
            }
        }

        [Header("Fly-Through Charge Mode")]
        public bool isChargeFlyThrough = false;
        public Vector2 chargeDirection = Vector2.zero;
        public float chargeSpeed = 9.2f;
        private float chargeTimer = 0f;
        public float maxChargeLifetime = 5f;

        public void SetChargeFlyThrough(Vector2 fixedDirection, float speed, float lifetime = 5f, bool track = false)
        {
            isChargeFlyThrough = true;
            chargeDirection = fixedDirection.normalized;
            chargeSpeed = speed;
            maxChargeLifetime = lifetime;
            chargeTimer = 0f;

            if (rb != null)
            {
                rb.linearVelocity = chargeDirection * chargeSpeed;
            }
            if (spriteRenderer != null && chargeDirection.x != 0)
            {
                spriteRenderer.flipX = chargeDirection.x < 0;
            }
        }

        private void FixedUpdate()
        {
            if (isDead) return;

            if (isChargeFlyThrough)
            {
                chargeTimer += Time.fixedDeltaTime;

                // Straight locked line trajectory: maintains fixed high velocity through the player's position!
                // NO steering or turning around - players who focus 100% and sidestep CAN successfully dodge!
                rb.linearVelocity = chargeDirection * chargeSpeed;

                if (spriteRenderer != null && chargeDirection.x != 0)
                {
                    spriteRenderer.flipX = chargeDirection.x < 0;
                }

                CheckPlayerContactDamage();

                if (chargeTimer >= maxChargeLifetime)
                {
                    Destroy(gameObject);
                }
                return;
            }

            if (playerTarget == null) return;

            // If attached to a PlantRingMember, circular tightening manages its movement!
            if (GetComponent<PlantRingMember>() == null)
            {
                Vector2 direction = (playerTarget.position - transform.position).normalized;
                rb.linearVelocity = direction * stats.moveSpeed;

                if (spriteRenderer != null && direction.x != 0)
                {
                    spriteRenderer.flipX = direction.x < 0;
                }
            }

            CheckPlayerContactDamage();
        }

        private void CheckPlayerContactDamage()
        {
            if (isDead || playerTarget == null) return;
            float dist = Vector2.Distance(transform.position, playerTarget.position);
            float hitRadius = 0.85f;
            if (dist <= hitRadius)
            {
                TryAttackPlayer(playerTarget.gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            TryAttackPlayer(collision.gameObject);
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            TryAttackPlayer(collision.gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryAttackPlayer(collision.gameObject);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            TryAttackPlayer(collision.gameObject);
        }

        private void TryAttackPlayer(GameObject target)
        {
            if (isDead || target == null) return;

            if (target.CompareTag("Player"))
            {
                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    lastAttackTime = Time.time;
                    if (animator != null)
                    {
                        animator.SetTrigger("Attack");
                    }

                    PlayerStats playerStats = target.GetComponent<PlayerStats>();
                    if (playerStats != null)
                    {
                        playerStats.TakeDamage(stats.attackDamage);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Controls a plant enemy spawned as part of the 360-degree tightening flower ring trap.
    /// Manages circular positioning and synchronized contraction toward center.
    /// </summary>
    public class PlantRingMember : MonoBehaviour
    {
        public Vector2 trapCenter;
        public float currentRadius = 13.5f;
        public float angleDeg = 0f;
        public float shrinkSpeed = 0.18f; // Very slow, claustrophobic crawl
        public bool isShrinking = false;

        private Rigidbody2D rb;
        private Transform playerTransform;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void Initialize(Vector2 center, float startRadius, float angle)
        {
            trapCenter = center;
            currentRadius = startRadius;
            angleDeg = angle;
            isShrinking = false;
            UpdatePositionDirect();
        }

        public void StartShrinking()
        {
            isShrinking = true;
        }

        private void FixedUpdate()
        {
            if (isShrinking)
            {
                if (playerTransform == null)
                {
                    var pc = FindAnyObjectByType<Player.PlayerController>();
                    if (pc != null) playerTransform = pc.transform;
                }

                // Check if player has escaped past the ring
                bool hasEscaped = false;
                if (playerTransform != null)
                {
                    float distToCenter = Vector2.Distance(playerTransform.position, trapCenter);
                    if (distToCenter > currentRadius + 1.5f)
                    {
                        hasEscaped = true;
                    }
                }

                if (hasEscaped && playerTransform != null)
                {
                    // Relentlessly crawl toward player position instead of shrinking to empty center!
                    Vector2 dirToPlayer = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
                    if (rb != null)
                    {
                        rb.linearVelocity = dirToPlayer * 0.75f;
                    }
                    else
                    {
                        transform.position += (Vector3)(dirToPlayer * 0.75f * Time.fixedDeltaTime);
                    }
                }
                else
                {
                    // Gradually shrink inward towards center at a very slow pace (0.18m/s)
                    currentRadius = Mathf.Max(0.75f, currentRadius - (shrinkSpeed * Time.fixedDeltaTime));

                    float rad = angleDeg * Mathf.Deg2Rad;
                    Vector2 targetPos = trapCenter + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * currentRadius;

                    if (rb != null)
                    {
                        rb.linearVelocity = (targetPos - (Vector2)transform.position) * 5f;
                    }
                    else
                    {
                        transform.position = targetPos;
                    }
                }
            }
        }

        private void UpdatePositionDirect()
        {
            float rad = angleDeg * Mathf.Deg2Rad;
            Vector2 targetPos = trapCenter + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * currentRadius;
            transform.position = new Vector3(targetPos.x, targetPos.y, 0f);
        }
    }
}
