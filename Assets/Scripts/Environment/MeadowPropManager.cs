using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VampireSurvivors.Environment
{
    /// <summary>
    /// Spawns modular 2D Obstacle Props (Trees, Ruin Pillars, Mossy Rocks, Fences, Logs)
    /// across Emerald Meadow tiles.
    /// Each prop features:
    /// 1. True physical Foot Colliders (blocking player and enemy movement at base/trunk).
    /// 2. Dynamic Y-Sorting (YSortProp) so tree canopies naturally conceal entities behind them.
    /// 3. Parented to MapTile so they seamlessly wrap with the 5x5 infinite scrolling environment!
    /// </summary>
    public static class MeadowPropManager
    {
        private static readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

        public static Sprite LoadPropSprite(string filename)
        {
            if (spriteCache.TryGetValue(filename, out Sprite cached) && cached != null)
                return cached;

            string path = "Assets/Sprites/Environment/Props/" + filename;

#if UNITY_EDITOR
            Sprite editorSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (editorSprite != null)
            {
                spriteCache[filename] = editorSprite;
                return editorSprite;
            }
#endif
            if (File.Exists(path))
            {
                try
                {
                    byte[] bytes = File.ReadAllBytes(path);
                    Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (tex.LoadImage(bytes))
                    {
                        tex.filterMode = FilterMode.Point;
                        tex.wrapMode = TextureWrapMode.Clamp;
                        Sprite s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.15f), 16f);
                        spriteCache[filename] = s;
                        return s;
                    }
                }
                catch { }
            }

            return null;
        }

        public static void PopulateTileProps(GameObject tileGO, int col, int row)
        {
            if (tileGO == null) return;

            Transform propsRoot = new GameObject("Props_Root").transform;
            propsRoot.SetParent(tileGO.transform, false);

            // Pseudo-random offset based on tile coordinates to add organic variety
            int seed = (col * 73856093) ^ (row * 19349663);
            Random.InitState(seed);

            // --- QUADRANT 1 (Top-Left): Ancient Ruins & Mossy Rocks ---
            SpawnProp(propsRoot, "Prop_Ruin_Arch.png", -6.5f, 3.8f, PropColliderType.Box, 1.2f, 0.45f);
            SpawnProp(propsRoot, "Prop_Ruin_PillarTall.png", -8.5f, 4.6f, PropColliderType.Box, 0.55f, 0.40f);
            SpawnProp(propsRoot, "Prop_Ruin_PillarTall.png", -4.2f, 4.2f, PropColliderType.Box, 0.55f, 0.40f);
            SpawnProp(propsRoot, "Prop_Ruin_PillarBroken.png", -7.2f, 2.0f, PropColliderType.Box, 0.55f, 0.35f);
            SpawnProp(propsRoot, "Prop_Rock_Large.png", -9.5f, 2.2f, PropColliderType.Circle, 0.55f);

            // --- QUADRANT 2 (Top-Right): Great Oak & Pine Forest Grove ---
            SpawnProp(propsRoot, "Prop_Tree_GreatOak.png", 7.2f, 4.5f, PropColliderType.Circle, 0.48f);
            SpawnProp(propsRoot, "Prop_Tree_Pine.png", 5.0f, 5.2f, PropColliderType.Circle, 0.38f);
            SpawnProp(propsRoot, "Prop_Tree_Oak.png", 9.8f, 4.0f, PropColliderType.Circle, 0.45f);
            SpawnProp(propsRoot, "Prop_Fence_Wood.png", 6.2f, 2.2f, PropColliderType.Box, 1.15f, 0.32f);
            SpawnProp(propsRoot, "Prop_Rock_Cluster.png", 8.8f, 1.8f, PropColliderType.Circle, 0.50f);

            // --- QUADRANT 3 (Bottom-Left): Pine Woods & Fallen Log ---
            SpawnProp(propsRoot, "Prop_Tree_Pine.png", -7.5f, -4.2f, PropColliderType.Circle, 0.38f);
            SpawnProp(propsRoot, "Prop_Tree_Pine.png", -10.0f, -3.2f, PropColliderType.Circle, 0.38f);
            SpawnProp(propsRoot, "Prop_Tree_RoundBush.png", -5.5f, -4.8f, PropColliderType.Circle, 0.42f);
            SpawnProp(propsRoot, "Prop_Log_Mossy.png", -4.2f, -2.2f, PropColliderType.Box, 1.25f, 0.35f);
            SpawnProp(propsRoot, "Prop_Ruin_PillarFallen.png", -7.8f, -1.8f, PropColliderType.Box, 0.95f, 0.35f);

            // --- QUADRANT 4 (Bottom-Right): Rocky Outpost & Forest Edge ---
            SpawnProp(propsRoot, "Prop_Tree_Oak.png", 8.0f, -4.6f, PropColliderType.Circle, 0.45f);
            SpawnProp(propsRoot, "Prop_Rock_Large.png", 5.2f, -3.2f, PropColliderType.Circle, 0.55f);
            SpawnProp(propsRoot, "Prop_Rock_Mossy.png", 9.5f, -2.2f, PropColliderType.Circle, 0.45f);
            SpawnProp(propsRoot, "Prop_Fence_Broken.png", 6.5f, -1.8f, PropColliderType.Box, 1.0f, 0.32f);
            SpawnProp(propsRoot, "Prop_Tree_RoundBush.png", 10.5f, -4.2f, PropColliderType.Circle, 0.42f);
        }

        private enum PropColliderType { Circle, Box }

        private static void SpawnProp(Transform parent, string spriteFilename, float localX, float localY, PropColliderType colType, float param1, float param2 = 0f)
        {
            Sprite s = LoadPropSprite(spriteFilename);
            if (s == null) return;

            GameObject propGO = new GameObject(spriteFilename.Replace(".png", ""));
            propGO.transform.SetParent(parent, false);
            propGO.transform.localPosition = new Vector3(localX, localY, 0f);

            // 1. Sprite Renderer
            SpriteRenderer sr = propGO.AddComponent<SpriteRenderer>();
            sr.sprite = s;
            sr.sortingLayerName = "Default";

            // 2. Dynamic Y-Sorting Component
            YSortProp ysort = propGO.AddComponent<YSortProp>();
            ysort.UpdateSorting();

            // 3. Physical Foot Collider (blocks player & enemy feet)
            if (colType == PropColliderType.Circle)
            {
                CircleCollider2D circle = propGO.AddComponent<CircleCollider2D>();
                circle.radius = param1;
                circle.offset = Vector2.zero; // Pivot is already at foot/base (0.5, 0.15)
            }
            else if (colType == PropColliderType.Box)
            {
                BoxCollider2D box = propGO.AddComponent<BoxCollider2D>();
                box.size = new Vector2(param1, param2);
                box.offset = Vector2.zero;
            }
        }
    }
}

