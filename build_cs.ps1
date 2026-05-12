# Catppuccin Coast — C# Build Script
# Publishes the WPF screensaver to dist_cs\ as a framework-dependent deployment.
# Requires: .NET 10 SDK  (dotnet --version)

$ErrorActionPreference = "Stop"
Write-Host "=== Catppuccin Coast — C# Build ===" -ForegroundColor Cyan

dotnet publish CatppuccinCoast.csproj -c Release -r win-x64 --no-self-contained -o dist_cs
if ($LASTEXITCODE -ne 0) { Write-Error "dotnet publish failed."; exit 1 }

Write-Host ""
Write-Host "Build complete!  Output: dist_cs\" -ForegroundColor Green
Write-Host ""
Get-ChildItem dist_cs | Where-Object { $_.Extension -in '.exe','.dll','.json' } |
    Select-Object Name, @{n='KB';e={[math]::Round($_.Length/1KB)}} |
    Format-Table -AutoSize

Write-Host "Run .\install.ps1 (as Administrator) to install." -ForegroundColor Cyan
