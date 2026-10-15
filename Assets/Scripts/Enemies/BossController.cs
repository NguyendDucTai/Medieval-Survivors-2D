using System.Collections;
using UnityEngine;
using VampireSurvivors.Player;
using VampireSurvivors.UI;

namespace VampireSurvivors.Enemies
{
    [RequireComponent(typeof(EnemyStats))]
    public class BossController : MonoBehaviour
    {
        [Header("Boss Details")]
        public string bossTitle = "King Slime";
        public float specialAttackCooldown = 4f;
        public float chargeSpeedMultiplier = 2.5f;

        private EnemyStats stats;
        private Rigidbody2D rb;
        private Transform playerTarget;
        private float specialTimer;
        private bool isPerformingSpecial = false;

        private void Awake()
        {
            stats = GetComponent<EnemyStats>();
            rb = GetComponent<Rigidbody2D>();
            stats.isBoss = true;
        }

        private void Start()
        {
            PlayerController p = FindObjectOfType<PlayerController>();
            if (p != null) playerTarget = p.transform;

            // Notify UI of boss arrival
            if (BossHealthBarUI.Instance != null)
            {
                BossHealthBarUI.Instance.ShowBoss(bossTitle, stats);
            }
        }

        private void Update()
        {
            if (playerTarget == null || stats.currentHealth <= 0 || isPerformingSpecial) return;

            specialTimer += Time.deltaTime;
            if (specialTimer >= specialAttackCooldown)
            {
                specialTimer = 0f;
                StartCoroutine(PerformChargeOrSlam());
            }
        }

        private IEnumerator PerformChargeOrSlam()
        {
            isPerformingSpecial = true;

            // Telegraph: flash or pause for 0.6s
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            Color original = sr != null ? sr.color : Color.white;
            if (sr != null) sr.color = Color.red;

            if (rb != null) rb.linearVelocity = Vector2.zero;
            yield return new WaitForSeconds(0.6f);

            if (sr != null) sr.color = original;

            // Dash towards player
            if (playerTarget != null && rb != null)
            {
                Vector2 dir = (playerTarget.position - transform.position).normalized;
                rb.linearVelocity = dir * (stats.moveSpeed * chargeSpeedMultiplier);

                // Screen shake on boss charge
                var cameraJuice = Camera.main != null ? Camera.main.GetComponent<Core.CameraJuice>() : null;
                if (cameraJuice != null) cameraJuice.Shake(0.25f, 0.3f);
            }

            yield return new WaitForSeconds(1.2f);
            isPerformingSpecial = false;
        }

        private void OnDestroy()
        {
            if (BossHealthBarUI.Instance != null)
            {
                BossHealthBarUI.Instance.HideBoss();
            }
        }
    }
}
