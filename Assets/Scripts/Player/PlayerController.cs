using System.Linq;
using UnityEngine;
using VampireSurvivors.Weapons;
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
        public Vector2 lastFacingDirection = Vector2.right;
        public Vector2 CurrentAimDirection
        {
            get
            {
                if (moveInput.sqrMagnitude > 0.01f) return moveInput;
                return lastFacingDirection.sqrMagnitude > 0.01f ? lastFacingDirection : Vector2.right;
            }
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            stats = GetComponent<PlayerStats>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            // Setup Rigidbody2D for top-down 2D
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            // Ensure WeaponInventory is attached
            if (GetComponent<WeaponInventory>() == null)
            {
                gameObject.AddComponent<WeaponInventory>();
            }
        }

        private void Start()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (animator == null) animator = GetComponent<Animator>();

            WeaponBase weapon = GetComponent<WeaponBase>();
            string sName = (spriteRenderer != null && spriteRenderer.sprite != null) ? spriteRenderer.sprite.name : "";
            string cName = (animator != null && animator.runtimeAnimatorController != null) ? animator.runtimeAnimatorController.name : "";
            string wName = (weapon != null) ? weapon.weaponName : "";

            // Auto-detect and initialize hero setup
            if (sName.Contains("Rogue") || cName.Contains("ShadowKnight") || wName.Contains("Dark") || wName.Contains("Crescent") || wName.Contains("Shadow"))
            {
                SwitchHero("ShadowKnight");
            }
            else if (sName.Contains("Archer") || cName.Contains("Archer") || wName.Contains("Storm"))
            {
                SwitchHero("Archer");
            }
            else if (sName.Contains("Ranger") || cName.Contains("Ranger") || sName.Contains("cung_thu") || cName.Contains("cung_thu") || wName.Contains("Arrow") || (wName.Contains("Bow") && !wName.Contains("Storm")))
            {
                SwitchHero("Ranger");
            }
            else if (sName.Contains("Archmage") || cName.Contains("Archmage") || wName.Contains("Arcane"))
            {
                SwitchHero("Archmage");
            }
            else if (sName.Contains("Knight") || cName.Contains("Knight") || wName.Contains("Holy"))
            {
                SwitchHero("Knight");
            }
            else if (sName.Contains("Mage") || cName.Contains("Mage") || wName.Contains("Fireball"))
            {
                SwitchHero("Mage");
            }
            else
            {
                SwitchHero(sName.Contains("Rogue") ? "ShadowKnight" : (sName.Contains("Ranger") ? "Ranger" : "Archer"));
            }
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

                // Hotkeys to switch heroes during testing: 1=Knight, 2=ShadowKnight, 3=Mage, 4=Archmage, 5=Ranger, 6=Archer
                if (Keyboard.current.digit1Key.wasPressedThisFrame) SwitchHero("Knight");
                if (Keyboard.current.digit2Key.wasPressedThisFrame) SwitchHero("ShadowKnight");
                if (Keyboard.current.digit3Key.wasPressedThisFrame) SwitchHero("Mage");
                if (Keyboard.current.digit4Key.wasPressedThisFrame) SwitchHero("Archmage");
                if (Keyboard.current.digit5Key.wasPressedThisFrame) SwitchHero("Ranger");
                if (Keyboard.current.digit6Key.wasPressedThisFrame) SwitchHero("Archer");
            }
#endif

            if (moveX == 0f && moveY == 0f)
            {
                try
                {
                    moveX = Input.GetAxisRaw("Horizontal");
                    moveY = Input.GetAxisRaw("Vertical");

                    if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchHero("Knight");
                    if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchHero("ShadowKnight");
                    if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchHero("Mage");
                    if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchHero("Archmage");
                    if (Input.GetKeyDown(KeyCode.Alpha5)) SwitchHero("Ranger");
                    if (Input.GetKeyDown(KeyCode.Alpha6)) SwitchHero("Archer");
                }
                catch { }
            }

            moveInput = new Vector2(moveX, moveY).normalized;
            if (moveInput.sqrMagnitude > 0.01f)
            {
                lastFacingDirection = moveInput;
            }

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

        public void SwitchHero(string hero)
        {
            WeaponBase weapon = GetComponent<WeaponBase>();
            if (weapon == null) weapon = gameObject.AddComponent<WeaponBase>();

            if (animator == null) animator = GetComponent<Animator>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            if (hero == "Knight")
            {
#if UNITY_EDITOR
                RuntimeAnimatorController ctrl = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Characters/Knight_Controller.controller");
                if (ctrl != null && animator != null) animator.runtimeAnimatorController = ctrl;

                Sprite s = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Characters/Hero_Knight_Idle.png")
                    .OfType<Sprite>().FirstOrDefault();
                if (s != null && spriteRenderer != null) spriteRenderer.sprite = s;

                GameObject slashPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX_SwordSlash.prefab");
                if (slashPrefab != null) weapon.projectilePrefab = slashPrefab;
#endif
                weapon.weaponName = "Holy Sword";
                weapon.fireRate = 1.3f;
                weapon.attackRange = 3.5f;
                Debug.Log("<color=cyan>[Hero Switched]</color> Knight with Holy Sword (Melee Slash)!");
            }
            else if (hero == "Mage")
            {
#if UNITY_EDITOR
                RuntimeAnimatorController ctrl = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Characters/Mage_Controller.controller");
                if (ctrl != null && animator != null) animator.runtimeAnimatorController = ctrl;

                Sprite s = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Characters/Hero_Mage_Idle.png")
                    .OfType<Sprite>().FirstOrDefault();
                if (s != null && spriteRenderer != null) spriteRenderer.sprite = s;

                GameObject firePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile_Fireball.prefab");
                if (firePrefab != null) weapon.projectilePrefab = firePrefab;
#endif
                weapon.weaponName = "Fireball";
                weapon.fireRate = 1.1f;
                weapon.attackRange = 9.0f;
                Debug.Log("<color=magenta>[Hero Switched]</color> Mage with Fireball (Magic Missile)!");
            }
            else if (hero == "Ranger")
            {
#if UNITY_EDITOR
                RuntimeAnimatorController ctrl = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Characters/Ranger_Controller.controller");
                if (ctrl != null && animator != null) animator.runtimeAnimatorController = ctrl;

                Sprite s = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Characters/Hero_Ranger_Idle.png")
                    .OfType<Sprite>().FirstOrDefault();
                if (s != null && spriteRenderer != null) spriteRenderer.sprite = s;

                GameObject arrowPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile_Arrow.prefab");
                if (arrowPrefab != null) weapon.projectilePrefab = arrowPrefab;
#endif
                weapon.weaponName = "Bow & Arrow";
                weapon.fireRate = 1.6f;
                weapon.attackRange = 10.0f;
                Debug.Log("<color=green>[Hero Switched]</color> Ranger with Bow & Arrow (Piercing Arrow)!");
            }
            else if (hero == "ShadowKnight" || hero == "Rogue")
            {
#if UNITY_EDITOR
                RuntimeAnimatorController ctrl = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Characters/ShadowKnight_Controller.controller");
                if (ctrl != null && animator != null) animator.runtimeAnimatorController = ctrl;

                Sprite s = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Characters/Hero_Rogue_Idle.png")
                    .OfType<Sprite>().FirstOrDefault();
                if (s != null && spriteRenderer != null) spriteRenderer.sprite = s;

                GameObject slashPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX_ShadowSlash.prefab");
                if (slashPrefab != null) weapon.projectilePrefab = slashPrefab;
#endif
                weapon.weaponName = "Dark Crescent Blade";
                weapon.fireRate = 1.8f;
                weapon.attackRange = 4.5f;
                Debug.Log("<color=purple>[Hero Switched]</color> Shadow Knight with Dark Crescent Blade (Upgraded Slash)!");
            }
            else if (hero == "Archmage" || hero == "PhapSu" || hero == "GrandMage")
            {
#if UNITY_EDITOR
                RuntimeAnimatorController ctrl = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Characters/Archmage_Controller.controller");
                if (ctrl == null)
                {
                    UnityEditor.AssetDatabase.Refresh();
                    ctrl = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Characters/Archmage_Controller.controller");
                }
                if (ctrl != null && animator != null) animator.runtimeAnimatorController = ctrl;

                Sprite s = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Characters/Hero_Archmage_Idle.png")
                    .OfType<Sprite>().FirstOrDefault();
                if (s == null)
                {
                    UnityEditor.AssetDatabase.Refresh();
                    s = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Characters/Hero_Archmage_Idle.png")
                        .OfType<Sprite>().FirstOrDefault();
                }
                if (s != null && spriteRenderer != null) spriteRenderer.sprite = s;

                GameObject arcanePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile_ArcaneOrb.prefab");
                if (arcanePrefab != null) weapon.projectilePrefab = arcanePrefab;
#endif
                weapon.weaponName = "Arcane Staff";
                weapon.fireRate = 1.4f;
                weapon.attackRange = 10.0f;
                Debug.Log("<color=cyan>[Hero Switched]</color> Archmage with Arcane Staff (Piercing Arcane Orb)!");
            }
            else if (hero == "Archer" || hero == "Hunter" || hero == "Marksman" || hero == "CungThu")
            {
#if UNITY_EDITOR
                RuntimeAnimatorController ctrl = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Characters/Archer_Controller.controller");
                if (ctrl == null)
                {
                    UnityEditor.AssetDatabase.Refresh();
                    ctrl = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Characters/Archer_Controller.controller");
                }
                if (ctrl != null && animator != null) animator.runtimeAnimatorController = ctrl;

                Sprite s = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Characters/Hero_Archer_Idle.png")
                    .OfType<Sprite>().FirstOrDefault();
                if (s == null)
                {
                    UnityEditor.AssetDatabase.Refresh();
                    s = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Characters/Hero_Archer_Idle.png")
                        .OfType<Sprite>().FirstOrDefault();
                }
                if (s != null && spriteRenderer != null) spriteRenderer.sprite = s;

                GameObject arrowPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile_StormArrow.prefab");
                if (arrowPrefab != null) weapon.projectilePrefab = arrowPrefab;
#endif
                weapon.weaponName = "Storm Bow";
                weapon.fireRate = 1.6f;
                weapon.attackRange = 11.0f;
                Debug.Log("<color=green>[Hero Switched]</color> Archer with Storm Bow (Rapid Piercing Storm Arrow)!");
            }

            WeaponInventory inv = GetComponent<WeaponInventory>();
            if (inv != null && weapon != null)
            {
                inv.equippedWeapons.Clear();
                inv.AddWeapon(inv.CreateInitialWeapon(weapon.weaponName, weapon.projectilePrefab));
            }
        }
    }
}
