Add-Type -AssemblyName System.Drawing

$basePath = "$PSScriptRoot\microsoft.png"
$ytmPath = "$PSScriptRoot\ytmusic.png"
$outputPath = "$PSScriptRoot\ytmusic.ico"
$previewPath = "$PSScriptRoot\combined_icon.png"

$base = [System.Drawing.Bitmap]::FromFile($basePath)
$ytm = [System.Drawing.Bitmap]::FromFile($ytmPath)

$w = 256
$h = 256

$canvas = New-Object System.Drawing.Bitmap($w, $h, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g = [System.Drawing.Graphics]::FromImage($canvas)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
$g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality

# 1. Draw Microsoft Windows logo as base background
$g.DrawImage($base, 0, 0, $w, $h)

# 2. Calculate YouTube Music logo size & center offset
$ytmW = 124
$ytmH = 124
$ytmLeft = [int](($w - $ytmW) / 2)
$ytmTop = [int](($h - $ytmH) / 2)

# 3. Draw a sleek dark circle container in center for high contrast against Windows tiles
$bgM = 7
$bgL = $ytmLeft - $bgM
$bgT = $ytmTop - $bgM
$bgW = $ytmW + ($bgM * 2)
$bgH = $ytmH + ($bgM * 2)

$darkBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(235, 15, 15, 15))
$g.FillEllipse($darkBrush, $bgL, $bgT, $bgW, $bgH)
$darkBrush.Dispose()

# Draw subtle white stroke around the center container
$pen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(120, 255, 255, 255), 2)
$g.DrawEllipse($pen, $bgL, $bgT, $bgW, $bgH)
$pen.Dispose()

# 4. Draw YouTube Music logo in center
$g.DrawImage($ytm, $ytmLeft, $ytmTop, $ytmW, $ytmH)

$g.Dispose()
$base.Dispose()
$ytm.Dispose()

# Save PNG stream
$ms = New-Object System.IO.MemoryStream
$canvas.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
$pngBytes = $ms.ToArray()
$ms.Dispose()

$canvas.Save($previewPath, [System.Drawing.Imaging.ImageFormat]::Png)
$canvas.Dispose()

# Build valid 256x256 PNG-embedded ICO file
$fs = [System.IO.File]::Create($outputPath)
$bw = New-Object System.IO.BinaryWriter($fs)

# ICONDIR Header (6 bytes)
$bw.Write([uint16]0)          # Reserved
$bw.Write([uint16]1)          # Type = ICO
$bw.Write([uint16]1)          # Count = 1

# ICONDIRENTRY (16 bytes)
$bw.Write([byte]0)            # Width (0 = 256px)
$bw.Write([byte]0)            # Height (0 = 256px)
$bw.Write([byte]0)            # Color count
$bw.Write([byte]0)            # Reserved
$bw.Write([uint16]1)          # Color planes
$bw.Write([uint16]32)         # Bits per pixel
$bw.Write([uint32]$pngBytes.Length) # Image size in bytes
$bw.Write([uint32]22)         # Image offset (6 + 16 = 22)

# Write PNG Payload
$bw.Write($pngBytes, 0, $pngBytes.Length)

$bw.Close()
$fs.Close()

Write-Output "Valid PNG-embedded ICO generated at: $outputPath"
