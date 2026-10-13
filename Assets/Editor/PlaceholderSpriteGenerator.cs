#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VampireSurvivors.EditorTools
{
    public class PlaceholderSpriteGenerator
    {
        [MenuItem("Tools/Generate Medieval Fantasy Pixel Art Sprites")]
        public static void GenerateSprites()
        {
            // Characters
            CreatePixelArtSprite("Assets/Sprites/Characters/Hero_Knight.png", 16, 16, DrawKnightPattern);
            CreatePixelArtSprite("Assets/Sprites/Characters/Hero_Mage.png", 16, 16, DrawMagePattern);

            // Enemies
            CreatePixelArtSprite("Assets/Sprites/Enemies/Slime_Green.png", 16, 16, DrawSlimePattern);
            CreatePixelArtSprite("Assets/Sprites/Enemies/Skeleton_White.png", 16, 16, DrawSkeletonPattern);
            CreatePixelArtSprite("Assets/Sprites/Enemies/Goblin_Green.png", 16, 16, DrawGoblinPattern);

            // Weapons
            CreatePixelArtSprite("Assets/Sprites/Weapons/Weapon_HolySword.png", 16, 16, DrawSwordPattern);
            CreatePixelArtSprite("Assets/Sprites/Weapons/Weapon_Fireball.png", 16, 16, DrawFireballPattern);

            // Collectibles
            CreatePixelArtSprite("Assets/Sprites/Collectibles/ExpGem_Blue.png", 16, 16, DrawExpBluePattern);
            CreatePixelArtSprite("Assets/Sprites/Collectibles/ExpGem_Red.png", 16, 16, DrawExpRedPattern);
            CreatePixelArtSprite("Assets/Sprites/Collectibles/ExpGem_Gold.png", 16, 16, DrawExpGoldPattern);
            CreatePixelArtSprite("Assets/Sprites/Collectibles/HealthPotion.png", 16, 16, DrawPotionPattern);
            CreatePixelArtSprite("Assets/Sprites/Collectibles/Chest_Gold.png", 16, 16, DrawChestGoldPattern);
            CreatePixelArtSprite("Assets/Sprites/Collectibles/Magnet.png", 16, 16, DrawMagnetPattern);

            // Environment
            CreatePixelArtSprite("Assets/Sprites/Environment/Grass_Tile.png", 16, 16, DrawGrassPattern);
            CreatePixelArtSprite("Assets/Sprites/Environment/Tree_Obstacle.png", 16, 16, DrawTreePattern);

            // UI
            CreatePixelArtSprite("Assets/Sprites/UI/WoodFrame_Panel.png", 32, 32, DrawWoodFramePattern);
            CreatePixelArtSprite("Assets/Sprites/UI/HealthBar_Fill.png", 32, 8, DrawHealthBarPattern);

            AssetDatabase.Refresh();
            Debug.Log("Successfully generated Medieval Fantasy pixel art sprites in Assets/Sprites/!");
        }

        private delegate void PixelDrawer(Texture2D tex);

        private static void CreatePixelArtSprite(string path, int width, int height, PixelDrawer drawer)
        {
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color[] clearColors = new Color[width * height];
            for (int i = 0; i < clearColors.Length; i++) clearColors[i] = Color.clear;
            tex.SetPixels(clearColors);

            drawer(tex);

            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);

            AssetDatabase.ImportAsset(path);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 16;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        // --- CHARACTERS ---
        private static void DrawKnightPattern(Texture2D tex)
        {
            Color steel = new Color(0.7f, 0.75f, 0.8f, 1f);
            Color visor = new Color(0.2f, 0.2f, 0.3f, 1f);
            Color gold = new Color(0.95f, 0.75f, 0.2f, 1f);
            Color redCape = new Color(0.85f, 0.15f, 0.15f, 1f);

            // Helmet & Visor
            for (int x = 5; x <= 10; x++) for (int y = 10; y <= 14; y++) tex.SetPixel(x, y, steel);
            for (int x = 6; x <= 9; x++) tex.SetPixel(x, 12, visor);
            tex.SetPixel(7, 14, gold); // Crest

            // Armor & Cape
            for (int x = 4; x <= 11; x++) for (int y = 4; y <= 9; y++) tex.SetPixel(x, y, steel);
            for (int y = 2; y <= 12; y++) { tex.SetPixel(3, y, redCape); tex.SetPixel(12, y, redCape); }

            // Boots
            Color boots = new Color(0.4f, 0.4f, 0.45f, 1f);
            for (int x = 5; x <= 6; x++) for (int y = 0; y <= 3; y++) tex.SetPixel(x, y, boots);
            for (int x = 9; x <= 10; x++) for (int y = 0; y <= 3; y++) tex.SetPixel(x, y, boots);
        }

        private static void DrawMagePattern(Texture2D tex)
        {
            Color robe = new Color(0.3f, 0.2f, 0.7f, 1f);
            Color hat = new Color(0.2f, 0.1f, 0.5f, 1f);
            Color gold = new Color(0.95f, 0.8f, 0.2f, 1f);
            Color skin = new Color(0.95f, 0.8f, 0.6f, 1f);

            // Wizard Hat
            for (int x = 4; x <= 11; x++) tex.SetPixel(x, 11, hat);
            for (int x = 5; x <= 10; x++) tex.SetPixel(x, 12, hat);
            for (int x = 6; x <= 9; x++) tex.SetPixel(x, 13, hat);
            for (int x = 7; x <= 8; x++) tex.SetPixel(x, 14, hat);
            tex.SetPixel(7, 15, gold);

            // Face & Beard
            for (int x = 6; x <= 9; x++) tex.SetPixel(x, 9, skin);
            for (int x = 5; x <= 10; x++) tex.SetPixel(x, 8, Color.white); // Beard

            // Robe
            for (int x = 4; x <= 11; x++) for (int y = 0; y <= 7; y++) tex.SetPixel(x, y, robe);
        }

        // --- ENEMIES ---
        private static void DrawSlimePattern(Texture2D tex)
        {
            Color green = new Color(0.2f, 0.85f, 0.35f, 1f);
            Color highlight = new Color(0.6f, 0.95f, 0.5f, 1f);
            Color eye = Color.black;

            for (int x = 3; x <= 12; x++)
            {
                for (int y = 1; y <= 9; y++)
                {
                    if ((x == 3 || x == 12) && y > 6) continue;
                    tex.SetPixel(x, y, green);
                }
            }
            tex.SetPixel(5, 7, highlight);
            tex.SetPixel(6, 8, highlight);
            tex.SetPixel(5, 4, eye);
            tex.SetPixel(9, 4, eye);
        }

        private static void DrawSkeletonPattern(Texture2D tex)
        {
            Color bone = new Color(0.9f, 0.9f, 0.85f, 1f);
            Color dark = new Color(0.15f, 0.15f, 0.15f, 1f);

            // Skull
            for (int x = 5; x <= 10; x++) for (int y = 10; y <= 14; y++) tex.SetPixel(x, y, bone);
            tex.SetPixel(6, 12, dark); tex.SetPixel(9, 12, dark); // Eye sockets
            tex.SetPixel(7, 10, dark); tex.SetPixel(8, 10, dark); // Teeth

            // Ribcage & Spine
            for (int y = 4; y <= 9; y++) tex.SetPixel(7, y, bone);
            for (int x = 5; x <= 9; x++) { tex.SetPixel(x, 8, bone); tex.SetPixel(x, 6, bone); }

            // Legs
            for (int y = 0; y <= 3; y++) { tex.SetPixel(5, y, bone); tex.SetPixel(9, y, bone); }
        }

        private static void DrawGoblinPattern(Texture2D tex)
        {
            Color skin = new Color(0.35f, 0.65f, 0.2f, 1f);
            Color cloth = new Color(0.5f, 0.3f, 0.15f, 1f);
            Color eyes = new Color(1f, 0.2f, 0.1f, 1f);

            // Ears & Head
            for (int x = 5; x <= 10; x++) for (int y = 9; y <= 13; y++) tex.SetPixel(x, y, skin);
            tex.SetPixel(3, 11, skin); tex.SetPixel(4, 11, skin); // Left ear
            tex.SetPixel(11, 11, skin); tex.SetPixel(12, 11, skin); // Right ear
            tex.SetPixel(6, 11, eyes); tex.SetPixel(9, 11, eyes);

            // Body
            for (int x = 5; x <= 10; x++) for (int y = 0; y <= 8; y++) tex.SetPixel(x, y, cloth);
        }

        // --- WEAPONS ---
        private static void DrawSwordPattern(Texture2D tex)
        {
            Color blade = new Color(0.9f, 0.95f, 1f, 1f);
            Color guard = new Color(0.95f, 0.75f, 0.2f, 1f);
            Color hilt = new Color(0.4f, 0.25f, 0.1f, 1f);

            // Diagonal Blade
            for (int i = 4; i <= 14; i++)
            {
                tex.SetPixel(i, i, blade);
                tex.SetPixel(i - 1, i, blade);
            }
            // Guard
            tex.SetPixel(3, 5, guard); tex.SetPixel(5, 3, guard);
            // Hilt
            tex.SetPixel(2, 2, hilt); tex.SetPixel(1, 1, hilt);
        }

        private static void DrawFireballPattern(Texture2D tex)
        {
            Color core = new Color(1f, 0.95f, 0.4f, 1f);
            Color flame = new Color(1f, 0.35f, 0.1f, 1f);

            for (int x = 4; x <= 11; x++)
            {
                for (int y = 4; y <= 11; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(7.5f, 7.5f));
                    if (dist <= 2f) tex.SetPixel(x, y, core);
                    else if (dist <= 4f) tex.SetPixel(x, y, flame);
                }
            }
            tex.SetPixel(2, 7, flame); tex.SetPixel(3, 8, flame); // Flame tail
        }

        // --- COLLECTIBLES (SHARP PIXEL ART) ---
        private static Color HexColor(string hex) => ColorUtility.TryParseHtmlString(hex, out Color c) ? c : Color.white;

        private static void DrawPixelPattern(Texture2D tex, string[] rows, System.Collections.Generic.Dictionary<char, Color> pal)
        {
            int h = rows.Length;
            int w = rows[0].Length;
            for (int y = 0; y < h; y++)
            {
                int texY = h - 1 - y;
                string row = rows[y];
                for (int x = 0; x < w; x++)
                {
                    char c = row[x];
                    if (pal.TryGetValue(c, out Color col))
                    {
                        tex.SetPixel(x, texY, col);
                    }
                }
            }
        }

        private static readonly string[] GemPattern = new string[]
        {
            "................",
            ".......W........",
            "......W#W.......",
            "....########....",
            "...#WLL11114#...",
            "..#WLL1111144#..",
            ".#WWLL11111444#.",
            ".#WLL111111444#.",
            "..#LL11111144#..",
            "...#11111144#...",
            "....#111144#....",
            ".....#1144#.....",
            "......#14#......",
            ".......##.......",
            "................",
            "................"
        };

        private static void DrawExpBluePattern(Texture2D tex)
        {
            var pal = new System.Collections.Generic.Dictionary<char, Color>
            {
                { '#', HexColor("#06132b") },
                { 'W', HexColor("#ffffff") },
                { 'L', HexColor("#70d6ff") },
                { '1', HexColor("#0088ff") },
                { '4', HexColor("#003380") }
            };
            DrawPixelPattern(tex, GemPattern, pal);
        }

        private static void DrawExpRedPattern(Texture2D tex)
        {
            var pal = new System.Collections.Generic.Dictionary<char, Color>
            {
                { '#', HexColor("#28040a") },
                { 'W', HexColor("#ffffff") },
                { 'L', HexColor("#ff6b8b") },
                { '1', HexColor("#ff1744") },
                { '4', HexColor("#80001a") }
            };
            DrawPixelPattern(tex, GemPattern, pal);
        }

        private static void DrawExpGoldPattern(Texture2D tex)
        {
            var pal = new System.Collections.Generic.Dictionary<char, Color>
            {
                { '#', HexColor("#2b1700") },
                { 'W', HexColor("#ffffff") },
                { 'L', HexColor("#ffea60") },
                { '1', HexColor("#ffb300") },
                { '4', HexColor("#8c4e00") }
            };
            DrawPixelPattern(tex, GemPattern, pal);
        }

        private static void DrawPotionPattern(Texture2D tex)
        {
            string[] potionPattern = new string[]
            {
                "................",
                "......####......",
                "......#CK#......",
                ".....######.....",
                ".....#GGGG#.....",
                "....##....##....",
                "...#W......#....",
                "..#WWWRRRRRR#...",
                ".#WWWRRRRRRDD#..",
                ".#WWWRRBBBRRDD#.",
                ".#WWWRRRRRRRDD#.",
                "..#WWRRRRRRDD#..",
                "...#RRRRRRRDD#..",
                "....##DDDD##....",
                "......####......",
                "................"
            };
            var pal = new System.Collections.Generic.Dictionary<char, Color>
            {
                { '#', HexColor("#160507") },
                { 'C', HexColor("#c47a37") },
                { 'K', HexColor("#7a4014") },
                { 'G', HexColor("#b8d9f5") },
                { 'W', HexColor("#ffffff") },
                { 'R', HexColor("#ff1744") },
                { 'B', HexColor("#ff859d") },
                { 'D', HexColor("#8c001a") }
            };
            DrawPixelPattern(tex, potionPattern, pal);
        }

        private static void DrawChestGoldPattern(Texture2D tex)
        {
            string[] chestPattern = new string[]
            {
                "................",
                "....########....",
                "..##YYYYYYYY##..",
                ".#WYOYYYYYYOYB#.",
                "#YYOOOOOOOOOYYB#",
                "#YYOOOOOOOOOYYB#",
                "#YYYYYYYYYYYYYY#",
                "#KKKKKK#YY#KKKK#",
                "#YYOOOO#KK#OYYB#",
                "#YYOOOO#YY#OYYB#",
                "#YYOOOOOOOOOYYB#",
                "#YYDDDDDDDDDYYB#",
                "#YYYYYYYYYYYYYY#",
                ".#BBBBBBBBBBBB#.",
                "..############..",
                "................"
            };
            var pal = new System.Collections.Generic.Dictionary<char, Color>
            {
                { '#', HexColor("#1c0d02") },
                { 'W', HexColor("#ffffff") },
                { 'Y', HexColor("#ffc400") },
                { 'B', HexColor("#b37400") },
                { 'O', HexColor("#8c4411") },
                { 'D', HexColor("#522405") },
                { 'K', HexColor("#0a0401") }
            };
            DrawPixelPattern(tex, chestPattern, pal);
        }

        private static void DrawMagnetPattern(Texture2D tex)
        {
            string[] magnetPattern = new string[]
            {
                "................",
                ".#####....#####.",
                "#WWSS#....#WWSS#",
                "#WWSS#....#WWSS#",
                "#CCCC#....#CCCC#",
                "#RRRR#....#RRDD#",
                "#RRRR#....#RRDD#",
                "#RRRR#.ZZ.#RRDD#",
                "#RRRR#ZZZZ#RRDD#",
                "#RRRR#.ZZ.#RRDD#",
                ".#RRR#....#RDD#.",
                "..#RR######DD#..",
                "...#RRRRRRDD#...",
                "....#RRRRDD#....",
                ".....######.....",
                "................"
            };
            var pal = new System.Collections.Generic.Dictionary<char, Color>
            {
                { '#', HexColor("#14141e") },
                { 'W', HexColor("#ffffff") },
                { 'S', HexColor("#c8d2e0") },
                { 'C', HexColor("#2b2d38") },
                { 'R', HexColor("#e62232") },
                { 'D', HexColor("#8a0e1a") },
                { 'Z', HexColor("#00e5ff") }
            };
            DrawPixelPattern(tex, magnetPattern, pal);
        }

        // --- ENVIRONMENT ---
        private static void DrawGrassPattern(Texture2D tex)
        {
            Color grassBg = new Color(0.3f, 0.65f, 0.25f, 1f);
            Color darkGrass = new Color(0.2f, 0.5f, 0.18f, 1f);

            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    tex.SetPixel(x, y, ((x * 3 + y * 7) % 11 == 0) ? darkGrass : grassBg);
                }
            }
        }

        private static void DrawTreePattern(Texture2D tex)
        {
            Color trunk = new Color(0.45f, 0.28f, 0.15f, 1f);
            Color leaves = new Color(0.15f, 0.55f, 0.2f, 1f);

            for (int x = 7; x <= 8; x++) for (int y = 0; y <= 5; y++) tex.SetPixel(x, y, trunk);
            for (int x = 3; x <= 12; x++) for (int y = 6; y <= 14; y++) tex.SetPixel(x, y, leaves);
        }

        // --- UI ---
        private static void DrawWoodFramePattern(Texture2D tex)
        {
            Color wood = new Color(0.45f, 0.28f, 0.15f, 1f);
            Color inner = new Color(0.2f, 0.15f, 0.1f, 0.9f);
            Color border = new Color(0.25f, 0.15f, 0.08f, 1f);

            for (int x = 0; x < 32; x++)
            {
                for (int y = 0; y < 32; y++)
                {
                    if (x < 2 || x > 29 || y < 2 || y > 29) tex.SetPixel(x, y, border);
                    else if (x < 4 || x > 27 || y < 4 || y > 27) tex.SetPixel(x, y, wood);
                    else tex.SetPixel(x, y, inner);
                }
            }
        }

        private static void DrawHealthBarPattern(Texture2D tex)
        {
            Color redFill = new Color(0.85f, 0.15f, 0.15f, 1f);
            Color border = new Color(0.15f, 0.1f, 0.05f, 1f);

            for (int x = 0; x < 32; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (x == 0 || x == 31 || y == 0 || y == 7) tex.SetPixel(x, y, border);
                    else tex.SetPixel(x, y, redFill);
                }
            }
        }
    }
}
#endif
