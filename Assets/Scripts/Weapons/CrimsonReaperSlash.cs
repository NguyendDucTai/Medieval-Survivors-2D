using System.Collections.Generic;
using UnityEngine;
using VampireSurvivors.Enemies;
using VampireSurvivors.Player;
using VampireSurvivors.UI;

namespace VampireSurvivors.Weapons
{
    [RequireComponent(typeof(Collider2D))]
    public class CrimsonReaperSlash : MeleeSlash
    {
        [Header("Crimson Reaper Lifesteal & Spin")]
        public float lifestealPercent = 0.02f; // 2% of damage dealt
        public float spinSpeed = 900f; // Fast spinning scythe
        public float minHealPerHit = 1f;

        private PlayerStats cachedPlayer;

        private void Start()
        {
#if UNITY_6000_0_OR_NEWER
            cachedPlayer = Object.FindAnyObjectByType<PlayerStats>();
#else
            cachedPlayer = Object.FindObjectOfType<PlayerStats>();
#endif
        }

        protected override void Update()
        {
            // Rotate the scythe continuously for a spinning crescent slash
            transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);

            // Execute base scale expansion and duration fade
            base.Update();
        }

        protected override void OnEnemyHit(EnemyStats enemy, float finalDamage)
        {
            // Apply lifesteal healing to player
            if (cachedPlayer != null && cachedPlayer.currentHealth > 0 && finalDamage > 0f)
            {
                float healAmount = Mathf.Max(minHealPerHit, finalDamage * lifestealPercent);
                cachedPlayer.Heal(healAmount);

                if (DamageNumberPool.Instance != null)
                {
                    DamageNumberPool.Instance.SpawnHeal(healAmount, cachedPlayer.transform.position + Vector3.up * 0.45f);
                }
            }
        }
    }
}
