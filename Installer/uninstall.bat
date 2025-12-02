@echo off
REM SM Copy Uninstaller Script
REM Run as Administrator

echo ========================================
echo SM Copy Uninstaller
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

echo Uninstalling SM Copy...
echo.

REM Get the directory where the installer is located
set "INSTALL_DIR=%~dp0..\SMCopy\bin\Release\net10.0-windows\"

REM Check if executable exists
if not exist "%INSTALL_DIR%SMCopy.exe" (
    echo WARNING: SMCopy.exe not found at expected location.
    echo Attempting to remove registry entries anyway...
    echo.
)

REM Unregister context menu
if exist "%INSTALL_DIR%SMCopy.exe" (
    echo Removing context menu entries...
    "%INSTALL_DIR%SMCopy.exe" --unregister
)

REM Clean up AppData
set "APPDATA_DIR=%APPDATA%\SMCopy"
if exist "%APPDATA_DIR%" (
    echo Removing application data...
    rmdir /s /q "%APPDATA_DIR%"
)

echo.
echo ========================================
echo Uninstallation completed!
echo ========================================
echo.
echo SM Copy has been removed from your system.
echo Context menu entries have been unregistered.
echo.

pause

