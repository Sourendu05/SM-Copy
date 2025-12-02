@echo off
REM SM Copy Build Script

echo ========================================
echo SM Copy Build Script
echo ========================================
echo.

REM Check if dotnet is installed
dotnet --version >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: .NET SDK not found!
    echo Please install .NET 10.0 SDK or later from:
    echo https://dotnet.microsoft.com/download
    echo.
    pause
    exit /b 1
)

echo Building SM Copy...
echo.

REM Navigate to solution directory
cd /d "%~dp0.."

REM Build the project in Release mode
dotnet build SMCopy.sln -c Release

if %errorLevel% equ 0 (
    echo.
    echo ========================================
    echo Build completed successfully!
    echo ========================================
    echo.
    echo Output location:
    echo SMCopy\bin\Release\net10.0-windows\SMCopy.exe
    echo.
    echo To install, run: Installer\install.bat as Administrator
    echo.
) else (
    echo.
    echo ERROR: Build failed!
    echo Please check the error messages above.
    echo.
)

pause

