Add-Type -AssemblyName System.Drawing

$basePath = "D:\Program\WinYotuTubeMusic\microsoft.png"
$ytmPath = "D:\Program\WinYotuTubeMusic\ytmusic.png"
$outputPath = "D:\Program\WinYotuTubeMusic\ytmusic.ico"
$previewPath = "D:\Program\WinYotuTubeMusic\combined_icon.png"

$base = [System.Drawing.Bitmap]::FromFile($basePath)
$ytm = [System.Drawing.Bitmap]::FromFile($ytmPath)

$w = 512
$h = 512

$canvas = New-Object System.Drawing.Bitmap($w, $h, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g = [System.Drawing.Graphics]::FromImage($canvas)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
$g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality

# 1. Draw Microsoft Windows logo as base background
$g.DrawImage($base, 0, 0, $w, $h)

# 2. Calculate YouTube Music logo size & center offset
$ytmW = 250
$ytmH = 250
$ytmLeft = [int](($w - $ytmW) / 2)
$ytmTop = [int](($h - $ytmH) / 2)

# 3. Draw a sleek dark circle container in center for high contrast against Windows tiles
$bgM = 14
$bgL = $ytmLeft - $bgM
$bgT = $ytmTop - $bgM
$bgW = $ytmW + ($bgM * 2)
$bgH = $ytmH + ($bgM * 2)

$darkBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(235, 15, 15, 15))
$g.FillEllipse($darkBrush, $bgL, $bgT, $bgW, $bgH)
$darkBrush.Dispose()

# Draw subtle white stroke around the center container
$pen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(120, 255, 255, 255), 4)
$g.DrawEllipse($pen, $bgL, $bgT, $bgW, $bgH)
$pen.Dispose()

# 4. Draw YouTube Music logo in center
$g.DrawImage($ytm, $ytmLeft, $ytmTop, $ytmW, $ytmH)

$g.Dispose()
$base.Dispose()
$ytm.Dispose()

# Save preview PNG
$canvas.Save($previewPath, [System.Drawing.Imaging.ImageFormat]::Png)

# Save as ICO file
$hIcon = $canvas.GetHicon()
$icon = [System.Drawing.Icon]::FromHandle($hIcon)
$fs = [System.IO.File]::Create($outputPath)
$icon.Save($fs)
$fs.Close()
$canvas.Dispose()

Write-Output "Custom icon created successfully at: $outputPath"
