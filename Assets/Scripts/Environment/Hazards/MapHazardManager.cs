using UnityEngine;
using VampireSurvivors.Core;
using VampireSurvivors.Player;
using VampireSurvivors.UI;

namespace VampireSurvivors.Environment.Hazards
{
    /// <summary>
    /// Central coordinator for UC-05 Map-Specific Hazards across all 4 maps:
    /// - Map 0 (Emerald Meadow): Destructible Ancient Bushes (Healing Potion / ExpGem Magnet)
    /// - Map 1 (Scorching Desert): Periodic Pixel Sandstorm pushing rb.linearVelocity via externalVelocity
    /// - Map 2 (Castle Ruins): Arcane Summoning Circle (10s channel -> Elite Skeleton/Goblin treasure shower)
    /// - Map 3 (Infernal Volcano): Lava Crack Fissures (2s warning -> 1.5s eruption hurting Player & Enemies)
    /// </summary>
    public class MapHazardManager : MonoBehaviour
    {
        public static MapHazardManager Instance { get; private set; }

        [Header("Active Map Index (0=Meadow, 1=Desert, 2=Ruins, 3=Volcano)")]
        public int activeMapIndex = 1;

        private MeadowBushHazard meadowHazard;
        private SandstormHazard desertHazard;
        private SummoningCircleHazard ruinsHazard;
        private MeteorShowerHazard volcanoHazard;
        private GameObject playerGlowGO;

        private Transform playerTransform;
        private bool wasPausedOrEnded = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(this);
                return;
            }

            EnsureSubHazards();
        }

        private void Start()
        {
            FindPlayer();
            int savedMap = 1;
            if (Environment.MapEnvironmentManager.Instance != null) savedMap = Environment.MapEnvironmentManager.Instance.currentMapIndex;
            else if (MainMenuManager.Instance != null) savedMap = MainMenuManager.Instance.selectedMapIndex;
            else savedMap = PlayerPrefs.GetInt("SelectedMapIndex", 1);
            ConfigureForMap(savedMap);
        }

        private void EnsureSubHazards()
        {
            if (meadowHazard == null) meadowHazard = GetComponent<MeadowBushHazard>() ?? gameObject.AddComponent<MeadowBushHazard>();
            if (desertHazard == null) desertHazard = GetComponent<SandstormHazard>() ?? gameObject.AddComponent<SandstormHazard>();
            if (ruinsHazard == null) ruinsHazard = GetComponent<SummoningCircleHazard>() ?? gameObject.AddComponent<SummoningCircleHazard>();
            if (volcanoHazard == null) volcanoHazard = GetComponent<MeteorShowerHazard>() ?? gameObject.AddComponent<MeteorShowerHazard>();
        }

        public void ConfigureForMap(int mapIndex)
        {
            EnsureSubHazards();
            ClearAllHazards();

            activeMapIndex = Mathf.Clamp(mapIndex, 0, 3);

            meadowHazard.enabled = (activeMapIndex == 0);
            desertHazard.enabled = (activeMapIndex == 1);
            ruinsHazard.enabled = (activeMapIndex == 2);
            volcanoHazard.enabled = (activeMapIndex == 3);
            UpdatePlayerGlow();

            Debug.Log($"<color=#55FFAA>[MapHazardManager]</color> Configured UC-05 Hazard for Map Index {activeMapIndex}.");
        }

        public void ClearAllHazards()
        {
            if (meadowHazard != null) meadowHazard.ClearAll();
            if (desertHazard != null) desertHazard.ClearAll();
            if (ruinsHazard != null) ruinsHazard.ClearAll();
            if (volcanoHazard != null) volcanoHazard.ClearAll();

            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.externalVelocity = Vector2.zero;
            }
        }

        private void Update()
        {
            bool menuOpen = (MainMenuManager.Instance != null && MainMenuManager.Instance.isMenuOpen);
            bool gameOver = (GameManager.Instance != null && GameManager.Instance.isGameOver);

            if (menuOpen || gameOver)
            {
                if (!wasPausedOrEnded)
                {
                    wasPausedOrEnded = true;
                    ClearAllHazards();
                }
                return;
            }

            wasPausedOrEnded = false;

            if (Time.timeScale <= 0.01f) return;

            if (playerTransform == null)
            {
                FindPlayer();
                if (playerTransform == null) return;
            }

            float dt = Time.deltaTime;
            UpdatePlayerGlow();

            switch (activeMapIndex)
            {
                case 0:
                    if (meadowHazard != null) meadowHazard.TickHazard(dt, playerTransform);
                    break;
                case 1:
                    if (desertHazard != null) desertHazard.TickHazard(dt, playerTransform);
                    break;
                case 2:
                    if (ruinsHazard != null) ruinsHazard.TickHazard(dt, playerTransform);
                    break;
                case 3:
                    if (volcanoHazard != null) volcanoHazard.TickHazard(dt, playerTransform);
                    break;
            }

            HandleDebugHotkeys();
        }

        private void HandleDebugHotkeys()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                EnsureSubHazards();
                meadowHazard.enabled = true;
                meadowHazard.ForceTrigger(playerTransform);
                Debug.Log("<color=yellow>[DEV TEST F5]</color> Forced Emerald Meadow Ancient Bushes!");
            }
            if (Input.GetKeyDown(KeyCode.F6))
            {
                EnsureSubHazards();
                desertHazard.enabled = true;
                desertHazard.ForceTrigger();
                Debug.Log("<color=yellow>[DEV TEST F6]</color> Forced Scorching Desert Sandstorm!");
            }
            if (Input.GetKeyDown(KeyCode.F7))
            {
                EnsureSubHazards();
                ruinsHazard.enabled = true;
                ruinsHazard.ForceTrigger(playerTransform);
                Debug.Log("<color=yellow>[DEV TEST F7]</color> Forced Castle Ruins Summoning Circle!");
            }
            if (Input.GetKeyDown(KeyCode.F8))
            {
                EnsureSubHazards();
                volcanoHazard.enabled = true;
                volcanoHazard.ForceTrigger(playerTransform);
                Debug.Log("<color=yellow>[DEV TEST F8]</color> Forced Infernal Volcano Meteor Shower!");
            }
        }

        private void UpdatePlayerGlow()
        {
            if (playerTransform == null) FindPlayer();
            if (playerTransform == null) return;

            if (activeMapIndex == 3)
            {
                if (playerGlowGO == null)
                {
                    playerGlowGO = new GameObject("Player_VolcanoAuraGlow");
                    playerGlowGO.transform.SetParent(playerTransform, false);
                    playerGlowGO.transform.localPosition = new Vector3(0f, -0.2f, 0f);
                    playerGlowGO.transform.localScale = Vector3.one * 3.2f;

                    SpriteRenderer glowSR = playerGlowGO.AddComponent<SpriteRenderer>();
                    glowSR.sprite = HazardSpriteLoader.GetPlayerAuraSprite();
                    glowSR.sortingOrder = -2; // Below player sprite, above ground
                }
                if (!playerGlowGO.activeSelf) playerGlowGO.SetActive(true);
            }
            else
            {
                if (playerGlowGO != null && playerGlowGO.activeSelf) playerGlowGO.SetActive(false);
            }
        }

        private void FindPlayer()
        {
            if (PlayerController.Instance != null)
            {
                playerTransform = PlayerController.Instance.transform;
                return;
            }

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }
    }
}
