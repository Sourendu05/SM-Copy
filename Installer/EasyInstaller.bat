@echo off
REM SM Copy One-Click Installer
REM This script handles building, installing, and running SM Copy

:: Check for admin rights
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo Requesting administrator privileges...
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b
)

:MENU
cls
echo ========================================
echo      SM Copy - Easy Installer
echo ========================================
echo.
echo What would you like to do?
echo.
echo [1] Install SM Copy (Build + Install)
echo [2] Uninstall SM Copy
echo [3] Rebuild Only (without installing)
echo [4] Exit
echo.
set /p choice="Enter your choice (1-4): "

if "%choice%"=="1" goto INSTALL
if "%choice%"=="2" goto UNINSTALL
if "%choice%"=="3" goto BUILD
if "%choice%"=="4" goto EXIT
goto MENU

:INSTALL
cls
echo ========================================
echo Installing SM Copy...
echo ========================================
echo.

:: Step 1: Uninstall previous version
echo [Step 1/3] Removing any previous installation...
call "%~dp0uninstall.bat" >nul 2>&1

:: Step 2: Build
echo [Step 2/3] Building SM Copy...
echo.
call "%~dp0build.bat"
if %errorLevel% neq 0 (
    echo.
    echo ERROR: Build failed!
    pause
    goto MENU
)

:: Step 3: Install
echo.
echo [Step 3/3] Installing context menu...
echo.
call "%~dp0install.bat"

echo.
echo ========================================
echo Installation Complete!
echo ========================================
echo.
echo SM Copy is now ready to use!
echo Right-click on any file or folder to see
echo the "SM Copy" and "SM Paste" options.
echo.
pause
goto MENU

:UNINSTALL
cls
echo ========================================
echo Uninstalling SM Copy...
echo ========================================
echo.
call "%~dp0uninstall.bat"
echo.
pause
goto MENU

:BUILD
cls
echo ========================================
echo Building SM Copy...
echo ========================================
echo.
call "%~dp0build.bat"
echo.
pause
goto MENU

:EXIT
exit
