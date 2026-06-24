# Publish-ToShare.ps1
# YOU run this (the maintainer) after building the plugin.
# It mirrors the freshly built OALayerManager.bundle to the network share and
# drops the colleague setup scripts next to it. Colleagues' machines pick up the
# new version automatically at their next logon (see Setup-AutoUpdate.ps1).
#
# Usage:  powershell -ExecutionPolicy Bypass -File Publish-ToShare.ps1
#         (optionally)  -SharePath "\\SERVER\Share\OA-Layer Manager"

param(
    # Office share for the OA Layer Manager plugin.
    # Note: this is a mapped drive. It works for the auto-updater because that runs
    # from the Startup folder (after drives reconnect at logon). If you ever switch
    # to a true logon script / scheduled task, use the UNC path instead.
    [string]$SharePath = "W:\0000-BIM\06-Extension\Autocad\OA-Layer Manager"
)

$ErrorActionPreference = "Stop"
$scriptDir   = Split-Path -Parent $MyInvocation.MyCommand.Path   # ...\deploy
$repoRoot    = Split-Path -Parent $scriptDir                     # repo root
$localBundle = Join-Path $repoRoot "OALayerManager.bundle"

Write-Host "=========================================="
Write-Host " OA Layer Manager - Publish to Share"
Write-Host "=========================================="

# --- sanity checks ----------------------------------------------------------
$dll = Join-Path $localBundle "Contents\OA.LayerBatcher.dll"
$pkg = Join-Path $localBundle "PackageContents.xml"
if (-not (Test-Path $dll) -or -not (Test-Path $pkg)) {
    Write-Host "ERROR: Built bundle not found. Build the project first." -ForegroundColor Red
    Write-Host "Expected: $dll"
    exit 1
}
if (-not (Test-Path $SharePath)) {
    Write-Host "ERROR: Cannot reach share: $SharePath" -ForegroundColor Red
    Write-Host "Check the path / that the server is online, or pass -SharePath."
    exit 1
}

$version = ([xml](Get-Content $pkg)).ApplicationPackage.AppVersion
Write-Host "Publishing version $version to: $SharePath"

# --- mirror the bundle (adds new/changed files, removes deleted ones) -------
$destBundle = Join-Path $SharePath "OALayerManager.bundle"
robocopy $localBundle $destBundle /MIR /R:2 /W:2 /NFL /NDL /NJH /NP | Out-Null
if ($LASTEXITCODE -ge 8) {
    Write-Host "ERROR: robocopy failed (code $LASTEXITCODE)." -ForegroundColor Red
    exit 1
}

# --- drop the colleague setup scripts next to the bundle --------------------
Copy-Item (Join-Path $scriptDir "Setup-AutoUpdate.ps1") $SharePath -Force
Copy-Item (Join-Path $scriptDir "Setup-AutoUpdate.bat") $SharePath -Force

# --- remove obsolete manual-install artifacts so colleagues use auto-update -
foreach ($stale in "Install.bat", "ReleaseBundle.zip") {
    $p = Join-Path $SharePath $stale
    if (Test-Path $p) { Remove-Item $p -Force }
}

Write-Host ""
Write-Host "Done. Version $version is live on the share." -ForegroundColor Green
Write-Host "Colleagues who ran Setup-AutoUpdate.bat once will get it at next logon."
