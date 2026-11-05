using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VampireSurvivors.Environment
{
    /// <summary>
    /// Spawns modular Castle Ruins props (Towers, Walls, Statues, Torches, Barrels, Rubble)
    /// across Castle Ruins tiles.
    /// Features:
    /// 1. True Solid Colliders (BoxCollider2D / CircleCollider2D covering the full solid body from the ground up, preventing walking through).
    /// 2. Natural PPU 120 scale matching hero and courtyard proportions (no oversized giant blocks).
    /// 3. Dynamic 2.5D Y-Sorting (YSortProp) for depth occlusion.
    /// 4. Wide-open central courtyard (leaving the circular mosaic emblem completely clear for combat).
    /// </summary>
    public static class CastlePropManager
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
                        Sprite s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.12f), 120f);
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

            Transform propsRoot = new GameObject("CastleProps_Root").transform;
            propsRoot.SetParent(tileGO.transform, false);

            // Deterministic random seed per tile
            int seed = (col * 314159) ^ (row * 271828);
            Random.InitState(seed);

            // 3 spacious archetypes, keeping the central courtyard (±7m X, ±5m Y) 100% open
            int archetype = Mathf.Abs((col * 3 + row * 7) % 3);

            switch (archetype)
            {
                case 0:
                    // --- ARCHETYPE 0: Watchtower Outpost & Ruined Ramparts ---
                    // Top-Left: Round Tower & Standing Torch
                    SpawnProp(propsRoot, "Castle_Tower_Round.png", -11.5f, 5.5f, PropColliderType.Circle, 1.25f, 0f, 1.1f);
                    SpawnProp(propsRoot, "Castle_Torch_Tall.png", -9.0f, 4.5f, PropColliderType.Circle, 0.40f, 0f, 0.4f);

                    // Top-Right: Ruined Wall Segment
                    SpawnProp(propsRoot, "Castle_Wall_Ruined.png", 10.5f, 5.8f, PropColliderType.Box, 2.7f, 1.8f, 0.9f);
                    SpawnProp(propsRoot, "Castle_Rubble_Large.png", 7.2f, 5.0f, PropColliderType.Circle, 1.2f, 0f, 0.6f);

                    // Bottom-Left: Supply Crates & Barrels
                    SpawnProp(propsRoot, "Castle_Crate_Cluster.png", -11.0f, -5.2f, PropColliderType.Box, 1.3f, 0.95f, 0.5f);
                    SpawnProp(propsRoot, "Castle_Barrel_Standing.png", -9.2f, -5.8f, PropColliderType.Circle, 0.45f, 0f, 0.45f);

                    // Bottom-Right: Classical Pillar & Low Wall
                    SpawnProp(propsRoot, "Castle_Low_Wall.png", 11.0f, -5.5f, PropColliderType.Box, 1.3f, 1.2f, 0.6f);
                    SpawnProp(propsRoot, "Castle_Pillar_Classical.png", 9.0f, -5.0f, PropColliderType.Box, 0.9f, 1.4f, 0.7f);
                    break;

                case 1:
                    // --- ARCHETYPE 1: Grand Portcullis & Knight Monument ---
                    // Top: Gate Arch (walkable center, solid left & right posts)
                    SpawnProp(propsRoot, "Castle_Gate_Portcullis.png", 0f, 6.2f, PropColliderType.ArchPillars, 2.3f, 2.2f, 1.1f);
                    SpawnProp(propsRoot, "Castle_Torch_Tall.png", -2.4f, 5.2f, PropColliderType.Circle, 0.40f, 0f, 0.4f);
                    SpawnProp(propsRoot, "Castle_Torch_Tall.png", 2.4f, 5.2f, PropColliderType.Circle, 0.40f, 0f, 0.4f);

                    // Top-Left: Straight Wall
                    SpawnProp(propsRoot, "Castle_Wall_Straight.png", -10.5f, 5.5f, PropColliderType.Box, 2.9f, 1.8f, 0.9f);

                    // Bottom-Left: Knight Monument Statue
                    SpawnProp(propsRoot, "Castle_Statue_Knight.png", -9.5f, -5.2f, PropColliderType.Box, 1.5f, 2.4f, 1.2f);
                    SpawnProp(propsRoot, "Castle_Pillar_Engraved.png", -7.5f, -5.8f, PropColliderType.Box, 0.9f, 1.4f, 0.7f);

                    // Bottom-Right: Stone Steps & Barrel
                    SpawnProp(propsRoot, "Castle_Stone_Steps.png", 10.5f, -5.2f, PropColliderType.Box, 2.3f, 1.1f, 0.55f);
                    SpawnProp(propsRoot, "Castle_Barrel_Rolling.png", 8.0f, -5.8f, PropColliderType.Circle, 0.55f, 0f, 0.55f);
                    break;

                default:
                    // --- ARCHETYPE 2: Ancient Keep Battlements & Altar ---
                    // Top-Right: Corner Wall & Wood Debris
                    SpawnProp(propsRoot, "Castle_Wall_Corner.png", 11.0f, 5.5f, PropColliderType.Box, 2.6f, 1.9f, 0.95f);
                    SpawnProp(propsRoot, "Castle_Wood_Debris.png", 8.0f, 4.8f, PropColliderType.Box, 1.9f, 1.0f, 0.5f);

                    // Top-Left: Gothic Railing & Altar
                    SpawnProp(propsRoot, "Castle_Gothic_Railing.png", -10.0f, 5.8f, PropColliderType.Box, 1.3f, 0.9f, 0.45f);
                    SpawnProp(propsRoot, "Castle_Altar_Relief.png", -7.2f, 5.2f, PropColliderType.Box, 1.1f, 1.1f, 0.55f);

                    // Bottom-Right: Knight Statue & Short Torch
                    SpawnProp(propsRoot, "Castle_Statue_Knight.png", 10.0f, -5.2f, PropColliderType.Box, 1.5f, 2.4f, 1.2f);
                    SpawnProp(propsRoot, "Castle_Torch_Short.png", 8.0f, -4.8f, PropColliderType.Circle, 0.40f, 0f, 0.4f);

                    // Bottom-Left: Rubble & Single Crate
                    SpawnProp(propsRoot, "Castle_Rubble_Large.png", -10.5f, -5.2f, PropColliderType.Circle, 1.2f, 0f, 0.6f);
                    SpawnProp(propsRoot, "Castle_Crate_Single.png", -7.8f, -5.8f, PropColliderType.Box, 0.95f, 0.85f, 0.45f);
                    break;
            }
        }

        private enum PropColliderType { Circle, Box, ArchPillars }

        private static void SpawnProp(Transform parent, string spriteFilename, float localX, float localY, PropColliderType colType, float param1, float param2 = 0f, float offsetY = 0f)
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

            // 2. Dynamic 2.5D Y-Sorting
            YSortProp ysort = propGO.AddComponent<YSortProp>();
            ysort.UpdateSorting();

            // 3. True Solid Physical Collider (covers solid body, blocks movement)
            if (colType == PropColliderType.Circle)
            {
                CircleCollider2D circle = propGO.AddComponent<CircleCollider2D>();
                circle.radius = param1;
                circle.offset = new Vector2(0f, offsetY);
                circle.isTrigger = false;
            }
            else if (colType == PropColliderType.Box)
            {
                BoxCollider2D box = propGO.AddComponent<BoxCollider2D>();
                box.size = new Vector2(param1, param2);
                box.offset = new Vector2(0f, offsetY);
                box.isTrigger = false;
            }
            else if (colType == PropColliderType.ArchPillars)
            {
                // Gate arch has two solid pillars (left and right post), leaving the center arch open to walk through!
                BoxCollider2D leftCol = propGO.AddComponent<BoxCollider2D>();
                leftCol.size = new Vector2(0.85f, param2);
                leftCol.offset = new Vector2(-param1 * 0.50f, offsetY);
                leftCol.isTrigger = false;

                BoxCollider2D rightCol = propGO.AddComponent<BoxCollider2D>();
                rightCol.size = new Vector2(0.85f, param2);
                rightCol.offset = new Vector2(param1 * 0.50f, offsetY);
                rightCol.isTrigger = false;
            }
        }
    }
}
