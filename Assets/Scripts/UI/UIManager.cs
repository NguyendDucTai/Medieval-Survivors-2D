using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using VampireSurvivors.Player;

namespace VampireSurvivors.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        [Header("References")]
        public PlayerStats playerStats;

        [Header("Primary HUD Elements")]
        public Slider healthBar;
        public Slider expBar;
        public Text levelText;
        public Text timerText;
        public Text killCountText;
        public Text goldText;
        public Text bannerText;

        [Header("UI Panels")]
        public GameObject levelUpPanel;
        public LevelUpUI levelUpUI;
        public RunSummaryUI runSummaryUI;
        public ShopUI shopUI;
        public AchievementUI achievementUI;
        public BossHealthBarUI bossHealthBarUI;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            if (bannerText != null) bannerText.gameObject.SetActive(false);
        }

        private void Start()
        {
            if (playerStats == null)
            {
                playerStats = FindObjectOfType<PlayerStats>();
            }

            if (Wave.WaveManager.Instance != null)
            {
                Wave.WaveManager.Instance.OnWaveBanner += ShowWaveBanner;
            }
        }

        private void OnEnable()
        {
            if (playerStats != null)
            {
                playerStats.OnHealthChanged += UpdateHealthUI;
                playerStats.OnExpChanged += UpdateExpUI;
                playerStats.OnLevelUp += OnPlayerLevelUp;
                playerStats.OnGoldChanged += UpdateGoldUI;
                playerStats.OnPlayerDeath += OnPlayerDeath;
            }
        }

        private void OnDisable()
        {
            if (playerStats != null)
            {
                playerStats.OnHealthChanged -= UpdateHealthUI;
                playerStats.OnExpChanged -= UpdateExpUI;
                playerStats.OnLevelUp -= OnPlayerLevelUp;
                playerStats.OnGoldChanged -= UpdateGoldUI;
                playerStats.OnPlayerDeath -= OnPlayerDeath;
            }

            if (Wave.WaveManager.Instance != null)
            {
                Wave.WaveManager.Instance.OnWaveBanner -= ShowWaveBanner;
            }
        }

        private void Update()
        {
            // Update Survival Timer HUD
            if (timerText != null && Core.GameManager.Instance != null)
            {
                float t = Core.GameManager.Instance.survivalTime;
                int mins = Mathf.FloorToInt(t / 60f);
                int secs = Mathf.FloorToInt(t % 60f);
                timerText.text = $"{mins:00}:{secs:00}";
            }

            // Update Kill Count HUD
            if (killCountText != null && playerStats != null)
            {
                killCountText.text = "💀 " + playerStats.inRunKills;
            }
        }

        public void UpdateHealthUI(float current, float max)
        {
            if (healthBar != null)
            {
                healthBar.maxValue = max;
                healthBar.value = current;
            }
        }

        public void UpdateExpUI(float current, float max)
        {
            if (expBar != null)
            {
                expBar.maxValue = max;
                expBar.value = current;
            }
        }

        public void UpdateGoldUI(int gold)
        {
            if (goldText != null)
            {
                goldText.text = "💰 " + gold;
            }
        }

        private void OnPlayerLevelUp(int newLevel)
        {
            if (levelText != null)
            {
                levelText.text = "LV " + newLevel;
            }

            // Use interactive LevelUpUI with 3 cards if available
            if (levelUpUI != null)
            {
                levelUpUI.OpenLevelUp();
            }
            else if (LevelUpUI.Instance != null)
            {
                LevelUpUI.Instance.OpenLevelUp();
            }
            else if (levelUpPanel != null)
            {
                levelUpPanel.SetActive(true);
                Time.timeScale = 0f;
            }
        }

        public void CloseLevelUpPanel()
        {
            if (levelUpUI != null)
            {
                levelUpUI.CloseLevelUp();
            }
            else if (levelUpPanel != null)
            {
                levelUpPanel.SetActive(false);
                Time.timeScale = 1f;
            }
        }

        public void OnPlayerDeath()
        {
            if (runSummaryUI != null)
            {
                runSummaryUI.ShowSummary(false);
            }
            else if (RunSummaryUI.Instance != null)
            {
                RunSummaryUI.Instance.ShowSummary(false);
            }
        }

        public void ShowWaveBanner(string message)
        {
            if (bannerText != null)
            {
                StopCoroutine("AnimateBanner");
                StartCoroutine(AnimateBanner(message));
            }
        }

        private IEnumerator AnimateBanner(string message)
        {
            bannerText.text = message;
            bannerText.gameObject.SetActive(true);
            yield return new WaitForSeconds(3.0f);
            bannerText.gameObject.SetActive(false);
        }
    }
}
