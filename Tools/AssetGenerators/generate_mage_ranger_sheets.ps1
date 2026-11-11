Add-Type -AssemblyName System.Drawing

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

# ==========================================
# PALETTE DEFINITIONS
# ==========================================
$black = [System.Drawing.Color]::FromArgb(255, 20, 20, 25)
$white = [System.Drawing.Color]::FromArgb(255, 255, 255, 255)

$mageHat = [System.Drawing.Color]::FromArgb(255, 60, 35, 130)        # Dark mystical purple
$mageRobe = [System.Drawing.Color]::FromArgb(255, 95, 60, 190)       # Royal violet
$mageGold = [System.Drawing.Color]::FromArgb(255, 255, 215, 60)      # Bright gold trim
$mageSkin = [System.Drawing.Color]::FromArgb(255, 250, 210, 165)     # Pale skin
$mageBeard = [System.Drawing.Color]::FromArgb(255, 235, 240, 250)    # White/silver beard
$mageStaff = [System.Drawing.Color]::FromArgb(255, 140, 95, 45)      # Wood brown
$mageAura = [System.Drawing.Color]::FromArgb(255, 90, 225, 255)      # Cyan mana glow

$rangerGreen = [System.Drawing.Color]::FromArgb(255, 45, 135, 65)    # Forest green tunic
$rangerDarkGreen = [System.Drawing.Color]::FromArgb(255, 28, 85, 40) # Shadow green
$rangerLeather = [System.Drawing.Color]::FromArgb(255, 145, 90, 40)  # Leather brown
$rangerDarkLeather = [System.Drawing.Color]::FromArgb(255, 85, 50, 20)# Dark boots
$rangerFeather = [System.Drawing.Color]::FromArgb(255, 235, 45, 45)  # Crimson cap feather
$rangerSkin = [System.Drawing.Color]::FromArgb(255, 245, 195, 150)   # Tanned skin
$rangerBow = [System.Drawing.Color]::FromArgb(255, 185, 125, 60)     # Bow wood
$rangerEye = [System.Drawing.Color]::FromArgb(255, 30, 30, 30)

# ==========================================
# 1. HERO MAGE IDLE (32 x 16 -> 2 Frames)
# ==========================================
$mageIdle = New-Canvas 32 16

function Draw-Mage {
    param($bmp, [int]$ox, [int]$oy, [bool]$sparkle)
    
    # Wizard Hat
    Set-SafePixel $bmp ($ox + 7) ($oy + 1) $mageGold
    for ($x=6; $x -le 8; $x++) { Set-SafePixel $bmp ($ox + $x) ($oy + 2) $mageHat }
    for ($x=5; $x -le 9; $x++) { Set-SafePixel $bmp ($ox + $x) ($oy + 3) $mageHat }
    for ($x=4; $x -le 10; $x++) { Set-SafePixel $bmp ($ox + $x) ($oy + 4) $mageHat }
    for ($x=3; $x -le 11; $x++) { Set-SafePixel $bmp ($ox + $x) ($oy + 5) $mageHat }
    
    # Face & Eyes
    for ($x=5; $x -le 9; $x++) { Set-SafePixel $bmp ($ox + $x) ($oy + 6) $mageSkin }
    Set-SafePixel $bmp ($ox + 6) ($oy + 6) $black
    Set-SafePixel $bmp ($ox + 8) ($oy + 6) $black
    
    # Silver Beard
    for ($x=5; $x -le 9; $x++) { Set-SafePixel $bmp ($ox + $x) ($oy + 7) $mageBeard }
    for ($x=6; $x -le 8; $x++) { Set-SafePixel $bmp ($ox + $x) ($oy + 8) $mageBeard }
    Set-SafePixel $bmp ($ox + 7) ($oy + 9) $mageBeard
    
    # Robe & Golden Trim
    for ($x=4; $x -le 10; $x++) { 
        for ($y=8; $y -le 13; $y++) { 
            Set-SafePixel $bmp ($ox + $x) ($oy + $y) $mageRobe 
        } 
    }
    for ($y=8; $y -le 13; $y++) { Set-SafePixel $bmp ($ox + 7) ($oy + $y) $mageGold }
    for ($x=4; $x -le 10; $x++) { Set-SafePixel $bmp ($ox + $x) ($oy + 14) $mageGold }
    
    # Magic Orb / Hand
    Set-SafePixel $bmp ($ox + 2) ($oy + 9) $mageSkin
    Set-SafePixel $bmp ($ox + 2) ($oy + 8) $mageAura
    Set-SafePixel $bmp ($ox + 1) ($oy + 8) $mageAura
    
    if ($sparkle) {
        Set-SafePixel $bmp ($ox + 1) ($oy + 7) $white
        Set-SafePixel $bmp ($ox + 12) ($oy + 11) $mageAura
    }
}

# Mage Idle Frame 0: Normal ground standing
Draw-Mage $mageIdle 0 0 $false
# Mage Idle Frame 1: Floating breath up 1 pixel with magic sparks
Draw-Mage $mageIdle 16 -1 $true

$mageIdle.Save("Assets/Sprites/Characters/Hero_Mage_Idle.png", [System.Drawing.Imaging.ImageFormat]::Png)
$mageIdle.Dispose()
Write-Output "Created Hero_Mage_Idle.png (32x16, 2 frames)"

# ==========================================
# 2. HERO MAGE RUN (64 x 16 -> 4 Frames)
# ==========================================
$mageRun = New-Canvas 64 16

# Mage Run Frame 0: Forward lean, robe back
Draw-Mage $mageRun 0 0 $false
for ($y=11; $y -le 14; $y++) { Set-SafePixel $mageRun 3 $y $mageRobe }
Set-SafePixel $mageRun 2 14 $mageRobe

# Mage Run Frame 1: Dip / step
Draw-Mage $mageRun 16 0 $false
for ($x=5; $x -le 9; $x++) { Set-SafePixel $mageRun (16 + $x) 15 $mageHat }

# Mage Run Frame 2: Stride other side, orb high
Draw-Mage $mageRun 32 0 $true
for ($y=11; $y -le 14; $y++) { Set-SafePixel $mageRun (32 + 3) $y $mageRobe }
Set-SafePixel $mageRun (32 + 11) 13 $mageRobe

# Mage Run Frame 3: Floating apex leap
Draw-Mage $mageRun 48 -1 $true
Set-SafePixel $mageRun (48 + 1) 5 $white

$mageRun.Save("Assets/Sprites/Characters/Hero_Mage_Run.png", [System.Drawing.Imaging.ImageFormat]::Png)
$mageRun.Dispose()
Write-Output "Created Hero_Mage_Run.png (64x16, 4 frames)"

# ==========================================
# 3. HERO RANGER IDLE (32 x 16 -> 2 Frames)
# ==========================================
$rangerIdle = New-Canvas 32 16

function Draw-Ranger {
    param($bmp, [int]$ox, [int]$oy, [int]$legPose)
    
    # Cap Feather
    Set-SafePixel $bmp ($ox + 10) ($oy + 0) $rangerFeather
    Set-SafePixel $bmp ($ox + 9) ($oy + 1) $rangerFeather
    
    # Archer Hat
    for ($x=5; $x -le 10; $x++) { Set-SafePixel $bmp ($ox + $x) ($oy + 2) $rangerGreen }
    for ($x=4; $x -le 11; $x++) { Set-SafePixel $bmp ($ox + $x) ($oy + 3) $rangerGreen }
    
    # Face & Eyes
    for ($x=5; $x -le 10; $x++) { Set-SafePixel $bmp ($ox + $x) ($oy + 4) $rangerSkin }
    Set-SafePixel $bmp ($ox + 6) ($oy + 4) $rangerEye
    Set-SafePixel $bmp ($ox + 9) ($oy + 4) $rangerEye
    
    # Tunic Body & Leather Sash
    for ($x=4; $x -le 11; $x++) { 
        for ($y=5; $y -le 10; $y++) { 
            Set-SafePixel $bmp ($ox + $x) ($oy + $y) $rangerGreen 
        } 
    }
    # Leather diagonal quiver strap
    Set-SafePixel $bmp ($ox + 5) ($oy + 5) $rangerLeather
    Set-SafePixel $bmp ($ox + 6) ($oy + 6) $rangerLeather
    Set-SafePixel $bmp ($ox + 7) ($oy + 7) $rangerLeather
    Set-SafePixel $bmp ($ox + 8) ($oy + 8) $rangerLeather
    Set-SafePixel $bmp ($ox + 9) ($oy + 9) $rangerLeather
    # Belt
    for ($x=4; $x -le 11; $x++) { Set-SafePixel $bmp ($ox + $x) ($oy + 10) $rangerLeather }
    
    # Bow on back/hand
    Set-SafePixel $bmp ($ox + 2) ($oy + 5) $rangerBow
    Set-SafePixel $bmp ($ox + 1) ($oy + 6) $rangerBow
    Set-SafePixel $bmp ($ox + 1) ($oy + 7) $rangerBow
    Set-SafePixel $bmp ($ox + 2) ($oy + 8) $rangerBow
    
    # Legs based on pose
    if ($legPose -eq 0) {
        # Idle standing
        for ($x=5; $x -le 6; $x++) { for ($y=11; $y -le 15; $y++) { Set-SafePixel $bmp ($ox + $x) ($oy + $y) $rangerDarkLeather } }
        for ($x=9; $x -le 10; $x++) { for ($y=11; $y -le 15; $y++) { Set-SafePixel $bmp ($ox + $x) ($oy + $y) $rangerDarkLeather } }
    }
    elseif ($legPose -eq 1) {
        # Left leg forward
        for ($x=3; $x -le 5; $x++) { for ($y=11; $y -le 15; $y++) { Set-SafePixel $bmp ($ox + $x) ($oy + $y) $rangerDarkLeather } }
        for ($x=9; $x -le 11; $x++) { for ($y=11; $y -le 13; $y++) { Set-SafePixel $bmp ($ox + $x) ($oy + $y) $rangerDarkLeather } }
    }
    elseif ($legPose -eq 2) {
        # Passing / dip
        for ($x=6; $x -le 9; $x++) { for ($y=11; $y -le 15; $y++) { Set-SafePixel $bmp ($ox + $x) ($oy + $y) $rangerDarkLeather } }
    }
    elseif ($legPose -eq 3) {
        # Right leg forward
        for ($x=4; $x -le 6; $x++) { for ($y=11; $y -le 13; $y++) { Set-SafePixel $bmp ($ox + $x) ($oy + $y) $rangerDarkLeather } }
        for ($x=10; $x -le 12; $x++) { for ($y=11; $y -le 15; $y++) { Set-SafePixel $bmp ($ox + $x) ($oy + $y) $rangerDarkLeather } }
    }
    elseif ($legPose -eq 4) {
        # Push off / apex
        for ($x=5; $x -le 6; $x++) { for ($y=11; $y -le 14; $y++) { Set-SafePixel $bmp ($ox + $x) ($oy + $y) $rangerDarkLeather } }
        for ($x=9; $x -le 10; $x++) { for ($y=11; $y -le 14; $y++) { Set-SafePixel $bmp ($ox + $x) ($oy + $y) $rangerDarkLeather } }
    }
}

# Ranger Idle 0: Alert pose
Draw-Ranger $rangerIdle 0 0 0
# Ranger Idle 1: Breathing up 1px
Draw-Ranger $rangerIdle 16 -1 0

$rangerIdle.Save("Assets/Sprites/Characters/Hero_Ranger_Idle.png", [System.Drawing.Imaging.ImageFormat]::Png)
$rangerIdle.Dispose()
Write-Output "Created Hero_Ranger_Idle.png (32x16, 2 frames)"

# ==========================================
# 4. HERO RANGER RUN (64 x 16 -> 4 Frames)
# ==========================================
$rangerRun = New-Canvas 64 16

# Ranger Run 0: Left stride forward
Draw-Ranger $rangerRun 0 0 1
# Ranger Run 1: Mid passing step
Draw-Ranger $rangerRun 16 0 2
# Ranger Run 2: Right stride forward
Draw-Ranger $rangerRun 32 0 3
# Ranger Run 3: Push-off spring leap
Draw-Ranger $rangerRun 48 -1 4

$rangerRun.Save("Assets/Sprites/Characters/Hero_Ranger_Run.png", [System.Drawing.Imaging.ImageFormat]::Png)
$rangerRun.Dispose()
Write-Output "Created Hero_Ranger_Run.png (64x16, 4 frames)"
