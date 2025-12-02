# SM Copy - Quick Start Guide

Get started with SM Copy in 5 minutes!

## Installation (3 steps)

### Step 1: Build the Application

Open a command prompt in the project folder:

```batch
cd Installer
build.bat
```

**Alternative:** Open `SMCopy.sln` in Visual Studio and build in Release mode.

### Step 2: Install Context Menu

Right-click `Installer\install.bat` → **Run as administrator**

Or run this PowerShell script (auto-elevates):
```powershell
.\Installer\install.ps1
```

### Step 3: Done!

You should see: "Installation completed successfully!"

## First Use

### Copy Something

1. **Right-click** on any file or folder
2. Look for **"SM Copy"** in the menu
3. Click it
4. You'll see: "Copied to SM Clipboard"

### Paste It

1. **Navigate** to where you want to paste
2. **Right-click** in an empty area
3. Click **"SM Paste"**
4. Watch the progress window!

## What Makes SM Copy Special?

### Speed Comparison

**Windows Copy:**
- 1GB file: ~2 minutes
- Single-threaded

**SM Copy (robocopy):**
- 1GB file: ~30 seconds
- Up to 32 threads
- Optimized for SSDs

### Features You Get

✅ **Multi-threaded** - Uses all your CPU cores  
✅ **Progress tracking** - See speed, ETA, completion  
✅ **Resume capability** - Handles interruptions  
✅ **Multi-select** - Copy multiple files/folders at once  
✅ **Safe & reliable** - Automatic retries on errors  

## Common Uses

### Backup Large Folders
```
Right-click folder → SM Copy
Navigate to backup drive → SM Paste
```

### Copy Multiple Files
```
Select multiple files (Ctrl+Click)
Right-click selection → SM Copy
Navigate to destination → SM Paste
```

### Move Project Folders
```
Right-click project folder → SM Copy
Navigate to new location → SM Paste
(Delete original if you want to "move")
```

## Troubleshooting

### Context Menu Not Showing?

1. **Restart Windows Explorer:**
   - Open Task Manager (Ctrl+Shift+Esc)
   - Find "Windows Explorer"
   - Right-click → Restart

2. **Verify Installation:**
   ```
   SMCopy.exe --register
   ```

### "Administrator Privileges Required"

- You only need admin rights to **install/uninstall**
- Regular copying doesn't need admin
- Right-click the installer → "Run as administrator"

### Copy Seems Slow?

- Check the progress window for details
- Network copies are limited by network speed
- Very small files (<1KB) may not benefit from multi-threading

## Advanced Tips

### Use Command Line

Copy files programmatically:
```batch
SMCopy.exe --copy "C:\source\file.txt"
SMCopy.exe --paste "D:\destination"
```

### Check What's Copied

The clipboard data is stored at:
```
%APPDATA%\SMCopy\clipboard.json
```

### Customize Settings (Future)

Currently uses optimal defaults:
- 32 threads (auto-adjusts to your system)
- 3 retries with 1 second wait
- All subdirectories and empty folders

## Uninstall

If you ever want to remove SM Copy:

1. Right-click `Installer\uninstall.bat` → **Run as administrator**

Or:
```powershell
.\Installer\uninstall.ps1
```

This removes:
- Context menu entries
- Application data
- Registry keys

## Need Help?

- **Documentation:** See `README.md` for full details
- **Testing Guide:** See `TESTING.md` for test scenarios
- **Installation Issues:** See `Installer\README.md`

## Performance Tips

### For Best Speed:

1. **Use on SSDs** - Robocopy really shines on fast drives
2. **Large files** - Most benefit with files >100MB
3. **Multiple files** - Multi-threading helps most here
4. **Local copies** - Network speed is the bottleneck for remote copies

### When Regular Copy is Fine:

- Single small file (<10MB)
- Quick drag-and-drop operations
- Files already on slow drive (bottleneck elsewhere)

## What's Next?

Now that you have SM Copy installed:

1. Try copying a large folder and watch the speed!
2. Select multiple files and copy them all at once
3. Check the progress window to see real-time stats
4. Compare speed with Windows native copy

**Enjoy fast copying! 🚀**

---

**Version:** 1.0  
**Platform:** Windows 10/11, Server 2016+  
**Requirements:** .NET 10.0 Runtime  

