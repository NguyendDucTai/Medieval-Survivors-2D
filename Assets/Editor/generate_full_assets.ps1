Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.IO

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
    param(
        [string]$RelativePath,
        [System.Drawing.Bitmap]$Bitmap,
        [int]$PPU = 16
    )
    $dir = [System.IO.Path]::GetDirectoryName($RelativePath)
    if (-not (Test-Path $dir)) {
        [System.IO.Directory]::CreateDirectory($dir) | Out-Null
    }
    $Bitmap.Save($RelativePath, [System.Drawing.Imaging.ImageFormat]::Png)
    New-UnityMetaFile -Path $RelativePath -PPU $PPU
    $Bitmap.Dispose()
    Write-Output "Created: $RelativePath"
}

function New-Canvas {
    param([int]$w, [int]$h)
    $bmp = New-Object System.Drawing.Bitmap $w, $h
    for ($x=0; $x -lt $w; $x++) {
        for ($y=0; $y -lt $h; $y++) {
            $bmp.SetPixel($x, $y, [System.Drawing.Color]::FromArgb(0, 0, 0, 0))
        }
    }
    return $bmp
}

function HexColor([string]$hex) {
    return [System.Drawing.ColorTranslator]::FromHtml($hex)
}

function Draw-PixelArt {
    param(
        [string[]]$Rows,
        [hashtable]$Palette
    )
    $h = $Rows.Length
    $w = $Rows[0].Length
    $bmp = New-Object System.Drawing.Bitmap $w, $h
    for ($y = 0; $y -lt $h; $y++) {
        $row = $Rows[$y]
        for ($x = 0; $x -lt $w; $x++) {
            $char = [string]$row[$x]
            if ($Palette.ContainsKey($char)) {
                $color = $Palette[$char]
                $bmp.SetPixel($x, $y, $color)
            } else {
                $bmp.SetPixel($x, $y, [System.Drawing.Color]::FromArgb(0, 0, 0, 0))
            }
        }
    }
    return $bmp
}


# --- CHARACTERS ---
# 1. Hero Knight (16x16)
$bmp = New-Canvas 16 16
$steel = [System.Drawing.Color]::FromArgb(255, 180, 190, 205)
$visor = [System.Drawing.Color]::FromArgb(255, 40, 45, 60)
$gold = [System.Drawing.Color]::FromArgb(255, 245, 195, 50)
$redCape = [System.Drawing.Color]::FromArgb(255, 215, 40, 40)
$darkSteel = [System.Drawing.Color]::FromArgb(255, 100, 110, 130)

# Helmet & Visor (In bitmap, y=0 is top)
for ($x=5; $x -le 10; $x++) { for ($y=1; $y -le 5; $y++) { $bmp.SetPixel($x, $y, $steel) } }
for ($x=6; $x -le 9; $x++) { $bmp.SetPixel($x, 3, $visor) }
$bmp.SetPixel(7, 1, $gold); $bmp.SetPixel(8, 1, $gold)
# Armor Body & Cape
for ($x=4; $x -le 11; $x++) { for ($y=6; $y -le 11; $y++) { $bmp.SetPixel($x, $y, $steel) } }
for ($y=3; $y -le 13; $y++) { $bmp.SetPixel(3, $y, $redCape); $bmp.SetPixel(12, $y, $redCape) }
$bmp.SetPixel(7, 7, $gold); $bmp.SetPixel(8, 7, $gold); $bmp.SetPixel(7, 8, $gold); $bmp.SetPixel(8, 8, $gold)
# Boots
for ($x=5; $x -le 6; $x++) { for ($y=12; $y -le 15; $y++) { $bmp.SetPixel($x, $y, $darkSteel) } }
for ($x=9; $x -le 10; $x++) { for ($y=12; $y -le 15; $y++) { $bmp.SetPixel($x, $y, $darkSteel) } }
Save-Sprite "Assets/Sprites/Characters/Hero_Knight.png" $bmp 16

# 2. Hero Mage (16x16)
$bmp = New-Canvas 16 16
$robe = [System.Drawing.Color]::FromArgb(255, 90, 50, 180)
$hat = [System.Drawing.Color]::FromArgb(255, 60, 30, 140)
$gold = [System.Drawing.Color]::FromArgb(255, 245, 200, 50)
$skin = [System.Drawing.Color]::FromArgb(255, 240, 200, 150)
$beard = [System.Drawing.Color]::FromArgb(255, 240, 240, 245)
# Wizard Hat
$bmp.SetPixel(7, 0, $gold); $bmp.SetPixel(8, 0, $gold)
for ($x=7; $x -le 8; $x++) { $bmp.SetPixel($x, 1, $hat) }
for ($x=6; $x -le 9; $x++) { $bmp.SetPixel($x, 2, $hat) }
for ($x=5; $x -le 10; $x++) { $bmp.SetPixel($x, 3, $hat) }
for ($x=4; $x -le 11; $x++) { $bmp.SetPixel($x, 4, $hat) }
for ($x=3; $x -le 12; $x++) { $bmp.SetPixel($x, 5, $gold) }
# Face & Beard
for ($x=6; $x -le 9; $x++) { $bmp.SetPixel($x, 6, $skin) }
$bmp.SetPixel(6, 6, [System.Drawing.Color]::Black); $bmp.SetPixel(9, 6, [System.Drawing.Color]::Black)
for ($x=5; $x -le 10; $x++) { for ($y=7; $y -le 8; $y++) { $bmp.SetPixel($x, $y, $beard) } }
# Robe Body
for ($x=4; $x -le 11; $x++) { for ($y=9; $y -le 15; $y++) { $bmp.SetPixel($x, $y, $robe) } }
for ($y=9; $y -le 15; $y++) { $bmp.SetPixel(7, $y, $gold); $bmp.SetPixel(8, $y, $gold) }
Save-Sprite "Assets/Sprites/Characters/Hero_Mage.png" $bmp 16

# 3. Hero Ranger (16x16)
$bmp = New-Canvas 16 16
$green = [System.Drawing.Color]::FromArgb(255, 45, 125, 60)
$leather = [System.Drawing.Color]::FromArgb(255, 140, 90, 45)
$feather = [System.Drawing.Color]::FromArgb(255, 220, 40, 40)
# Cap & Feather
$bmp.SetPixel(10, 0, $feather); $bmp.SetPixel(9, 1, $feather)
for ($x=5; $x -le 10; $x++) { $bmp.SetPixel($x, 2, $green); $bmp.SetPixel($x, 3, $green) }
# Face
for ($x=6; $x -le 9; $x++) { $bmp.SetPixel($x, 4, $skin) }
$bmp.SetPixel(6, 4, [System.Drawing.Color]::Black); $bmp.SetPixel(9, 4, [System.Drawing.Color]::Black)
# Body
for ($x=4; $x -le 11; $x++) { for ($y=5; $y -le 11; $y++) { $bmp.SetPixel($x, $y, $green) } }
for ($x=6; $x -le 9; $x++) { for ($y=7; $y -le 10; $y++) { $bmp.SetPixel($x, $y, $leather) } }
# Pants & Boots
for ($x=5; $x -le 6; $x++) { for ($y=12; $y -le 15; $y++) { $bmp.SetPixel($x, $y, $leather) } }
for ($x=9; $x -le 10; $x++) { for ($y=12; $y -le 15; $y++) { $bmp.SetPixel($x, $y, $leather) } }
Save-Sprite "Assets/Sprites/Characters/Hero_Ranger.png" $bmp 16

# --- ENEMIES ---
# 1. Slime Green (16x16)
$bmp = New-Canvas 16 16
$slimeCol = [System.Drawing.Color]::FromArgb(255, 60, 220, 90)
$highlight = [System.Drawing.Color]::FromArgb(255, 170, 250, 140)
$shadow = [System.Drawing.Color]::FromArgb(255, 30, 150, 50)
for ($x=3; $x -le 12; $x++) {
    for ($y=6; $y -le 14; $y++) {
        if (($x -eq 3 -or $x -eq 12) -and $y -lt 9) { continue }
        $bmp.SetPixel($x, $y, $slimeCol)
    }
}
$bmp.SetPixel(5, 7, $highlight); $bmp.SetPixel(6, 7, $highlight); $bmp.SetPixel(5, 8, $highlight)
$bmp.SetPixel(5, 10, [System.Drawing.Color]::Black); $bmp.SetPixel(9, 10, [System.Drawing.Color]::Black)
for ($x=4; $x -le 11; $x++) { $bmp.SetPixel($x, 14, $shadow) }
Save-Sprite "Assets/Sprites/Enemies/Slime_Green.png" $bmp 16

# 2. Skeleton White (16x16)
$bmp = New-Canvas 16 16
$bone = [System.Drawing.Color]::FromArgb(255, 230, 230, 220)
$boneDark = [System.Drawing.Color]::FromArgb(255, 160, 160, 150)
# Skull
for ($x=5; $x -le 10; $x++) { for ($y=1; $y -le 5; $y++) { $bmp.SetPixel($x, $y, $bone) } }
$bmp.SetPixel(6, 3, [System.Drawing.Color]::FromArgb(255, 20, 20, 25))
$bmp.SetPixel(9, 3, [System.Drawing.Color]::FromArgb(255, 20, 20, 25))
$bmp.SetPixel(7, 5, [System.Drawing.Color]::FromArgb(255, 30, 30, 35))
$bmp.SetPixel(8, 5, [System.Drawing.Color]::FromArgb(255, 30, 30, 35))
# Spine & Ribs
for ($y=6; $y -le 11; $y++) { $bmp.SetPixel(7, $y, $bone); $bmp.SetPixel(8, $y, $bone) }
for ($x=4; $x -le 11; $x++) { $bmp.SetPixel($x, 7, $bone); $bmp.SetPixel($x, 9, $bone) }
# Legs
for ($y=12; $y -le 15; $y++) { $bmp.SetPixel(5, $y, $boneDark); $bmp.SetPixel(10, $y, $boneDark) }
Save-Sprite "Assets/Sprites/Enemies/Skeleton_White.png" $bmp 16

# 3. Goblin Green (16x16)
$bmp = New-Canvas 16 16
$goblinSkin = [System.Drawing.Color]::FromArgb(255, 90, 170, 50)
$goblinCloth = [System.Drawing.Color]::FromArgb(255, 120, 70, 30)
$goblinEye = [System.Drawing.Color]::FromArgb(255, 255, 60, 30)
# Ears
$bmp.SetPixel(2, 4, $goblinSkin); $bmp.SetPixel(3, 4, $goblinSkin); $bmp.SetPixel(3, 5, $goblinSkin)
$bmp.SetPixel(13, 4, $goblinSkin); $bmp.SetPixel(12, 4, $goblinSkin); $bmp.SetPixel(12, 5, $goblinSkin)
# Head
for ($x=4; $x -le 11; $x++) { for ($y=2; $y -le 6; $y++) { $bmp.SetPixel($x, $y, $goblinSkin) } }
$bmp.SetPixel(5, 4, $goblinEye); $bmp.SetPixel(10, 4, $goblinEye)
# Body & Cloth
for ($x=4; $x -le 11; $x++) { for ($y=7; $y -le 12; $y++) { $bmp.SetPixel($x, $y, $goblinCloth) } }
for ($y=13; $y -le 15; $y++) { $bmp.SetPixel(5, $y, $goblinSkin); $bmp.SetPixel(10, $y, $goblinSkin) }
Save-Sprite "Assets/Sprites/Enemies/Goblin_Green.png" $bmp 16

# 4. Bat Shadow (16x16)
$bmp = New-Canvas 16 16
$batPurple = [System.Drawing.Color]::FromArgb(255, 75, 45, 100)
$batDark = [System.Drawing.Color]::FromArgb(255, 35, 20, 55)
# Wings
for ($x=0; $x -le 4; $x++) { for ($y=4; $y -le 8; $y++) { $bmp.SetPixel($x, $y, $batPurple) } }
for ($x=11; $x -le 15; $x++) { for ($y=4; $y -le 8; $y++) { $bmp.SetPixel($x, $y, $batPurple) } }
# Body
for ($x=5; $x -le 10; $x++) { for ($y=5; $y -le 11; $y++) { $bmp.SetPixel($x, $y, $batDark) } }
# Ears & Red eyes
$bmp.SetPixel(5, 3, $batDark); $bmp.SetPixel(10, 3, $batDark)
$bmp.SetPixel(6, 6, [System.Drawing.Color]::Red); $bmp.SetPixel(9, 6, [System.Drawing.Color]::Red)
Save-Sprite "Assets/Sprites/Enemies/Bat_Shadow.png" $bmp 16

# 5. Boss Golem (32x32)
$bmp = New-Canvas 32 32
$rock = [System.Drawing.Color]::FromArgb(255, 110, 115, 125)
$rockDark = [System.Drawing.Color]::FromArgb(255, 65, 70, 80)
$runeCyan = [System.Drawing.Color]::FromArgb(255, 40, 230, 255)
# Massive stone body
for ($x=6; $x -le 25; $x++) {
    for ($y=6; $y -le 25; $y++) {
        $bmp.SetPixel($x, $y, $rock)
    }
}
# Stone shoulders & outline
for ($x=4; $x -le 27; $x++) { $bmp.SetPixel($x, 6, $rockDark); $bmp.SetPixel($x, 25, $rockDark) }
for ($y=6; $y -le 25; $y++) { $bmp.SetPixel(6, $y, $rockDark); $bmp.SetPixel(25, $y, $rockDark) }
# Glowing Core / Runes
for ($x=14; $x -le 17; $x++) { for ($y=14; $y -le 17; $y++) { $bmp.SetPixel($x, $y, $runeCyan) } }
$bmp.SetPixel(10, 11, $runeCyan); $bmp.SetPixel(21, 11, $runeCyan) # Eyes
for ($y=26; $y -le 31; $y++) {
    for ($x=8; $x -le 12; $x++) { $bmp.SetPixel($x, $y, $rockDark) }
    for ($x=19; $x -le 23; $x++) { $bmp.SetPixel($x, $y, $rockDark) }
}
Save-Sprite "Assets/Sprites/Enemies/Boss_Golem.png" $bmp 16

# --- WEAPONS & PROJECTILES ---
# 1. Holy Sword (16x16)
$bmp = New-Canvas 16 16
$blade = [System.Drawing.Color]::FromArgb(255, 235, 245, 255)
$bladeShine = [System.Drawing.Color]::FromArgb(255, 160, 215, 255)
$gold = [System.Drawing.Color]::FromArgb(255, 250, 200, 50)
$hilt = [System.Drawing.Color]::FromArgb(255, 110, 60, 25)
for ($i=4; $i -le 14; $i++) {
    $bmp.SetPixel($i, 15 - $i, $blade)
    if ($i -lt 14) { $bmp.SetPixel($i+1, 15 - $i, $bladeShine) }
}
$bmp.SetPixel(4, 12, $gold); $bmp.SetPixel(3, 11, $gold); $bmp.SetPixel(5, 13, $gold)
$bmp.SetPixel(2, 13, $hilt); $bmp.SetPixel(1, 14, $hilt)
Save-Sprite "Assets/Sprites/Weapons/Weapon_HolySword.png" $bmp 16

# 2. Fireball (16x16)
$bmp = New-Canvas 16 16
$core = [System.Drawing.Color]::FromArgb(255, 255, 255, 180)
$fire1 = [System.Drawing.Color]::FromArgb(255, 255, 190, 40)
$fire2 = [System.Drawing.Color]::FromArgb(255, 240, 70, 20)
$fire3 = [System.Drawing.Color]::FromArgb(255, 160, 20, 10)
for ($x=3; $x -le 12; $x++) {
    for ($y=3; $y -le 12; $y++) {
        $d = [Math]::Sqrt([Math]::Pow($x - 8, 2) + [Math]::Pow($y - 8, 2))
        if ($d -le 2.0) { $bmp.SetPixel($x, $y, $core) }
        elseif ($d -le 3.5) { $bmp.SetPixel($x, $y, $fire1) }
        elseif ($d -le 4.8) { $bmp.SetPixel($x, $y, $fire2) }
    }
}
$bmp.SetPixel(2, 8, $fire3); $bmp.SetPixel(1, 7, $fire3); $bmp.SetPixel(2, 9, $fire3)
Save-Sprite "Assets/Sprites/Weapons/Weapon_Fireball.png" $bmp 16

# 3. Magic Staff (16x16)
$bmp = New-Canvas 16 16
$wood = [System.Drawing.Color]::FromArgb(255, 110, 70, 30)
$gemOrb = [System.Drawing.Color]::FromArgb(255, 180, 70, 250)
$gemShine = [System.Drawing.Color]::FromArgb(255, 240, 200, 255)
for ($i=2; $i -le 12; $i++) { $bmp.SetPixel($i, 15 - $i, $wood) }
# Crystal top
$bmp.SetPixel(12, 2, $gemOrb); $bmp.SetPixel(13, 1, $gemOrb); $bmp.SetPixel(14, 2, $gemOrb); $bmp.SetPixel(13, 3, $gemOrb)
$bmp.SetPixel(13, 2, $gemShine)
Save-Sprite "Assets/Sprites/Weapons/Weapon_MagicStaff.png" $bmp 16

# 4. Arrow (16x16)
$bmp = New-Canvas 16 16
$arrowWood = [System.Drawing.Color]::FromArgb(255, 160, 110, 60)
$steelTip = [System.Drawing.Color]::FromArgb(255, 210, 215, 225)
$featherRed = [System.Drawing.Color]::FromArgb(255, 220, 40, 40)
for ($x=3; $x -le 12; $x++) { $bmp.SetPixel($x, 8, $arrowWood) }
$bmp.SetPixel(13, 8, $steelTip); $bmp.SetPixel(14, 8, $steelTip); $bmp.SetPixel(13, 7, $steelTip); $bmp.SetPixel(13, 9, $steelTip)
$bmp.SetPixel(2, 7, $featherRed); $bmp.SetPixel(2, 9, $featherRed); $bmp.SetPixel(1, 6, $featherRed); $bmp.SetPixel(1, 10, $featherRed)
Save-Sprite "Assets/Sprites/Weapons/Weapon_Arrow.png" $bmp 16

# 5. VFX Slash (16x16)
$bmp = New-Canvas 16 16
$slashCore = [System.Drawing.Color]::FromArgb(255, 255, 255, 255)
$slashGlow = [System.Drawing.Color]::FromArgb(255, 100, 220, 255)
for ($i=2; $i -le 13; $i++) {
    $y = [int](8 - 4 * [Math]::Sin(($i - 2) * [Math]::PI / 11))
    $bmp.SetPixel($i, $y, $slashCore)
    $bmp.SetPixel($i, $y+1, $slashGlow)
}
Save-Sprite "Assets/Sprites/Weapons/VFX_Slash.png" $bmp 16

# --- COLLECTIBLES (BOLD & HIGH-CONTRAST ARCADE PIXEL ART - MATCHING HERO KNIGHT) ---

# Symmetrical 16x16 Diamond Gem Pattern
$gemPattern = @(
    "................",
    ".......W........",
    "......W#W.......",
    "....########....",
    "...#WLL11114#...",
    "..#WLL1111144#..",
    ".#WWLL11111444#.",
    ".#WLL111111444#.",
    "..#LL11111144#..",
    "...#11111144#...",
    "....#111144#....",
    ".....#1144#.....",
    "......#14#......",
    ".......##.......",
    "................",
    "................"
)

# 1. Exp Gem Blue (16x16 - Vivid Sapphire)
$palBlue = @{
    "#" = HexColor "#06132b"
    "W" = HexColor "#ffffff"
    "L" = HexColor "#70d6ff" # vivid light cyan
    "1" = HexColor "#0088ff" # pure electric blue
    "4" = HexColor "#003380" # clean dark blue shadow
}
$bmp = Draw-PixelArt $gemPattern $palBlue
Save-Sprite "Assets/Sprites/Collectibles/ExpGem_Blue.png" $bmp 16

# 2. Exp Gem Red (16x16 - Vivid Ruby)
$palRed = @{
    "#" = HexColor "#28040a"
    "W" = HexColor "#ffffff"
    "L" = HexColor "#ff6b8b" # bright rose
    "1" = HexColor "#ff1744" # pure vivid ruby red
    "4" = HexColor "#80001a" # clean dark ruby shadow
}
$bmp = Draw-PixelArt $gemPattern $palRed
Save-Sprite "Assets/Sprites/Collectibles/ExpGem_Red.png" $bmp 16

# 3. Exp Gem Gold (16x16 - Radiant Citrine)
$palGold = @{
    "#" = HexColor "#2b1700"
    "W" = HexColor "#ffffff"
    "L" = HexColor "#ffea60" # bright yellow
    "1" = HexColor "#ffb300" # pure vivid gold
    "4" = HexColor "#8c4e00" # clean dark gold shadow
}
$bmp = Draw-PixelArt $gemPattern $palGold
Save-Sprite "Assets/Sprites/Collectibles/ExpGem_Gold.png" $bmp 16

# Extra: Exp Gem Green (16x16 - Vivid Emerald)
$palGreen = @{
    "#" = HexColor "#021a08"
    "W" = HexColor "#ffffff"
    "L" = HexColor "#69f0ae" # bright mint
    "1" = HexColor "#00e676" # pure vivid green
    "4" = HexColor "#00591f" # clean dark green shadow
}
$bmp = Draw-PixelArt $gemPattern $palGreen
Save-Sprite "Assets/Sprites/Collectibles/ExpGem_Green.png" $bmp 16

# Extra: Exp Gem Purple (16x16 - Vivid Amethyst)
$palPurple = @{
    "#" = HexColor "#1d042e"
    "W" = HexColor "#ffffff"
    "L" = HexColor "#e085ff" # bright lilac
    "1" = HexColor "#b026ff" # pure vivid purple
    "4" = HexColor "#4d0080" # clean dark purple shadow
}
$bmp = Draw-PixelArt $gemPattern $palPurple
Save-Sprite "Assets/Sprites/Collectibles/ExpGem_Purple.png" $bmp 16

# 4. Health Potion (16x16 - Apothecary Flask with Vivid Red Elixir & Glass Glint)
$potionPattern = @(
    "................",
    "......####......",
    "......#CK#......",
    ".....######.....",
    ".....#GGGG#.....",
    "....##....##....",
    "...#W......#....",
    "..#WWWRRRRRR#...",
    ".#WWWRRRRRRDD#..",
    ".#WWWRRBBBRRDD#.",
    ".#WWWRRRRRRRDD#.",
    "..#WWRRRRRRDD#..",
    "...#RRRRRRRDD#..",
    "....##DDDD##....",
    "......####......",
    "................"
)
$palPotion = @{
    "#" = HexColor "#160507"
    "C" = HexColor "#c47a37" # warm oak cork
    "K" = HexColor "#7a4014" # dark cork
    "G" = HexColor "#b8d9f5" # clean glass rim
    "W" = HexColor "#ffffff" # bright white glass gleam
    "R" = HexColor "#ff1744" # PURE VIVID CHERRY RED ELIXIR
    "B" = HexColor "#ff859d" # glowing bubble
    "D" = HexColor "#8c001a" # clean dark red shadow
}
$bmp = Draw-PixelArt $potionPattern $palPotion
Save-Sprite "Assets/Sprites/Collectibles/HealthPotion.png" $bmp 16

# Extra: Mana Potion (16x16 - Arcane Vivid Blue Elixir)
$palManaPotion = @{
    "#" = HexColor "#050e1c"
    "C" = HexColor "#c47a37"
    "K" = HexColor "#7a4014"
    "G" = HexColor "#b8d9f5"
    "W" = HexColor "#ffffff"
    "R" = HexColor "#0091ff" # pure vivid blue mana
    "B" = HexColor "#80d0ff" # bubble
    "D" = HexColor "#003e8c" # deep blue shadow
}
$bmp = Draw-PixelArt $potionPattern $palManaPotion
Save-Sprite "Assets/Sprites/Collectibles/ManaPotion.png" $bmp 16

# 5. Chest Gold (16x16 - Bold Isometric Treasure Chest with Golden Latch & Wood Panels)
$chestPattern = @(
    "................",
    "....########....",
    "..##YYYYYYYY##..",
    ".#WYOYYYYYYOYB#.",
    "#YYOOOOOOOOOYYB#",
    "#YYOOOOOOOOOYYB#",
    "#YYYYYYYYYYYYYY#",
    "#KKKKKK#YY#KKKK#",
    "#YYOOOO#KK#OYYB#",
    "#YYOOOO#YY#OYYB#",
    "#YYOOOOOOOOOYYB#",
    "#YYDDDDDDDDDYYB#",
    "#YYYYYYYYYYYYYY#",
    ".#BBBBBBBBBBBB#.",
    "..############..",
    "................"
)
$palChest = @{
    "#" = HexColor "#1c0d02" # bold dark border
    "W" = HexColor "#ffffff" # glint
    "Y" = HexColor "#ffc400" # PURE VIVID SUN GOLD (like Hero_Knight emblem!)
    "B" = HexColor "#b37400" # bronze gold shadow
    "O" = HexColor "#8c4411" # rich warm wood
    "D" = HexColor "#522405" # dark oak shadow
    "K" = HexColor "#0a0401" # keyhole & seam
}
$bmp = Draw-PixelArt $chestPattern $palChest
Save-Sprite "Assets/Sprites/Collectibles/Chest_Gold.png" $bmp 16

# 6. Magnet (16x16 - Bold Horseshoe Magnet with Solid Red Body, Chrome Tips & Electric Arc)
$magnetPattern = @(
    "................",
    ".#####....#####.",
    "#WWSS#....#WWSS#",
    "#WWSS#....#WWSS#",
    "#CCCC#....#CCCC#",
    "#RRRR#....#RRDD#",
    "#RRRR#....#RRDD#",
    "#RRRR#.ZZ.#RRDD#",
    "#RRRR#ZZZZ#RRDD#",
    "#RRRR#.ZZ.#RRDD#",
    ".#RRR#....#RDD#.",
    "..#RR######DD#..",
    "...#RRRRRRDD#...",
    "....#RRRRDD#....",
    ".....######.....",
    "................"
)
$palMagnet = @{
    "#" = HexColor "#14141e" # dark bold outline
    "W" = HexColor "#ffffff" # pure white chrome
    "S" = HexColor "#c8d2e0" # clean steel silver
    "C" = HexColor "#2b2d38" # dark collar band
    "R" = HexColor "#e62232" # PURE VIVID CHERRY RED (bold like Hero_Knight's cape!)
    "D" = HexColor "#8a0e1a" # clean dark ruby red shadow
    "Z" = HexColor "#00e5ff" # electric neon cyan spark
}
$bmp = Draw-PixelArt $magnetPattern $palMagnet
Save-Sprite "Assets/Sprites/Collectibles/Magnet.png" $bmp 16

# Extra: Coin Gold (16x16 - Bold Medieval Gold Coin)
$coinPattern = @(
    "................",
    "......####......",
    "....##YYYY##....",
    "...#WWYYYYYY#...",
    "..#WWYYYYYYYYB#.",
    "..#YYYY#YYYYYB#.",
    ".#YYYY###YYYYYB#",
    ".#YYY#####YYYYB#",
    ".#YYYY###YYYYYB#",
    ".#YYYY#.#YYYYYB#",
    "..#YYYYYYYYYYB#.",
    "..#YYYYYYYYYYB#.",
    "...#YYBBBBBB#...",
    "....##BBBB##....",
    "......####......",
    "................"
)
$palCoin = @{
    "#" = HexColor "#2b1700"
    "W" = HexColor "#ffffff"
    "Y" = HexColor "#ffc400" # PURE VIVID GOLD
    "B" = HexColor "#995c00" # bronze rim shadow
}
$bmp = Draw-PixelArt $coinPattern $palCoin
Save-Sprite "Assets/Sprites/Collectibles/Coin_Gold.png" $bmp 16


# --- ENVIRONMENT ---
# 1. Grass Tile (16x16)
$bmp = New-Canvas 16 16
$grassBase = [System.Drawing.Color]::FromArgb(255, 65, 145, 60)
$grassDark = [System.Drawing.Color]::FromArgb(255, 45, 115, 45)
$grassLight = [System.Drawing.Color]::FromArgb(255, 95, 175, 80)
for ($x=0; $x -lt 16; $x++) {
    for ($y=0; $y -lt 16; $y++) {
        $r = ($x * 7 + $y * 13) % 17
        if ($r -eq 0) { $bmp.SetPixel($x, $y, $grassDark) }
        elseif ($r -eq 3) { $bmp.SetPixel($x, $y, $grassLight) }
        else { $bmp.SetPixel($x, $y, $grassBase) }
    }
}
Save-Sprite "Assets/Sprites/Environment/Grass_Tile.png" $bmp 16

# 2. Stone Path Tile (16x16)
$bmp = New-Canvas 16 16
$stone = [System.Drawing.Color]::FromArgb(255, 130, 135, 140)
$stoneDark = [System.Drawing.Color]::FromArgb(255, 85, 90, 95)
$stoneLight = [System.Drawing.Color]::FromArgb(255, 170, 175, 180)
for ($x=0; $x -lt 16; $x++) {
    for ($y=0; $y -lt 16; $y++) {
        if ($x -eq 0 -or $x -eq 8 -or $y -eq 0 -or $y -eq 8) { $bmp.SetPixel($x, $y, $stoneDark) }
        elseif (($x + $y) % 5 -eq 0) { $bmp.SetPixel($x, $y, $stoneLight) }
        else { $bmp.SetPixel($x, $y, $stone) }
    }
}
Save-Sprite "Assets/Sprites/Environment/Stone_Path_Tile.png" $bmp 16

# 3. Tree Obstacle (32x32)
$bmp = New-Canvas 32 32
$leaves = [System.Drawing.Color]::FromArgb(255, 35, 130, 50)
$leavesLight = [System.Drawing.Color]::FromArgb(255, 60, 170, 75)
$leavesDark = [System.Drawing.Color]::FromArgb(255, 20, 85, 35)
$trunk = [System.Drawing.Color]::FromArgb(255, 100, 60, 30)
# Foliage canopy
for ($x=4; $x -le 27; $x++) {
    for ($y=2; $y -le 22; $y++) {
        $dist = [Math]::Sqrt([Math]::Pow($x - 15.5, 2) + [Math]::Pow($y - 12, 2))
        if ($dist -le 10.5) {
            if ($y -lt 8 -and $x -lt 18) { $bmp.SetPixel($x, $y, $leavesLight) }
            elseif ($y -gt 15) { $bmp.SetPixel($x, $y, $leavesDark) }
            else { $bmp.SetPixel($x, $y, $leaves) }
        }
    }
}
# Trunk
for ($x=13; $x -le 18; $x++) { for ($y=21; $y -le 31; $y++) { $bmp.SetPixel($x, $y, $trunk) } }
Save-Sprite "Assets/Sprites/Environment/Tree_Obstacle.png" $bmp 16

# 4. Rock Obstacle (16x16)
$bmp = New-Canvas 16 16
$rockCol = [System.Drawing.Color]::FromArgb(255, 120, 125, 135)
$rockH = [System.Drawing.Color]::FromArgb(255, 175, 180, 190)
$rockD = [System.Drawing.Color]::FromArgb(255, 65, 70, 80)
for ($x=2; $x -le 13; $x++) {
    for ($y=4; $y -le 14; $y++) {
        $dist = [Math]::Sqrt([Math]::Pow($x - 7.5, 2) + [Math]::Pow($y - 9.5, 2))
        if ($dist -le 5.2) {
            if ($y -le 7) { $bmp.SetPixel($x, $y, $rockH) }
            elseif ($y -ge 12) { $bmp.SetPixel($x, $y, $rockD) }
            else { $bmp.SetPixel($x, $y, $rockCol) }
        }
    }
}
Save-Sprite "Assets/Sprites/Environment/Rock_Obstacle.png" $bmp 16

# --- UI ASSETS ---
# 1. WoodFrame Panel (32x32)
$bmp = New-Canvas 32 32
$woodBorder = [System.Drawing.Color]::FromArgb(255, 60, 35, 18)
$woodFace = [System.Drawing.Color]::FromArgb(255, 115, 70, 35)
$innerBg = [System.Drawing.Color]::FromArgb(240, 30, 20, 15)
for ($x=0; $x -lt 32; $x++) {
    for ($y=0; $y -lt 32; $y++) {
        if ($x -le 1 -or $x -ge 30 -or $y -le 1 -or $y -ge 30) { $bmp.SetPixel($x, $y, $woodBorder) }
        elseif ($x -le 3 -or $x -ge 28 -or $y -le 3 -or $y -ge 28) { $bmp.SetPixel($x, $y, $woodFace) }
        else { $bmp.SetPixel($x, $y, $innerBg) }
    }
}
Save-Sprite "Assets/Sprites/UI/WoodFrame_Panel.png" $bmp 16

# 2. HealthBar Fill (32x8)
$bmp = New-Canvas 32 8
$hpBorder = [System.Drawing.Color]::FromArgb(255, 40, 10, 10)
$hpRed = [System.Drawing.Color]::FromArgb(255, 220, 35, 35)
$hpShine = [System.Drawing.Color]::FromArgb(255, 255, 110, 110)
for ($x=0; $x -lt 32; $x++) {
    for ($y=0; $y -lt 8; $y++) {
        if ($x -eq 0 -or $x -eq 31 -or $y -eq 0 -or $y -eq 7) { $bmp.SetPixel($x, $y, $hpBorder) }
        elseif ($y -eq 1) { $bmp.SetPixel($x, $y, $hpShine) }
        else { $bmp.SetPixel($x, $y, $hpRed) }
    }
}
Save-Sprite "Assets/Sprites/UI/HealthBar_Fill.png" $bmp 16

# 3. ExpBar Fill (32x8)
$bmp = New-Canvas 32 8
$expBorder = [System.Drawing.Color]::FromArgb(255, 10, 30, 40)
$expBlue = [System.Drawing.Color]::FromArgb(255, 30, 180, 230)
$expShine = [System.Drawing.Color]::FromArgb(255, 140, 230, 255)
for ($x=0; $x -lt 32; $x++) {
    for ($y=0; $y -lt 8; $y++) {
        if ($x -eq 0 -or $x -eq 31 -or $y -eq 0 -or $y -eq 7) { $bmp.SetPixel($x, $y, $expBorder) }
        elseif ($y -eq 1) { $bmp.SetPixel($x, $y, $expShine) }
        else { $bmp.SetPixel($x, $y, $expBlue) }
    }
}
Save-Sprite "Assets/Sprites/UI/ExpBar_Fill.png" $bmp 16

# 4. Button Normal (32x16)
$bmp = New-Canvas 32 16
$btnBorder = [System.Drawing.Color]::FromArgb(255, 40, 25, 15)
$btnBg = [System.Drawing.Color]::FromArgb(255, 140, 95, 50)
$btnLight = [System.Drawing.Color]::FromArgb(255, 190, 140, 80)
for ($x=0; $x -lt 32; $x++) {
    for ($y=0; $y -lt 16; $y++) {
        if ($x -eq 0 -or $x -eq 31 -or $y -eq 0 -or $y -eq 15) { $bmp.SetPixel($x, $y, $btnBorder) }
        elseif ($x -eq 1 -or $y -eq 1) { $bmp.SetPixel($x, $y, $btnLight) }
        else { $bmp.SetPixel($x, $y, $btnBg) }
    }
}
Save-Sprite "Assets/Sprites/UI/Button_Normal.png" $bmp 16

# 5. Heart Icon (16x16)
$bmp = New-Canvas 16 16
$heartRed = [System.Drawing.Color]::FromArgb(255, 230, 30, 45)
$heartShine = [System.Drawing.Color]::FromArgb(255, 255, 160, 170)
$heartBorder = [System.Drawing.Color]::FromArgb(255, 80, 10, 20)
for ($x=2; $x -le 13; $x++) {
    for ($y=3; $y -le 14; $y++) {
        if ($y -eq 3 -and ($x -eq 2 -or $x -eq 7 -or $x -eq 8 -or $x -eq 13)) { continue }
        if ($y -gt 7) {
            $offset = $y - 7
            if ($x -lt (2 + $offset) -or $x -gt (13 - $offset)) { continue }
        }
        $bmp.SetPixel($x, $y, $heartRed)
    }
}
$bmp.SetPixel(4, 5, $heartShine); $bmp.SetPixel(5, 5, $heartShine)
Save-Sprite "Assets/Sprites/UI/Heart_Icon.png" $bmp 16

Write-Output "ALL SPRITES GENERATED SUCCESSFULLY!"
