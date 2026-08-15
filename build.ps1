# Builds a release: tests must pass, then publish self-contained, then wrap in an installer.
# Output: dist\EmailLock-<version>-setup.exe
#
#   .\build.ps1            full release
#   .\build.ps1 -SkipInstaller   app only, no Inno Setup needed

[CmdletBinding()]
param([switch]$SkipInstaller)

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

Write-Host "`n[1/3] tests" -ForegroundColor Cyan
dotnet test --nologo
if ($LASTEXITCODE -ne 0) { throw "tests failed — not building a release from red" }

Write-Host "`n[2/3] publish (self-contained, no .NET needed on the target machine)" -ForegroundColor Cyan
dotnet publish src\EmailLock\EmailLock.csproj `
  -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=false -p:DebugType=none --nologo
if ($LASTEXITCODE -ne 0) { throw "publish failed" }

if ($SkipInstaller) {
  Write-Host "`nskipped installer. app is in src\EmailLock\bin\Release\net8.0-windows\win-x64\publish\" -ForegroundColor Yellow
  return
}

Write-Host "`n[3/3] installer" -ForegroundColor Cyan
$iscc = @(
  "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
  "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
  "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $iscc) { throw "Inno Setup 6 not found. Install it: winget install JRSoftware.InnoSetup" }

& $iscc /Q installer\EmailLock.iss
if ($LASTEXITCODE -ne 0) { throw "installer build failed" }

$setup = Get-ChildItem dist\*-setup.exe | Sort-Object LastWriteTime | Select-Object -Last 1
Write-Host "`ndone: $($setup.FullName) ($([math]::Round($setup.Length/1MB,1)) MB)" -ForegroundColor Green
