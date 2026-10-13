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

$steel = [System.Drawing.Color]::FromArgb(255, 180, 190, 205)
$darkSteel = [System.Drawing.Color]::FromArgb(255, 100, 110, 130)
$visor = [System.Drawing.Color]::FromArgb(255, 40, 45, 60)
$gold = [System.Drawing.Color]::FromArgb(255, 245, 195, 50)
$redCape = [System.Drawing.Color]::FromArgb(255, 215, 40, 40)

# ==========================================
# 1. HERO KNIGHT IDLE (32 x 16 -> 2 Frames)
# ==========================================
$idleBmp = New-Canvas 32 16

# Frame 0 (x: 0..15)
for ($x=5; $x -le 10; $x++) { for ($y=2; $y -le 6; $y++) { $idleBmp.SetPixel($x, $y, $steel) } }
for ($x=6; $x -le 9; $x++) { $idleBmp.SetPixel($x, 4, $visor) }
$idleBmp.SetPixel(7, 2, $gold); $idleBmp.SetPixel(8, 2, $gold)
for ($x=4; $x -le 11; $x++) { for ($y=7; $y -le 12; $y++) { $idleBmp.SetPixel($x, $y, $steel) } }
for ($y=4; $y -le 13; $y++) { $idleBmp.SetPixel(3, $y, $redCape); $idleBmp.SetPixel(12, $y, $redCape) }
for ($x=5; $x -le 6; $x++) { for ($y=13; $y -le 15; $y++) { $idleBmp.SetPixel($x, $y, $darkSteel) } }
for ($x=9; $x -le 10; $x++) { for ($y=13; $y -le 15; $y++) { $idleBmp.SetPixel($x, $y, $darkSteel) } }

# Frame 1 (x: 16..31) - Breathing up 1px
for ($x=5; $x -le 10; $x++) { for ($y=1; $y -le 5; $y++) { $idleBmp.SetPixel(16 + $x, $y, $steel) } }
for ($x=6; $x -le 9; $x++) { $idleBmp.SetPixel(16 + $x, 3, $visor) }
$idleBmp.SetPixel(16 + 7, 1, $gold); $idleBmp.SetPixel(16 + 8, 1, $gold)
for ($x=4; $x -le 11; $x++) { for ($y=6; $y -le 11; $y++) { $idleBmp.SetPixel(16 + $x, $y, $steel) } }
for ($y=3; $y -le 13; $y++) { $idleBmp.SetPixel(16 + 3, $y, $redCape); $idleBmp.SetPixel(16 + 12, $y, $redCape) }
for ($x=5; $x -le 6; $x++) { for ($y=12; $y -le 15; $y++) { $idleBmp.SetPixel(16 + $x, $y, $darkSteel) } }
for ($x=9; $x -le 10; $x++) { for ($y=12; $y -le 15; $y++) { $idleBmp.SetPixel(16 + $x, $y, $darkSteel) } }

$idleBmp.Save("Assets/Sprites/Characters/Hero_Knight_Idle.png", [System.Drawing.Imaging.ImageFormat]::Png)
$idleBmp.Dispose()
Write-Output "Created Hero_Knight_Idle.png (32x16, 2 frames)"

# ==========================================
# 2. HERO KNIGHT RUN (64 x 16 -> 4 Frames)
# ==========================================
$runBmp = New-Canvas 64 16

# Run 0 (x: 0..15) - Left step forward
for ($x=5; $x -le 10; $x++) { for ($y=2; $y -le 6; $y++) { $runBmp.SetPixel($x, $y, $steel) } }
for ($x=6; $x -le 9; $x++) { $runBmp.SetPixel($x, 4, $visor) }
$runBmp.SetPixel(7, 2, $gold); $runBmp.SetPixel(8, 2, $gold)
for ($x=4; $x -le 11; $x++) { for ($y=7; $y -le 12; $y++) { $runBmp.SetPixel($x, $y, $steel) } }
for ($y=4; $y -le 12; $y++) { $runBmp.SetPixel(2, $y, $redCape); $runBmp.SetPixel(12, $y, $redCape) }
for ($x=3; $x -le 5; $x++) { for ($y=13; $y -le 15; $y++) { $runBmp.SetPixel($x, $y, $darkSteel) } }
for ($x=9; $x -le 11; $x++) { for ($y=12; $y -le 14; $y++) { $runBmp.SetPixel($x, $y, $darkSteel) } }

# Run 1 (x: 16..31) - Crouch / Passing
for ($x=5; $x -le 10; $x++) { for ($y=3; $y -le 7; $y++) { $runBmp.SetPixel(16 + $x, $y, $steel) } }
for ($x=6; $x -le 9; $x++) { $runBmp.SetPixel(16 + $x, 5, $visor) }
$runBmp.SetPixel(16 + 7, 3, $gold); $runBmp.SetPixel(16 + 8, 3, $gold)
for ($x=4; $x -le 11; $x++) { for ($y=8; $y -le 13; $y++) { $runBmp.SetPixel(16 + $x, $y, $steel) } }
for ($y=5; $y -le 13; $y++) { $runBmp.SetPixel(16 + 2, $y, $redCape); $runBmp.SetPixel(16 + 13, $y, $redCape) }
for ($x=6; $x -le 9; $x++) { for ($y=14; $y -le 15; $y++) { $runBmp.SetPixel(16 + $x, $y, $darkSteel) } }

# Run 2 (x: 32..47) - Right step forward
for ($x=5; $x -le 10; $x++) { for ($y=2; $y -le 6; $y++) { $runBmp.SetPixel(32 + $x, $y, $steel) } }
for ($x=6; $x -le 9; $x++) { $runBmp.SetPixel(32 + $x, 4, $visor) }
$runBmp.SetPixel(32 + 7, 2, $gold); $runBmp.SetPixel(32 + 8, 2, $gold)
for ($x=4; $x -le 11; $x++) { for ($y=7; $y -le 12; $y++) { $runBmp.SetPixel(32 + $x, $y, $steel) } }
for ($y=4; $y -le 12; $y++) { $runBmp.SetPixel(32 + 3, $y, $redCape); $runBmp.SetPixel(32 + 13, $y, $redCape) }
for ($x=4; $x -le 6; $x++) { for ($y=12; $y -le 14; $y++) { $runBmp.SetPixel(32 + $x, $y, $darkSteel) } }
for ($x=10; $x -le 12; $x++) { for ($y=13; $y -le 15; $y++) { $runBmp.SetPixel(32 + $x, $y, $darkSteel) } }

# Run 3 (x: 48..63) - Air apex
for ($x=5; $x -le 10; $x++) { for ($y=1; $y -le 5; $y++) { $runBmp.SetPixel(48 + $x, $y, $steel) } }
for ($x=6; $x -le 9; $x++) { $runBmp.SetPixel(48 + $x, 3, $visor) }
$runBmp.SetPixel(48 + 7, 1, $gold); $runBmp.SetPixel(48 + 8, 1, $gold)
for ($x=4; $x -le 11; $x++) { for ($y=6; $y -le 11; $y++) { $runBmp.SetPixel(48 + $x, $y, $steel) } }
for ($y=3; $y -le 11; $y++) { $runBmp.SetPixel(48 + 2, $y, $redCape); $runBmp.SetPixel(48 + 13, $y, $redCape) }
for ($x=5; $x -le 6; $x++) { for ($y=12; $y -le 14; $y++) { $runBmp.SetPixel(48 + $x, $y, $darkSteel) } }
for ($x=9; $x -le 10; $x++) { for ($y=12; $y -le 14; $y++) { $runBmp.SetPixel(48 + $x, $y, $darkSteel) } }

$runBmp.Save("Assets/Sprites/Characters/Hero_Knight_Run.png", [System.Drawing.Imaging.ImageFormat]::Png)
$runBmp.Dispose()
Write-Output "Created Hero_Knight_Run.png (64x16, 4 frames)"

# ==========================================
# 3. SLIME HOP (64 x 16 -> 4 Frames)
# ==========================================
$slimeCol = [System.Drawing.Color]::FromArgb(255, 60, 220, 90)
$slimeHi = [System.Drawing.Color]::FromArgb(255, 170, 250, 140)
$slimeBmp = New-Canvas 64 16

# Slime 0: Squash
for ($x=2; $x -le 13; $x++) { for ($y=9; $y -le 15; $y++) { $slimeBmp.SetPixel($x, $y, $slimeCol) } }
$slimeBmp.SetPixel(4, 10, $slimeHi); $slimeBmp.SetPixel(5, 10, $slimeHi)
$slimeBmp.SetPixel(4, 12, [System.Drawing.Color]::Black); $slimeBmp.SetPixel(10, 12, [System.Drawing.Color]::Black)

# Slime 1: Stretch launch
for ($x=5; $x -le 10; $x++) { for ($y=3; $y -le 14; $y++) { $slimeBmp.SetPixel(16 + $x, $y, $slimeCol) } }
$slimeBmp.SetPixel(16 + 6, 4, $slimeHi); $slimeBmp.SetPixel(16 + 7, 4, $slimeHi)
$slimeBmp.SetPixel(16 + 6, 7, [System.Drawing.Color]::Black); $slimeBmp.SetPixel(16 + 9, 7, [System.Drawing.Color]::Black)

# Slime 2: Peak air
for ($x=3; $x -le 12; $x++) { for ($y=2; $y -le 10; $y++) { $slimeBmp.SetPixel(32 + $x, $y, $slimeCol) } }
$slimeBmp.SetPixel(32 + 5, 3, $slimeHi); $slimeBmp.SetPixel(32 + 6, 3, $slimeHi)
$slimeBmp.SetPixel(32 + 5, 6, [System.Drawing.Color]::Black); $slimeBmp.SetPixel(32 + 9, 6, [System.Drawing.Color]::Black)

# Slime 3: Landing
for ($x=3; $x -le 12; $x++) { for ($y=6; $y -le 14; $y++) { $slimeBmp.SetPixel(48 + $x, $y, $slimeCol) } }
$slimeBmp.SetPixel(48 + 5, 7, $slimeHi); $slimeBmp.SetPixel(48 + 6, 7, $slimeHi)
$slimeBmp.SetPixel(48 + 5, 10, [System.Drawing.Color]::Black); $slimeBmp.SetPixel(48 + 9, 10, [System.Drawing.Color]::Black)

$slimeBmp.Save("Assets/Sprites/Enemies/Slime_Hop.png", [System.Drawing.Imaging.ImageFormat]::Png)
$slimeBmp.Dispose()
Write-Output "Created Slime_Hop.png (64x16, 4 frames)"

# Clean old single sheet files
Remove-Item "Assets/Sprites/Characters/Hero_Knight_Sheet.png*" -Force -ErrorAction SilentlyContinue
Remove-Item "Assets/Sprites/Enemies/Slime_Sheet.png*" -Force -ErrorAction SilentlyContinue

Write-Output "SEPARATED INTO 2 FILES (IDLE & RUN) SUCCESSFULLY!"
