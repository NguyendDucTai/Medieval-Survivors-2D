using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VampireSurvivors.MetaProgression;

namespace VampireSurvivors.UI
{
    public class AchievementUI : MonoBehaviour
    {
        public static AchievementUI Instance;

        [Header("UI Containers")]
        public GameObject panel;
        public Button closeButton;
        public Transform itemsScrollContent;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            if (panel != null) panel.SetActive(false);
            if (closeButton != null) closeButton.onClick.AddListener(CloseAchievements);
        }

        public void OpenAchievements()
        {
            if (panel == null) return;
            panel.SetActive(true);
            RefreshList();
        }

        public void CloseAchievements()
        {
            if (panel != null) panel.SetActive(false);
        }

        public void RefreshList()
        {
            if (AchievementManager.Instance == null || itemsScrollContent == null) return;

            // Clear old children
            foreach (Transform child in itemsScrollContent)
            {
                Destroy(child.gameObject);
            }

            var list = AchievementManager.Instance.allAchievements;
            foreach (var ach in list)
            {
                bool isDone = AchievementManager.Instance.IsCompleted(ach.id);
                CreateAchievementRow(ach, isDone);
            }
        }

        private void CreateAchievementRow(AchievementData ach, bool isDone)
        {
            GameObject rowGO = new GameObject("Ach_" + ach.id);
            rowGO.transform.SetParent(itemsScrollContent, false);

            LayoutElement le = rowGO.AddComponent<LayoutElement>();
            le.minHeight = 42f;
            le.preferredHeight = 42f;

            HorizontalLayoutGroup hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.spacing = 10f;

            // Text
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(rowGO.transform, false);
            Text t = textGO.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 13;
            t.alignment = TextAnchor.MiddleLeft;

            string statusColor = isDone ? "#2ecc71" : "#e74c3c";
            string statusIcon = isDone ? "✔ ĐÃ HOÀN THÀNH" : "⏳ ĐANG TIẾN HÀNH";
            string rewardHero = !string.IsNullOrEmpty(ach.unlockHeroReward) ? $" | Tướng: {ach.unlockHeroReward}" : "";

            t.text = $"<color={statusColor}><b>[{statusIcon}] {ach.title}</b></color>\n<size=11><color=#cccccc>{ach.description} (Thưởng: +{ach.goldReward} Vàng{rewardHero})</color></size>";
        }
    }
}
