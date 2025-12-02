@echo off
setlocal enabledelayedexpansion

echo ========================================
echo   SM Copy - Build Setup
echo ========================================
echo.

:: Check for .NET SDK
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET SDK not found!
    echo Please install .NET 8.0 SDK from:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

echo [1/3] Building SMCopy application...
dotnet publish SMCopy\SMCopy.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o SMCopy\bin\Publish >nul 2>&1
if errorlevel 1 (
    echo ERROR: Failed to build SMCopy!
    pause
    exit /b 1
)
echo       Done!

echo [2/3] Building Setup installer...
dotnet publish Setup\Setup.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o bin\Setup >nul 2>&1
if errorlevel 1 (
    echo ERROR: Failed to build Setup!
    pause
    exit /b 1
)
echo       Done!

echo [3/3] Creating final package...
:: Copy SMCopy.exe alongside Setup for extraction
copy /Y "SMCopy\bin\Publish\SMCopy.exe" "bin\Setup\SMCopy.exe" >nul
:: Copy setup to root as the single installer
copy /Y "bin\Setup\SMCopySetup.exe" "setup.exe" >nul
echo       Done!

echo.
echo ========================================
echo   BUILD COMPLETE!
echo ========================================
echo.
echo   Output: setup.exe
echo.
echo   To install SM Copy:
echo   1. Run setup.exe as Administrator
echo   2. Click "Install"
echo.
pause
