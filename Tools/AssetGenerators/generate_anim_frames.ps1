Add-Type -AssemblyName System.Drawing

function New-UnityMetaFile {
    param(
        [string]$Path,
        [int]$PPU = 16
    )
    $guid = [System.Guid]::NewGuid().ToString("N")
    $metaContent = @"
fileFormatVersion: 2
guid: $guid
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {}
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
  ignoreMasterTextureLimit: 0
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
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {x: 0.5, y: 0.5}
  spritePixelsToUnits: $PPU
  spriteBorder: {x: 0, y: 0, z: 0, w: 0}
  spriteGenerateFallbackPhysicsShape: 1
  alphaIsTransparency: 1
  spriteColliderType: 1
  textureType: 8
"@
    $metaPath = "$Path.meta"
    if (-not (Test-Path $metaPath)) {
        [System.IO.File]::WriteAllText($metaPath, $metaContent)
    }
}

function Save-Sprite {
    param([string]$Path, [System.Drawing.Bitmap]$Bmp, [int]$PPU = 16)
    $dir = [System.IO.Path]::GetDirectoryName($Path)
    if (-not (Test-Path $dir)) { [System.IO.Directory]::CreateDirectory($dir) | Out-Null }
    $Bmp.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
    New-UnityMetaFile -Path $Path -PPU $PPU
    $Bmp.Dispose()
    Write-Output "Created: $Path"
}

function New-Canvas {
    param([int]$w, [int]$h)
    $b = New-Object System.Drawing.Bitmap $w, $h
    for ($x=0; $x -lt $w; $x++) {
        for ($y=0; $y -lt $h; $y++) {
            $b.SetPixel($x, $y, [System.Drawing.Color]::FromArgb(0, 0, 0, 0))
        }
    }
    return $b
}

# Palettes
$steel = [System.Drawing.Color]::FromArgb(255, 180, 190, 205)
$darkSteel = [System.Drawing.Color]::FromArgb(255, 100, 110, 130)
$visor = [System.Drawing.Color]::FromArgb(255, 40, 45, 60)
$gold = [System.Drawing.Color]::FromArgb(255, 245, 195, 50)
$redCape = [System.Drawing.Color]::FromArgb(255, 215, 40, 40)
$darkCape = [System.Drawing.Color]::FromArgb(255, 150, 20, 20)

# --- KNIGHT FRAMES (16x16) ---

# 1. Knight Idle 0
$b = New-Canvas 16 16
for ($x=5; $x -le 10; $x++) { for ($y=2; $y -le 6; $y++) { $b.SetPixel($x, $y, $steel) } }
for ($x=6; $x -le 9; $x++) { $b.SetPixel($x, 4, $visor) }
$b.SetPixel(7, 2, $gold); $b.SetPixel(8, 2, $gold)
for ($x=4; $x -le 11; $x++) { for ($y=7; $y -le 12; $y++) { $b.SetPixel($x, $y, $steel) } }
for ($y=4; $y -le 13; $y++) { $b.SetPixel(3, $y, $redCape); $b.SetPixel(12, $y, $redCape) }
for ($x=5; $x -le 6; $x++) { for ($y=13; $y -le 15; $y++) { $b.SetPixel($x, $y, $darkSteel) } }
for ($x=9; $x -le 10; $x++) { for ($y=13; $y -le 15; $y++) { $b.SetPixel($x, $y, $darkSteel) } }
Save-Sprite "Assets/Sprites/Characters/Hero_Knight_Idle_0.png" $b 16

# 2. Knight Idle 1 (Chest breathe up 1px)
$b = New-Canvas 16 16
for ($x=5; $x -le 10; $x++) { for ($y=1; $y -le 5; $y++) { $b.SetPixel($x, $y, $steel) } }
for ($x=6; $x -le 9; $x++) { $b.SetPixel($x, 3, $visor) }
$b.SetPixel(7, 1, $gold); $b.SetPixel(8, 1, $gold)
for ($x=4; $x -le 11; $x++) { for ($y=6; $y -le 11; $y++) { $b.SetPixel($x, $y, $steel) } }
for ($y=3; $y -le 13; $y++) { $b.SetPixel(3, $y, $redCape); $b.SetPixel(12, $y, $redCape) }
for ($x=5; $x -le 6; $x++) { for ($y=12; $y -le 15; $y++) { $b.SetPixel($x, $y, $darkSteel) } }
for ($x=9; $x -le 10; $x++) { for ($y=12; $y -le 15; $y++) { $b.SetPixel($x, $y, $darkSteel) } }
Save-Sprite "Assets/Sprites/Characters/Hero_Knight_Idle_1.png" $b 16

# 3. Knight Run 0 (Left leg step forward)
$b = New-Canvas 16 16
for ($x=5; $x -le 10; $x++) { for ($y=2; $y -le 6; $y++) { $b.SetPixel($x, $y, $steel) } }
for ($x=6; $x -le 9; $x++) { $b.SetPixel($x, 4, $visor) }
$b.SetPixel(7, 2, $gold); $b.SetPixel(8, 2, $gold)
for ($x=4; $x -le 11; $x++) { for ($y=7; $y -le 12; $y++) { $b.SetPixel($x, $y, $steel) } }
for ($y=4; $y -le 12; $y++) { $b.SetPixel(2, $y, $redCape); $b.SetPixel(12, $y, $redCape) } # Cape tilts back
# Left leg forward (x=3..5, y=13..15), Right leg back (x=9..11, y=12..14)
for ($x=3; $x -le 5; $x++) { for ($y=13; $y -le 15; $y++) { $b.SetPixel($x, $y, $darkSteel) } }
for ($x=9; $x -le 11; $x++) { for ($y=12; $y -le 14; $y++) { $b.SetPixel($x, $y, $darkSteel) } }
Save-Sprite "Assets/Sprites/Characters/Hero_Knight_Run_0.png" $b 16

# 4. Knight Run 1 (Crouch / Passing stance)
$b = New-Canvas 16 16
for ($x=5; $x -le 10; $x++) { for ($y=3; $y -le 7; $y++) { $b.SetPixel($x, $y, $steel) } } # Dipped 1px
for ($x=6; $x -le 9; $x++) { $b.SetPixel($x, 5, $visor) }
$b.SetPixel(7, 3, $gold); $b.SetPixel(8, 3, $gold)
for ($x=4; $x -le 11; $x++) { for ($y=8; $y -le 13; $y++) { $b.SetPixel($x, $y, $steel) } }
for ($y=5; $y -le 13; $y++) { $b.SetPixel(2, $y, $redCape); $b.SetPixel(13, $y, $redCape) }
for ($x=6; $x -le 9; $x++) { for ($y=14; $y -le 15; $y++) { $b.SetPixel($x, $y, $darkSteel) } } # Both legs passing
Save-Sprite "Assets/Sprites/Characters/Hero_Knight_Run_1.png" $b 16

# 5. Knight Run 2 (Right leg step forward)
$b = New-Canvas 16 16
for ($x=5; $x -le 10; $x++) { for ($y=2; $y -le 6; $y++) { $b.SetPixel($x, $y, $steel) } }
for ($x=6; $x -le 9; $x++) { $b.SetPixel($x, 4, $visor) }
$b.SetPixel(7, 2, $gold); $b.SetPixel(8, 2, $gold)
for ($x=4; $x -le 11; $x++) { for ($y=7; $y -le 12; $y++) { $b.SetPixel($x, $y, $steel) } }
for ($y=4; $y -le 12; $y++) { $b.SetPixel(3, $y, $redCape); $b.SetPixel(13, $y, $redCape) }
# Left leg back (x=4..6, y=12..14), Right leg forward (x=10..12, y=13..15)
for ($x=4; $x -le 6; $x++) { for ($y=12; $y -le 14; $y++) { $b.SetPixel($x, $y, $darkSteel) } }
for ($x=10; $x -le 12; $x++) { for ($y=13; $y -le 15; $y++) { $b.SetPixel($x, $y, $darkSteel) } }
Save-Sprite "Assets/Sprites/Characters/Hero_Knight_Run_2.png" $b 16

# 6. Knight Run 3 (Air push / Apex stance)
$b = New-Canvas 16 16
for ($x=5; $x -le 10; $x++) { for ($y=1; $y -le 5; $y++) { $b.SetPixel($x, $y, $steel) } } # High 1px
for ($x=6; $x -le 9; $x++) { $b.SetPixel($x, 3, $visor) }
$b.SetPixel(7, 1, $gold); $b.SetPixel(8, 1, $gold)
for ($x=4; $x -le 11; $x++) { for ($y=6; $y -le 11; $y++) { $b.SetPixel($x, $y, $steel) } }
for ($y=3; $y -le 11; $y++) { $b.SetPixel(2, $y, $redCape); $b.SetPixel(13, $y, $redCape) } # Cape flies up
for ($x=5; $x -le 6; $x++) { for ($y=12; $y -le 14; $y++) { $b.SetPixel($x, $y, $darkSteel) } }
for ($x=9; $x -le 10; $x++) { for ($y=12; $y -le 14; $y++) { $b.SetPixel($x, $y, $darkSteel) } }
Save-Sprite "Assets/Sprites/Characters/Hero_Knight_Run_3.png" $b 16

# --- SLIME HOP FRAMES (16x16) ---
$slimeCol = [System.Drawing.Color]::FromArgb(255, 60, 220, 90)
$slimeHi = [System.Drawing.Color]::FromArgb(255, 170, 250, 140)

# Slime 0: Compressed / Squash on floor
$b = New-Canvas 16 16
for ($x=2; $x -le 13; $x++) { for ($y=9; $y -le 15; $y++) { $b.SetPixel($x, $y, $slimeCol) } }
$b.SetPixel(4, 10, $slimeHi); $b.SetPixel(5, 10, $slimeHi)
$b.SetPixel(4, 12, [System.Drawing.Color]::Black); $b.SetPixel(10, 12, [System.Drawing.Color]::Black)
Save-Sprite "Assets/Sprites/Enemies/Slime_Hop_0.png" $b 16

# Slime 1: Stretch tall (launching)
$b = New-Canvas 16 16
for ($x=5; $x -le 10; $x++) { for ($y=3; $y -le 14; $y++) { $b.SetPixel($x, $y, $slimeCol) } }
$b.SetPixel(6, 4, $slimeHi); $b.SetPixel(7, 4, $slimeHi)
$b.SetPixel(6, 7, [System.Drawing.Color]::Black); $b.SetPixel(9, 7, [System.Drawing.Color]::Black)
Save-Sprite "Assets/Sprites/Enemies/Slime_Hop_1.png" $b 16

# Slime 2: Peak air (round blob floating)
$b = New-Canvas 16 16
for ($x=3; $x -le 12; $x++) { for ($y=2; $y -le 10; $y++) { $b.SetPixel($x, $y, $slimeCol) } }
$b.SetPixel(5, 3, $slimeHi); $b.SetPixel(6, 3, $slimeHi)
$b.SetPixel(5, 6, [System.Drawing.Color]::Black); $b.SetPixel(9, 6, [System.Drawing.Color]::Black)
Save-Sprite "Assets/Sprites/Enemies/Slime_Hop_2.png" $b 16

# Slime 3: Normal landing
$b = New-Canvas 16 16
for ($x=3; $x -le 12; $x++) { for ($y=6; $y -le 14; $y++) { $b.SetPixel($x, $y, $slimeCol) } }
$b.SetPixel(5, 7, $slimeHi); $b.SetPixel(6, 7, $slimeHi)
$b.SetPixel(5, 10, [System.Drawing.Color]::Black); $b.SetPixel(9, 10, [System.Drawing.Color]::Black)
Save-Sprite "Assets/Sprites/Enemies/Slime_Hop_3.png" $b 16

Write-Output "ALL ANIMATION FRAMES CREATED SUCCESSFULLY!"
