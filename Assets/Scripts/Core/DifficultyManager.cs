using UnityEngine;

namespace VampireSurvivors.Core
{
    public enum DifficultyMode
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }

    /// <summary>
    /// UC-06: Global Difficulty Mode Manager (EASY, MEDIUM, HARD) applied across all 4 maps.
    /// Controls monster health, chase speed, attack damage, wave spawn density, environmental hazard intensity,
    /// and Gold / EXP reward multipliers.
    /// </summary>
    public class DifficultyManager : MonoBehaviour
    {
        public static DifficultyManager Instance { get; private set; }

        public const string PrefKeyDifficulty = "SelectedDifficulty";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        public static DifficultyMode GetCurrentDifficulty()
        {
            int val = Mathf.Clamp(PlayerPrefs.GetInt(PrefKeyDifficulty, 0), 0, 2);
            return (DifficultyMode)val;
        }

        public static int GetCurrentDifficultyIndex()
        {
            return Mathf.Clamp(PlayerPrefs.GetInt(PrefKeyDifficulty, 0), 0, 2);
        }

        public static void SetDifficulty(int difficultyIndex)
        {
            int clamped = Mathf.Clamp(difficultyIndex, 0, 2);
            PlayerPrefs.SetInt(PrefKeyDifficulty, clamped);
            PlayerPrefs.Save();
        }

        public static void SetDifficulty(DifficultyMode mode)
        {
            SetDifficulty((int)mode);
        }

        public static string GetDifficultyName(int index = -1)
        {
            DifficultyMode mode = index >= 0 ? (DifficultyMode)Mathf.Clamp(index, 0, 2) : GetCurrentDifficulty();
            switch (mode)
            {
                case DifficultyMode.Medium:
                    return "MEDIUM";
                case DifficultyMode.Hard:
                    return "HARD";
                default:
                    return "EASY";
            }
        }

        public static float GetHealthMultiplier(int index = -1)
        {
            DifficultyMode mode = index >= 0 ? (DifficultyMode)Mathf.Clamp(index, 0, 2) : GetCurrentDifficulty();
            switch (mode)
            {
                case DifficultyMode.Medium:
                    return 1.45f; // +45% Monster HP
                case DifficultyMode.Hard:
                    return 2.10f; // +110% Monster HP
                default:
                    return 1.00f;
            }
        }

        public static float GetSpeedMultiplier(int index = -1)
        {
            DifficultyMode mode = index >= 0 ? (DifficultyMode)Mathf.Clamp(index, 0, 2) : GetCurrentDifficulty();
            switch (mode)
            {
                case DifficultyMode.Medium:
                    return 1.20f; // +20% Chase Speed
                case DifficultyMode.Hard:
                    return 1.40f; // +40% Chase Speed
                default:
                    return 1.00f;
            }
        }

        public static float GetDamageMultiplier(int index = -1)
        {
            DifficultyMode mode = index >= 0 ? (DifficultyMode)Mathf.Clamp(index, 0, 2) : GetCurrentDifficulty();
            switch (mode)
            {
                case DifficultyMode.Medium:
                    return 1.30f; // +30% Enemy Attack Damage
                case DifficultyMode.Hard:
                    return 1.75f; // +75% Enemy Attack Damage
                default:
                    return 1.00f;
            }
        }

        public static float GetSpawnDensityMultiplier(int index = -1)
        {
            DifficultyMode mode = index >= 0 ? (DifficultyMode)Mathf.Clamp(index, 0, 2) : GetCurrentDifficulty();
            switch (mode)
            {
                case DifficultyMode.Medium:
                    return 1.35f; // +35% Swarm & Horde Density
                case DifficultyMode.Hard:
                    return 1.70f; // +70% Swarm & Horde Density
                default:
                    return 1.00f;
            }
        }

        public static float GetHazardMultiplier(int index = -1)
        {
            DifficultyMode mode = index >= 0 ? (DifficultyMode)Mathf.Clamp(index, 0, 2) : GetCurrentDifficulty();
            switch (mode)
            {
                case DifficultyMode.Medium:
                    return 1.25f; // +25% Hazard Intensity & Frequency
                case DifficultyMode.Hard:
                    return 1.60f; // +60% Hazard Intensity & Frequency
                default:
                    return 1.00f;
            }
        }

        public static float GetGoldMultiplier(int index = -1)
        {
            DifficultyMode mode = index >= 0 ? (DifficultyMode)Mathf.Clamp(index, 0, 2) : GetCurrentDifficulty();
            switch (mode)
            {
                case DifficultyMode.Medium:
                    return 1.50f; // +50% Gold Reward
                case DifficultyMode.Hard:
                    return 2.25f; // +125% Gold Reward
                default:
                    return 1.00f;
            }
        }

        public static float GetExpMultiplier(int index = -1)
        {
            DifficultyMode mode = index >= 0 ? (DifficultyMode)Mathf.Clamp(index, 0, 2) : GetCurrentDifficulty();
            switch (mode)
            {
                case DifficultyMode.Medium:
                    return 1.35f; // +35% EXP Reward
                case DifficultyMode.Hard:
                    return 1.80f; // +80% EXP Reward
                default:
                    return 1.00f;
            }
        }
    }
}

