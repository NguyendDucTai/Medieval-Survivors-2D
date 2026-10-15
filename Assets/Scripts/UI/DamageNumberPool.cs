using System.Collections.Generic;
using UnityEngine;

namespace VampireSurvivors.UI
{
    public class DamageNumberPool : MonoBehaviour
    {
        public static DamageNumberPool Instance;

        public int poolSize = 30;
        private List<FloatingDamageText> pool = new List<FloatingDamageText>();
        private GameObject poolContainer;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            poolContainer = new GameObject("DamageNumberPool_Container");
            poolContainer.transform.SetParent(transform);

            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = new GameObject("DamageNumber_" + i);
                obj.transform.SetParent(poolContainer.transform);
                TextMesh tm = obj.AddComponent<TextMesh>();
                tm.alignment = TextAlignment.Center;
                tm.anchor = TextAnchor.MiddleCenter;
                tm.fontStyle = FontStyle.Bold;

                FloatingDamageText fdt = obj.AddComponent<FloatingDamageText>();
                obj.SetActive(false);
                pool.Add(fdt);
            }
        }

        public void SpawnDamage(float damage, Vector3 position, bool isCrit = false)
        {
            FloatingDamageText item = GetAvailable();
            if (item == null) return;

            string text = isCrit ? Mathf.RoundToInt(damage).ToString() + "!" : Mathf.RoundToInt(damage).ToString();
            Color color = isCrit ? new Color(1f, 0.75f, 0.1f) : Color.white; // Gold for crit, White for normal
            float scale = isCrit ? 1.4f : 1.0f;

            item.Show(text, position, color, scale);
        }

        public void SpawnHeal(float healAmount, Vector3 position)
        {
            FloatingDamageText item = GetAvailable();
            if (item == null) return;

            string text = "+" + Mathf.RoundToInt(healAmount).ToString();
            Color color = new Color(0.2f, 1f, 0.3f); // Emerald green for heal
            item.Show(text, position, color, 1.1f);
        }

        public void SpawnGold(int goldAmount, Vector3 position)
        {
            FloatingDamageText item = GetAvailable();
            if (item == null) return;

            string text = "+" + goldAmount.ToString() + "G";
            Color color = new Color(1f, 0.85f, 0.2f); // Pure gold
            item.Show(text, position, color, 1.0f);
        }

        private FloatingDamageText GetAvailable()
        {
            for (int i = 0; i < pool.Count; i++)
            {
                if (!pool[i].gameObject.activeInHierarchy)
                {
                    return pool[i];
                }
            }

            // Expand pool if necessary
            GameObject obj = new GameObject("DamageNumber_" + pool.Count);
            obj.transform.SetParent(poolContainer.transform);
            TextMesh tm = obj.AddComponent<TextMesh>();
            tm.alignment = TextAlignment.Center;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.fontStyle = FontStyle.Bold;

            FloatingDamageText fdt = obj.AddComponent<FloatingDamageText>();
            obj.SetActive(false);
            pool.Add(fdt);
            return fdt;
        }
    }
}
