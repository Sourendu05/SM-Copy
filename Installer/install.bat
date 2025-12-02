@echo off
REM SM Copy Installer Script
REM Run as Administrator

echo ========================================
echo SM Copy Installer
echo ========================================
echo.

REM Check for administrator privileges
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: Administrator privileges required!
    echo Please right-click this file and select "Run as administrator"
    echo.
    pause
    exit /b 1
)

echo Installing SM Copy...
echo.

REM Get the directory where the installer is located
set "INSTALL_DIR=%~dp0..\SMCopy\bin\Release\net10.0-windows\"

REM Check if build exists
if not exist "%INSTALL_DIR%SMCopy.exe" (
    echo ERROR: SMCopy.exe not found!
    echo Please build the project first.
    echo Expected location: %INSTALL_DIR%SMCopy.exe
    echo.
    pause
    exit /b 1
)

REM Register context menu
echo Registering context menu entries...
"%INSTALL_DIR%SMCopy.exe" --register

if %errorLevel% equ 0 (
    echo.
    echo ========================================
    echo Installation completed successfully!
    echo ========================================
    echo.
    echo SM Copy has been installed and integrated
    echo into your Windows context menu.
    echo.
    echo You can now right-click on files/folders
    echo and use "SM Copy" and "SM Paste" options.
    echo.
) else (
    echo.
    echo ERROR: Registration failed!
    echo Please check the error message above.
    echo.
)

pause

