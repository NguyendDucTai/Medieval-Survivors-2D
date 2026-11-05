using System.IO;
using UnityEngine;

namespace VampireSurvivors.Environment.Hazards
{
    /// <summary>
    /// Loads 16x16 / 32x32 retro pixel-art hazard sprites with Point (no filter) & PPU 16,
    /// falling back to procedural pixel generation if any file is absent.
    /// </summary>
    public static class HazardSpriteLoader
    {
        private static Sprite cachedBush;
        private static Sprite cachedHealthPotion;
        private static Sprite cachedMagnet;
        private static Sprite cachedSummonRune;
        private static Sprite cachedLavaWarn;
        private static Sprite cachedLavaFire;
        private static Sprite cachedPixelSquare;
        private static Sprite cachedMeteor;
        private static Sprite cachedMeteorGiant;
        private static Sprite cachedMeteorWarn;
        private static Sprite cachedMeteorScorch;
        private static Sprite cachedAura;

        public static Sprite GetPlayerAuraSprite()
        {
            if (cachedAura != null) return cachedAura;
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            float center = (size - 1) / 2f;
            Color[] colors = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - center) / center;
                    float dy = (y - center) / center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01(1f - dist);
                    alpha = alpha * alpha;
                    colors[y * size + x] = new Color(1f, 0.5f, 0.18f, alpha * 0.45f);
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            cachedAura = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
            return cachedAura;
        }

        public static Sprite GetBushSprite()
        {
            if (cachedBush != null) return cachedBush;
            cachedBush = LoadOrCreate("Assets/Sprites/Environment/Hazards/Hazard_Bush.png", 16f, CreateProceduralBush);
            return cachedBush;
        }

        public static Sprite GetHealthPotionSprite()
        {
            if (cachedHealthPotion != null) return cachedHealthPotion;
            cachedHealthPotion = LoadOrCreate("Assets/Sprites/Collectibles/HealthPotion.png", 16f, null)
                ?? LoadOrCreate("Assets/Sprites/Environment/Hazards/Hazard_HealthPotion.png", 16f, CreateProceduralPotion);
            return cachedHealthPotion;
        }

        public static Sprite GetMagnetSprite()
        {
            if (cachedMagnet != null) return cachedMagnet;
            cachedMagnet = LoadOrCreate("Assets/Sprites/Collectibles/Magnet.png", 16f, null)
                ?? LoadOrCreate("Assets/Sprites/Environment/Hazards/Hazard_Magnet.png", 16f, CreateProceduralMagnet);
            return cachedMagnet;
        }

        public static Sprite GetSummonRuneSprite()
        {
            if (cachedSummonRune != null) return cachedSummonRune;
            cachedSummonRune = LoadOrCreate("Assets/Sprites/Environment/Hazards/Hazard_SummonRune.png", 16f, CreateProceduralRune);
            return cachedSummonRune;
        }

        public static Sprite GetLavaWarnSprite()
        {
            if (cachedLavaWarn != null) return cachedLavaWarn;
            cachedLavaWarn = LoadOrCreate("Assets/Sprites/Environment/Hazards/Hazard_LavaCrack_Warn.png", 16f, () => CreateProceduralLava(false));
            return cachedLavaWarn;
        }

        public static Sprite GetLavaFireSprite()
        {
            if (cachedLavaFire != null) return cachedLavaFire;
            cachedLavaFire = LoadOrCreate("Assets/Sprites/Environment/Hazards/Hazard_LavaCrack_Fire.png", 16f, () => CreateProceduralLava(true));
            return cachedLavaFire;
        }

        public static Sprite GetMeteorSprite()
        {
            if (cachedMeteor != null) return cachedMeteor;
            cachedMeteor = LoadOrCreate("Assets/Sprites/Environment/Hazards/Hazard_Meteor.png", 16f, null);
            return cachedMeteor;
        }

        public static Sprite GetMeteorGiantSprite()
        {
            if (cachedMeteorGiant != null) return cachedMeteorGiant;
            cachedMeteorGiant = LoadOrCreate("Assets/Sprites/Environment/Hazards/Hazard_MeteorGiant.png", 16f, null);
            return cachedMeteorGiant;
        }

        public static Sprite GetMeteorWarnSprite()
        {
            if (cachedMeteorWarn != null) return cachedMeteorWarn;
            cachedMeteorWarn = LoadOrCreate("Assets/Sprites/Environment/Hazards/Hazard_MeteorWarn.png", 16f, null);
            return cachedMeteorWarn;
        }

        public static Sprite GetMeteorScorchSprite()
        {
            if (cachedMeteorScorch != null) return cachedMeteorScorch;
            cachedMeteorScorch = LoadOrCreate("Assets/Sprites/Environment/Hazards/Hazard_MeteorScorch.png", 16f, null);
            return cachedMeteorScorch;
        }

        public static Sprite GetPixelSquareSprite()
        {
            if (cachedPixelSquare != null) return cachedPixelSquare;
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            Color[] cols = new Color[4] { Color.white, Color.white, Color.white, Color.white };
            tex.SetPixels(cols);
            tex.Apply();
            cachedPixelSquare = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 16f);
            return cachedPixelSquare;
        }

        private static Sprite LoadOrCreate(string path, float ppu, System.Func<Texture2D> fallbackFactory)
        {
#if UNITY_EDITOR
            Sprite editorSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (editorSprite != null) return editorSprite;

            Texture2D editorTex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (editorTex != null)
            {
                editorTex.filterMode = FilterMode.Point;
                return Sprite.Create(editorTex, new Rect(0, 0, editorTex.width, editorTex.height), new Vector2(0.5f, 0.5f), ppu);
            }
#endif
            if (File.Exists(path))
            {
                try
                {
                    byte[] bytes = File.ReadAllBytes(path);
                    Texture2D fileTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (fileTex.LoadImage(bytes))
                    {
                        fileTex.filterMode = FilterMode.Point;
                        fileTex.wrapMode = TextureWrapMode.Clamp;
                        return Sprite.Create(fileTex, new Rect(0, 0, fileTex.width, fileTex.height), new Vector2(0.5f, 0.5f), ppu);
                    }
                }
                catch { }
            }

            if (fallbackFactory != null)
            {
                Texture2D fallbackTex = fallbackFactory();
                if (fallbackTex != null)
                {
                    fallbackTex.filterMode = FilterMode.Point;
                    fallbackTex.wrapMode = TextureWrapMode.Clamp;
                    return Sprite.Create(fallbackTex, new Rect(0, 0, fallbackTex.width, fallbackTex.height), new Vector2(0.5f, 0.5f), ppu);
                }
            }

            return null;
        }

        private static Texture2D CreateProceduralBush()
        {
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color outline = new Color(0.08f, 0.2f, 0.1f, 1f);
            Color mid = new Color(0.22f, 0.56f, 0.24f, 1f);
            Color light = new Color(0.4f, 0.78f, 0.34f, 1f);
            Color berry = new Color(0.92f, 0.25f, 0.32f, 1f);

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(7.5f, 7.5f));
                    if (d > 6.8f) tex.SetPixel(x, y, clear);
                    else if (d > 5.6f) tex.SetPixel(x, y, outline);
                    else if (y > 8) tex.SetPixel(x, y, light);
                    else tex.SetPixel(x, y, mid);
                }
            }
            tex.SetPixel(5, 9, berry);
            tex.SetPixel(10, 10, berry);
            tex.SetPixel(8, 6, berry);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateProceduralPotion()
        {
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color ruby = new Color(0.9f, 0.18f, 0.28f, 1f);
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                    tex.SetPixel(x, y, (x >= 4 && x <= 11 && y >= 3 && y <= 12) ? ruby : clear);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateProceduralMagnet()
        {
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color red = new Color(0.9f, 0.2f, 0.25f, 1f);
            Color blue = new Color(0.22f, 0.52f, 0.96f, 1f);
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    if (x >= 3 && x <= 12 && y >= 3 && y <= 13)
                        tex.SetPixel(x, y, x < 8 ? red : blue);
                    else
                        tex.SetPixel(x, y, clear);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateProceduralRune()
        {
            Texture2D tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color ring = new Color(0.68f, 0.28f, 1f, 0.95f);
            Color inner = new Color(0.42f, 0.88f, 1f, 0.9f);
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float r = Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f));
                    if (r >= 13f && r <= 15f) tex.SetPixel(x, y, ring);
                    else if (r >= 8.8f && r <= 10.2f) tex.SetPixel(x, y, inner);
                    else tex.SetPixel(x, y, clear);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateProceduralLava(bool activeFire)
        {
            Texture2D tex = new Texture2D(16, 32, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color core = activeFire ? new Color(1f, 0.92f, 0.35f, 1f) : new Color(0.92f, 0.38f, 0.1f, 0.9f);
            Color edge = activeFire ? new Color(1f, 0.32f, 0.08f, 0.95f) : new Color(0.28f, 0.1f, 0.08f, 0.85f);
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    int d = Mathf.Abs(x - 8);
                    if (d <= 1) tex.SetPixel(x, y, core);
                    else if (d <= (activeFire ? 4 : 3)) tex.SetPixel(x, y, edge);
                    else tex.SetPixel(x, y, clear);
                }
            }
            tex.Apply();
            return tex;
        }
    }
}
