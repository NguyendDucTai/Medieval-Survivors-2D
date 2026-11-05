using UnityEngine;
using System.Collections.Generic;

namespace VampireSurvivors.Environment
{
    /// <summary>
    /// Manages the in-game background terrain, dynamically tiling/duplicating the selected map illustration
    /// (e.g. Scorching Desert, Emerald Meadow, Castle Ruins, Infernal Volcano) in a seamless 5x5 grid
    /// around the player, creating a massive/infinite playable battlefield.
    /// </summary>
    public class MapEnvironmentManager : MonoBehaviour
    {
        public static MapEnvironmentManager Instance { get; private set; }

        [Header("Tiling Grid Settings")]
        [Tooltip("Number of columns in the duplicated background grid")]
        public int gridCols = 5;

        [Tooltip("Number of rows in the duplicated background grid")]
        public int gridRows = 5;

        [Tooltip("Sorting layer order for background terrain tiles")]
        public int sortingOrder = -10;

        public Sprite currentMapSprite;
        public int currentMapIndex = 1;
        private List<Transform> tileTransforms = new List<Transform>();
        private float tileWidth = 27.52f;
        private float tileHeight = 15.36f;
        private Transform playerTransform;

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
        }

        private void Start()
        {
            FindPlayer();
            if (tileTransforms.Count == 0)
            {
                // Auto-load currently selected map from PlayerPrefs (Default: 1 = Scorching Desert)
                int mapIdx = PlayerPrefs.GetInt("SelectedMapIndex", 1);
                ApplyMapByIndex(mapIdx);
            }
        }

        public void ApplyMapByIndex(int mapIndex)
        {
            currentMapIndex = mapIndex;
            EnsureHazardManager(mapIndex);
            string path = "Assets/Sprites/Maps/Map_ScorchingDesert.png";
            if (mapIndex == 0) path = "Assets/Sprites/Maps/Map_EmeraldMeadow_Ground.png";
            else if (mapIndex == 1) path = "Assets/Sprites/Maps/Map_ScorchingDesert.png";
            else if (mapIndex == 2) path = "Assets/Sprites/Maps/Map_CastleRuins_Ground.png";
            else if (mapIndex == 3) path = "Assets/Sprites/Maps/Map_InfernalVolcano.png";

#if UNITY_EDITOR
            Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s == null && mapIndex == 0)
            {
                s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Maps/Map_EmeraldMeadow.png");
            }
            else if (s == null && mapIndex == 2)
            {
                s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Maps/Map_CastleRuins.png");
            }
            if (s != null)
            {
                ApplyMap(s, mapIndex);
                return;
            }
#endif
            if (System.IO.File.Exists(path))
            {
                try
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(path);
                    Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (tex.LoadImage(bytes))
                    {
                        tex.filterMode = (mapIndex == 0 || mapIndex == 2) ? FilterMode.Bilinear : FilterMode.Point;
                        float ppu = (mapIndex == 0) ? 50f : (mapIndex == 2 ? 60f : 16f);
                        Sprite loadedSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), ppu);
                        ApplyMap(loadedSprite, mapIndex);
                        return;
                    }
                }
                catch { }
            }

            if (currentMapSprite != null)
            {
                ApplyMap(currentMapSprite, mapIndex);
            }
        }

        public void ApplyMapIndex(int mapIndex, Sprite fallbackPreview = null)
        {
            currentMapIndex = mapIndex;
            EnsureHazardManager(mapIndex);
            if (mapIndex == 0 || mapIndex == 2)
            {
                // Prioritize the clean modular ground plate for Meadow & Castle Ruins so props can be spawned separately
                ApplyMapByIndex(mapIndex);
                return;
            }

            if (fallbackPreview != null)
            {
                ApplyMap(fallbackPreview, mapIndex);
            }
            else
            {
                ApplyMapByIndex(mapIndex);
            }
        }

        /// <summary>
        /// Applies the chosen map sprite and duplicates it into a massive 5x5 grid of tiles that loops infinitely.
        /// </summary>
        public void ApplyMap(Sprite mapSprite, int mapIndex = -1)
        {
            if (mapSprite == null) return;
            currentMapSprite = mapSprite;
            if (mapIndex >= 0)
            {
                currentMapIndex = mapIndex;
            }
            else if (mapSprite.name.Contains("EmeraldMeadow"))
            {
                currentMapIndex = 0;
            }

            // Turn off parent SpriteRenderer so only child duplicated tiles are rendered
            SpriteRenderer parentSR = GetComponent<SpriteRenderer>();
            if (parentSR != null)
            {
                parentSR.enabled = false;
            }

            // Calculate world dimensions of a single map plate
            tileWidth = mapSprite.rect.width / mapSprite.pixelsPerUnit;
            tileHeight = mapSprite.rect.height / mapSprite.pixelsPerUnit;

            // Safety check against zero or invalid size
            if (tileWidth <= 0.01f) tileWidth = 27.52f;
            if (tileHeight <= 0.01f) tileHeight = 15.36f;

            // Clean up old child tiles
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
            tileTransforms.Clear();

            // Spawn gridCols x gridRows duplicated tiles centered around origin
            int halfCols = gridCols / 2;
            int halfRows = gridRows / 2;

            for (int r = -halfRows; r <= halfRows; r++)
            {
                for (int c = -halfCols; c <= halfCols; c++)
                {
                    GameObject tileGO = new GameObject($"MapTile_{c}_{r}");
                    tileGO.transform.SetParent(transform, false);
                    tileGO.transform.position = new Vector3(c * tileWidth, r * tileHeight, 0f);

                    SpriteRenderer sr = tileGO.AddComponent<SpriteRenderer>();
                    sr.sprite = mapSprite;
                    sr.sortingOrder = sortingOrder;
                    sr.drawMode = SpriteDrawMode.Simple;
                    sr.color = Color.white;

                    // If Emerald Meadow or Castle Ruins, populate modular interactive props (trees, ruins, walls, rocks)
                    if (currentMapIndex == 0)
                    {
                        MeadowPropManager.PopulateTileProps(tileGO, c, r);
                    }
                    else if (currentMapIndex == 2)
                    {
                        CastlePropManager.PopulateTileProps(tileGO, c, r);
                    }

                    tileTransforms.Add(tileGO.transform);
                }
            }

            Debug.Log($"<color=green>[MapEnvironmentManager]</color> Applied map '{mapSprite.name}' (MapIndex: {currentMapIndex}) with 5x5 infinite grid ({gridCols * tileWidth:F1}m x {gridRows * tileHeight:F1}m)!");

            // Synchronize colors of all existing monsters in the scene to match the newly applied map
            Enemies.EnemyStats.RefreshAllEnemyColors();
        }

        private void LateUpdate()
        {
            if (tileTransforms.Count == 0 || currentMapSprite == null) return;

            if (playerTransform == null)
            {
                FindPlayer();
                if (playerTransform == null) return;
            }

            Vector3 playerPos = playerTransform.position;
            float totalWidth = gridCols * tileWidth;
            float totalHeight = gridRows * tileHeight;
            float halfWidth = totalWidth * 0.5f;
            float halfHeight = totalHeight * 0.5f;

            // Wrap each tile dynamically so the map loops seamlessly around the player in all directions
            for (int i = 0; i < tileTransforms.Count; i++)
            {
                Transform t = tileTransforms[i];
                if (t == null) continue;

                Vector3 pos = t.position;
                float diffX = pos.x - playerPos.x;
                while (diffX > halfWidth)
                {
                    pos.x -= totalWidth;
                    diffX -= totalWidth;
                }
                while (diffX < -halfWidth)
                {
                    pos.x += totalWidth;
                    diffX += totalWidth;
                }

                float diffY = pos.y - playerPos.y;
                while (diffY > halfHeight)
                {
                    pos.y -= totalHeight;
                    diffY -= totalHeight;
                }
                while (diffY < -halfHeight)
                {
                    pos.y += totalHeight;
                    diffY += totalHeight;
                }

                t.position = pos;
            }
        }

        private void FindPlayer()
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
            else
            {
                var pc = FindAnyObjectByType<Player.PlayerController>();
                if (pc != null) playerTransform = pc.transform;
            }
        }

        private void EnsureHazardManager(int mapIndex)
        {
            currentMapIndex = mapIndex;
            var hm = Hazards.MapHazardManager.Instance;
            if (hm == null) hm = FindAnyObjectByType<Hazards.MapHazardManager>();
            if (hm == null) hm = GetComponent<Hazards.MapHazardManager>();
            if (hm == null) hm = gameObject.AddComponent<Hazards.MapHazardManager>();
            hm.ConfigureForMap(mapIndex);
        }
    }
}
