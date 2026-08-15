# Generates src/EmailLock/ui/app.ico — amber rounded square with a padlock.
# Re-run only if the mark changes; the .ico is committed so contributors need no tooling.
Add-Type -AssemblyName System.Drawing

function New-Frame([int]$s) {
  $bmp = New-Object System.Drawing.Bitmap $s, $s
  $g = [System.Drawing.Graphics]::FromImage($bmp)
  $g.SmoothingMode = 'AntiAlias'
  $g.Clear([System.Drawing.Color]::Transparent)

  $r = [math]::Max(2, [int]($s * 0.22))
  $path = New-Object System.Drawing.Drawing2D.GraphicsPath
  $d = $r * 2
  $path.AddArc(0, 0, $d, $d, 180, 90)
  $path.AddArc($s - $d, 0, $d, $d, 270, 90)
  $path.AddArc($s - $d, $s - $d, $d, $d, 0, 90)
  $path.AddArc(0, $s - $d, $d, $d, 90, 90)
  $path.CloseFigure()

  $brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
    (New-Object System.Drawing.Point 0, 0),
    (New-Object System.Drawing.Point $s, $s),
    [System.Drawing.Color]::FromArgb(242, 186, 119),
    [System.Drawing.Color]::FromArgb(206, 140, 74))
  $g.FillPath($brush, $path)

  # Padlock, dark on amber.
  $ink = [System.Drawing.Color]::FromArgb(26, 18, 6)
  $bodyW = $s * 0.44; $bodyH = $s * 0.34
  $bodyX = ($s - $bodyW) / 2; $bodyY = $s * 0.48
  $br = New-Object System.Drawing.SolidBrush $ink
  $rad = [math]::Max(1, $s * 0.06)
  $bp = New-Object System.Drawing.Drawing2D.GraphicsPath
  $bd = $rad * 2
  $bp.AddArc($bodyX, $bodyY, $bd, $bd, 180, 90)
  $bp.AddArc($bodyX + $bodyW - $bd, $bodyY, $bd, $bd, 270, 90)
  $bp.AddArc($bodyX + $bodyW - $bd, $bodyY + $bodyH - $bd, $bd, $bd, 0, 90)
  $bp.AddArc($bodyX, $bodyY + $bodyH - $bd, $bd, $bd, 90, 90)
  $bp.CloseFigure()
  $g.FillPath($br, $bp)

  $pen = New-Object System.Drawing.Pen $ink, ([float]($s * 0.085))
  $pen.StartCap = 'Round'; $pen.EndCap = 'Round'
  $aw = $s * 0.28
  $g.DrawArc($pen, ($s - $aw) / 2, $s * 0.27, $aw, $aw * 1.05, 180, 180)

  $g.Dispose()
  $ms = New-Object System.IO.MemoryStream
  $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
  $bmp.Dispose()
  return , $ms.ToArray()
}

$sizes = 16, 32, 48, 64, 128, 256
$frames = $sizes | ForEach-Object { , (New-Frame $_) }

$out = New-Object System.IO.MemoryStream
$w = New-Object System.IO.BinaryWriter $out
$w.Write([uint16]0); $w.Write([uint16]1); $w.Write([uint16]$sizes.Count)

$offset = 6 + 16 * $sizes.Count
for ($i = 0; $i -lt $sizes.Count; $i++) {
  $s = $sizes[$i]
  $w.Write([byte]($(if ($s -ge 256) { 0 } else { $s })))
  $w.Write([byte]($(if ($s -ge 256) { 0 } else { $s })))
  $w.Write([byte]0); $w.Write([byte]0)
  $w.Write([uint16]1); $w.Write([uint16]32)
  $w.Write([uint32]$frames[$i].Length)
  $w.Write([uint32]$offset)
  $offset += $frames[$i].Length
}
$frames | ForEach-Object { $w.Write($_) }
$w.Flush()

$dest = Join-Path $PSScriptRoot "..\src\EmailLock\ui\app.ico"
[System.IO.File]::WriteAllBytes((Resolve-Path (Split-Path $dest)).Path + "\app.ico", $out.ToArray())
"wrote app.ico ($($out.Length) bytes, $($sizes.Count) sizes)"
