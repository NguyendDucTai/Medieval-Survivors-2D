#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace VampireSurvivors.EditorTools
{
    [InitializeOnLoad]
#pragma warning disable 0618
    public class SpriteSheetSlicer
    {
        static SpriteSheetSlicer()
        {
            EditorApplication.delayCall += AutoSliceSheets;
        }

        [MenuItem("Tools/Slice & Setup All SpriteSheets")]
        public static void SliceAndSetupAll()
        {
            // 1. Characters (Knight, Mage, Ranger)
            SliceSheet("Assets/Sprites/Characters/Hero_Knight_Idle.png", new string[] { "Hero_Knight_Idle_0", "Hero_Knight_Idle_1" });
            SliceSheet("Assets/Sprites/Characters/Hero_Knight_Run.png", new string[] { "Hero_Knight_Run_0", "Hero_Knight_Run_1", "Hero_Knight_Run_2", "Hero_Knight_Run_3" });
            SliceSheet("Assets/Sprites/Characters/Hero_Knight_Die.png", new string[] { "Hero_Knight_Die_0", "Hero_Knight_Die_1", "Hero_Knight_Die_2", "Hero_Knight_Die_3" });

            SliceSheet("Assets/Sprites/Characters/Hero_Mage_Idle.png", new string[] { "Hero_Mage_Idle_0", "Hero_Mage_Idle_1" });
            SliceSheet("Assets/Sprites/Characters/Hero_Mage_Run.png", new string[] { "Hero_Mage_Run_0", "Hero_Mage_Run_1", "Hero_Mage_Run_2", "Hero_Mage_Run_3" });
            SliceSheet("Assets/Sprites/Characters/Hero_Mage_Die.png", new string[] { "Hero_Mage_Die_0", "Hero_Mage_Die_1", "Hero_Mage_Die_2", "Hero_Mage_Die_3" });

            SliceSheet("Assets/Sprites/Characters/Hero_Ranger_Idle.png", new string[] { "Hero_Ranger_Idle_0", "Hero_Ranger_Idle_1" });
            SliceSheet("Assets/Sprites/Characters/Hero_Ranger_Run.png", new string[] { "Hero_Ranger_Run_0", "Hero_Ranger_Run_1", "Hero_Ranger_Run_2", "Hero_Ranger_Run_3" });
            SliceSheet("Assets/Sprites/Characters/Hero_Ranger_Die.png", new string[] { "Hero_Ranger_Die_0", "Hero_Ranger_Die_1", "Hero_Ranger_Die_2", "Hero_Ranger_Die_3" });

            // ShadowKnight (Rogue 64x64 frames, ppu 48)
            SliceSheet("Assets/Sprites/Characters/Hero_Rogue_Idle.png", new string[] { "Hero_Rogue_Idle_0", "Hero_Rogue_Idle_1", "Hero_Rogue_Idle_2", "Hero_Rogue_Idle_3" }, 64, 64, 48);
            SliceSheet("Assets/Sprites/Characters/Hero_Rogue_Run.png", new string[] { "Hero_Rogue_Run_0", "Hero_Rogue_Run_1", "Hero_Rogue_Run_2", "Hero_Rogue_Run_3" }, 64, 64, 48);
            SliceSheet("Assets/Sprites/Characters/Hero_Rogue_Die.png", new string[] { "Hero_Rogue_Die_0", "Hero_Rogue_Die_1", "Hero_Rogue_Die_2", "Hero_Rogue_Die_3" }, 64, 64, 48);

            // Archmage (Grand Mage: Idle 2 frames, Run 4 frames, Die 4 frames, 64x64, ppu 48)
            SliceSheet("Assets/Sprites/Characters/Hero_Archmage_Idle.png", new string[] { "Hero_Archmage_Idle_0", "Hero_Archmage_Idle_1" }, 64, 64, 48);
            SliceSheet("Assets/Sprites/Characters/Hero_Archmage_Run.png", new string[] { "Hero_Archmage_Run_0", "Hero_Archmage_Run_1", "Hero_Archmage_Run_2", "Hero_Archmage_Run_3" }, 64, 64, 48);
            SliceSheet("Assets/Sprites/Characters/Hero_Archmage_Die.png", new string[] { "Hero_Archmage_Die_0", "Hero_Archmage_Die_1", "Hero_Archmage_Die_2", "Hero_Archmage_Die_3" }, 64, 64, 48);

            // Archer (Marksman: Idle 4 frames, Run 4 frames, Die 4 frames, 64x64, ppu 48)
            SliceSheet("Assets/Sprites/Characters/Hero_Archer_Idle.png", new string[] { "Hero_Archer_Idle_0", "Hero_Archer_Idle_1", "Hero_Archer_Idle_2", "Hero_Archer_Idle_3" }, 64, 64, 48);
            SliceSheet("Assets/Sprites/Characters/Hero_Archer_Run.png", new string[] { "Hero_Archer_Run_0", "Hero_Archer_Run_1", "Hero_Archer_Run_2", "Hero_Archer_Run_3" }, 64, 64, 48);
            SliceSheet("Assets/Sprites/Characters/Hero_Archer_Die.png", new string[] { "Hero_Archer_Die_0", "Hero_Archer_Die_1", "Hero_Archer_Die_2", "Hero_Archer_Die_3" }, 64, 64, 48);

            // 2. Enemies (Slime, Skeleton, Goblin, Bat, Boss Golem)
            SliceSheet("Assets/Sprites/Enemies/Slime_Hop.png", new string[] { "Slime_Hop_0", "Slime_Hop_1", "Slime_Hop_2", "Slime_Hop_3" });
            SliceSheet("Assets/Sprites/Enemies/Slime_Attack.png", new string[] { "Slime_Attack_0", "Slime_Attack_1", "Slime_Attack_2", "Slime_Attack_3" });
            SliceSheet("Assets/Sprites/Enemies/Slime_Die.png", new string[] { "Slime_Die_0", "Slime_Die_1", "Slime_Die_2", "Slime_Die_3" });

            SliceSheet("Assets/Sprites/Enemies/Skeleton_Walk.png", new string[] { "Skeleton_Walk_0", "Skeleton_Walk_1", "Skeleton_Walk_2", "Skeleton_Walk_3" });
            SliceSheet("Assets/Sprites/Enemies/Skeleton_Attack.png", new string[] { "Skeleton_Attack_0", "Skeleton_Attack_1", "Skeleton_Attack_2", "Skeleton_Attack_3" });
            SliceSheet("Assets/Sprites/Enemies/Skeleton_Die.png", new string[] { "Skeleton_Die_0", "Skeleton_Die_1", "Skeleton_Die_2", "Skeleton_Die_3" });

            SliceSheet("Assets/Sprites/Enemies/Goblin_Walk.png", new string[] { "Goblin_Walk_0", "Goblin_Walk_1", "Goblin_Walk_2", "Goblin_Walk_3" });
            SliceSheet("Assets/Sprites/Enemies/Goblin_Attack.png", new string[] { "Goblin_Attack_0", "Goblin_Attack_1", "Goblin_Attack_2", "Goblin_Attack_3" });
            SliceSheet("Assets/Sprites/Enemies/Goblin_Die.png", new string[] { "Goblin_Die_0", "Goblin_Die_1", "Goblin_Die_2", "Goblin_Die_3" });

            SliceSheet("Assets/Sprites/Enemies/Bat_Fly.png", new string[] { "Bat_Fly_0", "Bat_Fly_1", "Bat_Fly_2", "Bat_Fly_3" });
            SliceSheet("Assets/Sprites/Enemies/Bat_Attack.png", new string[] { "Bat_Attack_0", "Bat_Attack_1", "Bat_Attack_2", "Bat_Attack_3" });
            SliceSheet("Assets/Sprites/Enemies/Bat_Die.png", new string[] { "Bat_Die_0", "Bat_Die_1", "Bat_Die_2", "Bat_Die_3" });

            SliceSheet("Assets/Sprites/Enemies/Boss_Golem_Walk.png", new string[] { "Boss_Golem_Walk_0", "Boss_Golem_Walk_1", "Boss_Golem_Walk_2", "Boss_Golem_Walk_3" });
            SliceSheet("Assets/Sprites/Enemies/Boss_Golem_Attack.png", new string[] { "Boss_Golem_Attack_0", "Boss_Golem_Attack_1", "Boss_Golem_Attack_2", "Boss_Golem_Attack_3" });
            SliceSheet("Assets/Sprites/Enemies/Boss_Golem_Die.png", new string[] { "Boss_Golem_Die_0", "Boss_Golem_Die_1", "Boss_Golem_Die_2", "Boss_Golem_Die_3" });

            // 3. Update All Animation Clips and Controllers
            UpdateAllAnimationClipsAndControllers();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("ĐÃ CẮT VÀ GỘP TOÀN BỘ HOẠT ẢNH CHARACTERS & ENEMIES THÀNH CÔNG!");
        }

        private static void AutoSliceSheets()
        {
            string[] testPaths = new string[] {
                "Assets/Sprites/Characters/Hero_Knight_Die.png",
                "Assets/Sprites/Characters/Hero_Rogue_Idle.png",
                "Assets/Sprites/Characters/Hero_Archmage_Idle.png",
                "Assets/Sprites/Characters/Hero_Archer_Idle.png",
                "Assets/Sprites/Enemies/Skeleton_Walk.png",
                "Assets/Sprites/Enemies/Boss_Golem_Attack.png"
            };

            bool needSlice = false;
            foreach (var p in testPaths)
            {
                if (File.Exists(p))
                {
                    TextureImporter ti = AssetImporter.GetAtPath(p) as TextureImporter;
                    if (ti != null && ti.spriteImportMode != SpriteImportMode.Multiple)
                    {
                        needSlice = true;
                        break;
                    }
                }
            }

            if (needSlice)
            {
                SliceAndSetupAll();
            }
        }

        private static void SliceSheet(string path, string[] frameNames, int frameWidth = 16, int frameHeight = 16, int ppu = 16)
        {
            if (!File.Exists(path)) return;

            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) return;

            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Multiple;
            ti.spritePixelsPerUnit = ppu;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;

            SpriteMetaData[] sheet = new SpriteMetaData[frameNames.Length];
            for (int i = 0; i < frameNames.Length; i++)
            {
                sheet[i] = new SpriteMetaData
                {
                    name = frameNames[i],
                    rect = new Rect(i * frameWidth, 0, frameWidth, frameHeight),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                };
            }

            ti.spritesheet = sheet;
            EditorUtility.SetDirty(ti);
            ti.SaveAndReimport();
        }

        public static void UpdateAllAnimationClipsAndControllers()
        {
            // --- CHARACTERS ---
            UpdateClipsForCharacter("Knight", "Assets/Sprites/Characters/Hero_Knight_Idle.png", "Assets/Sprites/Characters/Hero_Knight_Run.png", "Assets/Sprites/Characters/Hero_Knight_Die.png");
            UpdateClipsForCharacter("Mage", "Assets/Sprites/Characters/Hero_Mage_Idle.png", "Assets/Sprites/Characters/Hero_Mage_Run.png", "Assets/Sprites/Characters/Hero_Mage_Die.png");
            UpdateClipsForCharacter("Ranger", "Assets/Sprites/Characters/Hero_Ranger_Idle.png", "Assets/Sprites/Characters/Hero_Ranger_Run.png", "Assets/Sprites/Characters/Hero_Ranger_Die.png");
            UpdateClipsForCharacter("ShadowKnight", "Assets/Sprites/Characters/Hero_Rogue_Idle.png", "Assets/Sprites/Characters/Hero_Rogue_Run.png", "Assets/Sprites/Characters/Hero_Rogue_Die.png");
            UpdateClipsForCharacter("Archmage", "Assets/Sprites/Characters/Hero_Archmage_Idle.png", "Assets/Sprites/Characters/Hero_Archmage_Run.png", "Assets/Sprites/Characters/Hero_Archmage_Die.png");
            UpdateClipsForCharacter("Archer", "Assets/Sprites/Characters/Hero_Archer_Idle.png", "Assets/Sprites/Characters/Hero_Archer_Run.png", "Assets/Sprites/Characters/Hero_Archer_Die.png");

            // --- ENEMIES ---
            UpdateClipsForEnemy("Slime", "Assets/Sprites/Enemies/Slime_Hop.png", "Assets/Sprites/Enemies/Slime_Attack.png", "Assets/Sprites/Enemies/Slime_Die.png", 6f, 6f, 6f);
            UpdateClipsForEnemy("Skeleton", "Assets/Sprites/Enemies/Skeleton_Walk.png", "Assets/Sprites/Enemies/Skeleton_Attack.png", "Assets/Sprites/Enemies/Skeleton_Die.png", 6f, 8f, 6f);
            UpdateClipsForEnemy("Goblin", "Assets/Sprites/Enemies/Goblin_Walk.png", "Assets/Sprites/Enemies/Goblin_Attack.png", "Assets/Sprites/Enemies/Goblin_Die.png", 6f, 8f, 6f);
            UpdateClipsForEnemy("Bat", "Assets/Sprites/Enemies/Bat_Fly.png", "Assets/Sprites/Enemies/Bat_Attack.png", "Assets/Sprites/Enemies/Bat_Die.png", 8f, 8f, 6f);
            UpdateClipsForEnemy("Boss_Golem", "Assets/Sprites/Enemies/Boss_Golem_Walk.png", "Assets/Sprites/Enemies/Boss_Golem_Attack.png", "Assets/Sprites/Enemies/Boss_Golem_Die.png", 4f, 6f, 5f);
        }

        private static void UpdateClipsForCharacter(string charName, string idlePath, string runPath, string diePath)
        {
            string idleClipPath = $"Assets/Animations/Characters/{charName}_Idle.anim";
            string runClipPath = $"Assets/Animations/Characters/{charName}_Run.anim";
            string dieClipPath = $"Assets/Animations/Characters/{charName}_Die.anim";
            string controllerPath = $"Assets/Animations/Characters/{charName}_Controller.controller";

            // 1. Idle Sprites (2 frames, loop)
            Sprite[] idleSprites = LoadSprites(idlePath);
            if (idleSprites.Length >= 2) UpdateClip(idleClipPath, idleSprites, 3f, true);

            // 2. Run Sprites (4 frames, loop)
            Sprite[] runSprites = LoadSprites(runPath);
            if (runSprites.Length >= 4) UpdateClip(runClipPath, runSprites, 8f, true);

            // 3. Die Sprites (4 frames, no loop)
            Sprite[] dieSprites = LoadSprites(diePath);
            if (dieSprites.Length >= 4) UpdateClip(dieClipPath, dieSprites, 5f, false);

            // 4. Controller
            EnsureCharacterAnimatorController(controllerPath, idleClipPath, runClipPath, dieClipPath);
        }

        private static void UpdateClipsForEnemy(string enemyName, string movePath, string attackPath, string diePath, float moveFps, float atkFps, float dieFps)
        {
            string moveClipPath = enemyName == "Slime" ? "Assets/Animations/Enemies/Slime_Hop.anim" : (enemyName == "Bat" ? "Assets/Animations/Enemies/Bat_Fly.anim" : $"Assets/Animations/Enemies/{enemyName}_Walk.anim");
            string attackClipPath = $"Assets/Animations/Enemies/{enemyName}_Attack.anim";
            string dieClipPath = $"Assets/Animations/Enemies/{enemyName}_Die.anim";
            string controllerPath = $"Assets/Animations/Enemies/{enemyName}_Controller.controller";

            Sprite[] moveSprites = LoadSprites(movePath);
            if (moveSprites.Length >= 4) UpdateClip(moveClipPath, moveSprites, moveFps, true);

            Sprite[] atkSprites = LoadSprites(attackPath);
            if (atkSprites.Length >= 4) UpdateClip(attackClipPath, atkSprites, atkFps, false);

            Sprite[] dieSprites = LoadSprites(diePath);
            if (dieSprites.Length >= 4) UpdateClip(dieClipPath, dieSprites, dieFps, false);

            EnsureEnemyAnimatorController(controllerPath, moveClipPath, attackClipPath, dieClipPath);
        }

        private static Sprite[] LoadSprites(string path)
        {
            return AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<Sprite>()
                .OrderBy(s => s.name)
                .ToArray();
        }

        private static void UpdateClip(string clipPath, Sprite[] sprites, float fps, bool loop)
        {
            string dir = Path.GetDirectoryName(clipPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, clipPath);
            }

            clip.frameRate = fps;

            EditorCurveBinding binding = new EditorCurveBinding();
            binding.type = typeof(SpriteRenderer);
            binding.path = "";
            binding.propertyName = "m_Sprite";

            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Length];
            for (int i = 0; i < sprites.Length; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i / fps,
                    value = sprites[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            EditorUtility.SetDirty(clip);
        }

        private static void EnsureCharacterAnimatorController(string controllerPath, string idleClipPath, string runClipPath, string dieClipPath)
        {
            string dir = Path.GetDirectoryName(controllerPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            }

            if (!controller.parameters.Any(p => p.name == "IsRunning"))
                controller.AddParameter("IsRunning", AnimatorControllerParameterType.Bool);

            if (!controller.parameters.Any(p => p.name == "Die"))
                controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);

            AnimationClip idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(idleClipPath);
            AnimationClip runClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(runClipPath);
            AnimationClip dieClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(dieClipPath);

            var rootStateMachine = controller.layers[0].stateMachine;

            var idleState = rootStateMachine.states.FirstOrDefault(s => s.state.name == "Idle").state;
            if (idleState == null) idleState = rootStateMachine.AddState("Idle");
            idleState.motion = idleClip;
            rootStateMachine.defaultState = idleState;

            var runState = rootStateMachine.states.FirstOrDefault(s => s.state.name == "Run").state;
            if (runState == null) runState = rootStateMachine.AddState("Run");
            runState.motion = runClip;

            var dieState = rootStateMachine.states.FirstOrDefault(s => s.state.name == "Die").state;
            if (dieState == null) dieState = rootStateMachine.AddState("Die");
            dieState.motion = dieClip;

            // Idle <-> Run
            if (!idleState.transitions.Any(t => t.destinationState == runState))
            {
                var toRun = idleState.AddTransition(runState);
                toRun.AddCondition(AnimatorConditionMode.If, 0, "IsRunning");
                toRun.duration = 0f;
                toRun.hasExitTime = false;
            }

            if (!runState.transitions.Any(t => t.destinationState == idleState))
            {
                var toIdle = runState.AddTransition(idleState);
                toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsRunning");
                toIdle.duration = 0f;
                toIdle.hasExitTime = false;
            }

            // AnyState -> Die
            if (!rootStateMachine.anyStateTransitions.Any(t => t.destinationState == dieState))
            {
                var toDie = rootStateMachine.AddAnyStateTransition(dieState);
                toDie.AddCondition(AnimatorConditionMode.If, 0, "Die");
                toDie.duration = 0f;
                toDie.hasExitTime = false;
                toDie.canTransitionToSelf = false;
            }

            EditorUtility.SetDirty(controller);
        }

        private static void EnsureEnemyAnimatorController(string controllerPath, string moveClipPath, string atkClipPath, string dieClipPath)
        {
            string dir = Path.GetDirectoryName(controllerPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            }

            if (!controller.parameters.Any(p => p.name == "Attack"))
                controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);

            if (!controller.parameters.Any(p => p.name == "Die"))
                controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);

            AnimationClip moveClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(moveClipPath);
            AnimationClip atkClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(atkClipPath);
            AnimationClip dieClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(dieClipPath);

            var rootStateMachine = controller.layers[0].stateMachine;

            var moveState = rootStateMachine.states.FirstOrDefault(s => s.state.name == "Move" || s.state.name == "Hop").state;
            if (moveState == null) moveState = rootStateMachine.AddState("Move");
            moveState.name = "Move";
            moveState.motion = moveClip;
            rootStateMachine.defaultState = moveState;

            var atkState = rootStateMachine.states.FirstOrDefault(s => s.state.name == "Attack").state;
            if (atkState == null) atkState = rootStateMachine.AddState("Attack");
            atkState.motion = atkClip;

            var dieState = rootStateMachine.states.FirstOrDefault(s => s.state.name == "Die").state;
            if (dieState == null) dieState = rootStateMachine.AddState("Die");
            dieState.motion = dieClip;

            // Move -> Attack
            if (!moveState.transitions.Any(t => t.destinationState == atkState))
            {
                var toAtk = moveState.AddTransition(atkState);
                toAtk.AddCondition(AnimatorConditionMode.If, 0, "Attack");
                toAtk.duration = 0f;
                toAtk.hasExitTime = false;
            }

            // Attack -> Move
            if (!atkState.transitions.Any(t => t.destinationState == moveState))
            {
                var toMove = atkState.AddTransition(moveState);
                toMove.duration = 0f;
                toMove.hasExitTime = true;
                toMove.exitTime = 1f;
            }

            // AnyState -> Die
            if (!rootStateMachine.anyStateTransitions.Any(t => t.destinationState == dieState))
            {
                var toDie = rootStateMachine.AddAnyStateTransition(dieState);
                toDie.AddCondition(AnimatorConditionMode.If, 0, "Die");
                toDie.duration = 0f;
                toDie.hasExitTime = false;
                toDie.canTransitionToSelf = false;
            }
            EditorUtility.SetDirty(controller);
        }
    }
}
#endif
