# Generates docs/assets/social-preview.png (1280x640) for the GitHub repository card.
# Same palette and type as the app: warm near-black, one amber accent, Sitka Banner.
# Re-run only if the wording or mark changes.

$ErrorActionPreference = 'Stop'
Set-Location (Split-Path $PSScriptRoot -Parent)
Add-Type -AssemblyName System.Drawing

$W = 1280; $H = 640
$ink       = [System.Drawing.Color]::FromArgb(10, 9, 8)
$paper     = [System.Drawing.Color]::FromArgb(247, 242, 234)
$paperDim  = [System.Drawing.Color]::FromArgb(167, 156, 142)
$paperFaint= [System.Drawing.Color]::FromArgb(109, 100, 89)
$amber     = [System.Drawing.Color]::FromArgb(224, 161, 92)
$amberHot  = [System.Drawing.Color]::FromArgb(242, 186, 119)

$bmp = New-Object System.Drawing.Bitmap $W, $H
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = 'AntiAlias'
$g.TextRenderingHint = 'AntiAliasGridFit'
$g.Clear($ink)

# --- amber dusk from the top, same gesture as the lock screen ---------------
$glow = New-Object System.Drawing.Drawing2D.GraphicsPath
$glow.AddEllipse(-320, -520, ($W + 640), 1100)
$brush = New-Object System.Drawing.Drawing2D.PathGradientBrush $glow
$brush.CenterPoint = New-Object System.Drawing.PointF (($W / 2), 30)
$brush.CenterColor = [System.Drawing.Color]::FromArgb(70, 224, 161, 92)
$brush.SurroundColors = @([System.Drawing.Color]::FromArgb(0, 224, 161, 92))
$g.FillPath($brush, $glow)

# --- padlock mark, identical construction to the app icon ------------------
$mark = 96
$mx = [int](($W - $mark) / 2); $my = 120
$r = [int]($mark * 0.22); $d = $r * 2
$sq = New-Object System.Drawing.Drawing2D.GraphicsPath
$sq.AddArc($mx, $my, $d, $d, 180, 90)
$sq.AddArc($mx + $mark - $d, $my, $d, $d, 270, 90)
$sq.AddArc($mx + $mark - $d, $my + $mark - $d, $d, $d, 0, 90)
$sq.AddArc($mx, $my + $mark - $d, $d, $d, 90, 90)
$sq.CloseFigure()
$sqBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
  (New-Object System.Drawing.Point $mx, $my),
  (New-Object System.Drawing.Point ($mx + $mark), ($my + $mark)), $amberHot, $amber)
$g.FillPath($sqBrush, $sq)

$lockInk = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(26, 18, 6))
$bw = $mark * 0.44; $bh = $mark * 0.34
$bx = $mx + ($mark - $bw) / 2; $by = $my + $mark * 0.48
$br = $mark * 0.06; $bd = $br * 2
$body = New-Object System.Drawing.Drawing2D.GraphicsPath
$body.AddArc($bx, $by, $bd, $bd, 180, 90)
$body.AddArc($bx + $bw - $bd, $by, $bd, $bd, 270, 90)
$body.AddArc($bx + $bw - $bd, $by + $bh - $bd, $bd, $bd, 0, 90)
$body.AddArc($bx, $by + $bh - $bd, $bd, $bd, 90, 90)
$body.CloseFigure()
$g.FillPath($lockInk, $body)
$pen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(26, 18, 6)), ([float]($mark * 0.085))
$pen.StartCap = 'Round'; $pen.EndCap = 'Round'
$aw = $mark * 0.28
$g.DrawArc($pen, ($mx + ($mark - $aw) / 2), ($my + $mark * 0.27), $aw, ($aw * 1.05), 180, 180)

# --- type -------------------------------------------------------------------
$centre = New-Object System.Drawing.StringFormat
$centre.Alignment = 'Center'

$title = New-Object System.Drawing.Font 'Sitka Banner', 68, ([System.Drawing.FontStyle]::Regular), ([System.Drawing.GraphicsUnit]::Pixel)
$g.DrawString('EmailLock', $title, (New-Object System.Drawing.SolidBrush $paper),
              (New-Object System.Drawing.RectangleF 0, 268, $W, 100), $centre)

$tag = New-Object System.Drawing.Font 'Sitka Text', 30, ([System.Drawing.FontStyle]::Italic), ([System.Drawing.GraphicsUnit]::Pixel)
$g.DrawString("Stop opening work when you're off.", $tag, (New-Object System.Drawing.SolidBrush $paperDim),
              (New-Object System.Drawing.RectangleF 0, 368, $W, 60), $centre)

# hairline
$rule = New-Object System.Drawing.Pen $amber, 1
$g.DrawLine($rule, ($W / 2 - 26), 452, ($W / 2 + 26), 452)

# letterspaced footer, drawn glyph by glyph because GDI+ has no tracking
$foot = New-Object System.Drawing.Font 'Segoe UI', 15, ([System.Drawing.FontStyle]::Bold), ([System.Drawing.GraphicsUnit]::Pixel)
$text = 'OPEN SOURCE   ' + [char]0x2022 + '   WINDOWS'
$track = 4.0
$chars = $text.ToCharArray()
$width = 0.0
foreach ($c in $chars) { $width += $g.MeasureString([string]$c, $foot).Width - 4 + $track }
$x = ($W - $width) / 2
$footBrush = New-Object System.Drawing.SolidBrush $paperFaint
foreach ($c in $chars) {
  $g.DrawString([string]$c, $foot, $footBrush, $x, 500)
  $x += $g.MeasureString([string]$c, $foot).Width - 4 + $track
}

# --- vignette ---------------------------------------------------------------
$vig = New-Object System.Drawing.Drawing2D.GraphicsPath
$vig.AddEllipse(-260, -180, ($W + 520), ($H + 360))
$vb = New-Object System.Drawing.Drawing2D.PathGradientBrush $vig
$vb.CenterPoint = New-Object System.Drawing.PointF (($W / 2), ($H / 2))
$vb.CenterColor = [System.Drawing.Color]::FromArgb(0, 0, 0, 0)
$vb.SurroundColors = @([System.Drawing.Color]::FromArgb(190, 0, 0, 0))
$g.FillPath($vb, $vig)

$g.Dispose()
$out = Join-Path (Get-Location) 'docs\assets\social-preview.png'
$bmp.Save($out, [System.Drawing.Imaging.ImageFormat]::Png)
"wrote $out ($([math]::Round((Get-Item $out).Length/1KB)) KB, ${W}x${H})"

# --- the mark on its own, for the README hero -------------------------------
# Cropped from the card so the two can never drift apart.
$markOut = Join-Path (Get-Location) 'docs\assets\mark.png'
$crop = New-Object System.Drawing.Rectangle $mx, $my, $mark, $mark
$markBmp = $bmp.Clone($crop, $bmp.PixelFormat)
$big = New-Object System.Drawing.Bitmap 256, 256
$mg = [System.Drawing.Graphics]::FromImage($big)
$mg.InterpolationMode = 'HighQualityBicubic'
$mg.DrawImage($markBmp, 0, 0, 256, 256)
$mg.Dispose()
$big.Save($markOut, [System.Drawing.Imaging.ImageFormat]::Png)
$big.Dispose(); $markBmp.Dispose(); $bmp.Dispose()
"wrote $markOut ($([math]::Round((Get-Item $markOut).Length/1KB)) KB, 256x256)"
