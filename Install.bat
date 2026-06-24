@echo off
echo =========================================
echo OA Layer Manager - Installer
echo =========================================
echo.

set "BUNDLE_DIR=%~dp0OALayerManager.bundle"
set "DEST_DIR=%APPDATA%\Autodesk\ApplicationPlugins\OALayerManager.bundle"

:: Check that the bundle folder exists
if not exist "%BUNDLE_DIR%" (
    echo ERROR: Cannot find OALayerManager.bundle.
    echo Make sure you extracted the ZIP completely before running this file.
    pause
    exit /b 1
)

:: Warn if no compatible AutoCAD (2021-2026) is detected
set "ACAD_FOUND=0"
for %%Y in (2021 2022 2023 2024 2025 2026) do if exist "C:\Program Files\Autodesk\AutoCAD %%Y" set "ACAD_FOUND=1"
if "%ACAD_FOUND%"=="0" (
    echo WARNING: No AutoCAD 2021-2026 installation detected.
    echo This plugin supports AutoCAD 2021 and newer.
    echo Installation will continue, but the plugin may not load.
    echo.
)

if exist "%DEST_DIR%" (
    echo Removing old installation...
    rmdir /S /Q "%DEST_DIR%"
)

echo Installing plugin to ApplicationPlugins folder...
xcopy /E /I /Y "%BUNDLE_DIR%" "%DEST_DIR%" > nul

if errorlevel 1 (
    echo ERROR: Installation failed. Try running as Administrator.
    pause
    exit /b 1
)

echo.
echo =========================================
echo Installation Successful!
echo Please restart AutoCAD to load the plugin.
echo In AutoCAD, go to the "OA Tools" tab in the Ribbon.
echo =========================================
pause
