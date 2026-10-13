using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace VampireSurvivors.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerController : MonoBehaviour
    {
        private Rigidbody2D rb;
        private PlayerStats stats;
        private SpriteRenderer spriteRenderer;
        private Animator animator;

        private Vector2 moveInput;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            stats = GetComponent<PlayerStats>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            // Setup Rigidbody2D for top-down 2D
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        private void Update()
        {
            float moveX = 0f;
            float moveY = 0f;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveY += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveY -= 1f;
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX += 1f;
            }
#endif

            if (moveX == 0f && moveY == 0f)
            {
                try
                {
                    moveX = Input.GetAxisRaw("Horizontal");
                    moveY = Input.GetAxisRaw("Vertical");
                }
                catch { }
            }

            moveInput = new Vector2(moveX, moveY).normalized;

            // Flip sprite based on movement direction
            if (spriteRenderer != null && moveX != 0)
            {
                spriteRenderer.flipX = moveX < 0;
            }

            // Update Animator parameters if present
            if (animator != null)
            {
                animator.SetBool("IsRunning", moveInput.sqrMagnitude > 0.01f);
            }
        }

        private void FixedUpdate()
        {
            rb.linearVelocity = moveInput * stats.moveSpeed;
        }
    }
}
