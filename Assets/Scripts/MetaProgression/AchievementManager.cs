using System;
using System.Collections.Generic;
using UnityEngine;
using VampireSurvivors.Data;

namespace VampireSurvivors.MetaProgression
{
    public class AchievementManager : MonoBehaviour
    {
        private static AchievementManager instance;
        public static AchievementManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("AchievementManager");
                    instance = go.AddComponent<AchievementManager>();
                    DontDestroyOnLoad(go);
                    instance.InitializeAchievements();
                }
                return instance;
            }
        }

        public List<AchievementData> allAchievements = new List<AchievementData>();

        public event Action<AchievementData> OnAchievementUnlocked;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAchievements();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void InitializeAchievements()
        {
            if (allAchievements != null && allAchievements.Count > 0) return;

            allAchievements = new List<AchievementData>
            {
                // Kill Milestones
                new AchievementData { id = "kill_100", title = "Tân Binh Quét Quái", description = "Hạ gục tổng cộng 100 quái vật", type = AchievementType.TotalKills, targetValue = 100, goldReward = 200 },
                new AchievementData { id = "kill_500", title = "Chiến Binh Gan Dạ", description = "Hạ gục tổng cộng 500 quái vật", type = AchievementType.TotalKills, targetValue = 500, goldReward = 500 },
                new AchievementData { id = "kill_1000", title = "Bóng Đêm Tử Thần", description = "Hạ gục 1,000 quái vật - Mở khóa Sát Thủ Bóng Tối (Rogue)", type = AchievementType.TotalKills, targetValue = 1000, goldReward = 1000, unlockHeroReward = "Rogue" },
                new AchievementData { id = "kill_2500", title = "Đao Phủ Hàng Loạt", description = "Hạ gục tổng cộng 2,500 quái vật", type = AchievementType.TotalKills, targetValue = 2500, goldReward = 2000 },
                new AchievementData { id = "kill_5000", title = "Thần Hủy Diệt", description = "Hạ gục tổng cộng 5,000 quái vật", type = AchievementType.TotalKills, targetValue = 5000, goldReward = 5000 },
                new AchievementData { id = "kill_10000", title = "Huyền Thoại Bất Diệt", description = "Hạ gục 10,000 quái vật trên mọi chiến trường", type = AchievementType.TotalKills, targetValue = 10000, goldReward = 10000 },

                // Survival Milestones
                new AchievementData { id = "survive_180", title = "Bước Đầu Sống Sót", description = "Sống sót liên tục 3 phút trong 1 trận", type = AchievementType.SurvivalTime, targetValue = 180, goldReward = 200 },
                new AchievementData { id = "survive_300", title = "Ý Chí Kiên Cường", description = "Sống sót liên tục 5 phút trong 1 trận", type = AchievementType.SurvivalTime, targetValue = 300, goldReward = 500 },
                new AchievementData { id = "survive_600", title = "Bậc Thầy Trụ Hạng", description = "Sống sót liên tục 10 phút trong 1 trận", type = AchievementType.SurvivalTime, targetValue = 600, goldReward = 1000 },
                new AchievementData { id = "survive_900", title = "Chiến Binh Thép", description = "Sống sót liên tục 15 phút trong 1 trận", type = AchievementType.SurvivalTime, targetValue = 900, goldReward = 2000 },
                new AchievementData { id = "survive_1200", title = "Đấng Cứu Thế Vương Triều", description = "Sống sót trọn vẹn 20 phút - Thắng trận hoàn toàn!", type = AchievementType.SurvivalTime, targetValue = 1200, goldReward = 5000 },

                // Level Milestones
                new AchievementData { id = "level_10", title = "Khai Phá Tiềm Năng", description = "Đạt Cấp 10 trong một trận đấu", type = AchievementType.LevelReached, targetValue = 10, goldReward = 200 },
                new AchievementData { id = "level_20", title = "Sức Mạnh Bộc Phát", description = "Đạt Cấp 20 trong một trận đấu", type = AchievementType.LevelReached, targetValue = 20, goldReward = 500 },
                new AchievementData { id = "level_30", title = "Đại Pháp Sư Tối Thượng", description = "Đạt Cấp 30 - Mở khóa Đại Pháp Sư (Archmage)", type = AchievementType.LevelReached, targetValue = 30, goldReward = 1000, unlockHeroReward = "Archmage" },
                new AchievementData { id = "level_40", title = "Đỉnh Cao Tu Vi", description = "Đạt Cấp 40 trong một trận đấu", type = AchievementType.LevelReached, targetValue = 40, goldReward = 2500 },

                // Evolution Milestones
                new AchievementData { id = "evolve_1", title = "Bí Thuật Tiến Hóa", description = "Tiến hóa thành công 1 Thần Binh tối thượng", type = AchievementType.WeaponEvolved, targetValue = 1, goldReward = 500 },
                new AchievementData { id = "evolve_3", title = "Kho Thần Khí", description = "Tiến hóa thành công 3 loại Thần Binh khác nhau", type = AchievementType.WeaponEvolved, targetValue = 3, goldReward = 1500 },
                new AchievementData { id = "evolve_6", title = "Chủ Nhân Vạn Khí", description = "Tiến hóa đủ cả 6 loại Thần Binh tối thượng", type = AchievementType.WeaponEvolved, targetValue = 6, goldReward = 5000 },

                // Boss Milestones
                new AchievementData { id = "boss_slime", title = "Kẻ Diệt Vua Nhầy", description = "Hạ gục Vua Slime (King Slime)", type = AchievementType.BossDefeated, targetValue = 1, goldReward = 500 },
                new AchievementData { id = "boss_skeleton", title = "Nghiền Nát Xương Trắng", description = "Hạ gục Đại Tướng Bộ Xương (Skeleton Warlord)", type = AchievementType.BossDefeated, targetValue = 2, goldReward = 1000 },
                new AchievementData { id = "boss_golem", title = "Kẻ Chinh Phục Thần Thạch", description = "Hạ gục Đại Trùm Thế Giới: Golem Khổng Lồ", type = AchievementType.BossDefeated, targetValue = 3, goldReward = 3000 },

                // Gold Economy Milestones
                new AchievementData { id = "gold_1000", title = "Túi Tiền Rủng Rỉnh", description = "Thu thập tổng cộng 1,000 Vàng", type = AchievementType.GoldEarned, targetValue = 1000, goldReward = 300 },
                new AchievementData { id = "gold_5000", title = "Thương Nhân Giàu Có", description = "Thu thập tổng cộng 5,000 Vàng", type = AchievementType.GoldEarned, targetValue = 5000, goldReward = 1500 },
                new AchievementData { id = "gold_20000", title = "Kho Báu Hoàng Gia", description = "Thu thập tổng cộng 20,000 Vàng", type = AchievementType.GoldEarned, targetValue = 20000, goldReward = 6000 }
            };
        }

        public bool IsCompleted(string achievementId)
        {
            if (SaveManager.Instance.Data == null) SaveManager.Instance.Load();
            return SaveManager.Instance.Data.completedAchievements.Contains(achievementId);
        }

        public void CheckProgress(AchievementType type, float currentValue)
        {
            InitializeAchievements();
            if (SaveManager.Instance.Data == null) SaveManager.Instance.Load();

            foreach (var ach in allAchievements)
            {
                if (ach.type == type && !IsCompleted(ach.id))
                {
                    if (currentValue >= ach.targetValue)
                    {
                        UnlockAchievement(ach);
                    }
                }
            }
        }

        private void UnlockAchievement(AchievementData ach)
        {
            SaveManager.Instance.Data.completedAchievements.Add(ach.id);
            if (ach.goldReward > 0)
            {
                SaveManager.Instance.AddGold(ach.goldReward);
            }
            if (!string.IsNullOrEmpty(ach.unlockHeroReward))
            {
                SaveManager.Instance.UnlockHero(ach.unlockHeroReward);
            }
            SaveManager.Instance.Save();

            OnAchievementUnlocked?.Invoke(ach);
            Debug.Log($"<color=yellow>[ACHIEVEMENT UNLOCKED]</color> {ach.title}! Thưởng: +{ach.goldReward} Vàng!");
        }
    }
}
