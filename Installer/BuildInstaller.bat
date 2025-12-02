@echo off
REM Build SMCopyInstaller.exe

echo ========================================
echo Building SM Copy Installer...
echo ========================================
echo.

cd /d "%~dp0SMCopyInstaller"

dotnet build -c Release

if %errorLevel% equ 0 (
    echo.
    echo ========================================
    echo Build successful!
    echo ========================================
    echo.
    echo Output: SMCopyInstaller\bin\Release\net10.0-windows\SMCopyInstaller.exe
    echo.
    echo You can now run SMCopyInstaller.exe to install SM Copy.
    echo The installer will automatically build and install SM Copy.
    echo.
) else (
    echo.
    echo ERROR: Build failed!
    echo.
)

pause
