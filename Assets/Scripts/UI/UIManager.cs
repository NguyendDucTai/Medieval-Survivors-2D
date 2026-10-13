using UnityEngine;
using UnityEngine.UI;
using VampireSurvivors.Player;

namespace VampireSurvivors.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("References")]
        public PlayerStats playerStats;

        [Header("UI Elements")]
        public Slider healthBar;
        public Slider expBar;
        public Text levelText;
        public GameObject levelUpPanel;

        private void OnEnable()
        {
            if (playerStats != null)
            {
                playerStats.OnHealthChanged += UpdateHealthUI;
                playerStats.OnExpChanged += UpdateExpUI;
                playerStats.OnLevelUp += OnPlayerLevelUp;
            }
        }

        private void OnDisable()
        {
            if (playerStats != null)
            {
                playerStats.OnHealthChanged -= UpdateHealthUI;
                playerStats.OnExpChanged -= UpdateExpUI;
                playerStats.OnLevelUp -= OnPlayerLevelUp;
            }
        }

        private void UpdateHealthUI(float current, float max)
        {
            if (healthBar != null)
            {
                healthBar.maxValue = max;
                healthBar.value = current;
            }
        }

        private void UpdateExpUI(float current, float max)
        {
            if (expBar != null)
            {
                expBar.maxValue = max;
                expBar.value = current;
            }
        }

        private void OnPlayerLevelUp(int newLevel)
        {
            if (levelText != null)
            {
                levelText.text = "LV " + newLevel;
            }

            if (levelUpPanel != null)
            {
                levelUpPanel.SetActive(true);
                Time.timeScale = 0f; // Pause game for skill upgrade choice
            }
        }

        public void CloseLevelUpPanel()
        {
            if (levelUpPanel != null)
            {
                levelUpPanel.SetActive(false);
                Time.timeScale = 1f; // Resume game
            }
        }
    }
}
