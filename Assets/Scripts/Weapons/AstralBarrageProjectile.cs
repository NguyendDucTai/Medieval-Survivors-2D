using UnityEngine;
using VampireSurvivors.Enemies;

namespace VampireSurvivors.Weapons
{
    public class AstralBarrageProjectile : Projectile
    {
        [Header("Astral Barrage Special Settings")]
        public float celestialCritMultiplier = 2.5f;
        public float waveFrequency = 12f;
        public float waveAmplitude = 0.35f;

        private float birthTime;
        private Vector2 baseFlightDir;
        private Vector2 perpendicularDir;
        private bool hasCritSet = false;

        private void Awake()
        {
            birthTime = Time.time;
            isCrit = true; // Always 100% Critical Strike!
            pierceCount = 999; // Pierces everything in its path!
        }

        private void Start()
        {
            if (!hasCritSet)
            {
                isCrit = true;
                damage *= celestialCritMultiplier;
                hasCritSet = true;
            }
        }

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            base.OnTriggerEnter2D(collision);

            if (collision.CompareTag("Enemy"))
            {
                // Play subtle impact feedback
                var cameraJuice = Camera.main != null ? Camera.main.GetComponent<Core.CameraJuice>() : null;
                if (cameraJuice != null)
                {
                    cameraJuice.Shake(0.12f, 0.08f);
                }
            }
        }
    }
}
