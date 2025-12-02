# SM Copy PowerShell Installer
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
Write-Host "SM Copy Installer" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get paths
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$exePath = Join-Path $scriptPath "..\SMCopy\bin\Release\net10.0-windows\SMCopy.exe"

# Check if executable exists
if (-not (Test-Path $exePath)) {
    Write-Host "ERROR: SMCopy.exe not found!" -ForegroundColor Red
    Write-Host "Expected location: $exePath" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please build the project first using build.bat" -ForegroundColor Yellow
    Write-Host ""
    Read-Host "Press Enter to exit"
    exit 1
}

# Register context menu
Write-Host "Registering context menu entries..." -ForegroundColor Green

try {
    $result = & $exePath --register
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "Installation completed successfully!" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "SM Copy has been installed and integrated" -ForegroundColor White
        Write-Host "into your Windows context menu." -ForegroundColor White
        Write-Host ""
        Write-Host "You can now right-click on files/folders" -ForegroundColor Yellow
        Write-Host "and use 'SM Copy' and 'SM Paste' options." -ForegroundColor Yellow
        Write-Host ""
    } else {
        Write-Host ""
        Write-Host "ERROR: Registration failed!" -ForegroundColor Red
        Write-Host "Exit code: $LASTEXITCODE" -ForegroundColor Red
        Write-Host ""
    }
} catch {
    Write-Host ""
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
}

Read-Host "Press Enter to exit"

