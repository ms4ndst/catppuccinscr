# Catppuccin Coast Screensaver — Build Script
# Compiles catppuccin_coast.py into a Windows .scr screensaver file.
# Run from the project directory: .\build.ps1

$ErrorActionPreference = "Stop"

Write-Host "=== Catppuccin Coast Screensaver Build ===" -ForegroundColor Cyan

# 1. Compile with PyInstaller
Write-Host "`n[1/3] Compiling with PyInstaller..." -ForegroundColor Yellow
pyinstaller --noconfirm catppuccin_coast.spec

if (-not $?) {
    Write-Error "PyInstaller failed. Ensure it is installed: pip install pyinstaller"
    exit 1
}

# 2. Rename .exe to .scr
$exePath = "dist\catppuccin_coast.exe"
$scrPath = "dist\catppuccin_coast.scr"

if (Test-Path $scrPath) { Remove-Item $scrPath }
Rename-Item $exePath $scrPath

Write-Host "[2/3] Renamed to $scrPath" -ForegroundColor Green

# 3. Done
$absPath = Resolve-Path $scrPath
Write-Host "`n[3/3] Build complete!" -ForegroundColor Green
Write-Host "Screensaver: $absPath" -ForegroundColor White
Write-Host "`nNext step: run .\install.ps1 to install system-wide, or see README.md." -ForegroundColor Cyan
