# Script to generate/update all 5 enemy prefabs with their Animator controllers
$workspace = "c:\Users\nguye\Downloads\PRU213\Project-Pru213"

function Get-Guid-From-Meta {
    param($metaPath)
    if (Test-Path $metaPath) {
        $lines = Get-Content $metaPath -TotalCount 5
        foreach ($line in $lines) {
            if ($line -match "^guid:\s*([a-fA-F0-9]+)") {
                return $matches[1]
            }
        }
    }
    return [System.Guid]::NewGuid().ToString("N")
}

$enemyStatsScriptGuid = "2470f1a9b2b545f4fa81e4b3c75abdfa"
$enemyAiScriptGuid    = "d4cb46bcdfbb4ba4bbce5d5e973ebb9f"
$expGemPrefabGuid     = "c4da5eb207564d74baeaeb1c11068800"

function Generate-EnemyPrefab {
    param(
        [string]$prefabPath,
        [string]$prefabName,
        [string]$spriteTexMetaPath,
        [string]$ctrlMetaPath,
        [float]$hp,
        [float]$speed,
        [float]$damage,
        [float]$colRadius
    )

    $metaPath = "${prefabPath}.meta"
    $prefabGuid = Get-Guid-From-Meta $metaPath
    $texGuid = Get-Guid-From-Meta $spriteTexMetaPath
    $ctrlGuid = Get-Guid-From-Meta $ctrlMetaPath

    # Frame 0 id is 1213
    $spriteId = 1213

    # Unique 64-bit Unity FileIDs for components
    $goId    = [Math]::Abs([BitConverter]::ToInt64([Guid]::NewGuid().ToByteArray(), 0))
    $trId    = [Math]::Abs([BitConverter]::ToInt64([Guid]::NewGuid().ToByteArray(), 0))
    $srId    = [Math]::Abs([BitConverter]::ToInt64([Guid]::NewGuid().ToByteArray(), 0))
    $rbId    = [Math]::Abs([BitConverter]::ToInt64([Guid]::NewGuid().ToByteArray(), 0))
    $colId   = [Math]::Abs([BitConverter]::ToInt64([Guid]::NewGuid().ToByteArray(), 0))
    $statsId = [Math]::Abs([BitConverter]::ToInt64([Guid]::NewGuid().ToByteArray(), 0))
    $aiId    = [Math]::Abs([BitConverter]::ToInt64([Guid]::NewGuid().ToByteArray(), 0))
    $animId  = [Math]::Abs([BitConverter]::ToInt64([Guid]::NewGuid().ToByteArray(), 0))

    $hpStr = $hp.ToString("0.######", [System.Globalization.CultureInfo]::InvariantCulture)
    $speedStr = $speed.ToString("0.######", [System.Globalization.CultureInfo]::InvariantCulture)
    $dmgStr = $damage.ToString("0.######", [System.Globalization.CultureInfo]::InvariantCulture)
    $radStr = $colRadius.ToString("0.######", [System.Globalization.CultureInfo]::InvariantCulture)

    $yaml = @"
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &$goId
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: $trId}
  - component: {fileID: $srId}
  - component: {fileID: $rbId}
  - component: {fileID: $colId}
  - component: {fileID: $statsId}
  - component: {fileID: $aiId}
  - component: {fileID: $animId}
  m_Layer: 0
  m_Name: $prefabName
  m_TagString: Enemy
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &$trId
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: $goId}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 0}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!212 &$srId
SpriteRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: $goId}
  m_Enabled: 1
  m_CastShadows: 0
  m_ReceiveShadows: 0
  m_DynamicOccludee: 1
  m_StaticShadowCaster: 0
  m_MotionVectors: 1
  m_LightProbeUsage: 1
  m_ReflectionProbeUsage: 1
  m_RayTracingMode: 0
  m_RayTraceProcedural: 0
  m_RayTracingAccelStructBuildFlagsOverride: 0
  m_RayTracingAccelStructBuildFlags: 1
  m_SmallMeshCulling: 1
  m_RenderingLayerMask: 1
  m_RendererPriority: 0
  m_Materials:
  - {fileID: 10754, guid: 0000000000000000f000000000000000, type: 0}
  m_StaticBatchInfo:
    firstSubMesh: 0
    subMeshCount: 0
  m_StaticBatchRoot: {fileID: 0}
  m_ProbeAnchor: {fileID: 0}
  m_LightProbeVolumeOverride: {fileID: 0}
  m_ScaleInLightmap: 1
  m_ReceiveGI: 1
  m_PreserveUVs: 0
  m_IgnoreNormalsForChartDetection: 0
  m_ImportantGI: 0
  m_StitchLightmapSeams: 1
  m_SelectedEditorRenderState: 0
  m_MinimumChartSize: 4
  m_AutoUVMaxDistance: 0.5
  m_AutoUVMaxAngle: 89
  m_LightmapParameters: {fileID: 0}
  m_SortingLayerID: 0
  m_SortingLayer: 0
  m_SortingOrder: 2
  m_Sprite: {fileID: $spriteId, guid: $texGuid, type: 3}
  m_Color: {r: 1, g: 1, b: 1, a: 1}
  m_FlipX: 0
  m_FlipY: 0
  m_DrawMode: 0
  m_Size: {x: 1, y: 1}
  m_AdaptiveModeThreshold: 0.5
  m_SpriteTileMode: 0
  m_WasSpriteAssigned: 1
  m_MaskInteraction: 0
  m_SpriteSortPoint: 0
--- !u!50 &$rbId
Rigidbody2D:
  serializedVersion: 5
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: $goId}
  m_BodyType: 0
  m_Simulated: 1
  m_UseFullKinematicContacts: 0
  m_UseAutoMass: 0
  m_Mass: 1
  m_LinearDamping: 0
  m_AngularDamping: 0.05
  m_GravityScale: 0
  m_Material: {fileID: 0}
  m_IncludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_ExcludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_Interpolate: 0
  m_SleepingMode: 1
  m_CollisionDetection: 0
  m_Constraints: 4
--- !u!58 &$colId
CircleCollider2D:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: $goId}
  m_Enabled: 1
  m_Density: 1
  m_Material: {fileID: 0}
  m_IncludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_ExcludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_LayerOverridePriority: 0
  m_ForceSendLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_ForceReceiveLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_ContactCaptureLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_CallbackLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_IsTrigger: 0
  m_UsedByEffector: 0
  m_UsedByComposite: 0
  m_Offset: {x: 0, y: 0}
  serializedVersion: 2
  m_Radius: $radStr
--- !u!114 &$statsId
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: $goId}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: $enemyStatsScriptGuid, type: 3}
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::VampireSurvivors.Enemies.EnemyStats
  maxHealth: $hpStr
  currentHealth: $hpStr
  moveSpeed: $speedStr
  attackDamage: $dmgStr
  expValue: 10
  expGemPrefab: {fileID: 6023340057041731633, guid: $expGemPrefabGuid, type: 3}
--- !u!114 &$aiId
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: $goId}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: $enemyAiScriptGuid, type: 3}
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::VampireSurvivors.Enemies.EnemyAI
  attackCooldown: 0.8
--- !u!95 &$animId
Animator:
  serializedVersion: 7
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: $goId}
  m_Enabled: 1
  m_Avatar: {fileID: 0}
  m_Controller: {fileID: 9100000, guid: $ctrlGuid, type: 2}
  m_CullingMode: 0
  m_UpdateMode: 0
  m_ApplyRootMotion: 0
  m_LinearVelocityBlending: 0
  m_StabilizeFeet: 0
  m_AnimatePhysics: 0
  m_WarningMessage: 
  m_HasTransformHierarchy: 1
  m_AllowConstantClipSamplingOptimization: 1
  m_KeepAnimatorStateOnDisable: 0
  m_WriteDefaultValuesOnDisable: 0
"@

    [System.IO.File]::WriteAllText($prefabPath, $yaml)

    if (-not (Test-Path $metaPath)) {
        $metaYaml = @"
fileFormatVersion: 2
guid: $prefabGuid
PrefabImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"@
        [System.IO.File]::WriteAllText($metaPath, $metaYaml)
    }
    Write-Output "Generated Prefab: $prefabPath with Animator Controller: $ctrlGuid"
}

# Generate/Update All 5 Enemies:
Generate-EnemyPrefab "$workspace\Assets\Prefabs\Enemy_Slime.prefab" "Enemy_Slime" "$workspace\Assets\Sprites\Enemies\Slime_Hop.png.meta" "$workspace\Assets\Animations\Enemies\Slime_Controller.controller.meta" 20 2.2 8 0.45
Generate-EnemyPrefab "$workspace\Assets\Prefabs\Enemy_Skeleton.prefab" "Enemy_Skeleton" "$workspace\Assets\Sprites\Enemies\Skeleton_Walk.png.meta" "$workspace\Assets\Animations\Enemies\Skeleton_Controller.controller.meta" 35 2.8 12 0.45
Generate-EnemyPrefab "$workspace\Assets\Prefabs\Enemy_Goblin.prefab" "Enemy_Goblin" "$workspace\Assets\Sprites\Enemies\Goblin_Walk.png.meta" "$workspace\Assets\Animations\Enemies\Goblin_Controller.controller.meta" 25 3.2 10 0.45
Generate-EnemyPrefab "$workspace\Assets\Prefabs\Enemy_Bat.prefab" "Enemy_Bat" "$workspace\Assets\Sprites\Enemies\Bat_Fly.png.meta" "$workspace\Assets\Animations\Enemies\Bat_Controller.controller.meta" 15 3.5 6 0.4
Generate-EnemyPrefab "$workspace\Assets\Prefabs\Enemy_BossGolem.prefab" "Enemy_BossGolem" "$workspace\Assets\Sprites\Enemies\Boss_Golem_Walk.png.meta" "$workspace\Assets\Animations\Enemies\Boss_Golem_Controller.controller.meta" 250 1.5 25 0.55

Write-Output "ALL 5 ENEMY PREFABS FULLY GENERATED WITH ANIMATOR CONTROLLERS!"
