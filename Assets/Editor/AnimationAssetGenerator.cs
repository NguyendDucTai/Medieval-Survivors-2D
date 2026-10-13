#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace VampireSurvivors.EditorTools
{
    public class AnimationAssetGenerator
    {
        [MenuItem("Tools/Generate All Animations & Controllers")]
        public static void GenerateAnimations()
        {
            EnsureDirectory("Assets/Animations/Characters");
            EnsureDirectory("Assets/Animations/Enemies");

            // 1. Knight Idle Clip
            AnimationClip knightIdle = CreateSpriteAnimationClip(
                "Assets/Animations/Characters/Knight_Idle.anim",
                new string[] {
                    "Assets/Sprites/Characters/Hero_Knight_Idle_0.png",
                    "Assets/Sprites/Characters/Hero_Knight_Idle_1.png"
                },
                3f // 3 fps breathing
            );

            // 2. Knight Run Clip
            AnimationClip knightRun = CreateSpriteAnimationClip(
                "Assets/Animations/Characters/Knight_Run.anim",
                new string[] {
                    "Assets/Sprites/Characters/Hero_Knight_Run_0.png",
                    "Assets/Sprites/Characters/Hero_Knight_Run_1.png",
                    "Assets/Sprites/Characters/Hero_Knight_Run_2.png",
                    "Assets/Sprites/Characters/Hero_Knight_Run_3.png"
                },
                8f // 8 fps run cycle
            );

            // 3. Knight Animator Controller
            string knightControllerPath = "Assets/Animations/Characters/Knight_Controller.controller";
            AnimatorController knightController = AnimatorController.CreateAnimatorControllerAtPath(knightControllerPath);
            knightController.AddParameter("IsRunning", AnimatorControllerParameterType.Bool);

            var knightRoot = knightController.layers[0].stateMachine;
            var idleState = knightRoot.AddState("Idle");
            idleState.motion = knightIdle;
            knightRoot.defaultState = idleState;

            var runState = knightRoot.AddState("Run");
            runState.motion = knightRun;

            var toRun = idleState.AddTransition(runState);
            toRun.hasExitTime = false;
            toRun.duration = 0f;
            toRun.AddCondition(AnimatorConditionMode.If, 0, "IsRunning");

            var toIdle = runState.AddTransition(idleState);
            toIdle.hasExitTime = false;
            toIdle.duration = 0f;
            toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsRunning");

            // 4. Slime Hop Clip
            AnimationClip slimeHop = CreateSpriteAnimationClip(
                "Assets/Animations/Enemies/Slime_Hop.anim",
                new string[] {
                    "Assets/Sprites/Enemies/Slime_Hop_0.png",
                    "Assets/Sprites/Enemies/Slime_Hop_1.png",
                    "Assets/Sprites/Enemies/Slime_Hop_2.png",
                    "Assets/Sprites/Enemies/Slime_Hop_3.png"
                },
                6f // 6 fps slime hop
            );

            // 5. Slime Animator Controller
            string slimeControllerPath = "Assets/Animations/Enemies/Slime_Controller.controller";
            AnimatorController slimeController = AnimatorController.CreateAnimatorControllerAtPath(slimeControllerPath);
            var slimeRoot = slimeController.layers[0].stateMachine;
            var hopState = slimeRoot.AddState("Hop");
            hopState.motion = slimeHop;
            slimeRoot.defaultState = hopState;

            // 6. Assign Animator to Prefabs
            AssignControllerToPrefab("Assets/Prefabs/Player.prefab", knightController);
            AssignControllerToPrefab("Assets/Prefabs/Enemy_Slime.prefab", slimeController);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Successfully created all Frame-by-Frame Animations and Animator Controllers!");
        }

        private static AnimationClip CreateSpriteAnimationClip(string savePath, string[] spritePaths, float fps)
        {
            AnimationClip clip = new AnimationClip();
            clip.frameRate = fps;

            EditorCurveBinding binding = new EditorCurveBinding();
            binding.type = typeof(SpriteRenderer);
            binding.path = "";
            binding.propertyName = "m_Sprite";

            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[spritePaths.Length];
            for (int i = 0; i < spritePaths.Length; i++)
            {
                Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(spritePaths[i]);
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i / fps,
                    value = sp
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            AssetDatabase.CreateAsset(clip, savePath);
            return clip;
        }

        private static void AssignControllerToPrefab(string prefabPath, RuntimeAnimatorController controller)
        {
            if (!File.Exists(prefabPath)) return;
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                Animator anim = prefab.GetComponent<Animator>();
                if (anim == null) anim = prefab.AddComponent<Animator>();
                anim.runtimeAnimatorController = controller;
                EditorUtility.SetDirty(prefab);
            }
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }
    }
}
#endif

