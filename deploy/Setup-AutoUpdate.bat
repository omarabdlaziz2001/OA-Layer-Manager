@echo off
:: Double-click this ONCE from the shared plugin folder to enable auto-updates.
:: It launches the PowerShell setup with the execution policy bypassed so no
:: machine configuration is required.
echo Enabling OA Layer Manager auto-update...
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Setup-AutoUpdate.ps1"
echo.
pause
