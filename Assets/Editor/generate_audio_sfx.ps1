function New-WavFile {
    param(
        [string]$Path,
        [float[]]$Samples,
        [int]$SampleRate = 22050
    )
    $dir = [System.IO.Path]::GetDirectoryName($Path)
    if (-not (Test-Path $dir)) {
        [System.IO.Directory]::CreateDirectory($dir) | Out-Null
    }

    $numSamples = $Samples.Length
    $subchunk2Size = $numSamples * 2
    $chunkSize = 36 + $subchunk2Size

    $bytes = New-Object byte[] ($subchunk2Size)
    for ($i=0; $i -lt $numSamples; $i++) {
        $clamped = [Math]::Max(-1.0, [Math]::Min(1.0, $Samples[$i]))
        $val = [System.Int16]($clamped * 32767)
        $b = [System.BitConverter]::GetBytes($val)
        $bytes[$i * 2] = $b[0]
        $bytes[$i * 2 + 1] = $b[1]
    }

    $fs = [System.IO.File]::Create($Path)
    $bw = New-Object System.IO.BinaryWriter($fs)

    # RIFF header
    $bw.Write([System.Text.Encoding]::ASCII.GetBytes("RIFF"))
    $bw.Write([int]$chunkSize)
    $bw.Write([System.Text.Encoding]::ASCII.GetBytes("WAVE"))

    # fmt chunk
    $bw.Write([System.Text.Encoding]::ASCII.GetBytes("fmt "))
    $bw.Write([int]16)
    $bw.Write([System.Int16]1)      # PCM
    $bw.Write([System.Int16]1)      # Mono
    $bw.Write([int]$SampleRate)
    $bw.Write([int]($SampleRate * 2))
    $bw.Write([System.Int16]2)
    $bw.Write([System.Int16]16)     # 16-bit

    # data chunk
    $bw.Write([System.Text.Encoding]::ASCII.GetBytes("data"))
    $bw.Write([int]$subchunk2Size)
    $bw.Write($bytes)

    $bw.Close()
    $fs.Close()
    Write-Output "Created Audio: $Path"
}

$sampleRate = 22050

# 1. SFX Shoot: Chirp down from 800Hz to 200Hz (0.15s)
$dur = 0.15
$n = [int]($dur * $sampleRate)
$samples = New-Object float[] $n
$phase = 0.0
for ($i=0; $i -lt $n; $i++) {
    $t = $i / $sampleRate
    $freq = 800 - (600 * ($t / $dur))
    $phase += 2 * [Math]::PI * $freq * (1.0 / $sampleRate)
    $env = 1.0 - ($t / $dur)
    $samples[$i] = [Math]::Sin($phase) * $env * 0.6
}
New-WavFile "Assets/Audio/SFX/sfx_shoot.wav" $samples $sampleRate

# 2. SFX Hit: Low frequency thump + noise burst (0.12s)
$dur = 0.12
$n = [int]($dur * $sampleRate)
$samples = New-Object float[] $n
$rnd = New-Object System.Random
for ($i=0; $i -lt $n; $i++) {
    $t = $i / $sampleRate
    $env = [Math]::Exp(-25 * $t)
    $noise = ($rnd.NextDouble() * 2.0 - 1.0) * 0.4
    $tone = [Math]::Sin(2 * [Math]::PI * 110 * $t) * 0.6
    $samples[$i] = ($tone + $noise) * $env
}
New-WavFile "Assets/Audio/SFX/sfx_hit.wav" $samples $sampleRate

# 3. SFX Gem Pickup: High crystal ping (Arpeggio: 880Hz, 1320Hz, 1760Hz) (0.2s)
$dur = 0.2
$n = [int]($dur * $sampleRate)
$samples = New-Object float[] $n
$phase = 0.0
for ($i=0; $i -lt $n; $i++) {
    $t = $i / $sampleRate
    $freq = if ($t -lt 0.06) { 880 } elseif ($t -lt 0.12) { 1320 } else { 1760 }
    $phase += 2 * [Math]::PI * $freq * (1.0 / $sampleRate)
    $env = [Math]::Exp(-12 * ($t % 0.07))
    $samples[$i] = [Math]::Sin($phase) * $env * 0.5
}
New-WavFile "Assets/Audio/SFX/sfx_gem_pickup.wav" $samples $sampleRate

# 4. SFX Level Up: Fanfare chord progression (0.45s)
$dur = 0.45
$n = [int]($dur * $sampleRate)
$samples = New-Object float[] $n
$phase = 0.0
for ($i=0; $i -lt $n; $i++) {
    $t = $i / $sampleRate
    $freq = if ($t -lt 0.1) { 523.25 } elseif ($t -lt 0.2) { 659.25 } elseif ($t -lt 0.3) { 783.99 } else { 1046.5 }
    $phase += 2 * [Math]::PI * $freq * (1.0 / $sampleRate)
    $env = if ($t -gt 0.3) { [Math]::Exp(-4 * ($t - 0.3)) } else { 0.8 }
    $samples[$i] = [Math]::Sin($phase) * $env * 0.6
}
New-WavFile "Assets/Audio/SFX/sfx_levelup.wav" $samples $sampleRate

# 5. SFX Player Death: Descending retro slide (0.35s)
$dur = 0.35
$n = [int]($dur * $sampleRate)
$samples = New-Object float[] $n
$phase = 0.0
for ($i=0; $i -lt $n; $i++) {
    $t = $i / $sampleRate
    $freq = [Math]::Max(50.0, 400.0 - 350.0 * ($t / $dur))
    $phase += 2 * [Math]::PI * $freq * (1.0 / $sampleRate)
    $env = [Math]::Max(0.0, 1.0 - ($t / $dur))
    $samples[$i] = ([Math]::Sin($phase) + 0.3 * [Math]::Sin($phase * 0.5)) * $env * 0.6
}
New-WavFile "Assets/Audio/SFX/sfx_player_death.wav" $samples $sampleRate

Write-Output "ALL AUDIO SFX GENERATED SUCCESSFULLY!"
