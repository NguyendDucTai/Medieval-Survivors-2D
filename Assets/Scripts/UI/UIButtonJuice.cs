using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VampireSurvivors.Audio;

namespace VampireSurvivors.UI
{
    /// <summary>
    /// Professional UI Juice component providing hover scale-up, click compression,
    /// idle breathing, floating bobbing (Y-axis), horizontal nudge (X-axis arrows),
    /// color/alpha pulsing, and punch animations.
    /// Runs on Time.unscaledDeltaTime so all animations remain fluid even when gameplay is paused.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class UIButtonJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        public UnityEngine.Events.UnityAction onClickAction;
        [Header("Hover & Press Animation")]
        public float hoverScale = 1.08f;
        public float pressScale = 0.92f;
        public float animSpeed = 14f;

        [Header("Idle Breathing Pulse")]
        public bool enableBreathing = false;
        public float breathingSpeed = 2.4f;
        public float breathingAmount = 0.035f;

        [Header("Floating Bobbing (Y Axis)")]
        public bool enableFloating = false;
        public float floatingSpeed = 2.8f;
        public float floatingDistance = 7f;

        [Header("Horizontal Nudge (X Axis for Navigation Arrows)")]
        public bool enableHorizontalNudge = false;
        public float nudgeDistance = 6f;
        public float nudgeSpeed = 3.5f;
        public float nudgeDirection = 1f; // +1 = nudge right, -1 = nudge left

        [Header("Color / Alpha Pulse (Aura & Glow)")]
        public bool enableColorPulse = false;
        public Color pulseColorA = new Color(1f, 0.85f, 0.2f, 0.45f);
        public Color pulseColorB = new Color(1f, 0.98f, 0.6f, 1.0f);
        public float pulseSpeed = 3.2f;

        [Header("Audio Feedback")]
        public bool playSoundOnHover = false;
        public bool playSoundOnClick = true;

        private RectTransform rt;
        private Vector3 initialScale;
        private Vector2 initialPos;
        private float currentScaleMultiplier = 1f;
        private float targetScaleMultiplier = 1f;
        private bool isHovered = false;
        private bool isPressed = false;
        private float punchMultiplier = 1f;

        private Outline targetOutline;
        private Graphic targetGraphic;

        private void Awake()
        {
            rt = GetComponent<RectTransform>();
            initialScale = rt.localScale;
            initialPos = rt.anchoredPosition;

            targetOutline = GetComponent<Outline>();
            targetGraphic = GetComponent<Graphic>();
        }

        private void OnEnable()
        {
            if (rt == null) rt = GetComponent<RectTransform>();
            targetScaleMultiplier = 1f;
            currentScaleMultiplier = 1f;
            punchMultiplier = 1f;
            isHovered = false;
            isPressed = false;
        }

        private void Update()
        {
            // Determine base target scale from hover / pressed state
            if (isPressed)
            {
                targetScaleMultiplier = pressScale;
            }
            else if (isHovered)
            {
                targetScaleMultiplier = hoverScale;
            }
            else
            {
                targetScaleMultiplier = 1f;
            }

            // Interpolate smoothly towards target scale
            currentScaleMultiplier = Mathf.Lerp(currentScaleMultiplier, targetScaleMultiplier, Time.unscaledDeltaTime * animSpeed);

            // Punch recovery
            if (punchMultiplier > 1f)
            {
                punchMultiplier = Mathf.Lerp(punchMultiplier, 1f, Time.unscaledDeltaTime * 10f);
            }

            // Calculate Idle Breathing
            float breath = 1f;
            if (enableBreathing && !isPressed)
            {
                breath += Mathf.Sin(Time.unscaledTime * breathingSpeed) * breathingAmount;
            }

            // Apply final scale
            rt.localScale = initialScale * (currentScaleMultiplier * breath * punchMultiplier);

            // Apply Motion Offsets (Floating Bobbing & Horizontal Nudge)
            float xOffset = 0f;
            float yOffset = 0f;

            if (enableHorizontalNudge)
            {
                xOffset = Mathf.Sin(Time.unscaledTime * nudgeSpeed) * nudgeDistance * nudgeDirection;
            }

            if (enableFloating)
            {
                yOffset = Mathf.Sin(Time.unscaledTime * floatingSpeed) * floatingDistance;
            }

            if (enableHorizontalNudge || enableFloating)
            {
                rt.anchoredPosition = new Vector2(initialPos.x + xOffset, initialPos.y + yOffset);
            }

            // Apply Color / Alpha Pulse
            if (enableColorPulse)
            {
                float t = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f;
                Color col = Color.Lerp(pulseColorA, pulseColorB, t);

                if (targetOutline != null)
                {
                    targetOutline.effectColor = col;
                }
                else if (targetGraphic != null)
                {
                    targetGraphic.color = col;
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            if (playSoundOnHover && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfxByName("pickup");
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            isPressed = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isPressed = true;
            if (playSoundOnClick && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfxByName("click");
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPressed = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Only invoke fallback onClickAction if there is NO Button component on this GameObject
            // (prevents double-invoking actions like PrevHero/NextHero)
            if (onClickAction != null && GetComponent<Button>() == null)
            {
                onClickAction.Invoke();
            }
        }

        /// <summary>
        /// Punch scale effect when changing heroes, switching maps, clicking buttons, or adjusting sliders.
        /// </summary>
        public void Punch(float intensity = 1.14f)
        {
            punchMultiplier = intensity;
        }

        public void ResetPosition()
        {
            if (rt != null) rt.anchoredPosition = initialPos;
        }
    }
}
