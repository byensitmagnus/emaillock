# Captures docs/assets/* from the real running app.
#
# Two things are patched into a throwaway build and reverted afterwards:
#   1. culture forced to en-US, because the app follows Windows and the README is English
#   2. Settings opens on start, so it can be captured without driving the tray icon
# Neither changes shipped behaviour. Windows still decides the language for real users.
#
# Frames come from PrintWindow(PW_RENDERFULLCONTENT), not a screen grab, so nothing from
# the desktop behind the window can end up in a public screenshot.
#
# Needs ffmpeg on PATH for demo.gif.   Run from the repo root:  .\tools\capture-assets.ps1

[CmdletBinding()]
param([switch]$SkipGif)

$ErrorActionPreference = 'Stop'
Set-Location (Split-Path $PSScriptRoot -Parent)

$assets  = 'docs\assets'
$stage   = Join-Path $env:TEMP 'emaillock-capture'
$program = 'src\EmailLock\Program.cs'
$exe     = 'src\EmailLock\bin\Release\net8.0-windows\EmailLock.exe'
$cfg     = "$env:APPDATA\EmailLock\config.json"

New-Item -ItemType Directory -Force -Path $assets, $stage | Out-Null
Get-ChildItem $stage -File | Remove-Item -Force

Add-Type -AssemblyName System.Drawing
Add-Type @'
using System;using System.Collections.Generic;using System.Runtime.InteropServices;
public class Win {
  delegate bool Cb(IntPtr h, IntPtr l);
  [DllImport("user32.dll")] static extern bool EnumWindows(Cb cb, IntPtr l);
  [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
  [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr h);
  [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr h, IntPtr hdc, uint flags);
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out R r);
  [DllImport("user32.dll")] public static extern void mouse_event(uint f,int dx,int dy,int d,int e);
  [DllImport("user32.dll")] public static extern bool SetCursorPos(int x,int y);
  public struct R { public int L,T,Rt,B; }
  public static List<IntPtr> Visible(uint want) {
    var found = new List<IntPtr>();
    EnumWindows((h,l) => { uint pid; GetWindowThreadProcessId(h, out pid);
      if (pid == want && IsWindowVisible(h)) found.Add(h); return true; }, IntPtr.Zero);
    return found;
  }
}
'@ -ErrorAction SilentlyContinue

function Grab([IntPtr]$h) {
  $r = New-Object Win+R; [void][Win]::GetWindowRect($h, [ref]$r)
  $bmp = New-Object System.Drawing.Bitmap ($r.Rt - $r.L), ($r.B - $r.T)
  $g = [System.Drawing.Graphics]::FromImage($bmp)
  $hdc = $g.GetHdc(); [void][Win]::PrintWindow($h, $hdc, 2); $g.ReleaseHdc($hdc); $g.Dispose()
  return $bmp
}

function Save($bmp, [string]$path, [double]$scale = 1.0) {
  if ($scale -eq 1.0) { $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png); return }
  $out = New-Object System.Drawing.Bitmap ([int]($bmp.Width * $scale)), ([int]($bmp.Height * $scale))
  $g = [System.Drawing.Graphics]::FromImage($out)
  $g.InterpolationMode = 'HighQualityBicubic'
  $g.DrawImage($bmp, 0, 0, $out.Width, $out.Height); $g.Dispose()
  $out.Save($path, [System.Drawing.Imaging.ImageFormat]::Png); $out.Dispose()
}

function WaitForWindow([int]$procId, [int]$minWidth, [int]$timeoutMs = 20000) {
  $sw = [Diagnostics.Stopwatch]::StartNew()
  while ($sw.ElapsedMilliseconds -lt $timeoutMs) {
    foreach ($h in [Win]::Visible($procId)) {
      $r = New-Object Win+R; [void][Win]::GetWindowRect($h, [ref]$r)
      if (($r.Rt - $r.L) -ge $minWidth) { return $h }
    }
    Start-Sleep -Milliseconds 100
  }
  throw "no window at least ${minWidth}px wide appeared"
}

function StopApp { Get-Process EmailLock, notepad -EA SilentlyContinue | Stop-Process -Force; Start-Sleep -Milliseconds 400 }

function DemoConfig([string]$apps, [string]$code) {
  @"
{
  "owner": "Magnus",
  "sosCode": "$code",
  "apps": [ $apps ],
  "lockedDays": [ "Saturday", "Sunday" ],
  "openFrom": "07:00",
  "openUntil": "17:00",
  "sosMinutes": 60,
  "graceSeconds": 20,
  "message": "It's {day}, {owner}.\nYou're off. Go be off."
}
"@ | Set-Content $cfg -Encoding UTF8
}

$originalProgram = Get-Content $program -Raw
Set-Content "$stage\Program.cs.orig" $originalProgram -NoNewline -Encoding UTF8

# Your real settings are overwritten by the demo config below. Keep a copy on disk too, so
# an interrupted run leaves something to restore by hand rather than a demo config.
$savedConfig = if (Test-Path $cfg) { Get-Content $cfg -Raw } else { $null }
if ($savedConfig) { Set-Content "$stage\config.json.orig" $savedConfig -NoNewline -Encoding UTF8 }

try {
  $patched = $originalProgram `
    -replace '(ApplicationConfiguration\.Initialize\(\);)', @'
$1
        var en = new System.Globalization.CultureInfo("en-US");   // capture build only
        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = en;
        System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = en;
        Thread.CurrentThread.CurrentCulture = en;
        Thread.CurrentThread.CurrentUICulture = en;
'@ `
    -replace 'if \(_cfg\.Validate\(\)\.Count > 0\) _ = ShowSettings\(\);', '_ = ShowSettings();'

  Set-Content $program $patched -NoNewline -Encoding UTF8

  Write-Host "building capture build..." -ForegroundColor Cyan
  dotnet build -c Release --nologo -v q | Out-Null
  if ($LASTEXITCODE -ne 0) { throw "build failed" }

  # --- settings.png -------------------------------------------------------
  Write-Host "settings..." -ForegroundColor Cyan
  StopApp
  DemoConfig '"OUTLOOK", "olk", "Teams"' 'winter-harbour-4417'
  $proc = Start-Process $exe -PassThru
  $h = WaitForWindow $proc.Id 800
  Start-Sleep -Seconds 3
  $bmp = Grab $h; Save $bmp "$assets\settings.png"; $bmp.Dispose()
  StopApp

  # --- lock-screen.png + demo frames --------------------------------------
  Write-Host "lock screen..." -ForegroundColor Cyan
  DemoConfig '"notepad"' 'winter-harbour-4417'
  $proc = Start-Process $exe -PassThru
  Start-Sleep -Seconds 4
  Start-Process notepad
  $lock = WaitForWindow $proc.Id 1900

  if (-not $SkipGif) {
    Write-Host "demo frames..." -ForegroundColor Cyan
    1..46 | ForEach-Object {                       # entrance + countdown
      $f = Grab $lock; Save $f ("$stage\f{0:D3}.png" -f $_) 0.5; $f.Dispose()
    }
    [void][Win]::SetCursorPos(960, 728); Start-Sleep -Milliseconds 200
    [Win]::mouse_event(0x0002,0,0,0,0); [Win]::mouse_event(0x0004,0,0,0,0)
    47..74 | ForEach-Object {                      # SOS reveal
      $f = Grab $lock; Save $f ("$stage\f{0:D3}.png" -f $_) 0.5; $f.Dispose()
    }
    StopApp
    # fresh instance so the hero shot is the resting state, not the SOS panel
    $proc = Start-Process $exe -PassThru
    Start-Sleep -Seconds 4
    Start-Process notepad
    $lock = WaitForWindow $proc.Id 1900
    Start-Sleep -Seconds 3
  }
  else { Start-Sleep -Seconds 3 }

  $bmp = Grab $lock; Save $bmp "$assets\lock-screen.png" 0.75; $bmp.Dispose()
  StopApp
}
finally {
  Set-Content $program $originalProgram -NoNewline -Encoding UTF8
  if ($savedConfig) { Set-Content $cfg $savedConfig -NoNewline -Encoding UTF8 }
  Write-Host "restored $program and config" -ForegroundColor Yellow
}

if (-not $SkipGif) {
  Write-Host "encoding demo.gif..." -ForegroundColor Cyan
  $pal = "$stage\palette.png"
  & ffmpeg -y -loglevel error -framerate 12 -i "$stage\f%03d.png" `
    -vf "scale=720:-1:flags=lanczos,palettegen=max_colors=48" $pal
  & ffmpeg -y -loglevel error -framerate 12 -i "$stage\f%03d.png" -i $pal `
    -lavfi "scale=720:-1:flags=lanczos[x];[x][1:v]paletteuse=dither=bayer:bayer_scale=3" `
    -loop 0 "$assets\demo.gif"
}

Write-Host "`nrebuilding from restored source..." -ForegroundColor Cyan
dotnet build -c Release --nologo -v q | Out-Null
Get-ChildItem $assets | Select-Object Name, @{n = 'KB'; e = { [math]::Round($_.Length / 1KB) } }
