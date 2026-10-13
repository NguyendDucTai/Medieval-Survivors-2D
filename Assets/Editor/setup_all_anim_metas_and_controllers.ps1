# Script to configure all spritesheets (.meta), animation clips (.anim), and animator controllers (.controller)
$workspace = "c:\Users\nguye\Downloads\PRU213\Project-Pru213"

function Get-Or-Create-Guid {
    param($metaPath)
    if (Test-Path $metaPath) {
        $content = Get-Content $metaPath -TotalCount 5
        foreach ($line in $content) {
            if ($line -match "^guid:\s*([a-fA-F0-9]+)") {
                return $matches[1]
            }
        }
    }
    return [System.Guid]::NewGuid().ToString("N")
}

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
    Write-Output "Configured Meta: $metaPath"
}

function Create-AnimationClip {
    param($animPath, $animMetaPath, $animName, $textureGuid, [int]$frameCount, [float]$fps, [bool]$loop)

    $loopVal = if ($loop) { 1 } else { 0 }
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
    m_LoopTime: $loopVal
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

    $guid = Get-Or-Create-Guid $animMetaPath
    $animMeta = @"
fileFormatVersion: 2
guid: $guid
NativeFormatImporter:
  externalObjects: {}
  mainObjectFileID: 7400000
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"@
    [System.IO.File]::WriteAllText($animMetaPath, $animMeta)
    Write-Output "Created Clip: $animPath (guid: $guid)"
    return $guid
}

function Create-EnemyController {
    param($ctrlPath, $ctrlMetaPath, $ctrlName, $moveAnimGuid, $atkAnimGuid, $dieAnimGuid)

    $guid = Get-Or-Create-Guid $ctrlMetaPath

    $content = @"
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1101 &-5001
AnimatorStateTransition:
  m_ObjectHideFlags: 1
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: 
  m_Conditions:
  - m_ConditionMode: 1
    m_ConditionEvent: Die
    m_EventTreshold: 0
  m_DstStateMachine: {fileID: 0}
  m_DstState: {fileID: -6003}
  m_Solo: 0
  m_Mute: 0
  m_IsExit: 0
  serializedVersion: 3
  m_TransitionDuration: 0
  m_TransitionOffset: 0
  m_ExitTime: 0.75
  m_HasExitTime: 0
  m_HasFixedDuration: 1
  m_InterruptionSource: 0
  m_OrderedInterruption: 1
  m_CanTransitionToSelf: 0
--- !u!1101 &-5002
AnimatorStateTransition:
  m_ObjectHideFlags: 1
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: 
  m_Conditions:
  - m_ConditionMode: 1
    m_ConditionEvent: Attack
    m_EventTreshold: 0
  m_DstStateMachine: {fileID: 0}
  m_DstState: {fileID: -6002}
  m_Solo: 0
  m_Mute: 0
  m_IsExit: 0
  serializedVersion: 3
  m_TransitionDuration: 0
  m_TransitionOffset: 0
  m_ExitTime: 0.75
  m_HasExitTime: 0
  m_HasFixedDuration: 1
  m_InterruptionSource: 0
  m_OrderedInterruption: 1
  m_CanTransitionToSelf: 1
--- !u!1101 &-5003
AnimatorStateTransition:
  m_ObjectHideFlags: 1
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: 
  m_Conditions: []
  m_DstStateMachine: {fileID: 0}
  m_DstState: {fileID: -6001}
  m_Solo: 0
  m_Mute: 0
  m_IsExit: 0
  serializedVersion: 3
  m_TransitionDuration: 0
  m_TransitionOffset: 0
  m_ExitTime: 1
  m_HasExitTime: 1
  m_HasFixedDuration: 1
  m_InterruptionSource: 0
  m_OrderedInterruption: 1
  m_CanTransitionToSelf: 1
--- !u!1102 &-6001
AnimatorState:
  serializedVersion: 6
  m_ObjectHideFlags: 1
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: Move
  m_Speed: 1
  m_CycleOffset: 0
  m_Transitions:
  - {fileID: -5002}
  m_StateMachineBehaviours: []
  m_Position: {x: 50, y: 50, z: 0}
  m_IKOnFeet: 0
  m_WriteDefaultValues: 1
  m_Mirror: 0
  m_SpeedParameterActive: 0
  m_MirrorParameterActive: 0
  m_CycleOffsetParameterActive: 0
  m_TimeParameterActive: 0
  m_Motion: {fileID: 7400000, guid: $moveAnimGuid, type: 2}
  m_Tag: 
  m_SpeedParameter: 
  m_MirrorParameter: 
  m_CycleOffsetParameter: 
  m_TimeParameter: 
--- !u!1102 &-6002
AnimatorState:
  serializedVersion: 6
  m_ObjectHideFlags: 1
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: Attack
  m_Speed: 1
  m_CycleOffset: 0
  m_Transitions:
  - {fileID: -5003}
  m_StateMachineBehaviours: []
  m_Position: {x: 50, y: 150, z: 0}
  m_IKOnFeet: 0
  m_WriteDefaultValues: 1
  m_Mirror: 0
  m_SpeedParameterActive: 0
  m_MirrorParameterActive: 0
  m_CycleOffsetParameterActive: 0
  m_TimeParameterActive: 0
  m_Motion: {fileID: 7400000, guid: $atkAnimGuid, type: 2}
  m_Tag: 
  m_SpeedParameter: 
  m_MirrorParameter: 
  m_CycleOffsetParameter: 
  m_TimeParameter: 
--- !u!1102 &-6003
AnimatorState:
  serializedVersion: 6
  m_ObjectHideFlags: 1
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: Die
  m_Speed: 1
  m_CycleOffset: 0
  m_Transitions: []
  m_StateMachineBehaviours: []
  m_Position: {x: 50, y: -80, z: 0}
  m_IKOnFeet: 0
  m_WriteDefaultValues: 1
  m_Mirror: 0
  m_SpeedParameterActive: 0
  m_MirrorParameterActive: 0
  m_CycleOffsetParameterActive: 0
  m_TimeParameterActive: 0
  m_Motion: {fileID: 7400000, guid: $dieAnimGuid, type: 2}
  m_Tag: 
  m_SpeedParameter: 
  m_MirrorParameter: 
  m_CycleOffsetParameter: 
  m_TimeParameter: 
--- !u!1107 &-7001
AnimatorStateMachine:
  serializedVersion: 7
  m_ObjectHideFlags: 1
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: Base Layer
  m_ChildStates:
  - serializedVersion: 1
    m_State: {fileID: -6001}
    m_Position: {x: 240, y: 50, z: 0}
  - serializedVersion: 1
    m_State: {fileID: -6002}
    m_Position: {x: 240, y: 150, z: 0}
  - serializedVersion: 1
    m_State: {fileID: -6003}
    m_Position: {x: 240, y: -80, z: 0}
  m_ChildStateMachines: []
  m_AnyStateTransitions:
  - {fileID: -5001}
  m_EntryTransitions: []
  m_StateMachineTransitions: {}
  m_StateMachineBehaviours: []
  m_AnyStatePosition: {x: -20, y: -80, z: 0}
  m_EntryPosition: {x: 50, y: 60, z: 0}
  m_ExitPosition: {x: 800, y: 120, z: 0}
  m_ParentStateMachinePosition: {x: 800, y: 20, z: 0}
  m_DefaultState: {fileID: -6001}
--- !u!91 &9100000
AnimatorController:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: $ctrlName
  serializedVersion: 6
  m_AnimatorParameters:
  - m_Name: Attack
    m_Type: 9
    m_DefaultFloat: 0
    m_DefaultInt: 0
    m_DefaultBool: 0
    m_Controller: {fileID: 9100000}
  - m_Name: Die
    m_Type: 9
    m_DefaultFloat: 0
    m_DefaultInt: 0
    m_DefaultBool: 0
    m_Controller: {fileID: 9100000}
  m_AnimatorLayers:
  - serializedVersion: 5
    m_Name: Base Layer
    m_StateMachine: {fileID: -7001}
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
"@
    [System.IO.File]::WriteAllText($ctrlPath, $content)

    $metaContent = @"
fileFormatVersion: 2
guid: $guid
NativeFormatImporter:
  externalObjects: {}
  mainObjectFileID: 9100000
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"@
    [System.IO.File]::WriteAllText($ctrlMetaPath, $metaContent)
    Write-Output "Created Controller: $ctrlPath (guid: $guid)"
    return $guid
}

function Create-CharacterControllerWithDie {
    param($ctrlPath, $ctrlMetaPath, $ctrlName, $idleAnimGuid, $runAnimGuid, $dieAnimGuid)

    $guid = Get-Or-Create-Guid $ctrlMetaPath

    $content = @"
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1101 &-5001
AnimatorStateTransition:
  m_ObjectHideFlags: 1
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: 
  m_Conditions:
  - m_ConditionMode: 1
    m_ConditionEvent: Die
    m_EventTreshold: 0
  m_DstStateMachine: {fileID: 0}
  m_DstState: {fileID: -6003}
  m_Solo: 0
  m_Mute: 0
  m_IsExit: 0
  serializedVersion: 3
  m_TransitionDuration: 0
  m_TransitionOffset: 0
  m_ExitTime: 0.75
  m_HasExitTime: 0
  m_HasFixedDuration: 1
  m_InterruptionSource: 0
  m_OrderedInterruption: 1
  m_CanTransitionToSelf: 0
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
  m_Position: {x: 50, y: 150, z: 0}
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
--- !u!1102 &-6003
AnimatorState:
  serializedVersion: 6
  m_ObjectHideFlags: 1
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: Die
  m_Speed: 1
  m_CycleOffset: 0
  m_Transitions: []
  m_StateMachineBehaviours: []
  m_Position: {x: 50, y: -80, z: 0}
  m_IKOnFeet: 0
  m_WriteDefaultValues: 1
  m_Mirror: 0
  m_SpeedParameterActive: 0
  m_MirrorParameterActive: 0
  m_CycleOffsetParameterActive: 0
  m_TimeParameterActive: 0
  m_Motion: {fileID: 7400000, guid: $dieAnimGuid, type: 2}
  m_Tag: 
  m_SpeedParameter: 
  m_MirrorParameter: 
  m_CycleOffsetParameter: 
  m_TimeParameter: 
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
    m_Position: {x: 240, y: 50, z: 0}
  - serializedVersion: 1
    m_State: {fileID: 3253218429818599208}
    m_Position: {x: 240, y: 150, z: 0}
  - serializedVersion: 1
    m_State: {fileID: -6003}
    m_Position: {x: 240, y: -80, z: 0}
  m_ChildStateMachines: []
  m_AnyStateTransitions:
  - {fileID: -5001}
  m_EntryTransitions: []
  m_StateMachineTransitions: {}
  m_StateMachineBehaviours: []
  m_AnyStatePosition: {x: -20, y: -80, z: 0}
  m_EntryPosition: {x: 50, y: 60, z: 0}
  m_ExitPosition: {x: 800, y: 120, z: 0}
  m_ParentStateMachinePosition: {x: 800, y: 20, z: 0}
  m_DefaultState: {fileID: 4769346340342722103}
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
  - m_Name: Die
    m_Type: 9
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
"@
    [System.IO.File]::WriteAllText($ctrlPath, $content)

    $metaContent = @"
fileFormatVersion: 2
guid: $guid
NativeFormatImporter:
  externalObjects: {}
  mainObjectFileID: 9100000
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"@
    [System.IO.File]::WriteAllText($ctrlMetaPath, $metaContent)
    Write-Output "Updated Character Controller: $ctrlPath (guid: $guid)"
    return $guid
}

# ==============================================================================
# EXECUTE CONFIGURATION
# ==============================================================================

# 1. CHARACTER DEATH SHEETS
$knightDieTexGuid = Get-Or-Create-Guid "$workspace\Assets\Sprites\Characters\Hero_Knight_Die.png.meta"
Create-SpriteSheetMeta "$workspace\Assets\Sprites\Characters\Hero_Knight_Die.png.meta" $knightDieTexGuid "Hero_Knight_Die" 4
$knightDieAnimGuid = Create-AnimationClip "$workspace\Assets\Animations\Characters\Knight_Die.anim" "$workspace\Assets\Animations\Characters\Knight_Die.anim.meta" "Knight_Die" $knightDieTexGuid 4 5 $false

$mageDieTexGuid = Get-Or-Create-Guid "$workspace\Assets\Sprites\Characters\Hero_Mage_Die.png.meta"
Create-SpriteSheetMeta "$workspace\Assets\Sprites\Characters\Hero_Mage_Die.png.meta" $mageDieTexGuid "Hero_Mage_Die" 4
$mageDieAnimGuid = Create-AnimationClip "$workspace\Assets\Animations\Characters\Mage_Die.anim" "$workspace\Assets\Animations\Characters\Mage_Die.anim.meta" "Mage_Die" $mageDieTexGuid 4 5 $false

$rangerDieTexGuid = Get-Or-Create-Guid "$workspace\Assets\Sprites\Characters\Hero_Ranger_Die.png.meta"
Create-SpriteSheetMeta "$workspace\Assets\Sprites\Characters\Hero_Ranger_Die.png.meta" $rangerDieTexGuid "Hero_Ranger_Die" 4
$rangerDieAnimGuid = Create-AnimationClip "$workspace\Assets\Animations\Characters\Ranger_Die.anim" "$workspace\Assets\Animations\Characters\Ranger_Die.anim.meta" "Ranger_Die" $rangerDieTexGuid 4 5 $false

# Update Character Controllers with Die state
$knightIdleAnimGuid = Get-Or-Create-Guid "$workspace\Assets\Animations\Characters\Knight_Idle.anim.meta"
$knightRunAnimGuid  = Get-Or-Create-Guid "$workspace\Assets\Animations\Characters\Knight_Run.anim.meta"
Create-CharacterControllerWithDie "$workspace\Assets\Animations\Characters\Knight_Controller.controller" "$workspace\Assets\Animations\Characters\Knight_Controller.controller.meta" "Knight_Controller" $knightIdleAnimGuid $knightRunAnimGuid $knightDieAnimGuid

$mageIdleAnimGuid = Get-Or-Create-Guid "$workspace\Assets\Animations\Characters\Mage_Idle.anim.meta"
$mageRunAnimGuid  = Get-Or-Create-Guid "$workspace\Assets\Animations\Characters\Mage_Run.anim.meta"
Create-CharacterControllerWithDie "$workspace\Assets\Animations\Characters\Mage_Controller.controller" "$workspace\Assets\Animations\Characters\Mage_Controller.controller.meta" "Mage_Controller" $mageIdleAnimGuid $mageRunAnimGuid $mageDieAnimGuid

$rangerIdleAnimGuid = Get-Or-Create-Guid "$workspace\Assets\Animations\Characters\Ranger_Idle.anim.meta"
$rangerRunAnimGuid  = Get-Or-Create-Guid "$workspace\Assets\Animations\Characters\Ranger_Run.anim.meta"
Create-CharacterControllerWithDie "$workspace\Assets\Animations\Characters\Ranger_Controller.controller" "$workspace\Assets\Animations\Characters\Ranger_Controller.controller.meta" "Ranger_Controller" $rangerIdleAnimGuid $rangerRunAnimGuid $rangerDieAnimGuid

# 2. ENEMY 1: SLIME (Hop, Attack, Die)
$slimeHopTexGuid = Get-Or-Create-Guid "$workspace\Assets\Sprites\Enemies\Slime_Hop.png.meta"
Create-SpriteSheetMeta "$workspace\Assets\Sprites\Enemies\Slime_Hop.png.meta" $slimeHopTexGuid "Slime_Hop" 4
$slimeHopAnimGuid = Create-AnimationClip "$workspace\Assets\Animations\Enemies\Slime_Hop.anim" "$workspace\Assets\Animations\Enemies\Slime_Hop.anim.meta" "Slime_Hop" $slimeHopTexGuid 4 6 $true

$slimeAtkTexGuid = Get-Or-Create-Guid "$workspace\Assets\Sprites\Enemies\Slime_Attack.png.meta"
Create-SpriteSheetMeta "$workspace\Assets\Sprites\Enemies\Slime_Attack.png.meta" $slimeAtkTexGuid "Slime_Attack" 4
$slimeAtkAnimGuid = Create-AnimationClip "$workspace\Assets\Animations\Enemies\Slime_Attack.anim" "$workspace\Assets\Animations\Enemies\Slime_Attack.anim.meta" "Slime_Attack" $slimeAtkTexGuid 4 6 $false

$slimeDieTexGuid = Get-Or-Create-Guid "$workspace\Assets\Sprites\Enemies\Slime_Die.png.meta"
Create-SpriteSheetMeta "$workspace\Assets\Sprites\Enemies\Slime_Die.png.meta" $slimeDieTexGuid "Slime_Die" 4
$slimeDieAnimGuid = Create-AnimationClip "$workspace\Assets\Animations\Enemies\Slime_Die.anim" "$workspace\Assets\Animations\Enemies\Slime_Die.anim.meta" "Slime_Die" $slimeDieTexGuid 4 6 $false

Create-EnemyController "$workspace\Assets\Animations\Enemies\Slime_Controller.controller" "$workspace\Assets\Animations\Enemies\Slime_Controller.controller.meta" "Slime_Controller" $slimeHopAnimGuid $slimeAtkAnimGuid $slimeDieAnimGuid

# 3. ENEMY 2: SKELETON (Walk, Attack, Die)
$skelWalkTexGuid = Get-Or-Create-Guid "$workspace\Assets\Sprites\Enemies\Skeleton_Walk.png.meta"
Create-SpriteSheetMeta "$workspace\Assets\Sprites\Enemies\Skeleton_Walk.png.meta" $skelWalkTexGuid "Skeleton_Walk" 4
$skelWalkAnimGuid = Create-AnimationClip "$workspace\Assets\Animations\Enemies\Skeleton_Walk.anim" "$workspace\Assets\Animations\Enemies\Skeleton_Walk.anim.meta" "Skeleton_Walk" $skelWalkTexGuid 4 6 $true

$skelAtkTexGuid = Get-Or-Create-Guid "$workspace\Assets\Sprites\Enemies\Skeleton_Attack.png.meta"
Create-SpriteSheetMeta "$workspace\Assets\Sprites\Enemies\Skeleton_Attack.png.meta" $skelAtkTexGuid "Skeleton_Attack" 4
$skelAtkAnimGuid = Create-AnimationClip "$workspace\Assets\Animations\Enemies\Skeleton_Attack.anim" "$workspace\Assets\Animations\Enemies\Skeleton_Attack.anim.meta" "Skeleton_Attack" $skelAtkTexGuid 4 8 $false

$skelDieTexGuid = Get-Or-Create-Guid "$workspace\Assets\Sprites\Enemies\Skeleton_Die.png.meta"
Create-SpriteSheetMeta "$workspace\Assets\Sprites\Enemies\Skeleton_Die.png.meta" $skelDieTexGuid "Skeleton_Die" 4
$skelDieAnimGuid = Create-AnimationClip "$workspace\Assets\Animations\Enemies\Skeleton_Die.anim" "$workspace\Assets\Animations\Enemies\Skeleton_Die.anim.meta" "Skeleton_Die" $skelDieTexGuid 4 6 $false

Create-EnemyController "$workspace\Assets\Animations\Enemies\Skeleton_Controller.controller" "$workspace\Assets\Animations\Enemies\Skeleton_Controller.controller.meta" "Skeleton_Controller" $skelWalkAnimGuid $skelAtkAnimGuid $skelDieAnimGuid

# 4. ENEMY 3: GOBLIN (Walk, Attack, Die)
$goblinWalkTexGuid = Get-Or-Create-Guid "$workspace\Assets\Sprites\Enemies\Goblin_Walk.png.meta"
Create-SpriteSheetMeta "$workspace\Assets\Sprites\Enemies\Goblin_Walk.png.meta" $goblinWalkTexGuid "Goblin_Walk" 4
$goblinWalkAnimGuid = Create-AnimationClip "$workspace\Assets\Animations\Enemies\Goblin_Walk.anim" "$workspace\Assets\Animations\Enemies\Goblin_Walk.anim.meta" "Goblin_Walk" $goblinWalkTexGuid 4 6 $true

$goblinAtkTexGuid = Get-Or-Create-Guid "$workspace\Assets\Sprites\Enemies\Goblin_Attack.png.meta"
Create-SpriteSheetMeta "$workspace\Assets\Sprites\Enemies\Goblin_Attack.png.meta" $goblinAtkTexGuid "Goblin_Attack" 4
$goblinAtkAnimGuid = Create-AnimationClip "$workspace\Assets\Animations\Enemies\Goblin_Attack.anim" "$workspace\Assets\Animations\Enemies\Goblin_Attack.anim.meta" "Goblin_Attack" $goblinAtkTexGuid 4 8 $false

$goblinDieTexGuid = Get-Or-Create-Guid "$workspace\Assets\Sprites\Enemies\Goblin_Die.png.meta"
Create-SpriteSheetMeta "$workspace\Assets\Sprites\Enemies\Goblin_Die.png.meta" $goblinDieTexGuid "Goblin_Die" 4
$goblinDieAnimGuid = Create-AnimationClip "$workspace\Assets\Animations\Enemies\Goblin_Die.anim" "$workspace\Assets\Animations\Enemies\Goblin_Die.anim.meta" "Goblin_Die" $goblinDieTexGuid 4 6 $false

Create-EnemyController "$workspace\Assets\Animations\Enemies\Goblin_Controller.controller" "$workspace\Assets\Animations\Enemies\Goblin_Controller.controller.meta" "Goblin_Controller" $goblinWalkAnimGuid $goblinAtkAnimGuid $goblinDieAnimGuid

# 5. ENEMY 4: BAT (Fly, Attack, Die)
$batFlyTexGuid = Get-Or-Create-Guid "$workspace\Assets\Sprites\Enemies\Bat_Fly.png.meta"
Create-SpriteSheetMeta "$workspace\Assets\Sprites\Enemies\Bat_Fly.png.meta" $batFlyTexGuid "Bat_Fly" 4
$batFlyAnimGuid = Create-AnimationClip "$workspace\Assets\Animations\Enemies\Bat_Fly.anim" "$workspace\Assets\Animations\Enemies\Bat_Fly.anim.meta" "Bat_Fly" $batFlyTexGuid 4 8 $true

$batAtkTexGuid = Get-Or-Create-Guid "$workspace\Assets\Sprites\Enemies\Bat_Attack.png.meta"
Create-SpriteSheetMeta "$workspace\Assets\Sprites\Enemies\Bat_Attack.png.meta" $batAtkTexGuid "Bat_Attack" 4
$batAtkAnimGuid = Create-AnimationClip "$workspace\Assets\Animations\Enemies\Bat_Attack.anim" "$workspace\Assets\Animations\Enemies\Bat_Attack.anim.meta" "Bat_Attack" $batAtkTexGuid 4 8 $false

$batDieTexGuid = Get-Or-Create-Guid "$workspace\Assets\Sprites\Enemies\Bat_Die.png.meta"
Create-SpriteSheetMeta "$workspace\Assets\Sprites\Enemies\Bat_Die.png.meta" $batDieTexGuid "Bat_Die" 4
$batDieAnimGuid = Create-AnimationClip "$workspace\Assets\Animations\Enemies\Bat_Die.anim" "$workspace\Assets\Animations\Enemies\Bat_Die.anim.meta" "Bat_Die" $batDieTexGuid 4 6 $false

Create-EnemyController "$workspace\Assets\Animations\Enemies\Bat_Controller.controller" "$workspace\Assets\Animations\Enemies\Bat_Controller.controller.meta" "Bat_Controller" $batFlyAnimGuid $batAtkAnimGuid $batDieAnimGuid

# 6. ENEMY 5: BOSS GOLEM (Walk, Attack, Die)
$golemWalkTexGuid = Get-Or-Create-Guid "$workspace\Assets\Sprites\Enemies\Boss_Golem_Walk.png.meta"
Create-SpriteSheetMeta "$workspace\Assets\Sprites\Enemies\Boss_Golem_Walk.png.meta" $golemWalkTexGuid "Boss_Golem_Walk" 4
$golemWalkAnimGuid = Create-AnimationClip "$workspace\Assets\Animations\Enemies\Boss_Golem_Walk.anim" "$workspace\Assets\Animations\Enemies\Boss_Golem_Walk.anim.meta" "Boss_Golem_Walk" $golemWalkTexGuid 4 4 $true

$golemAtkTexGuid = Get-Or-Create-Guid "$workspace\Assets\Sprites\Enemies\Boss_Golem_Attack.png.meta"
Create-SpriteSheetMeta "$workspace\Assets\Sprites\Enemies\Boss_Golem_Attack.png.meta" $golemAtkTexGuid "Boss_Golem_Attack" 4
$golemAtkAnimGuid = Create-AnimationClip "$workspace\Assets\Animations\Enemies\Boss_Golem_Attack.anim" "$workspace\Assets\Animations\Enemies\Boss_Golem_Attack.anim.meta" "Boss_Golem_Attack" $golemAtkTexGuid 4 6 $false

$golemDieTexGuid = Get-Or-Create-Guid "$workspace\Assets\Sprites\Enemies\Boss_Golem_Die.png.meta"
Create-SpriteSheetMeta "$workspace\Assets\Sprites\Enemies\Boss_Golem_Die.png.meta" $golemDieTexGuid "Boss_Golem_Die" 4
$golemDieAnimGuid = Create-AnimationClip "$workspace\Assets\Animations\Enemies\Boss_Golem_Die.anim" "$workspace\Assets\Animations\Enemies\Boss_Golem_Die.anim.meta" "Boss_Golem_Die" $golemDieTexGuid 4 5 $false

Create-EnemyController "$workspace\Assets\Animations\Enemies\Boss_Golem_Controller.controller" "$workspace\Assets\Animations\Enemies\Boss_Golem_Controller.controller.meta" "Boss_Golem_Controller" $golemWalkAnimGuid $golemAtkAnimGuid $golemDieAnimGuid

Write-Output "ALL SPRITESHEET METAS, ANIMATION CLIPS, AND CONTROLLERS CREATED AND CONFIGURED SUCCESSFULLY!"
