Add-Type -AssemblyName System.Drawing

function Stitch-Sheet {
    param(
        [string[]]$SourceFiles,
        [string]$OutputFile,
        [int]$FrameWidth = 16,
        [int]$FrameHeight = 16
    )
    $count = $SourceFiles.Length
    $totalWidth = $FrameWidth * $count
    $sheetBmp = New-Object System.Drawing.Bitmap $totalWidth, $FrameHeight

    # Clear transparent
    for ($x=0; $x -lt $totalWidth; $x++) {
        for ($y=0; $y -lt $FrameHeight; $y++) {
            $sheetBmp.SetPixel($x, $y, [System.Drawing.Color]::FromArgb(0, 0, 0, 0))
        }
    }

    $g = [System.Drawing.Graphics]::FromImage($sheetBmp)
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half

    for ($i=0; $i -lt $count; $i++) {
        $frameBmp = [System.Drawing.Bitmap]::FromFile($SourceFiles[$i])
        $g.DrawImage($frameBmp, ($i * $FrameWidth), 0, $FrameWidth, $FrameHeight)
        $frameBmp.Dispose()
    }

    $g.Dispose()
    $sheetBmp.Save($OutputFile, [System.Drawing.Imaging.ImageFormat]::Png)
    $sheetBmp.Dispose()
    Write-Output "Created SpriteSheet: $OutputFile ($totalWidth x $FrameHeight)"
}

# 1. Stitch Hero Knight Sheet (96 x 16)
$knightFiles = @(
    "Assets/Sprites/Characters/Hero_Knight_Idle_0.png",
    "Assets/Sprites/Characters/Hero_Knight_Idle_1.png",
    "Assets/Sprites/Characters/Hero_Knight_Run_0.png",
    "Assets/Sprites/Characters/Hero_Knight_Run_1.png",
    "Assets/Sprites/Characters/Hero_Knight_Run_2.png",
    "Assets/Sprites/Characters/Hero_Knight_Run_3.png"
)
Stitch-Sheet -SourceFiles $knightFiles -OutputFile "Assets/Sprites/Characters/Hero_Knight_Sheet.png" -FrameWidth 16 -FrameHeight 16

# 2. Stitch Slime Sheet (64 x 16)
$slimeFiles = @(
    "Assets/Sprites/Enemies/Slime_Hop_0.png",
    "Assets/Sprites/Enemies/Slime_Hop_1.png",
    "Assets/Sprites/Enemies/Slime_Hop_2.png",
    "Assets/Sprites/Enemies/Slime_Hop_3.png"
)
Stitch-Sheet -SourceFiles $slimeFiles -OutputFile "Assets/Sprites/Enemies/Slime_Sheet.png" -FrameWidth 16 -FrameHeight 16

# 3. Clean up loose individual files
$looseFiles = @(
    "Assets/Sprites/Characters/Hero_Knight_Idle_0.png",
    "Assets/Sprites/Characters/Hero_Knight_Idle_1.png",
    "Assets/Sprites/Characters/Hero_Knight_Run_0.png",
    "Assets/Sprites/Characters/Hero_Knight_Run_1.png",
    "Assets/Sprites/Characters/Hero_Knight_Run_2.png",
    "Assets/Sprites/Characters/Hero_Knight_Run_3.png",
    "Assets/Sprites/Enemies/Slime_Hop_0.png",
    "Assets/Sprites/Enemies/Slime_Hop_1.png",
    "Assets/Sprites/Enemies/Slime_Hop_2.png",
    "Assets/Sprites/Enemies/Slime_Hop_3.png"
)

foreach ($f in $looseFiles) {
    if (Test-Path $f) { Remove-Item $f -Force }
    if (Test-Path "$f.meta") { Remove-Item "$f.meta" -Force }
}

Write-Output "Cleaned up all loose frame files successfully!"
