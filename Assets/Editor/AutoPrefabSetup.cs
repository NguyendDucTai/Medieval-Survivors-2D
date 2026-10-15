#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using VampireSurvivors.Player;
using VampireSurvivors.Enemies;
using VampireSurvivors.Weapons;
using VampireSurvivors.Collectibles;
using UnityEngine.UI;
using VampireSurvivors.UI;
using VampireSurvivors.Wave;
using VampireSurvivors.MetaProgression;
using VampireSurvivors.Data;

namespace VampireSurvivors.EditorTools
{
    public class AutoPrefabSetup
    {
        [MenuItem("Tools/1. Open Main Game Scene (SampleScene)", false, 0)]
        public static void OpenMainGameScene()
        {
            var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(activeScene.path) || activeScene.name == "Untitled")
            {
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", UnityEditor.SceneManagement.OpenSceneMode.Single);
            }
            else
            {
                if (UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", UnityEditor.SceneManagement.OpenSceneMode.Single);
                }
            }
            Debug.Log("Opened SampleScene successfully! Hit Play (▶) to test!");
        }

        [MenuItem("Tools/2. Setup Player & Test Enemies in Scene", false, 1)]
        public static void SetupAndSaveSampleScene()
        {
            string scenePath = "Assets/Scenes/SampleScene.unity";
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);

            EnsureTagExists("Player");
            EnsureTagExists("Enemy");

            // 1. Generate all prefabs and animations
            GeneratePrefabsAndSetupScene();

            // 2. Remove old test enemies and old player to prevent duplicates
            GameObject oldPlayer = GameObject.Find("Player");
            if (oldPlayer != null) Object.DestroyImmediate(oldPlayer);

            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (go.name.StartsWith("Enemy_"))
                {
                    Object.DestroyImmediate(go);
                }
            }

            // 3. Instantiate Player
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
            GameObject playerInScene = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
            if (playerInScene == null) playerInScene = Object.Instantiate(playerPrefab);
            playerInScene.name = "Player";
            playerInScene.tag = "Player";
            playerInScene.transform.position = Vector3.zero;
            SwitchToArcher();

            // 4. Background Grass Field
            GameObject bg = GameObject.Find("Environment_Background");
            if (bg == null)
            {
                bg = new GameObject("Environment_Background");
                SpriteRenderer bgSR = bg.AddComponent<SpriteRenderer>();
                Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/Castle_Stone_Light.png");
                if (bgSprite == null) bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/Castle_Cobblestone.png");
                bgSR.sprite = bgSprite;
                bgSR.drawMode = SpriteDrawMode.Tiled;
                bgSR.size = new Vector2(40f, 40f);
                bgSR.sortingOrder = -10;
                bg.transform.position = Vector3.zero;
            }

            // 5. Setup Camera Follow
            Camera cam = Camera.main;
            if (cam != null)
            {
                CameraFollow follow = cam.GetComponent<CameraFollow>();
                if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow>();
                follow.target = playerInScene.transform;
                follow.offset = new Vector3(0f, 0f, -10f);
                follow.smoothSpeed = 6f;
            }

            // 6. Setup GameManager
            GameObject slimePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Slime.prefab");
            GameObject gmGO = GameObject.Find("GameManager");
            if (gmGO == null)
            {
                gmGO = new GameObject("GameManager");
                Core.GameManager gm = gmGO.AddComponent<Core.GameManager>();
                gm.enemyPrefab = slimePrefab;
                gm.spawnInterval = 1.5f;
                gm.spawnRadius = 10f;
                gm.maxEnemiesAlive = 15;
            }
            else
            {
                Core.GameManager gm = gmGO.GetComponent<Core.GameManager>();
                if (gm != null && slimePrefab != null)
                {
                    gm.enemyPrefab = slimePrefab;
                }
            }

            // 7. Pre-place test enemies around Player
            GameObject skelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Skeleton.prefab");
            GameObject goblinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Goblin.prefab");
            GameObject batPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Bat.prefab");

            SpawnTestEnemy(slimePrefab, "Enemy_Slime_1", new Vector3(3f, 1.5f, 0f));
            SpawnTestEnemy(slimePrefab, "Enemy_Slime_2", new Vector3(-3.5f, 2f, 0f));
            SpawnTestEnemy(skelPrefab, "Enemy_Skeleton_1", new Vector3(0f, 3.5f, 0f));
            SpawnTestEnemy(goblinPrefab, "Enemy_Goblin_1", new Vector3(-3f, -2.5f, 0f));
            SpawnTestEnemy(batPrefab, "Enemy_Bat_1", new Vector3(4f, -2f, 0f));

            // 8. Save scene and assets
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("SUCCESS: SampleScene populated with Player and test enemies (Slimes, Skeleton, Goblin, Bat) and saved!");
        }

        private static void SpawnTestEnemy(GameObject prefab, string name, Vector3 pos)
        {
            if (prefab == null) return;
            GameObject inst = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (inst == null) inst = Object.Instantiate(prefab);
            inst.name = name;
            inst.transform.position = pos;
            inst.tag = "Enemy";
        }

        [MenuItem("Tools/Link Player Animations")]
        public static void AttachAnimationsToPlayerInScene()
        {
            EnsureTagExists("Player");
            EnsureTagExists("Enemy");

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) player = GameObject.Find("Player");

            if (player == null)
            {
                Debug.LogError("Player GameObject not found in Scene! Please run Setup Scene and All Prefabs first.");
                return;
            }

            // Attach Animator with active Hero Controller
            Animator anim = player.GetComponent<Animator>();
            if (anim == null) anim = player.AddComponent<Animator>();

            string ctrlPath = "Assets/Animations/Characters/Archer_Controller.controller";
            WeaponBase wb = player.GetComponent<WeaponBase>();
            if (wb != null && wb.weaponName == "Holy Sword")
                ctrlPath = "Assets/Animations/Characters/Knight_Controller.controller";
            else if (wb != null && wb.weaponName == "Fireball")
                ctrlPath = "Assets/Animations/Characters/Mage_Controller.controller";
            else if (wb != null && wb.weaponName == "Bow & Arrow")
                ctrlPath = "Assets/Animations/Characters/Ranger_Controller.controller";
            else if (wb != null && wb.weaponName == "Storm Bow")
                ctrlPath = "Assets/Animations/Characters/Archer_Controller.controller";
            else if (wb != null && wb.weaponName.Contains("Blade"))
                ctrlPath = "Assets/Animations/Characters/ShadowKnight_Controller.controller";
            else if (wb != null && wb.weaponName.Contains("Arcane"))
                ctrlPath = "Assets/Animations/Characters/Archmage_Controller.controller";

            RuntimeAnimatorController ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ctrlPath);
            if (ctrl == null)
            {
                AssetDatabase.Refresh();
                ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ctrlPath);
            }
            if (ctrl != null)
            {
                anim.runtimeAnimatorController = ctrl;
                Debug.Log($"Assigned {System.IO.Path.GetFileNameWithoutExtension(ctrlPath)} to Player Animator!");
            }
            else
            {
                Debug.LogError($"Controller not found at {ctrlPath}!");
            }

            // Ensure PlayerController
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc == null) pc = player.AddComponent<PlayerController>();

            // Ensure PlayerStats
            PlayerStats ps = player.GetComponent<PlayerStats>();
            if (ps == null) ps = player.AddComponent<PlayerStats>();

            // Ensure Rigidbody2D
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb == null) rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            // Ensure CircleCollider2D
            CircleCollider2D col = player.GetComponent<CircleCollider2D>();
            if (col == null) col = player.AddComponent<CircleCollider2D>();
            col.radius = 0.45f;

            EditorUtility.SetDirty(player);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );

            Selection.activeGameObject = player;
            Debug.Log("Player animations linked! Press Play to test movement.");
        }

        [MenuItem("Tools/Switch Hero/1. Knight (Thanh Kiếm)")]
        public static void SwitchToKnight()
        {
            SwitchPlayerCharacter(
                "Assets/Animations/Characters/Knight_Controller.controller",
                "Assets/Sprites/Characters/Hero_Knight_Idle.png",
                "Knight",
                "Assets/Prefabs/VFX_SwordSlash.prefab",
                "Holy Sword",
                1.3f,
                3.5f
            );
        }

        [MenuItem("Tools/Switch Hero/2. Mage (Cầu Lửa)")]
        public static void SwitchToMage()
        {
            SwitchPlayerCharacter(
                "Assets/Animations/Characters/Mage_Controller.controller",
                "Assets/Sprites/Characters/Hero_Mage_Idle.png",
                "Mage",
                "Assets/Prefabs/Projectile_Fireball.prefab",
                "Fireball",
                1.1f,
                9.0f
            );
        }

        [MenuItem("Tools/Switch Hero/3. Ranger (Cung Tên)")]
        public static void SwitchToRanger()
        {
            SwitchPlayerCharacter(
                "Assets/Animations/Characters/Ranger_Controller.controller",
                "Assets/Sprites/Characters/Hero_Ranger_Idle.png",
                "Ranger",
                "Assets/Prefabs/Projectile_Arrow.prefab",
                "Bow & Arrow",
                1.6f,
                10.0f
            );
        }

        [MenuItem("Tools/Switch Hero/4. Shadow Knight (Hắc Nguyệt Kiếm - Upgraded)")]
        public static void SwitchToShadowKnight()
        {
            SwitchPlayerCharacter(
                "Assets/Animations/Characters/ShadowKnight_Controller.controller",
                "Assets/Sprites/Characters/Hero_Rogue_Idle.png",
                "ShadowKnight",
                "Assets/Prefabs/VFX_ShadowSlash.prefab",
                "Dark Crescent Blade",
                1.8f,
                4.5f
            );
        }

        [MenuItem("Tools/Switch Hero/5. Archmage (Pháp Sư Tối Thượng - Cầu Arcane)")]
        public static void SwitchToArchmage()
        {
            SwitchPlayerCharacter(
                "Assets/Animations/Characters/Archmage_Controller.controller",
                "Assets/Sprites/Characters/Hero_Archmage_Idle.png",
                "Archmage",
                "Assets/Prefabs/Projectile_ArcaneOrb.prefab",
                "Arcane Staff",
                1.4f,
                10.0f
            );
        }

        [MenuItem("Tools/Switch Hero/6. Archer (Thần Tiễn - Cung Thần Phong)")]
        public static void SwitchToArcher()
        {
            SwitchPlayerCharacter(
                "Assets/Animations/Characters/Archer_Controller.controller",
                "Assets/Sprites/Characters/Hero_Archer_Idle.png",
                "Archer",
                "Assets/Prefabs/Projectile_StormArrow.prefab",
                "Storm Bow",
                1.6f,
                11.0f
            );
        }

        private static void SwitchPlayerCharacter(string controllerPath, string idleSpriteSheetPath, string characterName, string weaponPrefabPath, string weaponName, float fireRate, float attackRange)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) player = GameObject.Find("Player");
            if (player == null)
            {
                Debug.LogError("Player GameObject not found in Scene!");
                return;
            }

            // In Play Mode, call PlayerController.SwitchHero directly for instant runtime swap
            if (Application.isPlaying)
            {
                PlayerController pc = player.GetComponent<PlayerController>();
                if (pc != null)
                {
                    pc.SwitchHero(characterName);
                    Selection.activeGameObject = player;
                    return;
                }
            }

            Animator anim = player.GetComponent<Animator>();
            if (anim == null) anim = player.AddComponent<Animator>();

            RuntimeAnimatorController ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
            if (ctrl == null)
            {
                AssetDatabase.Refresh();
                ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
            }
            if (ctrl != null)
            {
                anim.runtimeAnimatorController = ctrl;
                EditorUtility.SetDirty(anim);
                PrefabUtility.RecordPrefabInstancePropertyModifications(anim);
            }

            SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Sprite firstSprite = AssetDatabase.LoadAllAssetsAtPath(idleSpriteSheetPath).OfType<Sprite>().FirstOrDefault();
                if (firstSprite == null)
                {
                    AssetDatabase.Refresh();
                    firstSprite = AssetDatabase.LoadAllAssetsAtPath(idleSpriteSheetPath).OfType<Sprite>().FirstOrDefault();
                }
                if (firstSprite != null)
                {
                    sr.sprite = firstSprite;
                    EditorUtility.SetDirty(sr);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(sr);
                }
            }

            WeaponBase weapon = player.GetComponent<WeaponBase>();
            if (weapon == null) weapon = player.AddComponent<WeaponBase>();
            weapon.weaponName = weaponName;
            weapon.fireRate = fireRate;
            weapon.attackRange = attackRange;

            GameObject wPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(weaponPrefabPath);
            if (wPrefab != null)
            {
                weapon.projectilePrefab = wPrefab;
            }
            else
            {
                Debug.LogWarning($"Weapon prefab not loaded at {weaponPrefabPath}");
            }

            EditorUtility.SetDirty(weapon);
            PrefabUtility.RecordPrefabInstancePropertyModifications(weapon);

            EditorUtility.SetDirty(player);
            PrefabUtility.RecordPrefabInstancePropertyModifications(player);

            var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(activeScene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(activeScene);

            Selection.activeGameObject = player;
            Debug.Log($"Switched character to {characterName} with weapon {weaponName} and SAVED scene successfully!");
        }

        [MenuItem("Tools/Switch Background/1. Royal Castle Stone (Đá Hoàng Gia Sáng Rõ - Khuyên Dùng)")]
        public static void SwitchToLightCastleStoneBackground()
        {
            SwitchBackgroundSprite("Assets/Sprites/Environment/Castle_Stone_Light.png", "Royal Castle Stone (Light)");
        }

        [MenuItem("Tools/Switch Background/2. Castle Cobblestone (Đá Lâu Đài Tối)")]
        public static void SwitchToCobblestoneBackground()
        {
            SwitchBackgroundSprite("Assets/Sprites/Environment/Castle_Cobblestone.png", "Castle Cobblestone (Dark)");
        }

        [MenuItem("Tools/Switch Background/3. Dark Forest Meadow (Cỏ Rừng Trung Cổ)")]
        public static void SwitchToGrassBackground()
        {
            SwitchBackgroundSprite("Assets/Sprites/Environment/Grass_Tile.png", "Dark Forest Meadow");
        }

        private static void SwitchBackgroundSprite(string spritePath, string backgroundName)
        {
            GameObject bg = GameObject.Find("Environment_Background");
            if (bg == null)
            {
                Debug.LogError("Environment_Background GameObject not found in Scene!");
                return;
            }

            SpriteRenderer sr = bg.GetComponent<SpriteRenderer>();
            if (sr == null) sr = bg.AddComponent<SpriteRenderer>();

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite != null)
            {
                sr.sprite = sprite;
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = new Vector2(40f, 40f);
                sr.sortingOrder = -10;
                EditorUtility.SetDirty(bg);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                    UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
                );
                Debug.Log($"Background switched to {backgroundName}!");
            }
            else
            {
                Debug.LogError($"Sprite not found at {spritePath}");
            }
        }

        [MenuItem("Tools/Setup Scene & All Prefabs")]
        public static void GeneratePrefabsAndSetupScene()
        {
            EnsureTagExists("Player");
            EnsureTagExists("Enemy");
            EnsureDirectory("Assets/Prefabs");

            // Auto-slice and configure all Spritesheets, Animations & Animator Controllers
            SpriteSheetSlicer.SliceAndSetupAll();

            // 1. ExpGem Prefab
            Sprite expSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Collectibles/ExpGem_Blue.png");
            GameObject gemGO = new GameObject("ExpGem");
            gemGO.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
            SpriteRenderer gemSR = gemGO.AddComponent<SpriteRenderer>();
            gemSR.sprite = expSprite;
            gemSR.sortingOrder = 1;
            CircleCollider2D gemCol = gemGO.AddComponent<CircleCollider2D>();
            gemCol.isTrigger = true;
            gemCol.radius = 0.4f;
            gemGO.AddComponent<ExpGem>();
            gemGO.tag = "Finish";
            string gemPrefabPath = "Assets/Prefabs/ExpGem.prefab";
            GameObject gemPrefab = PrefabUtility.SaveAsPrefabAsset(gemGO, gemPrefabPath);
            Object.DestroyImmediate(gemGO);
            Debug.Log("Created Prefab: " + gemPrefabPath);

            // 2. Projectile Fireball Prefab
            Sprite fireballSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Weapons/Weapon_Fireball.png");
            GameObject projGO = new GameObject("Projectile_Fireball");
            SpriteRenderer projSR = projGO.AddComponent<SpriteRenderer>();
            projSR.sprite = fireballSprite;
            projSR.sortingOrder = 5;
            CircleCollider2D projCol = projGO.AddComponent<CircleCollider2D>();
            projCol.isTrigger = true;
            projCol.radius = 0.35f;
            projGO.AddComponent<Projectile>();
            string projPrefabPath = "Assets/Prefabs/Projectile_Fireball.prefab";
            GameObject projPrefab = PrefabUtility.SaveAsPrefabAsset(projGO, projPrefabPath);
            Object.DestroyImmediate(projGO);
            Debug.Log("Created Prefab: " + projPrefabPath);

            // 2b. Projectile Arrow Prefab (Ranger)
            Sprite arrowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Weapons/Weapon_Arrow.png");
            GameObject arrowGO = new GameObject("Projectile_Arrow");
            SpriteRenderer arrowSR = arrowGO.AddComponent<SpriteRenderer>();
            arrowSR.sprite = arrowSprite;
            arrowSR.sortingOrder = 5;
            CircleCollider2D arrowCol = arrowGO.AddComponent<CircleCollider2D>();
            arrowCol.isTrigger = true;
            arrowCol.radius = 0.35f;
            Projectile arrowProj = arrowGO.AddComponent<Projectile>();
            arrowProj.speed = 14f;
            arrowProj.damage = 15f;
            arrowProj.lifeTime = 2.5f;
            arrowProj.pierceCount = 2;
            string arrowPrefabPath = "Assets/Prefabs/Projectile_Arrow.prefab";
            GameObject arrowPrefab = PrefabUtility.SaveAsPrefabAsset(arrowGO, arrowPrefabPath);
            Object.DestroyImmediate(arrowGO);
            Debug.Log("Created Prefab: " + arrowPrefabPath);

            // 2c. VFX Sword Slash Prefab (Knight)
            Sprite slashSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Weapons/VFX_Slash.png");
            GameObject slashGO = new GameObject("VFX_SwordSlash");
            SpriteRenderer slashSR = slashGO.AddComponent<SpriteRenderer>();
            slashSR.sprite = slashSprite;
            slashSR.sortingOrder = 6;
            CircleCollider2D slashCol = slashGO.AddComponent<CircleCollider2D>();
            slashCol.isTrigger = true;
            slashCol.radius = 0.75f;
            MeleeSlash slashComp = slashGO.AddComponent<MeleeSlash>();
            slashComp.damage = 30f;
            slashComp.duration = 0.22f;
            slashComp.initialScale = 0.8f;
            slashComp.finalScale = 1.6f;
            slashComp.knockbackForce = 3f;
            string slashPrefabPath = "Assets/Prefabs/VFX_SwordSlash.prefab";
            GameObject slashPrefab = PrefabUtility.SaveAsPrefabAsset(slashGO, slashPrefabPath);
            Object.DestroyImmediate(slashGO);
            Debug.Log("Created Prefab: " + slashPrefabPath);

            // 3. Enemy Slime Prefab
            GameObject slimePrefab = CreateEnemyPrefab("Enemy_Slime", "Assets/Sprites/Enemies/Slime_Green.png", "Assets/Animations/Enemies/Slime_Controller.controller", gemPrefab, 20f, 1.4f, 8f);

            // 4. Enemy Skeleton Prefab
            GameObject skelPrefab = CreateEnemyPrefab("Enemy_Skeleton", "Assets/Sprites/Enemies/Skeleton_White.png", "Assets/Animations/Enemies/Skeleton_Controller.controller", gemPrefab, 35f, 1.8f, 12f);

            // 5. Enemy Goblin Prefab
            GameObject goblinPrefab = CreateEnemyPrefab("Enemy_Goblin", "Assets/Sprites/Enemies/Goblin_Green.png", "Assets/Animations/Enemies/Goblin_Controller.controller", gemPrefab, 25f, 2.2f, 10f);

            // 6. Enemy Bat Prefab
            GameObject batPrefab = CreateEnemyPrefab("Enemy_Bat", "Assets/Sprites/Enemies/Bat_Fly.png", "Assets/Animations/Enemies/Bat_Controller.controller", gemPrefab, 15f, 2.4f, 6f);

            // 7. Enemy Boss Golem Prefab
            GameObject golemPrefab = CreateEnemyPrefab("Enemy_BossGolem", "Assets/Sprites/Enemies/Boss_Golem.png", "Assets/Animations/Enemies/Boss_Golem_Controller.controller", gemPrefab, 250f, 1.0f, 25f);

            // 8. Player Prefab
            Sprite heroSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Characters/Hero_Knight.png");
            GameObject playerGO = new GameObject("Player");
            SpriteRenderer playerSR = playerGO.AddComponent<SpriteRenderer>();
            playerSR.sprite = heroSprite;
            playerSR.sortingOrder = 3;
            Rigidbody2D playerRB = playerGO.AddComponent<Rigidbody2D>();
            playerRB.gravityScale = 0f;
            playerRB.freezeRotation = true;
            CircleCollider2D playerCol = playerGO.AddComponent<CircleCollider2D>();
            playerCol.radius = 0.45f;
            playerGO.AddComponent<PlayerStats>();
            playerGO.AddComponent<PlayerController>();

            RuntimeAnimatorController knightCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Characters/Knight_Controller.controller");
            if (knightCtrl != null)
            {
                Animator anim = playerGO.AddComponent<Animator>();
                anim.runtimeAnimatorController = knightCtrl;
            }

            WeaponBase weapon = playerGO.AddComponent<WeaponBase>();
            weapon.weaponName = "Holy Sword";
            weapon.projectilePrefab = slashPrefab;
            weapon.fireRate = 1.3f;
            weapon.attackRange = 3.5f;
            playerGO.tag = "Player";
            string playerPrefabPath = "Assets/Prefabs/Player.prefab";
            GameObject playerPrefab = PrefabUtility.SaveAsPrefabAsset(playerGO, playerPrefabPath);
            Object.DestroyImmediate(playerGO);
            Debug.Log("Created Prefab: " + playerPrefabPath);

            // 9. Setup Entities in the Current Active Scene
            SetupSceneEntities(playerPrefab, slimePrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("ALL PREFABS & SCENE SETUP COMPLETED SUCCESSFULLY!");
        }

        private static void SetupSceneEntities(GameObject playerPrefab, GameObject enemyPrefab)
        {
            // A. Instantiate or Find Player
            GameObject playerInScene = GameObject.FindGameObjectWithTag("Player");
            if (playerInScene == null)
            {
                playerInScene = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
                if (playerInScene == null) playerInScene = Object.Instantiate(playerPrefab);
                playerInScene.name = "Player";
                playerInScene.tag = "Player";
                playerInScene.transform.position = Vector3.zero;
                Undo.RegisterCreatedObjectUndo(playerInScene, "Spawn Player in Scene");
            }

            // B. Background Grass Field
            GameObject bg = GameObject.Find("Environment_Background");
            if (bg == null)
            {
                bg = new GameObject("Environment_Background");
                SpriteRenderer bgSR = bg.AddComponent<SpriteRenderer>();
                Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/Castle_Cobblestone.png");
                if (bgSprite == null) bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/Grass_Tile.png");
                bgSR.sprite = bgSprite;
                bgSR.drawMode = SpriteDrawMode.Tiled;
                bgSR.size = new Vector2(40f, 40f);
                bgSR.sortingOrder = -10;
                bg.transform.position = Vector3.zero;
                Undo.RegisterCreatedObjectUndo(bg, "Create Background");
            }

            // C. Setup Camera Follow
            Camera cam = Camera.main;
            if (cam != null)
            {
                CameraFollow follow = cam.GetComponent<CameraFollow>();
                if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow>();
                follow.target = playerInScene.transform;
                follow.offset = new Vector3(0f, 0f, -10f);
                follow.smoothSpeed = 6f;
            }

            // D. GameManager Spawner
            GameObject gmGO = GameObject.Find("GameManager");
            if (gmGO == null)
            {
                gmGO = new GameObject("GameManager");
                Core.GameManager gm = gmGO.AddComponent<Core.GameManager>();
                gm.enemyPrefab = enemyPrefab;
                gm.spawnInterval = 1.5f;
                gm.spawnRadius = 10f;
                gm.maxEnemiesAlive = 15;
                Undo.RegisterCreatedObjectUndo(gmGO, "Create GameManager");
            }

            // Frame player in Scene view
            Selection.activeGameObject = playerInScene;
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.FrameSelected();
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );
        }

        private static GameObject CreateEnemyPrefab(string name, string spritePath, string controllerPath, GameObject expGemPrefab, float hp, float speed, float damage)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite == null)
            {
                sprite = AssetDatabase.LoadAllAssetsAtPath(spritePath).OfType<Sprite>().FirstOrDefault();
            }

            GameObject go = new GameObject(name);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 2;

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.45f;

            EnemyStats stats = go.AddComponent<EnemyStats>();
            stats.maxHealth = hp;
            stats.moveSpeed = speed;
            stats.attackDamage = damage;
            stats.expGemPrefab = expGemPrefab;

            go.AddComponent<EnemyAI>();
            go.tag = "Enemy";

            RuntimeAnimatorController ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
            if (ctrl != null)
            {
                Animator anim = go.AddComponent<Animator>();
                anim.runtimeAnimatorController = ctrl;
            }

            string path = $"Assets/Prefabs/{name}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            Debug.Log("Created Prefab: " + path);
            return prefab;
        }

        private static void EnsureTagExists(string tag)
        {
            UnityEngine.Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (asset != null && asset.Length > 0)
            {
                SerializedObject so = new SerializedObject(asset[0]);
                SerializedProperty tags = so.FindProperty("tags");
                for (int i = 0; i < tags.arraySize; ++i)
                {
                    if (tags.GetArrayElementAtIndex(i).stringValue.Equals(tag)) return;
                }
                tags.InsertArrayElementAtIndex(tags.arraySize);
                tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
                so.ApplyModifiedProperties();
            }
        }

        private static void EnsureDirectory(string path)
        {
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
        }

        [MenuItem("Tools/3. Setup Complete 2-Month Retention Gameplay & UI in Scene", false, 2)]
        public static void SetupCompleteGameAndUI()
        {
            string scenePath = "Assets/Scenes/SampleScene.unity";
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);

            // 1. Generate Sprites first
            PixelArtAssetPack.GenerateAllSprites();

            // 2. Setup Player & Scene
            SetupAndSaveSampleScene();

            // 3. Ensure EventSystem exists
            UnityEngine.EventSystems.EventSystem es = Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (es == null)
            {
                GameObject esGO = new GameObject("EventSystem");
                esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // 4. Setup Canvas
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            GameObject canvasGO = canvas != null ? canvas.gameObject : null;
            if (canvasGO == null)
            {
                canvasGO = new GameObject("Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler cs = canvasGO.AddComponent<CanvasScaler>();
                cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                cs.referenceResolution = new Vector2(960, 540);
                cs.matchWidthOrHeight = 0.5f;
                canvasGO.AddComponent<GraphicRaycaster>();
            }

            UIManager uiMgr = canvasGO.GetComponent<UIManager>();
            if (uiMgr == null) uiMgr = canvasGO.AddComponent<UIManager>();

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Sprite woodFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/WoodFrame_Panel.png");
            Sprite btnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Button_Normal.png");
            Sprite heartSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Heart_Icon.png");
            Sprite hpFillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/HealthBar_Fill.png");
            Sprite expFillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/ExpBar_Fill.png");

            // --- HUD Root ---
            Transform hudT = canvasGO.transform.Find("HUD");
            GameObject hudGO = hudT != null ? hudT.gameObject : new GameObject("HUD", typeof(RectTransform));
            hudGO.transform.SetParent(canvasGO.transform, false);
            RectTransform hudRT = hudGO.GetComponent<RectTransform>();
            hudRT.anchorMin = Vector2.zero; hudRT.anchorMax = Vector2.one; hudRT.sizeDelta = Vector2.zero;

            // Health Bar (Top Left)
            Transform hpBarT = hudGO.transform.Find("HealthBar");
            GameObject hpBarGO = hpBarT != null ? hpBarT.gameObject : CreateSliderGO("HealthBar", hudGO.transform, new Vector2(25, -25), new Vector2(160, 18), new Vector2(0, 1), new Vector2(0, 1), hpFillSprite, new Color(0.85f, 0.2f, 0.2f));
            Slider hpSlider = hpBarGO.GetComponent<Slider>();
            uiMgr.healthBar = hpSlider;

            // Exp Bar (Top Across)
            Transform expBarT = hudGO.transform.Find("ExpBar");
            GameObject expBarGO = expBarT != null ? expBarT.gameObject : CreateSliderGO("ExpBar", hudGO.transform, new Vector2(0, -6), new Vector2(-60, 10), new Vector2(0, 1), new Vector2(1, 1), expFillSprite, new Color(0.2f, 0.7f, 1f));
            Slider expSlider = expBarGO.GetComponent<Slider>();
            uiMgr.expBar = expSlider;

            // Level Text
            Text lvlText = GetOrCreateText(hudGO.transform, "LevelText", "LV 1", 14, defaultFont, Color.white, new Vector2(20, -6), new Vector2(80, 20), new Vector2(0, 1), new Vector2(0, 1));
            uiMgr.levelText = lvlText;

            // Timer Text (Top Center)
            Text timerText = GetOrCreateText(hudGO.transform, "TimerText", "00:00", 22, defaultFont, new Color(1f, 0.85f, 0.2f), new Vector2(0, -28), new Vector2(140, 30), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            timerText.alignment = TextAnchor.MiddleCenter;
            uiMgr.timerText = timerText;

            // Kill & Gold Counters (Top Right)
            Text killsText = GetOrCreateText(hudGO.transform, "KillCountText", "💀 0", 15, defaultFont, Color.white, new Vector2(-150, -25), new Vector2(100, 25), new Vector2(1, 1), new Vector2(1, 1));
            killsText.alignment = TextAnchor.MiddleRight;
            uiMgr.killCountText = killsText;

            Text goldText = GetOrCreateText(hudGO.transform, "GoldText", "💰 0", 15, defaultFont, new Color(1f, 0.85f, 0.2f), new Vector2(-25, -25), new Vector2(100, 25), new Vector2(1, 1), new Vector2(1, 1));
            goldText.alignment = TextAnchor.MiddleRight;
            uiMgr.goldText = goldText;

            // Wave Announcement Banner (Center)
            Text bannerText = GetOrCreateText(hudGO.transform, "BannerText", "", 22, defaultFont, new Color(1f, 0.35f, 0.1f), new Vector2(0, 70), new Vector2(600, 40), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            bannerText.alignment = TextAnchor.MiddleCenter;
            bannerText.gameObject.SetActive(false);
            uiMgr.bannerText = bannerText;

            // Boss Health Bar (Top Center below timer)
            Transform bossBarT = hudGO.transform.Find("BossHealthBar");
            GameObject bossBarGO = bossBarT != null ? bossBarT.gameObject : CreateSliderGO("BossHealthBar", hudGO.transform, new Vector2(0, -60), new Vector2(320, 20), new Vector2(0.5f, 1), new Vector2(0.5f, 1), hpFillSprite, new Color(0.9f, 0.1f, 0.3f));
            BossHealthBarUI bossUI = bossBarGO.GetComponent<BossHealthBarUI>() ?? bossBarGO.AddComponent<BossHealthBarUI>();
            bossUI.container = bossBarGO;
            bossUI.healthSlider = bossBarGO.GetComponent<Slider>();
            Text bossTitleText = GetOrCreateText(bossBarGO.transform, "BossTitle", "KING SLIME", 13, defaultFont, Color.white, new Vector2(0, 16), new Vector2(300, 20), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            bossTitleText.alignment = TextAnchor.MiddleCenter;
            bossUI.bossTitleText = bossTitleText;
            bossBarGO.SetActive(false);
            uiMgr.bossHealthBarUI = bossUI;

            // --- Level Up Modal Panel ---
            Transform lvlPanelT = canvasGO.transform.Find("LevelUpPanel");
            GameObject lvlPanelGO = lvlPanelT != null ? lvlPanelT.gameObject : new GameObject("LevelUpPanel", typeof(RectTransform));
            lvlPanelGO.transform.SetParent(canvasGO.transform, false);
            RectTransform lvlRT = lvlPanelGO.GetComponent<RectTransform>();
            lvlRT.anchorMin = Vector2.zero; lvlRT.anchorMax = Vector2.one; lvlRT.sizeDelta = Vector2.zero;
            Image lvlBg = lvlPanelGO.GetComponent<Image>() ?? lvlPanelGO.AddComponent<Image>();
            lvlBg.color = new Color(0, 0, 0, 0.85f);

            LevelUpUI levelUpUI = lvlPanelGO.GetComponent<LevelUpUI>() ?? lvlPanelGO.AddComponent<LevelUpUI>();
            levelUpUI.panel = lvlPanelGO;

            Text lvlHeader = GetOrCreateText(lvlPanelGO.transform, "Header", "★ CHỌN NÂNG CẤP KỸ NĂNG ★", 24, defaultFont, new Color(1f, 0.85f, 0.2f), new Vector2(0, 180), new Vector2(600, 40), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            lvlHeader.alignment = TextAnchor.MiddleCenter;

            // 3 Card Slots
            levelUpUI.cardSlots = new GameObject[3];
            levelUpUI.titleTexts = new Text[3];
            levelUpUI.descTexts = new Text[3];
            levelUpUI.badgeTexts = new Text[3];
            levelUpUI.selectButtons = new Button[3];

            float[] cardXOffsets = { -240f, 0f, 240f };
            for (int i = 0; i < 3; i++)
            {
                string cardName = "CardSlot_" + i;
                Transform cardT = lvlPanelGO.transform.Find(cardName);
                GameObject cardGO = cardT != null ? cardT.gameObject : new GameObject(cardName, typeof(RectTransform));
                cardGO.transform.SetParent(lvlPanelGO.transform, false);
                RectTransform crt = cardGO.GetComponent<RectTransform>();
                crt.anchorMin = new Vector2(0.5f, 0.5f); crt.anchorMax = new Vector2(0.5f, 0.5f);
                crt.anchoredPosition = new Vector2(cardXOffsets[i], 0);
                crt.sizeDelta = new Vector2(210, 270);

                Image cimg = cardGO.GetComponent<Image>() ?? cardGO.AddComponent<Image>();
                if (woodFrame != null) { cimg.sprite = woodFrame; cimg.type = Image.Type.Sliced; }
                else cimg.color = new Color(0.18f, 0.15f, 0.25f);

                Text badge = GetOrCreateText(cardGO.transform, "Badge", "NÂNG CẤP", 12, defaultFont, new Color(1f, 0.85f, 0.2f), new Vector2(0, 100), new Vector2(180, 22), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
                badge.alignment = TextAnchor.MiddleCenter;

                Text title = GetOrCreateText(cardGO.transform, "Title", "Tên Kỹ Năng", 15, defaultFont, Color.white, new Vector2(0, 65), new Vector2(180, 40), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
                title.alignment = TextAnchor.MiddleCenter;

                Text desc = GetOrCreateText(cardGO.transform, "Desc", "Mô tả chi tiết kỹ năng...", 12, defaultFont, new Color(0.85f, 0.85f, 0.85f), new Vector2(0, 0), new Vector2(180, 70), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
                desc.alignment = TextAnchor.MiddleCenter;

                GameObject btnGO = CreateButtonGO("SelectButton", cardGO.transform, new Vector2(0, -90), new Vector2(140, 38), btnSprite, "CHỌN", defaultFont);
                Button selectBtn = btnGO.GetComponent<Button>();

                levelUpUI.cardSlots[i] = cardGO;
                levelUpUI.titleTexts[i] = title;
                levelUpUI.descTexts[i] = desc;
                levelUpUI.badgeTexts[i] = badge;
                levelUpUI.selectButtons[i] = selectBtn;
            }

            GameObject rerollBtnGO = CreateButtonGO("RerollButton", lvlPanelGO.transform, new Vector2(0, -180), new Vector2(160, 38), btnSprite, "Đổi Lại (0)", defaultFont);
            levelUpUI.rerollButton = rerollBtnGO.GetComponent<Button>();
            levelUpUI.rerollText = rerollBtnGO.GetComponentInChildren<Text>();
            lvlPanelGO.SetActive(false);
            uiMgr.levelUpUI = levelUpUI;

            // --- Run Summary Modal Panel ---
            Transform summaryT = canvasGO.transform.Find("RunSummaryPanel");
            GameObject summaryGO = summaryT != null ? summaryT.gameObject : new GameObject("RunSummaryPanel", typeof(RectTransform));
            summaryGO.transform.SetParent(canvasGO.transform, false);
            RectTransform srt = summaryGO.GetComponent<RectTransform>();
            srt.anchorMin = Vector2.zero; srt.anchorMax = Vector2.one; srt.sizeDelta = Vector2.zero;
            Image sbg = summaryGO.GetComponent<Image>() ?? summaryGO.AddComponent<Image>();
            sbg.color = new Color(0, 0, 0, 0.88f);

            RunSummaryUI runSummaryUI = summaryGO.GetComponent<RunSummaryUI>() ?? summaryGO.AddComponent<RunSummaryUI>();
            runSummaryUI.panel = summaryGO;

            GameObject sFrame = new GameObject("Frame", typeof(RectTransform));
            sFrame.transform.SetParent(summaryGO.transform, false);
            RectTransform sfrt = sFrame.GetComponent<RectTransform>();
            sfrt.sizeDelta = new Vector2(460, 380);
            Image sfImg = sFrame.AddComponent<Image>();
            if (woodFrame != null) { sfImg.sprite = woodFrame; sfImg.type = Image.Type.Sliced; }
            else sfImg.color = new Color(0.15f, 0.12f, 0.22f);

            runSummaryUI.resultTitleText = GetOrCreateText(sFrame.transform, "Title", "KẾT THÚC TRẬN ĐẤU", 22, defaultFont, new Color(1f, 0.85f, 0.2f), new Vector2(0, 140), new Vector2(400, 35), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            runSummaryUI.resultTitleText.alignment = TextAnchor.MiddleCenter;

            runSummaryUI.survivalTimeText = GetOrCreateText(sFrame.transform, "Time", "Thời Gian Sống Sót: 00:00", 14, defaultFont, Color.white, new Vector2(0, 85), new Vector2(380, 25), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            runSummaryUI.survivalTimeText.alignment = TextAnchor.MiddleCenter;

            runSummaryUI.levelText = GetOrCreateText(sFrame.transform, "Level", "Cấp Độ Đạt Được: Lv 1", 14, defaultFont, Color.white, new Vector2(0, 55), new Vector2(380, 25), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            runSummaryUI.levelText.alignment = TextAnchor.MiddleCenter;

            runSummaryUI.killsText = GetOrCreateText(sFrame.transform, "Kills", "Quái Vật Tiêu Diệt: 0", 14, defaultFont, Color.white, new Vector2(0, 25), new Vector2(380, 25), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            runSummaryUI.killsText.alignment = TextAnchor.MiddleCenter;

            runSummaryUI.damageText = GetOrCreateText(sFrame.transform, "Damage", "Tổng Sát Thương: 0", 14, defaultFont, Color.white, new Vector2(0, -5), new Vector2(380, 25), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            runSummaryUI.damageText.alignment = TextAnchor.MiddleCenter;

            runSummaryUI.runGoldText = GetOrCreateText(sFrame.transform, "RunGold", "Vàng Thu Thập: +0 Vàng", 15, defaultFont, new Color(1f, 0.85f, 0.2f), new Vector2(0, -35), new Vector2(380, 25), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            runSummaryUI.runGoldText.alignment = TextAnchor.MiddleCenter;

            runSummaryUI.bankGoldText = GetOrCreateText(sFrame.transform, "BankGold", "Quỹ Vàng Hiện Có: 0 Vàng", 14, defaultFont, new Color(0.4f, 0.9f, 1f), new Vector2(0, -65), new Vector2(380, 25), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            runSummaryUI.bankGoldText.alignment = TextAnchor.MiddleCenter;

            GameObject playAgainBtn = CreateButtonGO("PlayAgainBtn", sFrame.transform, new Vector2(-120, -125), new Vector2(110, 36), btnSprite, "Chơi Lại", defaultFont);
            runSummaryUI.playAgainButton = playAgainBtn.GetComponent<Button>();

            GameObject shopBtn = CreateButtonGO("ShopBtn", sFrame.transform, new Vector2(0, -125), new Vector2(110, 36), btnSprite, "Cửa Hàng", defaultFont);
            runSummaryUI.shopButton = shopBtn.GetComponent<Button>();

            GameObject achBtn = CreateButtonGO("AchBtn", sFrame.transform, new Vector2(120, -125), new Vector2(110, 36), btnSprite, "Thành Tựu", defaultFont);
            runSummaryUI.achievementsButton = achBtn.GetComponent<Button>();

            summaryGO.SetActive(false);
            uiMgr.runSummaryUI = runSummaryUI;

            // --- Shop Modal Panel ---
            Transform shopT = canvasGO.transform.Find("ShopPanel");
            GameObject shopGO = shopT != null ? shopT.gameObject : new GameObject("ShopPanel", typeof(RectTransform));
            shopGO.transform.SetParent(canvasGO.transform, false);
            RectTransform shrt = shopGO.GetComponent<RectTransform>();
            shrt.anchorMin = Vector2.zero; shrt.anchorMax = Vector2.one; shrt.sizeDelta = Vector2.zero;
            Image shbg = shopGO.GetComponent<Image>() ?? shopGO.AddComponent<Image>();
            shbg.color = new Color(0, 0, 0, 0.9f);

            ShopUI shopUI = shopGO.GetComponent<ShopUI>() ?? shopGO.AddComponent<ShopUI>();
            shopUI.panel = shopGO;

            GameObject shFrame = new GameObject("Frame", typeof(RectTransform));
            shFrame.transform.SetParent(shopGO.transform, false);
            RectTransform shfrt = shFrame.GetComponent<RectTransform>();
            shfrt.sizeDelta = new Vector2(500, 420);
            Image shfImg = shFrame.AddComponent<Image>();
            if (woodFrame != null) { shfImg.sprite = woodFrame; shfImg.type = Image.Type.Sliced; }
            else shfImg.color = new Color(0.15f, 0.12f, 0.22f);

            Text shTitle = GetOrCreateText(shFrame.transform, "Title", "CỬA HÀNG NÂNG CẤP VĨNH VIỄN", 20, defaultFont, new Color(1f, 0.85f, 0.2f), new Vector2(0, 175), new Vector2(460, 30), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            shTitle.alignment = TextAnchor.MiddleCenter;

            shopUI.goldBalanceText = GetOrCreateText(shFrame.transform, "GoldBalance", "Quỹ Vàng: 0 Vàng", 15, defaultFont, new Color(1f, 0.85f, 0.2f), new Vector2(0, 145), new Vector2(460, 25), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            shopUI.goldBalanceText.alignment = TextAnchor.MiddleCenter;

            GameObject scrollGO = new GameObject("ScrollContent", typeof(RectTransform));
            scrollGO.transform.SetParent(shFrame.transform, false);
            RectTransform scrt = scrollGO.GetComponent<RectTransform>();
            scrt.anchoredPosition = new Vector2(0, -10);
            scrt.sizeDelta = new Vector2(440, 250);
            VerticalLayoutGroup svlg = scrollGO.AddComponent<VerticalLayoutGroup>();
            svlg.childControlWidth = true; svlg.childControlHeight = false; svlg.childForceExpandWidth = true; svlg.spacing = 8f;
            shopUI.itemsScrollContent = scrollGO.transform;

            GameObject shCloseBtn = CreateButtonGO("CloseButton", shFrame.transform, new Vector2(0, -175), new Vector2(130, 35), btnSprite, "Đóng Cửa Hàng", defaultFont);
            shopUI.closeButton = shCloseBtn.GetComponent<Button>();
            shopGO.SetActive(false);
            uiMgr.shopUI = shopUI;

            // --- Achievement Modal Panel ---
            Transform achT = canvasGO.transform.Find("AchievementPanel");
            GameObject achGO = achT != null ? achT.gameObject : new GameObject("AchievementPanel", typeof(RectTransform));
            achGO.transform.SetParent(canvasGO.transform, false);
            RectTransform achrt = achGO.GetComponent<RectTransform>();
            achrt.anchorMin = Vector2.zero; achrt.anchorMax = Vector2.one; achrt.sizeDelta = Vector2.zero;
            Image achbg = achGO.GetComponent<Image>() ?? achGO.AddComponent<Image>();
            achbg.color = new Color(0, 0, 0, 0.9f);

            AchievementUI achUI = achGO.GetComponent<AchievementUI>() ?? achGO.AddComponent<AchievementUI>();
            achUI.panel = achGO;

            GameObject achFrame = new GameObject("Frame", typeof(RectTransform));
            achFrame.transform.SetParent(achGO.transform, false);
            RectTransform achfrt = achFrame.GetComponent<RectTransform>();
            achfrt.sizeDelta = new Vector2(500, 420);
            Image achfImg = achFrame.AddComponent<Image>();
            if (woodFrame != null) { achfImg.sprite = woodFrame; achfImg.type = Image.Type.Sliced; }
            else achfImg.color = new Color(0.15f, 0.12f, 0.22f);

            Text achTitle = GetOrCreateText(achFrame.transform, "Title", "BẢNG THÀNH TỰU & NHIỆM VỤ 60 NGÀY", 20, defaultFont, new Color(1f, 0.85f, 0.2f), new Vector2(0, 175), new Vector2(460, 30), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            achTitle.alignment = TextAnchor.MiddleCenter;

            GameObject achScrollGO = new GameObject("ScrollContent", typeof(RectTransform));
            achScrollGO.transform.SetParent(achFrame.transform, false);
            RectTransform achscrt = achScrollGO.GetComponent<RectTransform>();
            achscrt.anchoredPosition = new Vector2(0, -10);
            achscrt.sizeDelta = new Vector2(440, 270);
            VerticalLayoutGroup achvlg = achScrollGO.AddComponent<VerticalLayoutGroup>();
            achvlg.childControlWidth = true; achvlg.childControlHeight = false; achvlg.childForceExpandWidth = true; achvlg.spacing = 8f;
            achUI.itemsScrollContent = achScrollGO.transform;

            GameObject achCloseBtn = CreateButtonGO("CloseButton", achFrame.transform, new Vector2(0, -175), new Vector2(130, 35), btnSprite, "Đóng", defaultFont);
            achUI.closeButton = achCloseBtn.GetComponent<Button>();
            achGO.SetActive(false);
            uiMgr.achievementUI = achUI;

            // 5. Ensure WaveManager & Singletons in Scene
            GameObject waveMgrGO = GameObject.Find("WaveManager");
            if (waveMgrGO == null)
            {
                waveMgrGO = new GameObject("WaveManager");
                waveMgrGO.AddComponent<WaveManager>();
            }

            GameObject dmgPoolGO = GameObject.Find("DamageNumberPool");
            if (dmgPoolGO == null)
            {
                dmgPoolGO = new GameObject("DamageNumberPool");
                dmgPoolGO.AddComponent<DamageNumberPool>();
            }

            // Camera Juice
            if (Camera.main != null && Camera.main.GetComponent<Core.CameraJuice>() == null)
            {
                Camera.main.gameObject.AddComponent<Core.CameraJuice>();
            }

            // Save Scene
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("<color=green>SUCCESS: Fully set up 2-Month Retention Gameplay & Complete UI Suite in SampleScene!</color>");
        }

        private static GameObject CreateSliderGO(string name, Transform parent, Vector2 pos, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Sprite fillSprite, Color fillColor)
        {
            GameObject sGO = new GameObject(name, typeof(RectTransform));
            sGO.transform.SetParent(parent, false);
            RectTransform srt = sGO.GetComponent<RectTransform>();
            srt.anchorMin = anchorMin; srt.anchorMax = anchorMax;
            srt.anchoredPosition = pos; srt.sizeDelta = size;

            Slider slider = sGO.AddComponent<Slider>();

            // Background
            GameObject bgGO = new GameObject("Background", typeof(RectTransform));
            bgGO.transform.SetParent(sGO.transform, false);
            RectTransform bgrt = bgGO.GetComponent<RectTransform>();
            bgrt.anchorMin = Vector2.zero; bgrt.anchorMax = Vector2.one; bgrt.sizeDelta = Vector2.zero;
            Image bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0.12f, 0.12f, 0.15f, 0.9f);

            // Fill Area
            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sGO.transform, false);
            RectTransform fart = fillArea.GetComponent<RectTransform>();
            fart.anchorMin = Vector2.zero; fart.anchorMax = Vector2.one; fart.sizeDelta = Vector2.zero;

            GameObject fill = new GameObject("Fill", typeof(RectTransform));
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform frt = fill.GetComponent<RectTransform>();
            frt.sizeDelta = Vector2.zero;
            Image fillImg = fill.AddComponent<Image>();
            if (fillSprite != null) { fillImg.sprite = fillSprite; fillImg.type = Image.Type.Sliced; }
            fillImg.color = fillColor;

            slider.fillRect = frt;
            slider.targetGraphic = fillImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 100f;
            slider.value = 100f;

            return sGO;
        }

        private static Text GetOrCreateText(Transform parent, string name, string defaultContent, int fontSize, Font font, Color color, Vector2 pos, Vector2 size, Vector2 anchorMin, Vector2 anchorMax)
        {
            Transform existing = parent.Find(name);
            GameObject textGO = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
            textGO.transform.SetParent(parent, false);

            RectTransform rt = textGO.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.anchoredPosition = pos; rt.sizeDelta = size;

            Text t = textGO.GetComponent<Text>() ?? textGO.AddComponent<Text>();
            t.font = font;
            t.fontSize = fontSize;
            t.color = color;
            t.text = defaultContent;
            return t;
        }

        private static GameObject CreateButtonGO(string name, Transform parent, Vector2 pos, Vector2 size, Sprite btnSprite, string btnLabel, Font font)
        {
            GameObject btnGO = new GameObject(name, typeof(RectTransform));
            btnGO.transform.SetParent(parent, false);
            RectTransform rt = btnGO.GetComponent<RectTransform>();
            rt.anchoredPosition = pos; rt.sizeDelta = size;

            Image img = btnGO.AddComponent<Image>();
            if (btnSprite != null) { img.sprite = btnSprite; img.type = Image.Type.Sliced; }
            else img.color = new Color(0.25f, 0.6f, 0.35f);

            Button btn = btnGO.AddComponent<Button>();

            GameObject textGO = new GameObject("Text", typeof(RectTransform));
            textGO.transform.SetParent(btnGO.transform, false);
            RectTransform trt = textGO.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.sizeDelta = Vector2.zero;

            Text t = textGO.AddComponent<Text>();
            t.font = font;
            t.fontSize = 14;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            t.text = btnLabel;

            return btnGO;
        }
    }
}
#endif
