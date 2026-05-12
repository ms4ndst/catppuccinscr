# Catppuccin Coast Screensaver — Install Script (C# / WPF edition)
# Installs to %ProgramFiles%\CatppuccinCoast\ and sets the registry.
# Run as Administrator.

$ErrorActionPreference = "Stop"

$src  = Join-Path $PSScriptRoot "dist_cs"
$dest = "$env:ProgramFiles\CatppuccinCoast"
$scr  = Join-Path $dest "catppuccin_coast.scr"

if (-not (Test-Path "$src\catppuccin_coast.exe")) {
    Write-Error "dist_cs\catppuccin_coast.exe not found. Run .\build_cs.ps1 first."
    exit 1
}

# Elevate if needed
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
    [Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Start-Process pwsh -ArgumentList "-NoProfile -ExecutionPolicy Bypass -File `"$PSCommandPath`"" -Verb RunAs
    exit
}

Write-Host "=== Catppuccin Coast — Install ===" -ForegroundColor Cyan

# 1. Create install dir
Write-Host "[1/3] Installing to $dest ..." -ForegroundColor Yellow
New-Item -ItemType Directory -Force -Path $dest | Out-Null

# Copy the .exe as .scr (the .NET host still finds catppuccin_coast.dll by stem name)
Copy-Item "$src\catppuccin_coast.exe"            $scr                                -Force
Copy-Item "$src\catppuccin_coast.dll"            "$dest\catppuccin_coast.dll"         -Force
Copy-Item "$src\catppuccin_coast.deps.json"      "$dest\catppuccin_coast.deps.json"   -Force
Copy-Item "$src\catppuccin_coast.runtimeconfig.json" "$dest\catppuccin_coast.runtimeconfig.json" -Force
Write-Host "      Done." -ForegroundColor Green

# 2. Registry
Write-Host "[2/3] Configuring registry ..." -ForegroundColor Yellow
$reg = "HKCU:\Control Panel\Desktop"
Set-ItemProperty $reg "SCRNSAVE.EXE"        $scr
Set-ItemProperty $reg "ScreenSaveActive"    "1"
Set-ItemProperty $reg "ScreenSaverIsSecure" "0"
Set-ItemProperty $reg "ScreenSaveTimeOut"   "300"
Write-Host "      Done." -ForegroundColor Green

Write-Host "[3/3] Installation complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Screensaver:  $scr"    -ForegroundColor White
Write-Host "Timeout:      5 min (change in Settings > Lock screen > Screen saver settings)" -ForegroundColor Cyan
Write-Host ""
Write-Host "Preview now:" -ForegroundColor Yellow
Write-Host "  & `"$scr`" /s" -ForegroundColor White
Write-Host ""
Write-Host "Settings dialog:" -ForegroundColor Yellow
Write-Host "  & `"$scr`" /c" -ForegroundColor White
