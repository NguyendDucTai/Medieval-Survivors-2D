Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.IO

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

function Set-SafePixel {
    param($bmp, [int]$x, [int]$y, [System.Drawing.Color]$col)
    if ($x -ge 0 -and $x -lt $bmp.Width -and $y -ge 0 -and $y -lt $bmp.Height) {
        $bmp.SetPixel($x, $y, $col)
    }
}

$colWhite = [System.Drawing.Color]::FromArgb(255, 255, 255, 255)
$colRed = [System.Drawing.Color]::FromArgb(255, 235, 35, 45)
$colBlack = [System.Drawing.Color]::FromArgb(255, 15, 15, 20)

function Save-AnimSheet {
    param(
        [string]$Path,
        [System.Drawing.Bitmap]$Bmp
    )
    $dir = [System.IO.Path]::GetDirectoryName($Path)
    if (-not (Test-Path $dir)) {
        [System.IO.Directory]::CreateDirectory($dir) | Out-Null
    }
    $Bmp.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
    $Bmp.Dispose()
    Write-Output "Created Animation Sheet: $Path"
}

# ==============================================================================
# 1. CHARACTER DEATH ANIMATIONS (Knight, Mage, Ranger - 64x16, 4 frames)
# ==============================================================================

# --- HERO KNIGHT DIE ---
$steel = [System.Drawing.Color]::FromArgb(255, 180, 190, 205)
$darkSteel = [System.Drawing.Color]::FromArgb(255, 100, 110, 130)
$visor = [System.Drawing.Color]::FromArgb(255, 40, 45, 60)
$gold = [System.Drawing.Color]::FromArgb(255, 245, 195, 50)
$redCape = [System.Drawing.Color]::FromArgb(255, 215, 40, 40)
$tombstone = [System.Drawing.Color]::FromArgb(255, 140, 145, 155)

$knightDie = New-Canvas 64 16
# Frame 0 (0..15): Stagger back
for ($x=4; $x -le 9; $x++) { for ($y=2; $y -le 6; $y++) { Set-SafePixel $knightDie $x $y $steel } }
for ($x=5; $x -le 8; $x++) { Set-SafePixel $knightDie $x 4 $visor }
for ($x=3; $x -le 10; $x++) { for ($y=7; $y -le 12; $y++) { Set-SafePixel $knightDie $x $y $steel } }
for ($y=4; $y -le 13; $y++) { Set-SafePixel $knightDie 2 $y $redCape; Set-SafePixel $knightDie 11 $y $redCape }
for ($x=4; $x -le 5; $x++) { for ($y=13; $y -le 15; $y++) { Set-SafePixel $knightDie $x $y $darkSteel } }
for ($x=8; $x -le 9; $x++) { for ($y=13; $y -le 15; $y++) { Set-SafePixel $knightDie $x $y $darkSteel } }

# Frame 1 (16..31): Dropping to knees
for ($x=5; $x -le 10; $x++) { for ($y=5; $y -le 9; $y++) { Set-SafePixel $knightDie (16+$x) $y $steel } }
for ($x=6; $x -le 9; $x++) { Set-SafePixel $knightDie (16+$x) 7 $visor }
for ($x=4; $x -le 11; $x++) { for ($y=10; $y -le 13; $y++) { Set-SafePixel $knightDie (16+$x) $y $steel } }
for ($y=7; $y -le 14; $y++) { Set-SafePixel $knightDie (16+3) $y $redCape; Set-SafePixel $knightDie (16+12) $y $redCape }
for ($x=4; $x -le 11; $x++) { for ($y=14; $y -le 15; $y++) { Set-SafePixel $knightDie (16+$x) $y $darkSteel } }

# Frame 2 (32..47): Collapsed on ground
for ($x=2; $x -le 13; $x++) { for ($y=11; $y -le 14; $y++) { Set-SafePixel $knightDie (32+$x) $y $steel } }
for ($x=2; $x -le 5; $x++) { for ($y=10; $y -le 13; $y++) { Set-SafePixel $knightDie (32+$x) $y $steel } }
Set-SafePixel $knightDie (32+3) 11 $visor; Set-SafePixel $knightDie (32+4) 11 $visor
for ($x=6; $x -le 14; $x++) { Set-SafePixel $knightDie (32+$x) 15 $redCape }
for ($x=11; $x -le 14; $x++) { for ($y=12; $y -le 14; $y++) { Set-SafePixel $knightDie (32+$x) $y $darkSteel } }

# Frame 3 (48..63): Heroic Sword resting upright & Fallen Helmet
# Sword upright at x=7..9
for ($y=5; $y -le 14; $y++) { Set-SafePixel $knightDie (48+8) $y $steel }
for ($x=6; $x -le 10; $x++) { Set-SafePixel $knightDie (48+$x) 6 $gold }
Set-SafePixel $knightDie (48+8) 4 $darkSteel; Set-SafePixel $knightDie (48+8) 5 $darkSteel
# Red ribbon on sword
Set-SafePixel $knightDie (48+9) 7 $redCape; Set-SafePixel $knightDie (48+10) 8 $redCape
# Fallen Helmet beside sword at x=2..5, y=11..14
for ($x=2; $x -le 5; $x++) { for ($y=12; $y -le 15; $y++) { Set-SafePixel $knightDie (48+$x) $y $steel } }
Set-SafePixel $knightDie (48+3) 13 $visor; Set-SafePixel $knightDie (48+4) 13 $visor

Save-AnimSheet "Assets/Sprites/Characters/Hero_Knight_Die.png" $knightDie

# --- HERO MAGE DIE ---
$mageHat = [System.Drawing.Color]::FromArgb(255, 60, 35, 130)
$mageRobe = [System.Drawing.Color]::FromArgb(255, 95, 60, 190)
$mageGold = [System.Drawing.Color]::FromArgb(255, 255, 215, 60)
$mageSkin = [System.Drawing.Color]::FromArgb(255, 250, 210, 165)
$mageBeard = [System.Drawing.Color]::FromArgb(255, 235, 240, 250)
$manaSpark = [System.Drawing.Color]::FromArgb(255, 0, 229, 255)

$mageDie = New-Canvas 64 16
# Frame 0: Stagger back
for ($x=5; $x -le 10; $x++) { for ($y=3; $y -le 6; $y++) { Set-SafePixel $mageDie $x $y $mageHat } }
for ($x=6; $x -le 9; $x++) { Set-SafePixel $mageDie $x 7 $mageSkin }
for ($x=5; $x -le 10; $x++) { Set-SafePixel $mageDie $x 8 $mageBeard }
for ($x=4; $x -le 11; $x++) { for ($y=9; $y -le 15; $y++) { Set-SafePixel $mageDie $x $y $mageRobe } }

# Frame 1: Dropping to knees, staff falling
for ($x=5; $x -le 10; $x++) { for ($y=6; $y -le 9; $y++) { Set-SafePixel $mageDie (16+$x) $y $mageHat } }
for ($x=6; $x -le 9; $x++) { Set-SafePixel $mageDie (16+$x) 10 $mageSkin }
for ($x=4; $x -le 11; $x++) { for ($y=11; $y -le 15; $y++) { Set-SafePixel $mageDie (16+$x) $y $mageRobe } }

# Frame 2: Robe pooling on ground
for ($x=2; $x -le 13; $x++) { for ($y=12; $y -le 15; $y++) { Set-SafePixel $mageDie (32+$x) $y $mageRobe } }
for ($x=5; $x -le 9; $x++) { for ($y=9; $y -le 12; $y++) { Set-SafePixel $mageDie (32+$x) $y $mageHat } }

# Frame 3: Empty Robe & Hat with glowing mana dust
for ($x=3; $x -le 12; $x++) { for ($y=13; $y -le 15; $y++) { Set-SafePixel $mageDie (48+$x) $y $mageRobe } }
for ($x=5; $x -le 8; $x++) { for ($y=11; $y -le 13; $y++) { Set-SafePixel $mageDie (48+$x) $y $mageHat } }
Set-SafePixel $mageDie (48+4) 8 $manaSpark; Set-SafePixel $mageDie (48+9) 6 $manaSpark; Set-SafePixel $mageDie (48+11) 9 $manaSpark

Save-AnimSheet "Assets/Sprites/Characters/Hero_Mage_Die.png" $mageDie

# --- HERO RANGER DIE ---
$rangerGreen = [System.Drawing.Color]::FromArgb(255, 45, 135, 65)
$rangerLeather = [System.Drawing.Color]::FromArgb(255, 145, 90, 40)
$rangerFeather = [System.Drawing.Color]::FromArgb(255, 235, 45, 45)
$rangerSkin = [System.Drawing.Color]::FromArgb(255, 245, 195, 150)

$rangerDie = New-Canvas 64 16
# Frame 0: Stagger back
Set-SafePixel $rangerDie 10 1 $rangerFeather
for ($x=5; $x -le 10; $x++) { for ($y=2; $y -le 4; $y++) { Set-SafePixel $rangerDie $x $y $rangerGreen } }
for ($x=6; $x -le 9; $x++) { Set-SafePixel $rangerDie $x 5 $rangerSkin }
for ($x=4; $x -le 11; $x++) { for ($y=6; $y -le 12; $y++) { Set-SafePixel $rangerDie $x $y $rangerGreen } }
for ($x=5; $x -le 10; $x++) { for ($y=13; $y -le 15; $y++) { Set-SafePixel $rangerDie $x $y $rangerLeather } }

# Frame 1: Down to one knee
Set-SafePixel $rangerDie (16+10) 4 $rangerFeather
for ($x=5; $x -le 10; $x++) { for ($y=5; $y -le 7; $y++) { Set-SafePixel $rangerDie (16+$x) $y $rangerGreen } }
for ($x=6; $x -le 9; $x++) { Set-SafePixel $rangerDie (16+$x) 8 $rangerSkin }
for ($x=4; $x -le 11; $x++) { for ($y=9; $y -le 13; $y++) { Set-SafePixel $rangerDie (16+$x) $y $rangerGreen } }
for ($x=5; $x -le 11; $x++) { for ($y=14; $y -le 15; $y++) { Set-SafePixel $rangerDie (16+$x) $y $rangerLeather } }

# Frame 2: Collapsing forward
for ($x=3; $x -le 13; $x++) { for ($y=10; $y -le 14; $y++) { Set-SafePixel $rangerDie (32+$x) $y $rangerGreen } }
for ($x=2; $x -le 5; $x++) { for ($y=9; $y -le 11; $y++) { Set-SafePixel $rangerDie (32+$x) $y $rangerGreen } }
Set-SafePixel $rangerDie (32+4) 8 $rangerFeather
for ($x=10; $x -le 14; $x++) { for ($y=12; $y -le 14; $y++) { Set-SafePixel $rangerDie (32+$x) $y $rangerLeather } }

# Frame 3: Lying resting flat on grass
for ($x=2; $x -le 13; $x++) { for ($y=12; $y -le 15; $y++) { Set-SafePixel $rangerDie (48+$x) $y $rangerGreen } }
for ($x=2; $x -le 5; $x++) { for ($y=11; $y -le 13; $y++) { Set-SafePixel $rangerDie (48+$x) $y $rangerGreen } }
Set-SafePixel $rangerDie (48+3) 10 $rangerFeather
for ($x=11; $x -le 14; $x++) { for ($y=13; $y -le 15; $y++) { Set-SafePixel $rangerDie (48+$x) $y $rangerLeather } }

Save-AnimSheet "Assets/Sprites/Characters/Hero_Ranger_Die.png" $rangerDie


# ==============================================================================
# 2. ENEMY 1: SLIME ANIMATIONS (Attack, Die - 64x16)
# ==============================================================================
$slimeCol = [System.Drawing.Color]::FromArgb(255, 60, 220, 90)
$slimeHi = [System.Drawing.Color]::FromArgb(255, 170, 250, 140)
$slimeEye = [System.Drawing.Color]::FromArgb(255, 10, 10, 10)
$slimeAcid = [System.Drawing.Color]::FromArgb(255, 230, 255, 80)

# Slime Attack
$slimeAtk = New-Canvas 64 16
# Frame 0: Squash down
for ($x=1; $x -le 14; $x++) { for ($y=10; $y -le 15; $y++) { Set-SafePixel $slimeAtk $x $y $slimeCol } }
Set-SafePixel $slimeAtk 4 11 $slimeHi; Set-SafePixel $slimeAtk 3 13 $slimeEye; Set-SafePixel $slimeAtk 11 13 $slimeEye
# Frame 1: Stretch forward lunge with spikes
for ($x=4; $x -le 13; $x++) { for ($y=4; $y -le 13; $y++) { Set-SafePixel $slimeAtk (16+$x) $y $slimeCol } }
Set-SafePixel $slimeAtk (16+14) 7 $slimeCol; Set-SafePixel $slimeAtk (16+15) 8 $slimeAcid
Set-SafePixel $slimeAtk (16+8) 6 $slimeEye; Set-SafePixel $slimeAtk (16+12) 6 $slimeEye
# Frame 2: Max bite strike
for ($x=5; $x -le 15; $x++) { for ($y=5; $y -le 13; $y++) { Set-SafePixel $slimeAtk (32+$x) $y $slimeCol } }
Set-SafePixel $slimeAtk (32+12) 8 $colWhite; Set-SafePixel $slimeAtk (32+13) 9 $colWhite
Set-SafePixel $slimeAtk (32+14) 10 $slimeAcid
# Frame 3: Land bounce
for ($x=3; $x -le 13; $x++) { for ($y=7; $y -le 15; $y++) { Set-SafePixel $slimeAtk (48+$x) $y $slimeCol } }
Set-SafePixel $slimeAtk (48+5) 8 $slimeHi; Set-SafePixel $slimeAtk (48+5) 10 $slimeEye; Set-SafePixel $slimeAtk (48+10) 10 $slimeEye
Save-AnimSheet "Assets/Sprites/Enemies/Slime_Attack.png" $slimeAtk

# Slime Die
$slimeDie = New-Canvas 64 16
# Frame 0: Shaking distortion
for ($x=2; $x -le 13; $x++) { for ($y=7; $y -le 14; $y++) { Set-SafePixel $slimeDie $x $y $slimeCol } }
Set-SafePixel $slimeDie 4 8 $colRed; Set-SafePixel $slimeDie 10 8 $colRed
# Frame 1: Popping into green globules
for ($x=2; $x -le 5; $x++) { for ($y=6; $y -le 9; $y++) { Set-SafePixel $slimeDie (16+$x) $y $slimeCol } }
for ($x=10; $x -le 13; $x++) { for ($y=6; $y -le 9; $y++) { Set-SafePixel $slimeDie (16+$x) $y $slimeCol } }
for ($x=6; $x -le 9; $x++) { for ($y=11; $y -le 14; $y++) { Set-SafePixel $slimeDie (16+$x) $y $slimeCol } }
# Frame 2: Splatter puddle
for ($x=1; $x -le 14; $x++) { for ($y=13; $y -le 15; $y++) { Set-SafePixel $slimeDie (32+$x) $y $slimeCol } }
Set-SafePixel $slimeDie (32+4) 12 $slimeAcid; Set-SafePixel $slimeDie (32+11) 12 $slimeAcid
# Frame 3: Evaporating bubbles
Set-SafePixel $slimeDie (48+3) 14 $slimeCol; Set-SafePixel $slimeDie (48+7) 13 $slimeHi; Set-SafePixel $slimeDie (48+12) 14 $slimeCol
Save-AnimSheet "Assets/Sprites/Enemies/Slime_Die.png" $slimeDie


# ==============================================================================
# 3. ENEMY 2: SKELETON ANIMATIONS (Walk, Attack, Die - 64x16)
# ==============================================================================
$bone = [System.Drawing.Color]::FromArgb(255, 235, 235, 225)
$boneDark = [System.Drawing.Color]::FromArgb(255, 140, 140, 135)
$boneEye = [System.Drawing.Color]::FromArgb(255, 20, 20, 25)
$rustySword = [System.Drawing.Color]::FromArgb(255, 160, 100, 70)
$slashTrail = [System.Drawing.Color]::FromArgb(255, 255, 255, 255)

# Skeleton Walk
$skelWalk = New-Canvas 64 16
for ($f=0; $f -lt 4; $f++) {
    $ox = $f * 16
    $bob = ($f % 2) # 0 or 1
    # Skull
    for ($x=5; $x -le 10; $x++) { for ($y=1+$bob; $y -le 5+$bob; $y++) { Set-SafePixel $skelWalk ($ox+$x) $y $bone } }
    Set-SafePixel $skelWalk ($ox+6) (3+$bob) $boneEye; Set-SafePixel $skelWalk ($ox+9) (3+$bob) $boneEye
    # Spine & Ribs
    for ($y=6+$bob; $y -le 10+$bob; $y++) { Set-SafePixel $skelWalk ($ox+7) $y $bone; Set-SafePixel $skelWalk ($ox+8) $y $bone }
    for ($x=5; $x -le 10; $x++) { Set-SafePixel $skelWalk ($ox+$x) (7+$bob) $bone; Set-SafePixel $skelWalk ($ox+$x) (9+$bob) $bone }
    # Legs based on frame
    if ($f -eq 0) {
        for ($y=11; $y -le 15; $y++) { Set-SafePixel $skelWalk ($ox+4) $y $boneDark; Set-SafePixel $skelWalk ($ox+9) $y $bone }
    } elseif ($f -eq 1) {
        for ($y=11; $y -le 15; $y++) { Set-SafePixel $skelWalk ($ox+6) $y $bone; Set-SafePixel $skelWalk ($ox+8) $y $bone }
    } elseif ($f -eq 2) {
        for ($y=11; $y -le 15; $y++) { Set-SafePixel $skelWalk ($ox+5) $y $bone; Set-SafePixel $skelWalk ($ox+10) $y $boneDark }
    } else {
        for ($y=11; $y -le 15; $y++) { Set-SafePixel $skelWalk ($ox+7) $y $bone; Set-SafePixel $skelWalk ($ox+8) $y $bone }
    }
}
Save-AnimSheet "Assets/Sprites/Enemies/Skeleton_Walk.png" $skelWalk

# Skeleton Attack
$skelAtk = New-Canvas 64 16
# Frame 0: Windup
for ($x=4; $x -le 9; $x++) { for ($y=1; $y -le 5; $y++) { Set-SafePixel $skelAtk $x $y $bone } }
Set-SafePixel $skelAtk 5 3 $boneEye; Set-SafePixel $skelAtk 8 3 $boneEye
for ($y=6; $y -le 11; $y++) { Set-SafePixel $skelAtk 6 $y $bone; Set-SafePixel $skelAtk 7 $y $bone }
for ($x=1; $x -le 3; $x++) { for ($y=2; $y -le 4; $y++) { Set-SafePixel $skelAtk $x $y $rustySword } }
for ($x=5; $x -le 8; $x++) { for ($y=12; $y -le 15; $y++) { Set-SafePixel $skelAtk $x $y $bone } }

# Frame 1: Thrust forward
for ($x=5; $x -le 10; $x++) { for ($y=2; $y -le 6; $y++) { Set-SafePixel $skelAtk (16+$x) $y $bone } }
for ($y=7; $y -le 11; $y++) { Set-SafePixel $skelAtk (16+7) $y $bone; Set-SafePixel $skelAtk (16+8) $y $bone }
for ($x=11; $x -le 15; $x++) { Set-SafePixel $skelAtk (16+$x) 7 $rustySword }
for ($x=6; $x -le 9; $x++) { for ($y=12; $y -le 15; $y++) { Set-SafePixel $skelAtk (16+$x) $y $bone } }

# Frame 2: Slash Arc
for ($x=5; $x -le 10; $x++) { for ($y=2; $y -le 6; $y++) { Set-SafePixel $skelAtk (32+$x) $y $bone } }
for ($y=7; $y -le 11; $y++) { Set-SafePixel $skelAtk (32+7) $y $bone }
for ($x=11; $x -le 15; $x++) { Set-SafePixel $skelAtk (32+$x) 10 $rustySword }
# White slash arc
for ($y=4; $y -le 12; $y++) { Set-SafePixel $skelAtk (32+14) $y $slashTrail }

# Frame 3: Recovery
for ($x=5; $x -le 10; $x++) { for ($y=1; $y -le 5; $y++) { Set-SafePixel $skelAtk (48+$x) $y $bone } }
for ($y=6; $y -le 11; $y++) { Set-SafePixel $skelAtk (48+7) $y $bone }
for ($y=6; $y -le 11; $y++) { Set-SafePixel $skelAtk (48+11) $y $rustySword }
for ($x=6; $x -le 9; $x++) { for ($y=12; $y -le 15; $y++) { Set-SafePixel $skelAtk (48+$x) $y $bone } }
Save-AnimSheet "Assets/Sprites/Enemies/Skeleton_Attack.png" $skelAtk

# Skeleton Die
$skelDie = New-Canvas 64 16
# Frame 0: Crack
for ($x=5; $x -le 10; $x++) { for ($y=1; $y -le 5; $y++) { Set-SafePixel $skelDie $x $y $bone } }
Set-SafePixel $skelDie 7 2 $boneDark; Set-SafePixel $skelDie 8 3 $boneDark
for ($y=6; $y -le 11; $y++) { Set-SafePixel $skelDie 7 $y $bone }
for ($x=5; $x -le 9; $x++) { for ($y=12; $y -le 15; $y++) { Set-SafePixel $skelDie $x $y $bone } }

# Frame 1: Collapse falling
for ($x=3; $x -le 7; $x++) { for ($y=5; $y -le 8; $y++) { Set-SafePixel $skelDie (16+$x) $y $bone } }
for ($x=8; $x -le 12; $x++) { for ($y=7; $y -le 10; $y++) { Set-SafePixel $skelDie (16+$x) $y $bone } }
for ($x=4; $x -le 11; $x++) { for ($y=12; $y -le 15; $y++) { Set-SafePixel $skelDie (16+$x) $y $boneDark } }

# Frame 2: Bones scattering
for ($x=2; $x -le 5; $x++) { for ($y=10; $y -le 13; $y++) { Set-SafePixel $skelDie (32+$x) $y $bone } }
for ($x=7; $x -le 14; $x++) { Set-SafePixel $skelDie (32+$x) 14 $bone }

# Frame 3: Bone pile on ground
for ($x=3; $x -le 6; $x++) { for ($y=12; $y -le 15; $y++) { Set-SafePixel $skelDie (48+$x) $y $bone } }
Set-SafePixel $skelDie (48+4) 13 $boneEye; Set-SafePixel $skelDie (48+5) 13 $boneEye
for ($x=7; $x -le 13; $x++) { for ($y=13; $y -le 15; $y++) { Set-SafePixel $skelDie (48+$x) $y $boneDark } }
Save-AnimSheet "Assets/Sprites/Enemies/Skeleton_Die.png" $skelDie


# ==============================================================================
# 4. ENEMY 3: GOBLIN ANIMATIONS (Walk, Attack, Die - 64x16)
# ==============================================================================
$goblinSkin = [System.Drawing.Color]::FromArgb(255, 70, 160, 50)
$goblinDark = [System.Drawing.Color]::FromArgb(255, 40, 110, 30)
$goblinCloth = [System.Drawing.Color]::FromArgb(255, 130, 80, 40)
$goblinEye = [System.Drawing.Color]::FromArgb(255, 255, 40, 40)
$daggerMetal = [System.Drawing.Color]::FromArgb(255, 200, 210, 220)

# Goblin Walk
$gobWalk = New-Canvas 64 16
for ($f=0; $f -lt 4; $f++) {
    $ox = $f * 16
    $ear = ($f % 2)
    # Head & Ears
    for ($x=5; $x -le 10; $x++) { for ($y=4; $y -le 8; $y++) { Set-SafePixel $gobWalk ($ox+$x) $y $goblinSkin } }
    Set-SafePixel $gobWalk ($ox+3) (5+$ear) $goblinSkin; Set-SafePixel $gobWalk ($ox+4) (5+$ear) $goblinSkin
    Set-SafePixel $gobWalk ($ox+11) (5+$ear) $goblinSkin; Set-SafePixel $gobWalk ($ox+12) (5+$ear) $goblinSkin
    Set-SafePixel $gobWalk ($ox+6) 6 $goblinEye; Set-SafePixel $gobWalk ($ox+9) 6 $goblinEye
    # Torso
    for ($x=5; $x -le 10; $x++) { for ($y=9; $y -le 12; $y++) { Set-SafePixel $gobWalk ($ox+$x) $y $goblinCloth } }
    # Feet
    if ($f -eq 0 -or $f -eq 1) {
        for ($x=4; $x -le 6; $x++) { for ($y=13; $y -le 15; $y++) { Set-SafePixel $gobWalk ($ox+$x) $y $goblinDark } }
        for ($x=9; $x -le 11; $x++) { for ($y=13; $y -le 14; $y++) { Set-SafePixel $gobWalk ($ox+$x) $y $goblinDark } }
    } else {
        for ($x=4; $x -le 6; $x++) { for ($y=13; $y -le 14; $y++) { Set-SafePixel $gobWalk ($ox+$x) $y $goblinDark } }
        for ($x=9; $x -le 11; $x++) { for ($y=13; $y -le 15; $y++) { Set-SafePixel $gobWalk ($ox+$x) $y $goblinDark } }
    }
}
Save-AnimSheet "Assets/Sprites/Enemies/Goblin_Walk.png" $gobWalk

# Goblin Attack
$gobAtk = New-Canvas 64 16
# Frame 0: Crouch windup
for ($x=5; $x -le 10; $x++) { for ($y=5; $y -le 9; $y++) { Set-SafePixel $gobAtk $x $y $goblinSkin } }
Set-SafePixel $gobAtk 6 7 $goblinEye; Set-SafePixel $gobAtk 9 7 $goblinEye
for ($x=5; $x -le 10; $x++) { for ($y=10; $y -le 14; $y++) { Set-SafePixel $gobAtk $x $y $goblinCloth } }
Set-SafePixel $gobAtk 3 8 $daggerMetal; Set-SafePixel $gobAtk 2 7 $daggerMetal

# Frame 1: Lunge forward
for ($x=7; $x -le 12; $x++) { for ($y=4; $y -le 8; $y++) { Set-SafePixel $gobAtk (16+$x) $y $goblinSkin } }
for ($x=6; $x -le 11; $x++) { for ($y=9; $y -le 13; $y++) { Set-SafePixel $gobAtk (16+$x) $y $goblinCloth } }
for ($x=13; $x -le 15; $x++) { Set-SafePixel $gobAtk (16+$x) 6 $daggerMetal }

# Frame 2: Slash down
for ($x=6; $x -le 11; $x++) { for ($y=4; $y -le 8; $y++) { Set-SafePixel $gobAtk (32+$x) $y $goblinSkin } }
for ($x=5; $x -le 10; $x++) { for ($y=9; $y -le 13; $y++) { Set-SafePixel $gobAtk (32+$x) $y $goblinCloth } }
for ($x=12; $x -le 15; $x++) { Set-SafePixel $gobAtk (32+$x) 10 $daggerMetal }
Set-SafePixel $gobAtk (32+14) 7 $slashTrail; Set-SafePixel $gobAtk (32+14) 8 $slashTrail

# Frame 3: Recovery
for ($x=5; $x -le 10; $x++) { for ($y=4; $y -le 8; $y++) { Set-SafePixel $gobAtk (48+$x) $y $goblinSkin } }
for ($x=5; $x -le 10; $x++) { for ($y=9; $y -le 13; $y++) { Set-SafePixel $gobAtk (48+$x) $y $goblinCloth } }
Save-AnimSheet "Assets/Sprites/Enemies/Goblin_Attack.png" $gobAtk

# Goblin Die
$gobDie = New-Canvas 64 16
# Frame 0: Stagger
for ($x=4; $x -le 9; $x++) { for ($y=4; $y -le 8; $y++) { Set-SafePixel $gobDie $x $y $goblinSkin } }
for ($x=4; $x -le 9; $x++) { for ($y=9; $y -le 13; $y++) { Set-SafePixel $gobDie $x $y $goblinCloth } }
# Frame 1: Tumbling backward
for ($x=3; $x -le 7; $x++) { for ($y=7; $y -le 11; $y++) { Set-SafePixel $gobDie (16+$x) $y $goblinSkin } }
for ($x=8; $x -le 12; $x++) { for ($y=9; $y -le 13; $y++) { Set-SafePixel $gobDie (16+$x) $y $goblinCloth } }
# Frame 2: Lying on back
for ($x=2; $x -le 13; $x++) { for ($y=12; $y -le 15; $y++) { Set-SafePixel $gobDie (32+$x) $y $goblinSkin } }
# Frame 3: Dissolving smoke
Set-SafePixel $gobDie (48+4) 11 $goblinDark; Set-SafePixel $gobDie (48+8) 10 $goblinDark; Set-SafePixel $gobDie (48+11) 12 $goblinDark
Save-AnimSheet "Assets/Sprites/Enemies/Goblin_Die.png" $gobDie


# ==============================================================================
# 5. ENEMY 4: BAT ANIMATIONS (Fly, Attack, Die - 64x16)
# ==============================================================================
$batBody = [System.Drawing.Color]::FromArgb(255, 60, 40, 80)
$batWing = [System.Drawing.Color]::FromArgb(255, 35, 20, 50)
$batEye = [System.Drawing.Color]::FromArgb(255, 255, 30, 60)

# Bat Fly
$batFly = New-Canvas 64 16
# Frame 0: Wings High
for ($x=6; $x -le 9; $x++) { for ($y=6; $y -le 10; $y++) { Set-SafePixel $batFly $x $y $batBody } }
Set-SafePixel $batFly 7 7 $batEye; Set-SafePixel $batFly 8 7 $batEye
for ($x=1; $x -le 5; $x++) { for ($y=1; $y -le 6; $y++) { if ($x + $y -ge 5) { Set-SafePixel $batFly $x $y $batWing } } }
for ($x=10; $x -le 14; $x++) { for ($y=1; $y -le 6; $y++) { if ($x - $y -le 10) { Set-SafePixel $batFly $x $y $batWing } } }

# Frame 1: Wings Flat
for ($x=6; $x -le 9; $x++) { for ($y=6; $y -le 10; $y++) { Set-SafePixel $batFly (16+$x) $y $batBody } }
Set-SafePixel $batFly (16+7) 7 $batEye; Set-SafePixel $batFly (16+8) 7 $batEye
for ($x=1; $x -le 5; $x++) { for ($y=6; $y -le 8; $y++) { Set-SafePixel $batFly (16+$x) $y $batWing } }
for ($x=10; $x -le 14; $x++) { for ($y=6; $y -le 8; $y++) { Set-SafePixel $batFly (16+$x) $y $batWing } }

# Frame 2: Wings Down
for ($x=6; $x -le 9; $x++) { for ($y=5; $y -le 9; $y++) { Set-SafePixel $batFly (32+$x) $y $batBody } }
Set-SafePixel $batFly (32+7) 6 $batEye; Set-SafePixel $batFly (32+8) 6 $batEye
for ($x=2; $x -le 5; $x++) { for ($y=9; $y -le 14; $y++) { Set-SafePixel $batFly (32+$x) $y $batWing } }
for ($x=10; $x -le 13; $x++) { for ($y=9; $y -le 14; $y++) { Set-SafePixel $batFly (32+$x) $y $batWing } }

# Frame 3: Glide
for ($x=6; $x -le 9; $x++) { for ($y=6; $y -le 10; $y++) { Set-SafePixel $batFly (48+$x) $y $batBody } }
Set-SafePixel $batFly (48+7) 7 $batEye; Set-SafePixel $batFly (48+8) 7 $batEye
for ($x=2; $x -le 5; $x++) { for ($y=5; $y -le 8; $y++) { Set-SafePixel $batFly (48+$x) $y $batWing } }
for ($x=10; $x -le 13; $x++) { for ($y=5; $y -le 8; $y++) { Set-SafePixel $batFly (48+$x) $y $batWing } }
Save-AnimSheet "Assets/Sprites/Enemies/Bat_Fly.png" $batFly

# Bat Attack
$batAtk = New-Canvas 64 16
# Frame 0: Fangs
for ($x=6; $x -le 9; $x++) { for ($y=6; $y -le 10; $y++) { Set-SafePixel $batAtk $x $y $batBody } }
Set-SafePixel $batAtk 7 8 $colWhite; Set-SafePixel $batAtk 8 8 $colWhite
# Frame 1: Dive
for ($x=6; $x -le 9; $x++) { for ($y=8; $y -le 12; $y++) { Set-SafePixel $batAtk (16+$x) $y $batBody } }
Set-SafePixel $batAtk (16+7) 9 $batEye; Set-SafePixel $batAtk (16+8) 9 $batEye
# Frame 2: Bite snap
for ($x=7; $x -le 11; $x++) { for ($y=8; $y -le 12; $y++) { Set-SafePixel $batAtk (32+$x) $y $batBody } }
Set-SafePixel $batAtk (32+12) 10 $colRed; Set-SafePixel $batAtk (32+13) 9 $colRed
# Frame 3: Pull up
for ($x=6; $x -le 9; $x++) { for ($y=5; $y -le 9; $y++) { Set-SafePixel $batAtk (48+$x) $y $batBody } }
Save-AnimSheet "Assets/Sprites/Enemies/Bat_Attack.png" $batAtk

# Bat Die
$batDie = New-Canvas 64 16
# Frame 0: Wings fold
for ($x=6; $x -le 9; $x++) { for ($y=5; $y -le 10; $y++) { Set-SafePixel $batDie $x $y $batBody } }
# Frame 1: Spin
for ($x=6; $x -le 9; $x++) { for ($y=7; $y -le 12; $y++) { Set-SafePixel $batDie (16+$x) $y $batWing } }
# Frame 2: Crash ground
for ($x=5; $x -le 10; $x++) { for ($y=12; $y -le 15; $y++) { Set-SafePixel $batDie (32+$x) $y $batBody } }
# Frame 3: Puff
Set-SafePixel $batDie (48+7) 12 $batWing; Set-SafePixel $batDie (48+8) 11 $batWing; Set-SafePixel $batDie (48+6) 13 $batWing
Save-AnimSheet "Assets/Sprites/Enemies/Bat_Die.png" $batDie


# ==============================================================================
# 6. ENEMY 5: BOSS GOLEM ANIMATIONS (Walk, Attack, Die - 64x16)
# ==============================================================================
$stone = [System.Drawing.Color]::FromArgb(255, 120, 130, 140)
$stoneDark = [System.Drawing.Color]::FromArgb(255, 70, 80, 90)
$runeGlow = [System.Drawing.Color]::FromArgb(255, 0, 229, 255)

# Boss Golem Walk
$golemWalk = New-Canvas 64 16
for ($f=0; $f -lt 4; $f++) {
    $ox = $f * 16
    $bob = ($f % 2)
    # Head & Shoulders
    for ($x=4; $x -le 11; $x++) { for ($y=2+$bob; $y -le 5+$bob; $y++) { Set-SafePixel $golemWalk ($ox+$x) $y $stone } }
    Set-SafePixel $golemWalk ($ox+6) (3+$bob) $runeGlow; Set-SafePixel $golemWalk ($ox+9) (3+$bob) $runeGlow
    # Core Chest
    for ($x=3; $x -le 12; $x++) { for ($y=6+$bob; $y -le 10+$bob; $y++) { Set-SafePixel $golemWalk ($ox+$x) $y $stoneDark } }
    Set-SafePixel $golemWalk ($ox+7) (7+$bob) $runeGlow; Set-SafePixel $golemWalk ($ox+8) (7+$bob) $runeGlow
    # Heavy stone legs
    if ($f -eq 0) {
        for ($x=3; $x -le 6; $x++) { for ($y=11; $y -le 15; $y++) { Set-SafePixel $golemWalk ($ox+$x) $y $stone } }
        for ($x=9; $x -le 12; $x++) { for ($y=11; $y -le 14; $y++) { Set-SafePixel $golemWalk ($ox+$x) $y $stoneDark } }
    } elseif ($f -eq 2) {
        for ($x=3; $x -le 6; $x++) { for ($y=11; $y -le 14; $y++) { Set-SafePixel $golemWalk ($ox+$x) $y $stoneDark } }
        for ($x=9; $x -le 12; $x++) { for ($y=11; $y -le 15; $y++) { Set-SafePixel $golemWalk ($ox+$x) $y $stone } }
    } else {
        for ($x=3; $x -le 6; $x++) { for ($y=11; $y -le 15; $y++) { Set-SafePixel $golemWalk ($ox+$x) $y $stone } }
        for ($x=9; $x -le 12; $x++) { for ($y=11; $y -le 15; $y++) { Set-SafePixel $golemWalk ($ox+$x) $y $stone } }
    }
}
Save-AnimSheet "Assets/Sprites/Enemies/Boss_Golem_Walk.png" $golemWalk

# Boss Golem Attack (Fist Slam)
$golemAtk = New-Canvas 64 16
# Frame 0: Fists High
for ($x=4; $x -le 11; $x++) { for ($y=4; $y -le 9; $y++) { Set-SafePixel $golemAtk $x $y $stone } }
for ($x=1; $x -le 4; $x++) { for ($y=1; $y -le 4; $y++) { Set-SafePixel $golemAtk $x $y $stoneDark } }
for ($x=11; $x -le 14; $x++) { for ($y=1; $y -le 4; $y++) { Set-SafePixel $golemAtk $x $y $stoneDark } }
for ($x=4; $x -le 11; $x++) { for ($y=10; $y -le 15; $y++) { Set-SafePixel $golemAtk $x $y $stoneDark } }

# Frame 1: Slamming Down
for ($x=4; $x -le 11; $x++) { for ($y=4; $y -le 9; $y++) { Set-SafePixel $golemAtk (16+$x) $y $stone } }
for ($x=2; $x -le 5; $x++) { for ($y=8; $y -le 12; $y++) { Set-SafePixel $golemAtk (16+$x) $y $stoneDark } }
for ($x=10; $x -le 13; $x++) { for ($y=8; $y -le 12; $y++) { Set-SafePixel $golemAtk (16+$x) $y $stoneDark } }
for ($x=5; $x -le 10; $x++) { for ($y=10; $y -le 15; $y++) { Set-SafePixel $golemAtk (16+$x) $y $stoneDark } }

# Frame 2: Ground Impact & Shockwave
for ($x=4; $x -le 11; $x++) { for ($y=5; $y -le 10; $y++) { Set-SafePixel $golemAtk (32+$x) $y $stone } }
for ($x=2; $x -le 5; $x++) { for ($y=12; $y -le 15; $y++) { Set-SafePixel $golemAtk (32+$x) $y $stoneDark } }
for ($x=10; $x -le 13; $x++) { for ($y=12; $y -le 15; $y++) { Set-SafePixel $golemAtk (32+$x) $y $stoneDark } }
# Shockwave sparks
Set-SafePixel $golemAtk (32+0) 14 $runeGlow; Set-SafePixel $golemAtk (32+1) 13 $runeGlow
Set-SafePixel $golemAtk (32+14) 13 $runeGlow; Set-SafePixel $golemAtk (32+15) 14 $runeGlow

# Frame 3: Recovery
for ($x=4; $x -le 11; $x++) { for ($y=3; $y -le 8; $y++) { Set-SafePixel $golemAtk (48+$x) $y $stone } }
for ($x=3; $x -le 12; $x++) { for ($y=9; $y -le 15; $y++) { Set-SafePixel $golemAtk (48+$x) $y $stoneDark } }
Save-AnimSheet "Assets/Sprites/Enemies/Boss_Golem_Attack.png" $golemAtk

# Boss Golem Die
$golemDie = New-Canvas 64 16
# Frame 0: Runes extinguish, cracks appear
for ($x=4; $x -le 11; $x++) { for ($y=2; $y -le 6; $y++) { Set-SafePixel $golemDie $x $y $stone } }
for ($x=3; $x -le 12; $x++) { for ($y=7; $y -le 15; $y++) { Set-SafePixel $golemDie $x $y $stoneDark } }
Set-SafePixel $golemDie 7 4 $colBlack; Set-SafePixel $golemDie 8 8 $colBlack

# Frame 1: Torso breaking apart
for ($x=3; $x -le 7; $x++) { for ($y=4; $y -le 9; $y++) { Set-SafePixel $golemDie (16+$x) $y $stone } }
for ($x=8; $x -le 12; $x++) { for ($y=5; $y -le 10; $y++) { Set-SafePixel $golemDie (16+$x) $y $stoneDark } }
for ($x=2; $x -le 13; $x++) { for ($y=11; $y -le 15; $y++) { Set-SafePixel $golemDie (16+$x) $y $stoneDark } }

# Frame 2: Crumbling boulders
for ($x=2; $x -le 6; $x++) { for ($y=10; $y -le 14; $y++) { Set-SafePixel $golemDie (32+$x) $y $stone } }
for ($x=7; $x -le 10; $x++) { for ($y=11; $y -le 15; $y++) { Set-SafePixel $golemDie (32+$x) $y $stoneDark } }
for ($x=11; $x -le 14; $x++) { for ($y=12; $y -le 15; $y++) { Set-SafePixel $golemDie (32+$x) $y $stone } }

# Frame 3: Pile of ancient rubble & dust
for ($x=1; $x -le 14; $x++) { for ($y=13; $y -le 15; $y++) { Set-SafePixel $golemDie (48+$x) $y $stoneDark } }
for ($x=3; $x -le 6; $x++) { for ($y=11; $y -le 13; $y++) { Set-SafePixel $golemDie (48+$x) $y $stone } }
for ($x=9; $x -le 12; $x++) { for ($y=12; $y -le 13; $y++) { Set-SafePixel $golemDie (48+$x) $y $stone } }
Save-AnimSheet "Assets/Sprites/Enemies/Boss_Golem_Die.png" $golemDie

Write-Output "ALL 17 ANIMATION SPRITESHEETS CREATED SUCCESSFULLY!"
