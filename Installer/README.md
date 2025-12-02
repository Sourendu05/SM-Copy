# SM Copy Installer

## Quick Start - Easy Installation

### For Users (Simple Method)

1. **Open the Installer folder**
2. **Double-click `SMCopyInstaller.exe`**
   - If you don't have this file yet, run `BuildInstaller.bat` first (see below)
3. **Click "Install"** in the GUI window
4. **Done!** SM Copy is now installed and ready to use

The installer will:
- Automatically build SM Copy
- Install it to your system
- Register the context menu entries
- Show you a log of everything it's doing

### Building the Installer (First Time Only)

If `SMCopyInstaller.exe` doesn't exist yet:

1. Run `BuildInstaller.bat`
2. Wait for the build to complete
3. Then run `SMCopyInstaller.exe`

---

## What the Installer Does

### Install Mode
1. Uninstalls any previous version (silent)
2. Builds SM Copy from source
3. Registers context menu entries
4. Shows success message

### Uninstall Mode
- Removes context menu entries
- Cleans up application data
- Shows confirmation dialog first

---

## For Developers

### Project Structure

```
Installer/
├── SMCopyInstaller/         # GUI Installer Project
│   ├── Program.cs           # Entry point with admin elevation
│   ├── InstallerForm.cs     # Main GUI form
│   ├── app.manifest         # Admin privileges manifest
│   └── SMCopyInstaller.csproj
├── BuildInstaller.bat       # Build the installer
├── EasyInstaller.bat        # Alternative: Interactive menu
├── build.bat                # Manual build SM Copy
├── install.bat              # Manual install
└── uninstall.bat            # Manual uninstall
```

### Manual Installation (Old Method)

If you prefer the old way:

#### Option 1: Interactive Menu
1. Run `EasyInstaller.bat` as Administrator
2. Choose option [1] Install

#### Option 2: Step-by-Step
1. Run `uninstall.bat` as Administrator (to remove old version)
2. Run `build.bat` as Administrator
3. Run `install.bat` as Administrator

---

## Features of the GUI Installer

✓ **Auto-Elevation**: Automatically requests admin privileges  
✓ **Progress Tracking**: Shows build and install progress  
✓ **Live Logging**: Displays all output in real-time  
✓ **One-Click Install**: Handles everything automatically  
✓ **One-Click Uninstall**: Clean removal with confirmation  
✓ **Error Handling**: Clear error messages if something fails  
✓ **Professional UI**: Modern WinForms interface  

---

## Requirements

- Windows 10 or later
- .NET 10.0 SDK or later (for building)
- Administrator privileges (installer requests automatically)

---

## Troubleshooting

**"Administrator privileges required"**  
→ The installer should request admin automatically. If not, right-click and "Run as administrator"

**"Build failed"**  
→ Make sure .NET 10.0 SDK is installed: https://dotnet.microsoft.com/download

**"SMCopy.exe not found"**  
→ The installer will build it automatically, but you can manually run `build.bat` first if needed

**Context menu not appearing**  
→ Try logging out and back in, or restart Windows

---

## Quick Reference

| File | Purpose |
|------|---------|
| **SMCopyInstaller.exe** | Main installer (double-click to use) |
| BuildInstaller.bat | Build the installer |
| EasyInstaller.bat | Interactive menu (alternative) |
| build.bat | Manual build |
| install.bat | Manual install |
| uninstall.bat | Manual uninstall |
