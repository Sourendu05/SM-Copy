# SM Copy PowerShell Uninstaller
# This script will elevate to administrator if needed

# Check if running as administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "Requesting administrator privileges..." -ForegroundColor Yellow
    
    # Re-launch the script with administrator privileges
    Start-Process powershell.exe -ArgumentList "-NoProfile -ExecutionPolicy Bypass -File `"$PSCommandPath`"" -Verb RunAs
    exit
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SM Copy Uninstaller" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get paths
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$exePath = Join-Path $scriptPath "..\SMCopy\bin\Release\net10.0-windows\SMCopy.exe"

# Unregister context menu
if (Test-Path $exePath) {
    Write-Host "Removing context menu entries..." -ForegroundColor Green
    
    try {
        $result = & $exePath --unregister
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Warning: Unregistration returned exit code $LASTEXITCODE" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "Warning: $($_.Exception.Message)" -ForegroundColor Yellow
    }
} else {
    Write-Host "Warning: SMCopy.exe not found, skipping executable unregister..." -ForegroundColor Yellow
}

# Clean up AppData
$appdataDir = Join-Path $env:APPDATA "SMCopy"
if (Test-Path $appdataDir) {
    Write-Host "Removing application data..." -ForegroundColor Green
    Remove-Item -Path $appdataDir -Recurse -Force
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Uninstallation completed!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "SM Copy has been removed from your system." -ForegroundColor White
Write-Host "Context menu entries have been unregistered." -ForegroundColor White
Write-Host ""

Read-Host "Press Enter to exit"

