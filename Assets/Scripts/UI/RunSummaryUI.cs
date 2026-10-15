using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VampireSurvivors.Data;
using VampireSurvivors.Player;

namespace VampireSurvivors.UI
{
    public class RunSummaryUI : MonoBehaviour
    {
        public static RunSummaryUI Instance;

        [Header("Panel Container")]
        public GameObject panel;

        [Header("Labels")]
        public Text resultTitleText;
        public Text survivalTimeText;
        public Text levelText;
        public Text killsText;
        public Text damageText;
        public Text runGoldText;
        public Text bankGoldText;

        [Header("Buttons")]
        public Button playAgainButton;
        public Button shopButton;
        public Button achievementsButton;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            if (panel != null) panel.SetActive(false);

            if (playAgainButton != null) playAgainButton.onClick.AddListener(PlayAgain);
            if (shopButton != null) shopButton.onClick.AddListener(OpenShop);
            if (achievementsButton != null) achievementsButton.onClick.AddListener(OpenAchievements);
        }

        public void ShowSummary(bool isVictory)
        {
            if (panel == null) return;

            PlayerStats player = FindObjectOfType<PlayerStats>();
            float time = Core.GameManager.Instance != null ? Core.GameManager.Instance.survivalTime : 0f;
            int level = player != null ? player.currentLevel : 1;
            int kills = player != null ? player.inRunKills : 0;
            float damage = player != null ? player.inRunDamageDealt : 0f;
            int runGold = player != null ? player.inRunGold : 0;

            // Save to persistent storage
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.RecordRunEnd(time, level, kills, runGold);
            }

            // Check survival achievements
            if (MetaProgression.AchievementManager.Instance != null)
            {
                MetaProgression.AchievementManager.Instance.CheckProgress(MetaProgression.AchievementType.SurvivalTime, time);
                MetaProgression.AchievementManager.Instance.CheckProgress(MetaProgression.AchievementType.GoldEarned, SaveManager.Instance.Data.gold);
            }

            if (resultTitleText != null)
            {
                resultTitleText.text = isVictory ? "★ CHIẾN THẮNG HUY HOÀNG ★" : "KẾT THÚC TRẬN ĐẤU";
                resultTitleText.color = isVictory ? new Color(1f, 0.85f, 0.2f) : new Color(0.9f, 0.3f, 0.3f);
            }

            int mins = Mathf.FloorToInt(time / 60f);
            int secs = Mathf.FloorToInt(time % 60f);
            if (survivalTimeText != null) survivalTimeText.text = $"Thời Gian Sống Sót: {mins:00}:{secs:00}";
            if (levelText != null) levelText.text = $"Cấp Độ Đạt Được: Lv {level}";
            if (killsText != null) killsText.text = $"Quái Vật Tiêu Diệt: {kills:N0}";
            if (damageText != null) damageText.text = $"Tổng Sát Thương: {damage:N0}";
            if (runGoldText != null) runGoldText.text = $"Vàng Thu Thập: +{runGold:N0} Vàng";
            if (bankGoldText != null) bankGoldText.text = $"Quỹ Vàng Hiện Có: {SaveManager.Instance.Data.gold:N0} Vàng";

            panel.SetActive(true);
            Time.timeScale = 0f;
        }

        public void PlayAgain()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void OpenShop()
        {
            if (ShopUI.Instance != null)
            {
                ShopUI.Instance.OpenShop();
            }
        }

        public void OpenAchievements()
        {
            if (AchievementUI.Instance != null)
            {
                AchievementUI.Instance.OpenAchievements();
            }
        }
    }
}
