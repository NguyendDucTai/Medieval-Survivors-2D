using UnityEngine;
using UnityEngine.UI;
using VampireSurvivors.Enemies;

namespace VampireSurvivors.UI
{
    public class BossHealthBarUI : MonoBehaviour
    {
        public static BossHealthBarUI Instance;

        [Header("UI Components")]
        public GameObject container;
        public Slider healthSlider;
        public Text bossTitleText;

        private EnemyStats currentBossStats;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            if (container != null) container.SetActive(false);
        }

        public void ShowBoss(string title, EnemyStats stats)
        {
            currentBossStats = stats;
            if (bossTitleText != null) bossTitleText.text = title;
            if (healthSlider != null)
            {
                healthSlider.maxValue = stats.maxHealth;
                healthSlider.value = stats.currentHealth;
            }
            if (container != null) container.SetActive(true);
        }

        public void HideBoss()
        {
            currentBossStats = null;
            if (container != null) container.SetActive(false);
        }

        private void Update()
        {
            if (currentBossStats != null && healthSlider != null)
            {
                healthSlider.value = currentBossStats.currentHealth;
                if (currentBossStats.currentHealth <= 0)
                {
                    HideBoss();
                }
            }
        }
    }
}
