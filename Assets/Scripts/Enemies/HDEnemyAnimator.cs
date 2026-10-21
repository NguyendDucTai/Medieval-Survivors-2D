using System.Collections.Generic;
using UnityEngine;

namespace VampireSurvivors.Enemies
{
    /// <summary>
    /// Smooth 10-16 FPS Frame Animator for high-definition 2D monsters (Ghost, Bat, Skeleton, Goblin, Slime, Boss Golem).
    /// Dynamically slices the 2752x1536 sprite sheet into high-quality animation frames.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class HDEnemyAnimator : MonoBehaviour
    {
        [Header("Sprite Sheet Configuration")]
        public Texture2D spriteSheet;
        public int columns = 8;
        public int rows = 2;
        public int maxFrames = 16;
        public float fps = 12f;
        public float pixelsPerUnit = 250f;

        [Header("Ghostly / Visual Effects")]
        public bool isGhost = false;
        public float floatAmplitude = 0.15f;
        public float floatFrequency = 3f;

        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;
        private Sprite[] frames;
        private int currentFrameIndex = 0;
        private float frameTimer = 0f;
        private float floatTimer = 0f;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();

            InitializeFrames();
        }

        private void InitializeFrames()
        {
#if UNITY_EDITOR
            if (spriteSheet == null)
            {
                if (gameObject.name.Contains("Plant"))
                {
                    spriteSheet = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Enemies/Enemy_Plant.png");
                    columns = 4;
                    rows = 1;
                    maxFrames = 4;
                    pixelsPerUnit = 120f;
                    fps = 6f;
                }
            }

            if (spriteSheet != null)
            {
                string path = UnityEditor.AssetDatabase.GetAssetPath(spriteSheet);
                if (!string.IsNullOrEmpty(path))
                {
                    UnityEngine.Object[] allAssets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
                    List<Sprite> loadedSprites = new List<Sprite>();
                    foreach (var obj in allAssets)
                    {
                        if (obj is Sprite sp) loadedSprites.Add(sp);
                    }
                    if (loadedSprites.Count > 0)
                    {
                        frames = loadedSprites.ToArray();
                        if (spriteRenderer != null && frames.Length > 0)
                        {
                            spriteRenderer.sprite = frames[0];
                        }
                        return;
                    }
                }
            }
#endif

            if (spriteSheet == null) return;

            List<Sprite> frameList = new List<Sprite>();
            float frameWidth = (float)spriteSheet.width / columns;
            float frameHeight = (float)spriteSheet.height / rows;

            // Extract frames from top row to bottom row, left to right
            for (int r = rows - 1; r >= 0; r--)
            {
                for (int c = 0; c < columns; c++)
                {
                    if (frameList.Count >= maxFrames) break;

                    Rect rect = new Rect(c * frameWidth, r * frameHeight, frameWidth, frameHeight);
                    Sprite s = Sprite.Create(spriteSheet, rect, new Vector2(0.5f, 0.5f), pixelsPerUnit);
                    frameList.Add(s);
                }
            }

            frames = frameList.ToArray();

            if (frames.Length > 0 && spriteRenderer != null)
            {
                spriteRenderer.sprite = frames[0];
            }
        }

        private void Update()
        {
            if (frames == null || frames.Length == 0) return;

            // Advance animation frames
            frameTimer += Time.deltaTime;
            float frameInterval = 1f / Mathf.Max(1f, fps);

            if (frameTimer >= frameInterval)
            {
                frameTimer -= frameInterval;
                currentFrameIndex = (currentFrameIndex + 1) % frames.Length;
                spriteRenderer.sprite = frames[currentFrameIndex];
            }

            // Ghostly hover and transparency pulse
            if (isGhost)
            {
                floatTimer += Time.deltaTime;
                float alpha = 0.75f + (Mathf.Sin(floatTimer * 4f) * 0.18f);
                Color c = spriteRenderer.color;
                c.a = alpha;
                spriteRenderer.color = c;
            }

            // Flip facing direction based on movement
            if (rb != null && Mathf.Abs(rb.linearVelocity.x) > 0.05f)
            {
                spriteRenderer.flipX = rb.linearVelocity.x < 0;
            }
        }
    }
}
