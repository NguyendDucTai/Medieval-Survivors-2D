using UnityEngine;

namespace VampireSurvivors.Environment
{
    /// <summary>
    /// Dynamically updates SpriteRenderer.sortingOrder based on camera-relative Y position
    /// for authentic 2.5D top-down layering (props cover entities when they walk behind,
    /// entities cover props when in front) while preventing overflow/underflow in infinite worlds.
    /// </summary>
    public class YSortProp : MonoBehaviour
    {
        private SpriteRenderer sr;
        public float yOffset = 0f;
        private static Transform cachedMainCam;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            UpdateSorting();
        }

        private void LateUpdate()
        {
            UpdateSorting();
        }

        public void UpdateSorting()
        {
            if (sr != null)
            {
                if (cachedMainCam == null && Camera.main != null) cachedMainCam = Camera.main.transform;
                float camY = cachedMainCam != null ? cachedMainCam.position.y : 0f;
                float relY = (transform.position.y + yOffset) - camY;
                sr.sortingOrder = Mathf.Clamp(500 - Mathf.RoundToInt(relY * 16f), 50, 950);
            }
        }
    }
}
