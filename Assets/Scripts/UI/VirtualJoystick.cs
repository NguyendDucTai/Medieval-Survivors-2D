using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VampireSurvivors.UI
{
    /// <summary>
    /// High-performance responsive Touch Virtual Joystick for mobile Android/iOS
    /// and desktop mouse testing. Supports both fixed and dynamic floating thumb positioning.
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public static VirtualJoystick Instance { get; private set; }

        [Header("Joystick Rects")]
        public RectTransform touchArea;
        public RectTransform backgroundRect;
        public RectTransform handleRect;

        [Header("Tuning Settings")]
        [Tooltip("Maximum movement radius of the thumb knob (in UI pixels)")]
        public float handleRange = 85f;

        [Tooltip("Dead zone threshold below which input is considered zero")]
        public float deadZone = 0.08f;

        [Tooltip("If true, the joystick base appears wherever the player first taps in the touch zone")]
        public bool dynamicFloating = true;

        [Tooltip("Default anchored position of joystick when idle or reset")]
        public Vector2 defaultPosition = new Vector2(230f, 230f);

        [Tooltip("Opacity multiplier when idle vs active")]
        public float idleAlpha = 0.65f;
        public float activeAlpha = 0.95f;

        [Header("Read-only Output")]
        public Vector2 inputVector = Vector2.zero;
        public Vector2 InputVector => inputVector;
        public bool IsDragging { get; private set; } = false;

        private Canvas parentCanvas;
        private Camera uiCamera;
        private CanvasGroup canvasGroup;
        private Vector2 joystickCenter = Vector2.zero;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                uiCamera = parentCanvas.worldCamera;
            }

            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            SetOpacity(idleAlpha);
            ResetHandle();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnDisable()
        {
            inputVector = Vector2.zero;
            IsDragging = false;
            ResetHandle();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Time.timeScale <= 0.0001f) return; // Ignore during pause or modals

            IsDragging = true;
            SetOpacity(activeAlpha);

            if (backgroundRect != null)
            {
                if (dynamicFloating && touchArea != null)
                {
                    // Reposition base under thumb
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        touchArea,
                        eventData.position,
                        uiCamera,
                        out Vector2 localPoint
                    );
                    backgroundRect.anchoredPosition = localPoint;
                }

                // Compute center in screen space
                joystickCenter = RectTransformUtility.WorldToScreenPoint(uiCamera, backgroundRect.position);
            }

            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!IsDragging || backgroundRect == null || handleRect == null) return;
            if (Time.timeScale <= 0.0001f)
            {
                OnPointerUp(eventData);
                return;
            }

            Vector2 currentCenter = RectTransformUtility.WorldToScreenPoint(uiCamera, backgroundRect.position);
            Vector2 direction = eventData.position - currentCenter;

            // Scale by canvas scaler factor if necessary
            float scaleFactor = parentCanvas != null ? parentCanvas.scaleFactor : 1f;
            float maxDistance = handleRange * scaleFactor;

            float distance = direction.magnitude;
            Vector2 normalizedDir = (distance > 0.001f) ? direction / distance : Vector2.zero;

            float clampedDistance = Mathf.Min(distance, maxDistance);
            handleRect.anchoredPosition = normalizedDir * (clampedDistance / scaleFactor);

            // Calculate normalized input vector
            float normalizedMagnitude = clampedDistance / maxDistance;
            if (normalizedMagnitude < deadZone)
            {
                inputVector = Vector2.zero;
            }
            else
            {
                // Smooth deadzone curve
                float remappedMag = Mathf.InverseLerp(deadZone, 1f, normalizedMagnitude);
                inputVector = normalizedDir * remappedMag;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            IsDragging = false;
            inputVector = Vector2.zero;
            SetOpacity(idleAlpha);
            ResetHandle();

            if (dynamicFloating && backgroundRect != null)
            {
                backgroundRect.anchoredPosition = defaultPosition;
            }
        }

        private void ResetHandle()
        {
            if (handleRect != null)
            {
                handleRect.anchoredPosition = Vector2.zero;
            }
            inputVector = Vector2.zero;
        }

        private void SetOpacity(float alpha)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
            }
        }

        // =========================================================
        // PROCEDURAL FACTORY (Integrates with HUD)
        // =========================================================
        public static VirtualJoystick CreateProceduralJoystick(Transform hudRoot, Canvas canvas)
        {
            if (Instance != null) return Instance;

            // 1. Touch Zone Root (Bottom-Left quarter of screen)
            GameObject touchZoneGO = new GameObject("Touch_Joystick_Zone", typeof(RectTransform));
            touchZoneGO.transform.SetParent(hudRoot, false);
            RectTransform zoneRT = touchZoneGO.GetComponent<RectTransform>();
            zoneRT.anchorMin = new Vector2(0f, 0f);
            zoneRT.anchorMax = new Vector2(0.5f, 0.6f); // Left half, lower 60%
            zoneRT.pivot = new Vector2(0f, 0f);
            zoneRT.anchoredPosition = Vector2.zero;
            zoneRT.sizeDelta = Vector2.zero;

            // Transparent image to capture raycast pointer events
            Image zoneImg = touchZoneGO.AddComponent<Image>();
            zoneImg.color = new Color(0f, 0f, 0f, 0.001f); // Nearly transparent raycast catcher
            zoneImg.raycastTarget = true;

            VirtualJoystick joystick = touchZoneGO.AddComponent<VirtualJoystick>();
            joystick.touchArea = zoneRT;
            joystick.parentCanvas = canvas;

            // Load generated pixel art sprites
            Sprite baseSprite = null;
            Sprite knobSprite = null;
#if UNITY_EDITOR
            baseSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Joystick_Base.png");
            knobSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Joystick_Knob.png");
#endif
            if (baseSprite == null)
            {
                baseSprite = LoadSpriteRuntime("Assets/Sprites/UI/Joystick_Base.png");
            }
            if (knobSprite == null)
            {
                knobSprite = LoadSpriteRuntime("Assets/Sprites/UI/Joystick_Knob.png");
            }

            // 2. Base Ring (Outer Circle)
            GameObject baseGO = new GameObject("Joystick_Base", typeof(RectTransform));
            baseGO.transform.SetParent(touchZoneGO.transform, false);
            RectTransform baseRT = baseGO.GetComponent<RectTransform>();
            baseRT.anchorMin = new Vector2(0f, 0f);
            baseRT.anchorMax = new Vector2(0f, 0f);
            baseRT.pivot = new Vector2(0.5f, 0.5f);
            baseRT.anchoredPosition = joystick.defaultPosition;
            baseRT.sizeDelta = new Vector2(200f, 200f);

            Image baseImg = baseGO.AddComponent<Image>();
            baseImg.sprite = baseSprite;
            baseImg.color = Color.white;
            baseImg.raycastTarget = false; // Events pass through to touch zone
            joystick.backgroundRect = baseRT;

            // 3. Knob Handle (Inner Knob)
            GameObject knobGO = new GameObject("Joystick_Knob", typeof(RectTransform));
            knobGO.transform.SetParent(baseGO.transform, false);
            RectTransform knobRT = knobGO.GetComponent<RectTransform>();
            knobRT.anchorMin = new Vector2(0.5f, 0.5f);
            knobRT.anchorMax = new Vector2(0.5f, 0.5f);
            knobRT.pivot = new Vector2(0.5f, 0.5f);
            knobRT.anchoredPosition = Vector2.zero;
            knobRT.sizeDelta = new Vector2(96f, 96f);

            Image knobImg = knobGO.AddComponent<Image>();
            knobImg.sprite = knobSprite;
            knobImg.color = Color.white;
            knobImg.raycastTarget = false;
            joystick.handleRect = knobRT;

            joystick.handleRange = 85f;
            joystick.deadZone = 0.08f;
            joystick.dynamicFloating = true;

            Debug.Log("<color=green>[VirtualJoystick]</color> Touch Virtual Joystick procedural setup complete!");
            return joystick;
        }

        private static Sprite LoadSpriteRuntime(string path)
        {
            if (System.IO.File.Exists(path))
            {
                try
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(path);
                    Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (tex.LoadImage(bytes))
                    {
                        tex.filterMode = FilterMode.Point;
                        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                    }
                }
                catch { }
            }
            return null;
        }
    }
}
