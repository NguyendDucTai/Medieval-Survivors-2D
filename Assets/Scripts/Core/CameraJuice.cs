using System.Collections;
using UnityEngine;

namespace VampireSurvivors.Core
{
    public class CameraJuice : MonoBehaviour
    {
        public static CameraJuice Instance;

        private float currentTrauma = 0f;
        public float traumaDecay = 2.0f;
        public float maxShakeOffset = 0.4f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this)
            {
                Destroy(this);
            }
        }

        public void Shake(float amount, float duration = 0.2f)
        {
            currentTrauma = Mathf.Clamp01(currentTrauma + amount);
        }

        public Vector3 GetShakeOffset()
        {
            if (currentTrauma <= 0.001f) return Vector3.zero;

            float shake = currentTrauma * currentTrauma;
            float offsetX = (Mathf.PerlinNoise(Time.time * 25f, 0f) - 0.5f) * 2f * maxShakeOffset * shake;
            float offsetY = (Mathf.PerlinNoise(0f, Time.time * 25f) - 0.5f) * 2f * maxShakeOffset * shake;

            return new Vector3(offsetX, offsetY, 0f);
        }

        private void Update()
        {
            if (currentTrauma > 0f)
            {
                currentTrauma = Mathf.Max(0f, currentTrauma - (traumaDecay * Time.deltaTime));
            }
        }

        public void HitStop(float duration = 0.05f)
        {
            StartCoroutine(DoHitStop(duration));
        }

        private IEnumerator DoHitStop(float duration)
        {
            float originalScale = Time.timeScale;
            Time.timeScale = 0.05f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = originalScale;
        }
    }
}
