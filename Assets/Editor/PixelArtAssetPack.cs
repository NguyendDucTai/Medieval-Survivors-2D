#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VampireSurvivors.EditorTools
{
    public class PixelArtAssetPack
    {
        [MenuItem("Tools/Generate 2-Month Retention Pixel Art Pack")]
        public static void GenerateAllSprites()
        {
            string passivesDir = "Assets/Sprites/Passives";
            if (!Directory.Exists(passivesDir))
            {
                Directory.CreateDirectory(passivesDir);
                AssetDatabase.Refresh();
            }

            // 1. Passives
            CreateIronArmorSprite(Path.Combine(passivesDir, "Passive_IronArmor.png"));
            CreateSpellTomeSprite(Path.Combine(passivesDir, "Passive_SpellTome.png"));
            CreateWindQuiverSprite(Path.Combine(passivesDir, "Passive_WindQuiver.png"));
            CreateMagicRingSprite(Path.Combine(passivesDir, "Passive_MagicRing.png"));
            CreateBloodChaliceSprite(Path.Combine(passivesDir, "Passive_BloodChalice.png"));
            CreateCloverSprite(Path.Combine(passivesDir, "Passive_Clover.png"));
            CreateGreedCoinSprite(Path.Combine(passivesDir, "Passive_GreedCoin.png"));
            CreateCrownSprite(Path.Combine(passivesDir, "Passive_Crown.png"));
            CreateMagnetTalismanSprite(Path.Combine(passivesDir, "Passive_MagnetTalisman.png"));
            CreateHolyCrossSprite(Path.Combine(passivesDir, "Passive_HolyCross.png"));

            // 2. Evolved Weapon VFX
            string weaponsDir = "Assets/Sprites/Weapons";
            CreateExcaliburSlash(Path.Combine(weaponsDir, "VFX_ExcaliburSlash.png"));
            CreateTempestArrow(Path.Combine(weaponsDir, "VFX_TempestArrow.png"));
            CreateMeteorSprite(Path.Combine(weaponsDir, "VFX_Meteor.png"));
            CreateSingularitySprite(Path.Combine(weaponsDir, "VFX_Singularity.png"));

            AssetDatabase.Refresh();
            ConfigureImportSettings(passivesDir);
            ConfigureImportSettings(weaponsDir);
            AssetDatabase.SaveAssets();

            Debug.Log("<color=green>SUCCESS: Generated 10 Passive Relic Sprites & 4 Evolved Weapon VFX Sprites!</color>");
        }

        private static void ConfigureImportSettings(string folder)
        {
            string[] files = Directory.GetFiles(folder, "*.png");
            foreach (string file in files)
            {
                TextureImporter importer = AssetImporter.GetAtPath(file) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = 16;
                    importer.filterMode = FilterMode.Point;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.mipmapEnabled = false;
                    importer.SaveAndReimport();
                }
            }
        }

        private static void SaveTexture(Texture2D tex, string path)
        {
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            Object.DestroyImmediate(tex);
        }

        // --- 1. Iron Armor (16x16) ---
        private static void CreateIronArmorSprite(string path)
        {
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color dark = new Color(0.15f, 0.18f, 0.22f);
            Color steel = new Color(0.55f, 0.60f, 0.68f);
            Color highlight = new Color(0.85f, 0.90f, 0.95f);
            Color gold = new Color(0.95f, 0.75f, 0.2f);

            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                    tex.SetPixel(x, y, clear);

            for (int y = 3; y <= 13; y++)
            {
                int minX = (y >= 10) ? 2 : 4;
                int maxX = (y >= 10) ? 13 : 11;
                for (int x = minX; x <= maxX; x++)
                {
                    bool isBorder = (x == minX || x == maxX || y == 3 || y == 13);
                    if (isBorder) tex.SetPixel(x, y, dark);
                    else if (y == 12 || x == minX + 1) tex.SetPixel(x, y, highlight);
                    else if (x == 7 || x == 8) tex.SetPixel(x, y, gold);
                    else tex.SetPixel(x, y, steel);
                }
            }
            tex.Apply();
            SaveTexture(tex, path);
        }

        // --- 2. Spell Tome (16x16) ---
        private static void CreateSpellTomeSprite(string path)
        {
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color leather = new Color(0.35f, 0.12f, 0.45f);
            Color gold = new Color(0.95f, 0.8f, 0.25f);
            Color pages = new Color(0.9f, 0.85f, 0.75f);
            Color rune = new Color(0.3f, 0.9f, 1f);

            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                    tex.SetPixel(x, y, clear);

            for (int y = 2; y <= 13; y++)
            {
                for (int x = 3; x <= 12; x++)
                {
                    if (x == 3 || y == 2 || y == 13) tex.SetPixel(x, y, gold);
                    else if (x == 12) tex.SetPixel(x, y, pages);
                    else tex.SetPixel(x, y, leather);
                }
            }
            // Rune in center
            tex.SetPixel(7, 7, rune); tex.SetPixel(8, 7, rune);
            tex.SetPixel(7, 8, rune); tex.SetPixel(8, 8, rune);
            tex.Apply();
            SaveTexture(tex, path);
        }

        // --- 3. Wind Quiver (16x16) ---
        private static void CreateWindQuiverSprite(string path)
        {
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color brown = new Color(0.45f, 0.25f, 0.12f);
            Color dark = new Color(0.25f, 0.12f, 0.05f);
            Color feather = new Color(0.2f, 0.85f, 0.45f);
            Color shaft = new Color(0.85f, 0.75f, 0.5f);

            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                    tex.SetPixel(x, y, clear);

            for (int y = 2; y <= 11; y++)
            {
                for (int x = 5; x <= 10; x++)
                {
                    if (x == 5 || x == 10 || y == 2) tex.SetPixel(x, y, dark);
                    else tex.SetPixel(x, y, brown);
                }
            }
            // Arrow fletchings sticking out top
            tex.SetPixel(6, 12, shaft); tex.SetPixel(6, 13, feather); tex.SetPixel(5, 14, feather);
            tex.SetPixel(8, 12, shaft); tex.SetPixel(8, 13, feather); tex.SetPixel(8, 14, feather);
            tex.SetPixel(9, 12, shaft); tex.SetPixel(9, 13, feather); tex.SetPixel(10, 14, feather);
            tex.Apply();
            SaveTexture(tex, path);
        }

        // --- 4. Magic Ring (16x16) ---
        private static void CreateMagicRingSprite(string path)
        {
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color gold = new Color(1f, 0.8f, 0.2f);
            Color darkGold = new Color(0.6f, 0.45f, 0.1f);
            Color gem = new Color(0.2f, 0.6f, 1f);
            Color gemGlow = new Color(0.7f, 0.9f, 1f);

            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                    tex.SetPixel(x, y, clear);

            // Ring band
            int[] ringX = { 6, 7, 8, 9, 5, 10, 5, 10, 6, 7, 8, 9 };
            int[] ringY = { 3, 3, 3, 3, 4, 4, 7, 7, 8, 8, 8, 8 };
            for (int i = 0; i < ringX.Length; i++)
            {
                tex.SetPixel(ringX[i], ringY[i], (ringY[i] <= 4) ? darkGold : gold);
            }

            // Sapphire Gem on top
            tex.SetPixel(7, 9, gem); tex.SetPixel(8, 9, gem);
            tex.SetPixel(7, 10, gemGlow); tex.SetPixel(8, 10, gem);
            tex.SetPixel(7, 11, gemGlow);
            tex.Apply();
            SaveTexture(tex, path);
        }

        // --- 5. Blood Chalice (16x16) ---
        private static void CreateBloodChaliceSprite(string path)
        {
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color silver = new Color(0.7f, 0.72f, 0.78f);
            Color blood = new Color(0.85f, 0.05f, 0.15f);
            Color dark = new Color(0.2f, 0.2f, 0.25f);

            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                    tex.SetPixel(x, y, clear);

            // Base and stem
            for (int x = 5; x <= 10; x++) tex.SetPixel(x, 2, silver);
            for (int y = 3; y <= 6; y++) { tex.SetPixel(7, y, silver); tex.SetPixel(8, y, silver); }

            // Cup
            for (int y = 7; y <= 12; y++)
            {
                int minX = (y <= 8) ? 6 : 4;
                int maxX = (y <= 8) ? 9 : 11;
                for (int x = minX; x <= maxX; x++)
                {
                    if (x == minX || x == maxX || y == 7) tex.SetPixel(x, y, dark);
                    else tex.SetPixel(x, y, blood);
                }
            }
            tex.Apply();
            SaveTexture(tex, path);
        }

        // --- 6. Clover of Luck (16x16) ---
        private static void CreateCloverSprite(string path)
        {
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color green = new Color(0.15f, 0.75f, 0.25f);
            Color light = new Color(0.4f, 0.95f, 0.45f);
            Color stem = new Color(0.1f, 0.5f, 0.15f);

            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                    tex.SetPixel(x, y, clear);

            // 4 leaves
            DrawLeaf(tex, 5, 10, green, light);
            DrawLeaf(tex, 10, 10, green, light);
            DrawLeaf(tex, 5, 5, green, light);
            DrawLeaf(tex, 10, 5, green, light);

            // Stem
            tex.SetPixel(7, 4, stem); tex.SetPixel(7, 3, stem); tex.SetPixel(6, 2, stem);
            tex.Apply();
            SaveTexture(tex, path);
        }

        private static void DrawLeaf(Texture2D tex, int cx, int cy, Color c, Color h)
        {
            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                    tex.SetPixel(cx + dx, cy + dy, (dx == -1 && dy == 1) ? h : c);
        }

        // --- 7. Greed Coin (16x16) ---
        private static void CreateGreedCoinSprite(string path)
        {
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color gold = new Color(1f, 0.85f, 0.2f);
            Color shadow = new Color(0.7f, 0.5f, 0.1f);
            Color highlight = new Color(1f, 0.95f, 0.6f);

            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                    tex.SetPixel(x, y, clear);

            for (int y = 3; y <= 12; y++)
            {
                for (int x = 3; x <= 12; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(7.5f, 7.5f));
                    if (d <= 4.8f)
                    {
                        if (d >= 4.0f) tex.SetPixel(x, y, shadow);
                        else if (y >= 10 && x <= 8) tex.SetPixel(x, y, highlight);
                        else tex.SetPixel(x, y, gold);
                    }
                }
            }
            tex.Apply();
            SaveTexture(tex, path);
        }

        // --- 8. Royal Crown (16x16) ---
        private static void CreateCrownSprite(string path)
        {
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color gold = new Color(1f, 0.82f, 0.15f);
            Color darkGold = new Color(0.6f, 0.45f, 0.1f);
            Color ruby = new Color(0.9f, 0.1f, 0.2f);
            Color emerald = new Color(0.1f, 0.85f, 0.3f);

            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                    tex.SetPixel(x, y, clear);

            // Band
            for (int x = 3; x <= 12; x++)
            {
                tex.SetPixel(x, 4, darkGold);
                tex.SetPixel(x, 5, gold);
            }

            // Spikes
            tex.SetPixel(3, 6, gold); tex.SetPixel(3, 7, gold); tex.SetPixel(3, 8, ruby);
            tex.SetPixel(7, 6, gold); tex.SetPixel(7, 7, gold); tex.SetPixel(7, 8, gold); tex.SetPixel(7, 9, emerald);
            tex.SetPixel(8, 6, gold); tex.SetPixel(8, 7, gold); tex.SetPixel(8, 8, gold); tex.SetPixel(8, 9, emerald);
            tex.SetPixel(12, 6, gold); tex.SetPixel(12, 7, gold); tex.SetPixel(12, 8, ruby);
            tex.Apply();
            SaveTexture(tex, path);
        }

        // --- 9. Magnet Talisman (16x16) ---
        private static void CreateMagnetTalismanSprite(string path)
        {
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color red = new Color(0.9f, 0.2f, 0.2f);
            Color blue = new Color(0.2f, 0.4f, 0.95f);
            Color silver = new Color(0.8f, 0.8f, 0.85f);

            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                    tex.SetPixel(x, y, clear);

            // Horseshoe shape
            for (int y = 3; y <= 12; y++)
            {
                tex.SetPixel(4, y, y <= 7 ? red : silver);
                tex.SetPixel(5, y, y <= 7 ? red : silver);
                tex.SetPixel(10, y, y <= 7 ? blue : silver);
                tex.SetPixel(11, y, y <= 7 ? blue : silver);
            }
            for (int x = 4; x <= 11; x++)
            {
                tex.SetPixel(x, 12, silver);
                tex.SetPixel(x, 13, silver);
            }
            tex.Apply();
            SaveTexture(tex, path);
        }

        // --- 10. Holy Cross (16x16) ---
        private static void CreateHolyCrossSprite(string path)
        {
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color gold = new Color(1f, 0.9f, 0.3f);
            Color white = new Color(1f, 1f, 0.9f);
            Color dark = new Color(0.7f, 0.55f, 0.1f);

            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                    tex.SetPixel(x, y, clear);

            for (int y = 2; y <= 14; y++)
            {
                tex.SetPixel(7, y, white);
                tex.SetPixel(8, y, gold);
            }
            for (int x = 4; x <= 11; x++)
            {
                tex.SetPixel(x, 9, white);
                tex.SetPixel(x, 10, gold);
            }
            tex.Apply();
            SaveTexture(tex, path);
        }

        // --- 11. Excalibur Slash (32x32) ---
        private static void CreateExcaliburSlash(string path)
        {
            Texture2D tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color gold = new Color(1f, 0.85f, 0.2f);
            Color bright = new Color(1f, 1f, 0.8f);

            for (int y = 0; y < 32; y++)
                for (int x = 0; x < 32; x++)
                    tex.SetPixel(x, y, clear);

            // Arc curve
            for (int a = 0; a <= 180; a += 2)
            {
                float rad = a * Mathf.Deg2Rad;
                int px = Mathf.RoundToInt(16 + 13 * Mathf.Cos(rad));
                int py = Mathf.RoundToInt(16 + 13 * Mathf.Sin(rad));
                if (px >= 0 && px < 32 && py >= 0 && py < 32)
                {
                    tex.SetPixel(px, py, bright);
                    if (px + 1 < 32) tex.SetPixel(px + 1, py, gold);
                    if (py - 1 >= 0) tex.SetPixel(px, py - 1, gold);
                }
            }
            tex.Apply();
            SaveTexture(tex, path);
        }

        // --- 12. Tempest Arrow (16x16) ---
        private static void CreateTempestArrow(string path)
        {
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color cyan = new Color(0.2f, 0.9f, 1f);
            Color bright = new Color(0.8f, 1f, 1f);
            Color dark = new Color(0.05f, 0.4f, 0.7f);

            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                    tex.SetPixel(x, y, clear);

            // Pointing East (+X)
            for (int x = 2; x <= 12; x++) tex.SetPixel(x, 7, cyan);
            for (int x = 3; x <= 13; x++) tex.SetPixel(x, 8, bright);

            // Arrow head
            tex.SetPixel(14, 8, bright);
            tex.SetPixel(13, 9, cyan); tex.SetPixel(13, 7, cyan);
            tex.SetPixel(12, 10, dark); tex.SetPixel(12, 6, dark);

            // Fletching
            tex.SetPixel(2, 9, cyan); tex.SetPixel(1, 10, dark);
            tex.SetPixel(2, 6, cyan); tex.SetPixel(1, 5, dark);
            tex.Apply();
            SaveTexture(tex, path);
        }

        // --- 13. Meteor (32x32) ---
        private static void CreateMeteorSprite(string path)
        {
            Texture2D tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color orange = new Color(1f, 0.45f, 0.05f);
            Color yellow = new Color(1f, 0.9f, 0.2f);
            Color dark = new Color(0.4f, 0.1f, 0.05f);

            for (int y = 0; y < 32; y++)
                for (int x = 0; x < 32; x++)
                    tex.SetPixel(x, y, clear);

            for (int y = 8; y <= 24; y++)
            {
                for (int x = 8; x <= 24; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(16, 16));
                    if (d <= 7f)
                    {
                        if (d <= 3f) tex.SetPixel(x, y, yellow);
                        else if (d <= 5.5f) tex.SetPixel(x, y, orange);
                        else tex.SetPixel(x, y, dark);
                    }
                }
            }
            tex.Apply();
            SaveTexture(tex, path);
        }

        // --- 14. Singularity / Black Hole (32x32) ---
        private static void CreateSingularitySprite(string path)
        {
            Texture2D tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color purple = new Color(0.6f, 0.1f, 0.9f);
            Color cyan = new Color(0.2f, 0.9f, 1f);
            Color black = new Color(0.05f, 0.02f, 0.1f);

            for (int y = 0; y < 32; y++)
                for (int x = 0; x < 32; x++)
                    tex.SetPixel(x, y, clear);

            for (int y = 6; y <= 26; y++)
            {
                for (int x = 6; x <= 26; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(16, 16));
                    if (d <= 8.5f)
                    {
                        if (d <= 4f) tex.SetPixel(x, y, black);
                        else if (d <= 6.5f) tex.SetPixel(x, y, purple);
                        else tex.SetPixel(x, y, cyan);
                    }
                }
            }
            tex.Apply();
            SaveTexture(tex, path);
        }
    }
}
#endif
