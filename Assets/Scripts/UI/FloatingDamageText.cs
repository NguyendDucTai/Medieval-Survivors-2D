using System.Collections;
using UnityEngine;

namespace VampireSurvivors.UI
{
    [RequireComponent(typeof(TextMesh))]
    public class FloatingDamageText : MonoBehaviour
    {
        private TextMesh textMesh;
        private MeshRenderer meshRenderer;

        private void Awake()
        {
            textMesh = GetComponent<TextMesh>();
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.sortingLayerName = "Default";
                meshRenderer.sortingOrder = 1000;
            }
        }

        public void Show(string text, Vector3 worldPosition, Color color, float size = 1f)
        {
            if (textMesh == null) Awake();

            transform.position = worldPosition + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(0.2f, 0.4f), 0f);
            textMesh.text = text;
            textMesh.color = color;
            textMesh.characterSize = 0.12f * size;
            textMesh.fontSize = 28;
            gameObject.SetActive(true);

            StopAllCoroutines();
            StartCoroutine(AnimateText());
        }

        private IEnumerator AnimateText()
        {
            float duration = 0.65f;
            float elapsed = 0f;
            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + new Vector3(Random.Range(-0.3f, 0.3f), 0.8f, 0f);
            Color startColor = textMesh.color;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                transform.position = Vector3.Lerp(startPos, endPos, Mathf.Sin(t * Mathf.PI * 0.5f));

                Color c = startColor;
                c.a = Mathf.Lerp(1f, 0f, t * t);
                textMesh.color = c;

                elapsed += Time.deltaTime;
                yield return null;
            }

            gameObject.SetActive(false);
        }
    }
}
