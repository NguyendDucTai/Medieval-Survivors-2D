#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using VampireSurvivors.Player;
using VampireSurvivors.Enemies;
using VampireSurvivors.Weapons;
using VampireSurvivors.Collectibles;

namespace VampireSurvivors.EditorTools
{
    public class AutoPrefabSetup
    {
        [MenuItem("Tools/1. Open Main Game Scene (SampleScene)", false, 0)]
        public static void OpenMainGameScene()
        {
            if (UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
                Debug.Log("Opened SampleScene successfully! Hit Play (▶) to test!");
            }
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

            // 4. Background Grass Field
            GameObject bg = GameObject.Find("Environment_Background");
            if (bg == null)
            {
                bg = new GameObject("Environment_Background");
                SpriteRenderer bgSR = bg.AddComponent<SpriteRenderer>();
                Sprite grassSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/Grass_Tile.png");
                bgSR.sprite = grassSprite;
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

            // Attach Animator with Knight_Controller
            Animator anim = player.GetComponent<Animator>();
            if (anim == null) anim = player.AddComponent<Animator>();

            RuntimeAnimatorController knightCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Characters/Knight_Controller.controller");
            if (knightCtrl != null)
            {
                anim.runtimeAnimatorController = knightCtrl;
                Debug.Log("Assigned Knight_Controller to Player Animator!");
            }
            else
            {
                Debug.LogError("Knight_Controller.controller not found at Assets/Animations/Characters/Knight_Controller.controller!");
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

        [MenuItem("Tools/Switch Hero/Knight")]
        public static void SwitchToKnight()
        {
            SwitchPlayerCharacter("Assets/Animations/Characters/Knight_Controller.controller", "Assets/Sprites/Characters/Hero_Knight_Idle.png", "Knight");
        }

        [MenuItem("Tools/Switch Hero/Mage")]
        public static void SwitchToMage()
        {
            SwitchPlayerCharacter("Assets/Animations/Characters/Mage_Controller.controller", "Assets/Sprites/Characters/Hero_Mage_Idle.png", "Mage");
        }

        [MenuItem("Tools/Switch Hero/Ranger")]
        public static void SwitchToRanger()
        {
            SwitchPlayerCharacter("Assets/Animations/Characters/Ranger_Controller.controller", "Assets/Sprites/Characters/Hero_Ranger_Idle.png", "Ranger");
        }

        private static void SwitchPlayerCharacter(string controllerPath, string idleSpriteSheetPath, string characterName)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) player = GameObject.Find("Player");
            if (player == null)
            {
                Debug.LogError("Player GameObject not found in Scene!");
                return;
            }

            Animator anim = player.GetComponent<Animator>();
            if (anim == null) anim = player.AddComponent<Animator>();

            RuntimeAnimatorController ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
            if (ctrl != null) anim.runtimeAnimatorController = ctrl;

            SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Sprite firstSprite = AssetDatabase.LoadAllAssetsAtPath(idleSpriteSheetPath).OfType<Sprite>().FirstOrDefault();
                if (firstSprite != null) sr.sprite = firstSprite;
            }

            EditorUtility.SetDirty(player);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );

            Selection.activeGameObject = player;
            Debug.Log($"Switched character to {characterName} with Idle and Run animations!");
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

            // 3. Enemy Slime Prefab
            GameObject slimePrefab = CreateEnemyPrefab("Enemy_Slime", "Assets/Sprites/Enemies/Slime_Green.png", "Assets/Animations/Enemies/Slime_Controller.controller", gemPrefab, 20f, 2.2f, 8f);

            // 4. Enemy Skeleton Prefab
            GameObject skelPrefab = CreateEnemyPrefab("Enemy_Skeleton", "Assets/Sprites/Enemies/Skeleton_White.png", "Assets/Animations/Enemies/Skeleton_Controller.controller", gemPrefab, 35f, 2.8f, 12f);

            // 5. Enemy Goblin Prefab
            GameObject goblinPrefab = CreateEnemyPrefab("Enemy_Goblin", "Assets/Sprites/Enemies/Goblin_Green.png", "Assets/Animations/Enemies/Goblin_Controller.controller", gemPrefab, 25f, 3.2f, 10f);

            // 6. Enemy Bat Prefab
            GameObject batPrefab = CreateEnemyPrefab("Enemy_Bat", "Assets/Sprites/Enemies/Bat_Fly.png", "Assets/Animations/Enemies/Bat_Controller.controller", gemPrefab, 15f, 3.5f, 6f);

            // 7. Enemy Boss Golem Prefab
            GameObject golemPrefab = CreateEnemyPrefab("Enemy_BossGolem", "Assets/Sprites/Enemies/Boss_Golem.png", "Assets/Animations/Enemies/Boss_Golem_Controller.controller", gemPrefab, 250f, 1.5f, 25f);

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
            weapon.weaponName = "Fireball";
            weapon.projectilePrefab = projPrefab;
            weapon.fireRate = 1.2f;
            weapon.attackRange = 9f;
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
                Sprite grassSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/Grass_Tile.png");
                bgSR.sprite = grassSprite;
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
    }
}
#endif
