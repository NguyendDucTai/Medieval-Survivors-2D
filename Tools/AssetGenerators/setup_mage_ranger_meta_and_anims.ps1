# PowerShell script to create .meta, .anim, and .controller files for Mage and Ranger

$mageIdleGuid = "62a1b2c3d4e5f60718293a4b5c6d7e01"
$mageRunGuid  = "62a1b2c3d4e5f60718293a4b5c6d7e02"
$rangerIdleGuid = "73a1b2c3d4e5f60718293a4b5c6d7e01"
$rangerRunGuid  = "73a1b2c3d4e5f60718293a4b5c6d7e02"

$mageIdleAnimGuid = "62a1b2c3d4e5f60718293a4b5c6d7e11"
$mageRunAnimGuid  = "62a1b2c3d4e5f60718293a4b5c6d7e12"
$mageCtrlGuid     = "62a1b2c3d4e5f60718293a4b5c6d7e13"

$rangerIdleAnimGuid = "73a1b2c3d4e5f60718293a4b5c6d7e11"
$rangerRunAnimGuid  = "73a1b2c3d4e5f60718293a4b5c6d7e12"
$rangerCtrlGuid     = "73a1b2c3d4e5f60718293a4b5c6d7e13"

function Create-SpriteSheetMeta {
    param($metaPath, $guid, $prefix, [int]$frameCount)

    $idTable = ""
    $spriteBlocks = ""
    $nameIdMap = ""

    for ($i=0; $i -lt $frameCount; $i++) {
        $frameName = "${prefix}_${i}"
        $id = ($i + 1) * 1000 + 213
        $idTable += @"
  - first:
      213: $id
    second: $frameName

"@
        $x = $i * 16
        $spriteBlocks += @"
    - serializedVersion: 2
      name: $frameName
      rect:
        serializedVersion: 2
        x: $x
        y: 0
        width: 16
        height: 16
      alignment: 0
      pivot: {x: 0.5, y: 0.5}
      border: {x: 0, y: 0, z: 0, w: 0}
      customData: 
      outline: []
      physicsShape: []
      tessellationDetail: -1
      bones: []
      spriteID: 00000000000000000000000000000000
      internalID: $id
      vertices: []
      indices: 
      edges: []
      weights: []

"@
        $nameIdMap += "      ${frameName}: $id`n"
    }

    $content = @"
fileFormatVersion: 2
guid: $guid
TextureImporter:
  internalIDToNameTable:
$idTable  externalObjects: {}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 2
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {x: 0.5, y: 0.5}
  spritePixelsToUnits: 16
  spriteBorder: {x: 0, y: 0, z: 0, w: 0}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 0
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites:
$spriteBlocks    outline: []
    customData: 
    physicsShape: []
    bones: []
    spriteID: 
    internalID: 0
    vertices: []
    indices: 
    edges: []
    weights: []
    secondaryTextures: []
    spriteCustomMetadata:
      entries: []
    nameFileIdTable:
$nameIdMap  mipmapLimitGroupName: 
  pSDRemoveMatte: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"@
    [System.IO.File]::WriteAllText($metaPath, $content)
}

function Create-AnimationClip {
    param($animPath, $metaPath, $animName, $textureGuid, [int]$frameCount, [float]$fps)

    $curveKeys = ""
    $pptrMap = ""

    for ($i=0; $i -lt $frameCount; $i++) {
        $id = ($i + 1) * 1000 + 213
        $time = $i / $fps
        $timeStr = $time.ToString("0.######", [System.Globalization.CultureInfo]::InvariantCulture)
        $curveKeys += @"
    - time: $timeStr
      value: {fileID: $id, guid: $textureGuid, type: 3}

"@
        $pptrMap += "    - {fileID: $id, guid: $textureGuid, type: 3}`n"
    }

    $stopTime = ($frameCount / $fps).ToString("0.######", [System.Globalization.CultureInfo]::InvariantCulture)

    $animContent = @"
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!74 &7400000
AnimationClip:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: $animName
  serializedVersion: 7
  m_Legacy: 0
  m_Compressed: 0
  m_UseHighQualityCurve: 1
  m_RotationCurves: []
  m_CompressedRotationCurves: []
  m_EulerCurves: []
  m_PositionCurves: []
  m_ScaleCurves: []
  m_FloatCurves: []
  m_PPtrCurves:
  - serializedVersion: 2
    curve:
$curveKeys    attribute: m_Sprite
    path: 
    classID: 212
    script: {fileID: 0}
    flags: 2
  m_SampleRate: $fps
  m_WrapMode: 0
  m_Bounds:
    m_Center: {x: 0, y: 0, z: 0}
    m_Extent: {x: 0, y: 0, z: 0}
  m_ClipBindingConstant:
    genericBindings:
    - serializedVersion: 2
      path: 0
      attribute: 0
      script: {fileID: 0}
      typeID: 212
      customType: 23
      isPPtrCurve: 1
      isIntCurve: 0
      isSerializeReferenceCurve: 0
    pptrCurveMapping:
$pptrMap  m_AnimationClipSettings:
    serializedVersion: 2
    m_AdditiveReferencePoseClip: {fileID: 0}
    m_AdditiveReferencePoseTime: 0
    m_StartTime: 0
    m_StopTime: $stopTime
    m_OrientationOffsetY: 0
    m_Level: 0
    m_CycleOffset: 0
    m_HasAdditiveReferencePose: 0
    m_LoopTime: 1
    m_LoopBlend: 0
    m_LoopBlendOrientation: 0
    m_LoopBlendPositionY: 0
    m_LoopBlendPositionXZ: 0
    m_KeepOriginalOrientation: 0
    m_KeepOriginalPositionY: 1
    m_KeepOriginalPositionXZ: 0
    m_HeightFromFeet: 0
    m_Mirror: 0
  m_EditorCurves: []
  m_EulerEditorCurves: []
  m_HasGenericRootTransform: 0
  m_HasMotionFloatCurves: 0
  m_Events: []
"@
    [System.IO.File]::WriteAllText($animPath, $animContent)

    $animMeta = @"
fileFormatVersion: 2
guid: $([System.Guid]::NewGuid().ToString("N"))
NativeFormatImporter:
  externalObjects: {}
  mainObjectFileID: 7400000
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"@
    [System.IO.File]::WriteAllText($metaPath, $animMeta)
}

function Create-Controller {
    param($ctrlPath, $metaPath, $ctrlName, $idleAnimGuid, $runAnimGuid)

    $content = @"
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1101 &-4344036819405596669
AnimatorStateTransition:
  m_ObjectHideFlags: 1
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: 
  m_Conditions:
  - m_ConditionMode: 1
    m_ConditionEvent: IsRunning
    m_EventTreshold: 0
  m_DstStateMachine: {fileID: 0}
  m_DstState: {fileID: 3253218429818599208}
  m_Solo: 0
  m_Mute: 0
  m_IsExit: 0
  serializedVersion: 3
  m_TransitionDuration: 0
  m_TransitionOffset: 0
  m_ExitTime: 0.9
  m_HasExitTime: 0
  m_HasFixedDuration: 1
  m_InterruptionSource: 0
  m_OrderedInterruption: 1
  m_CanTransitionToSelf: 1
--- !u!1107 &-4104323630627263679
AnimatorStateMachine:
  serializedVersion: 7
  m_ObjectHideFlags: 1
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: Base Layer
  m_ChildStates:
  - serializedVersion: 1
    m_State: {fileID: 4769346340342722103}
    m_Position: {x: 200, y: 0, z: 0}
  - serializedVersion: 1
    m_State: {fileID: 3253218429818599208}
    m_Position: {x: 235, y: 65, z: 0}
  m_ChildStateMachines: []
  m_AnyStateTransitions: []
  m_EntryTransitions: []
  m_StateMachineTransitions: {}
  m_StateMachineBehaviours: []
  m_AnyStatePosition: {x: 50, y: 20, z: 0}
  m_EntryPosition: {x: 50, y: 120, z: 0}
  m_ExitPosition: {x: 800, y: 120, z: 0}
  m_ParentStateMachinePosition: {x: 800, y: 20, z: 0}
  m_DefaultState: {fileID: 4769346340342722103}
--- !u!1101 &-1969600490433120663
AnimatorStateTransition:
  m_ObjectHideFlags: 1
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: 
  m_Conditions:
  - m_ConditionMode: 2
    m_ConditionEvent: IsRunning
    m_EventTreshold: 0
  m_DstStateMachine: {fileID: 0}
  m_DstState: {fileID: 4769346340342722103}
  m_Solo: 0
  m_Mute: 0
  m_IsExit: 0
  serializedVersion: 3
  m_TransitionDuration: 0
  m_TransitionOffset: 0
  m_ExitTime: 0.9
  m_HasExitTime: 0
  m_HasFixedDuration: 1
  m_InterruptionSource: 0
  m_OrderedInterruption: 1
  m_CanTransitionToSelf: 1
--- !u!91 &9100000
AnimatorController:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: $ctrlName
  serializedVersion: 6
  m_AnimatorParameters:
  - m_Name: IsRunning
    m_Type: 4
    m_DefaultFloat: 0
    m_DefaultInt: 0
    m_DefaultBool: 0
    m_Controller: {fileID: 9100000}
  m_AnimatorLayers:
  - serializedVersion: 5
    m_Name: Base Layer
    m_StateMachine: {fileID: -4104323630627263679}
    m_Mask: {fileID: 0}
    m_Motions: []
    m_Behaviours: []
    m_BlendingMode: 0
    m_SyncedLayerIndex: -1
    m_DefaultWeight: 0
    m_IKPass: 0
    m_SyncedLayerAffectsTiming: 0
    m_Controller: {fileID: 9100000}
  m_EvaluateTransitionsOnStart: 1
--- !u!1102 &3253218429818599208
AnimatorState:
  serializedVersion: 6
  m_ObjectHideFlags: 1
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: Run
  m_Speed: 1
  m_CycleOffset: 0
  m_Transitions:
  - {fileID: -1969600490433120663}
  m_StateMachineBehaviours: []
  m_Position: {x: 50, y: 50, z: 0}
  m_IKOnFeet: 0
  m_WriteDefaultValues: 1
  m_Mirror: 0
  m_SpeedParameterActive: 0
  m_MirrorParameterActive: 0
  m_CycleOffsetParameterActive: 0
  m_TimeParameterActive: 0
  m_Motion: {fileID: 7400000, guid: $runAnimGuid, type: 2}
  m_Tag: 
  m_SpeedParameter: 
  m_MirrorParameter: 
  m_CycleOffsetParameter: 
  m_TimeParameter: 
--- !u!1102 &4769346340342722103
AnimatorState:
  serializedVersion: 6
  m_ObjectHideFlags: 1
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: Idle
  m_Speed: 1
  m_CycleOffset: 0
  m_Transitions:
  - {fileID: -4344036819405596669}
  m_StateMachineBehaviours: []
  m_Position: {x: 50, y: 50, z: 0}
  m_IKOnFeet: 0
  m_WriteDefaultValues: 1
  m_Mirror: 0
  m_SpeedParameterActive: 0
  m_MirrorParameterActive: 0
  m_CycleOffsetParameterActive: 0
  m_TimeParameterActive: 0
  m_Motion: {fileID: 7400000, guid: $idleAnimGuid, type: 2}
  m_Tag: 
  m_SpeedParameter: 
  m_MirrorParameter: 
  m_CycleOffsetParameter: 
  m_TimeParameter: 
"@
    [System.IO.File]::WriteAllText($ctrlPath, $content)

    $ctrlMeta = @"
fileFormatVersion: 2
guid: $([System.Guid]::NewGuid().ToString("N"))
NativeFormatImporter:
  externalObjects: {}
  mainObjectFileID: 9100000
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"@
    [System.IO.File]::WriteAllText($metaPath, $ctrlMeta)
}

# 1. SpriteSheet Metas
Create-SpriteSheetMeta "Assets/Sprites/Characters/Hero_Mage_Idle.png.meta" $mageIdleGuid "Hero_Mage_Idle" 2
Create-SpriteSheetMeta "Assets/Sprites/Characters/Hero_Mage_Run.png.meta" $mageRunGuid "Hero_Mage_Run" 4
Create-SpriteSheetMeta "Assets/Sprites/Characters/Hero_Ranger_Idle.png.meta" $rangerIdleGuid "Hero_Ranger_Idle" 2
Create-SpriteSheetMeta "Assets/Sprites/Characters/Hero_Ranger_Run.png.meta" $rangerRunGuid "Hero_Ranger_Run" 4
Write-Output "Created Meta files for Mage and Ranger SpriteSheets (SpriteMode: Multiple, PPU 16)"

# 2. Animation Clips
Create-AnimationClip "Assets/Animations/Characters/Mage_Idle.anim" "Assets/Animations/Characters/Mage_Idle.anim.meta" "Mage_Idle" $mageIdleGuid 2 3
Create-AnimationClip "Assets/Animations/Characters/Mage_Run.anim" "Assets/Animations/Characters/Mage_Run.anim.meta" "Mage_Run" $mageRunGuid 4 8

Create-AnimationClip "Assets/Animations/Characters/Ranger_Idle.anim" "Assets/Animations/Characters/Ranger_Idle.anim.meta" "Ranger_Idle" $rangerIdleGuid 2 3
Create-AnimationClip "Assets/Animations/Characters/Ranger_Run.anim" "Assets/Animations/Characters/Ranger_Run.anim.meta" "Ranger_Run" $rangerRunGuid 4 8
Write-Output "Created Mage and Ranger Animation Clips (Idle & Run)"

# Read actual GUIDs from generated anim metas
$mageIdleMeta = Get-Content "Assets/Animations/Characters/Mage_Idle.anim.meta" -Raw
$mageIdleMeta -match 'guid:\s*([a-f0-9]+)' | Out-Null
$actualMageIdleGuid = $matches[1]

$mageRunMeta = Get-Content "Assets/Animations/Characters/Mage_Run.anim.meta" -Raw
$mageRunMeta -match 'guid:\s*([a-f0-9]+)' | Out-Null
$actualMageRunGuid = $matches[1]

$rangerIdleMeta = Get-Content "Assets/Animations/Characters/Ranger_Idle.anim.meta" -Raw
$rangerIdleMeta -match 'guid:\s*([a-f0-9]+)' | Out-Null
$actualRangerIdleGuid = $matches[1]

$rangerRunMeta = Get-Content "Assets/Animations/Characters/Ranger_Run.anim.meta" -Raw
$rangerRunMeta -match 'guid:\s*([a-f0-9]+)' | Out-Null
$actualRangerRunGuid = $matches[1]

# 3. Controllers
Create-Controller "Assets/Animations/Characters/Mage_Controller.controller" "Assets/Animations/Characters/Mage_Controller.controller.meta" "Mage_Controller" $actualMageIdleGuid $actualMageRunGuid
Create-Controller "Assets/Animations/Characters/Ranger_Controller.controller" "Assets/Animations/Characters/Ranger_Controller.controller.meta" "Ranger_Controller" $actualRangerIdleGuid $actualRangerRunGuid
Write-Output "Created Mage_Controller and Ranger_Controller with IsRunning transitions"
