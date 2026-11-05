using UnityEngine;

namespace VampireSurvivors.Environment
{
    /// <summary>
    /// Dynamically updates SpriteRenderer.sortingOrder based on world Y position
    /// for authentic 2.5D top-down layering (props cover entities when they walk behind,
    /// entities cover props when in front).
    /// </summary>
    public class YSortProp : MonoBehaviour
    {
        private SpriteRenderer sr;
        public float yOffset = 0f;

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
                sr.sortingOrder = 1000 - (int)((transform.position.y + yOffset) * 16f);
            }
        }
    }
}

