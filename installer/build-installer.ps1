# build-installer.ps1
# Builds the plugin in Release, then compiles the Inno Setup installer EXE.
# Output: installer\Output\OALayerManager-Setup-<version>.exe
#
# Usage:  powershell -ExecutionPolicy Bypass -File installer\build-installer.ps1

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot  = Split-Path -Parent $scriptDir
$csproj    = Join-Path $repoRoot "OA-Layer Manager\OA.LayerBatcher.csproj"
$iss       = Join-Path $scriptDir "OALayerManager.iss"

Write-Host "=========================================="
Write-Host " OA Layer Manager - Build Installer"
Write-Host "=========================================="

# 1. Locate the Inno Setup compiler.
$iscc = @(
    "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $iscc) {
    Write-Host "ERROR: Inno Setup (ISCC.exe) not found." -ForegroundColor Red
    Write-Host "Install it with:  winget install JRSoftware.InnoSetup"
    exit 1
}

# 2. Build the plugin (regenerates the bundle the installer packages).
Write-Host "Building plugin (Release)..."
dotnet build $csproj -c Release -v quiet --nologo
if ($LASTEXITCODE -ne 0) { Write-Host "ERROR: build failed." -ForegroundColor Red; exit 1 }

# 3. Compile the installer.
Write-Host "Compiling installer..."
& $iscc $iss
if ($LASTEXITCODE -ne 0) { Write-Host "ERROR: ISCC failed." -ForegroundColor Red; exit 1 }

$exe = Get-ChildItem (Join-Path $scriptDir "Output") -Filter "OALayerManager-Setup-*.exe" |
       Sort-Object LastWriteTime -Descending | Select-Object -First 1
Write-Host ""
Write-Host "Done: $($exe.FullName)" -ForegroundColor Green
