using System;
using UnityEngine;

namespace VampireSurvivors.Data
{
    public class SaveManager : MonoBehaviour
    {
        private static SaveManager instance;
        public static SaveManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("SaveManager");
                    instance = go.AddComponent<SaveManager>();
                    DontDestroyOnLoad(go);
                    instance.Load();
                }
                return instance;
            }
        }

        private const string SAVE_KEY = "PRU213_MEDIEVAL_SURVIVORS_SAVE";

        public GameSaveData Data { get; private set; }

        public event Action<int> OnGoldChanged;
        public event Action OnDataSaved;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                Load();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void Load()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                try
                {
                    string json = PlayerPrefs.GetString(SAVE_KEY);
                    Data = JsonUtility.FromJson<GameSaveData>(json);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[SaveManager] Failed to parse save json: " + e.Message);
                    Data = new GameSaveData();
                }
            }

            if (Data == null)
            {
                Data = new GameSaveData();
                // Bonus starting gold for fresh player
                Data.gold = 150;
                Save();
            }
        }

        public void Save()
        {
            if (Data == null) return;
            string json = JsonUtility.ToJson(Data, false);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
            OnDataSaved?.Invoke();
        }

        public void AddGold(int amount)
        {
            if (Data == null) Load();
            Data.gold += amount;
            if (Data.gold < 0) Data.gold = 0;
            Save();
            OnGoldChanged?.Invoke(Data.gold);
        }

        public bool SpendGold(int amount)
        {
            if (Data == null) Load();
            if (Data.gold >= amount)
            {
                Data.gold -= amount;
                Save();
                OnGoldChanged?.Invoke(Data.gold);
                return true;
            }
            return false;
        }

        public void RecordRunEnd(float survivalTime, int level, int kills, int runGold)
        {
            if (Data == null) Load();

            Data.lifetimeRuns++;
            Data.lifetimeSurvivalSeconds += survivalTime;
            Data.lifetimeKills += kills;

            if (survivalTime > Data.bestSurvivalSeconds)
            {
                Data.bestSurvivalSeconds = survivalTime;
            }

            if (level > Data.highestLevelReached)
            {
                Data.highestLevelReached = level;
            }

            AddGold(runGold);
            Save();
        }

        public void UnlockHero(string heroName)
        {
            if (Data == null) Load();
            if (!Data.unlockedHeroes.Contains(heroName))
            {
                Data.unlockedHeroes.Add(heroName);
                Save();
            }
        }
    }
}
