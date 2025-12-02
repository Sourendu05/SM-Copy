# SM Copy - Complete User Guide

## Table of Contents

1. [Introduction](#introduction)
2. [Installation](#installation)
3. [How to Use](#how-to-use)
4. [Features](#features)
5. [Understanding the Progress Window](#understanding-the-progress-window)
6. [Tips & Tricks](#tips--tricks)
7. [Troubleshooting](#troubleshooting)
8. [Technical Details](#technical-details)
9. [FAQ](#faq)

---

## Introduction

SM Copy is a powerful file copying tool for Windows that integrates Microsoft's robocopy technology into your right-click context menu. It provides significantly faster file transfers compared to Windows' built-in copy function, especially for large files and folders.

### Why SM Copy?

- **Faster:** Uses multi-threading (up to 32 threads) for maximum speed
- **Reliable:** Automatic retry logic handles temporary failures
- **Informative:** Real-time progress with speed and ETA
- **Convenient:** Right-click integration - no command line needed
- **Safe:** Non-destructive copying with verification

### System Requirements

- Windows 10, Windows 11, or Windows Server 2016+
- .NET 10.0 Runtime or SDK
- Administrator privileges (for installation only)
- Robocopy (included with Windows)

---

## Installation

### Quick Install

1. **Build the project:**
   ```
   Double-click: Installer\build.bat
   ```

2. **Install context menu (as Admin):**
   ```
   Right-click: Installer\install.bat
   Select: "Run as administrator"
   ```

3. **Restart Explorer (recommended):**
   - Press `Ctrl+Shift+Esc`
   - Find "Windows Explorer"
   - Right-click → Restart

### Verification

After installation, right-click on any file. You should see "SM Copy" in the menu.

---

## How to Use

### Basic Copy & Paste

1. **Copy:**
   - Right-click on a file or folder
   - Click "SM Copy"
   - See confirmation: "Copied to SM Clipboard"

2. **Paste:**
   - Navigate to destination folder
   - Right-click in empty space
   - Click "SM Paste"
   - Progress window appears

3. **Wait:**
   - Monitor the progress window
   - See real-time speed and ETA
   - Click "Cancel" if needed

### Multiple Selection

**Copy several files at once:**
1. Select multiple files (Ctrl+Click or drag-select)
2. Right-click on any selected file
3. Click "SM Copy"
4. All selected items will be copied

**Copy several folders:**
1. Select multiple folders
2. Right-click on selection
3. Click "SM Copy"
4. Navigate and paste - all folders copied

**Mixed selection:**
- You can select both files and folders
- All will be copied together
- Folder structures preserved

---

## Features

### Speed Optimization

SM Copy uses robocopy with optimized settings:

| Setting | Value | Benefit |
|---------|-------|---------|
| Threads | 32 | Maximum parallelization |
| Retries | 3 | Handles temporary failures |
| Wait Time | 1 second | Quick retry without overload |
| Subdirectories | Included | Complete folder copying |
| Empty Folders | Included | Preserves structure |

### Progress Tracking

The progress window shows:

- **Progress Bar:** Visual completion percentage (0-100%)
- **Status:** Current operation and item count
- **Current File:** Name of file being copied right now
- **Speed:** Transfer rate (KB/s, MB/s, or GB/s)
- **Time Remaining:** Estimated time to completion
- **Total Size:** Size of all items being copied
- **Log:** Detailed robocopy output

### Smart Clipboard

- SM Copy maintains its own "clipboard"
- Doesn't interfere with Windows clipboard
- Stored in: `%APPDATA%\SMCopy\clipboard.json`
- Persists between uses
- Last copied items available until next copy

---

## Understanding the Progress Window

### Window Components

```
┌─────────────────────────────────────────┐
│ Copying item 1/3... 45%                 │ ← Status
├─────────────────────────────────────────┤
│ ████████████░░░░░░░░░░░░░░░░░░░░░░░░░░ │ ← Progress Bar
├─────────────────────────────────────────┤
│ Current file: document.pdf              │ ← Current File
│ Speed: 125.5 MB/s                       │ ← Speed
│ Time remaining: 2m 15s                  │ ← ETA
│ Total: 1.2 GB                           │ ← Size
├─────────────────────────────────────────┤
│ [Log Output]                            │ ← Robocopy Log
│ [HH:MM:SS] Copying: document.pdf        │
│ [HH:MM:SS] Completed: image.jpg         │
├─────────────────────────────────────────┤
│              [Pause]  [Cancel]          │ ← Controls
└─────────────────────────────────────────┘
```

### Status Messages

- **"Preparing to copy..."** - Initial setup
- **"Copying item X/Y..."** - Active copying
- **"Copy completed successfully!"** - All done
- **"Operation cancelled"** - User cancelled
- **"Failed or incomplete"** - Error occurred

### Reading the Log

The log shows robocopy's output:
- Each line timestamped
- Shows file-by-file progress
- Displays any errors or warnings
- Useful for troubleshooting

---

## Tips & Tricks

### Maximum Speed

For fastest copying:
1. ✓ Copy to/from SSD drives
2. ✓ Close other disk-intensive programs
3. ✓ Use local drives (network adds latency)
4. ✓ Copy fewer, larger files rather than many tiny files

### Best Use Cases

**SM Copy excels at:**
- Large files (>100MB)
- Folders with many files
- Backup operations
- Moving project directories
- Copying to external drives

**Windows copy is fine for:**
- Single small files (<10MB)
- Quick drag-and-drop
- Files already copying slowly due to hardware

### Workflow Integration

**Backup workflow:**
```
1. Right-click important folder → SM Copy
2. Navigate to backup drive
3. Right-click → SM Paste
4. Verify completion message
```

**Project migration:**
```
1. Select multiple project folders
2. SM Copy
3. Navigate to new location
4. SM Paste
5. Verify, then delete originals if desired
```

### Keyboard Shortcuts

While SM Copy doesn't add keyboard shortcuts, you can:
- Use `Tab` to navigate in Explorer
- Use arrow keys to move between folders
- Use `Enter` to confirm dialogs
- Use `Esc` to close messages quickly

---

## Troubleshooting

### Context Menu Not Appearing

**Solution 1: Restart Explorer**
```
Ctrl+Shift+Esc → Windows Explorer → Restart
```

**Solution 2: Re-register**
```
Run as Admin: SMCopy.exe --register
```

**Solution 3: Check installation**
```
Verify file exists: SMCopy\bin\Release\net10.0-windows\SMCopy.exe
```

### "Administrator Privileges Required"

- Only installation/uninstallation requires admin
- Regular copying doesn't need admin
- Right-click .bat file → "Run as administrator"

### Copy is Slow

**Check these factors:**
- **Drive speed:** USB 2.0 is slow (~30 MB/s max)
- **Network speed:** Limited by network, not SM Copy
- **File size:** Many tiny files copy slower than one large file
- **Antivirus:** May scan files during copy

**View actual speed:**
- Check progress window
- Compare with drive's rated speed
- Network copies limited to network speed

### "No items to paste"

**Cause:** No files copied yet

**Solution:**
1. First use SM Copy on file/folder
2. Then use SM Paste

### Files Not Copied

**Check the log window:**
- Look for red error messages
- Common issues:
  - Permission denied
  - File in use
  - Destination full
  - File name too long

**Solutions:**
- Close programs using files
- Check available space
- Run with appropriate permissions
- Shorten file paths if needed

### Progress Stuck at 0%

**Causes:**
- Very fast copy already completed
- Very large file (progress updates periodically)
- Robocopy output parsing issue

**Check:**
- Look at the log for activity
- Wait a few seconds for update
- Check if destination files appearing

### Can't Uninstall

**Solution:**
```
Run as Admin: SMCopy.exe --unregister

Or manually:
- Delete registry key: HKEY_CLASSES_ROOT\*\shell\SMCopy
- Delete registry key: HKEY_CLASSES_ROOT\Directory\shell\SMCopy
- Delete registry key: HKEY_CLASSES_ROOT\Directory\Background\shell\SMPaste
- Delete folder: %APPDATA%\SMCopy
```

---

## Technical Details

### Robocopy Parameters Used

**For files:**
```
robocopy <source_dir> <dest_dir> <filename> /MT:32 /R:3 /W:1 /BYTES /NP /NDL /NFL
```

**For folders:**
```
robocopy <source> <dest> /E /MT:32 /R:3 /W:1 /BYTES /NP /NDL /NFL
```

**Parameter meanings:**
- `/E` - Copy subdirectories including empty ones
- `/MT:32` - Multi-threaded with up to 32 threads
- `/R:3` - Retry 3 times on failures
- `/W:1` - Wait 1 second between retries
- `/BYTES` - Show sizes in bytes (for accurate parsing)
- `/NP` - No percentage in output (we calculate our own)
- `/NDL` - No directory list
- `/NFL` - No file list (reduces output clutter)

### Storage Format

Clipboard stored as JSON:
```json
{
  "items": [
    {
      "path": "C:\\Users\\Name\\file.txt",
      "type": "file",
      "size": 1024000
    }
  ],
  "timestamp": "2025-11-18T20:45:00"
}
```

### Registry Entries

**File context menu:**
```
HKEY_CLASSES_ROOT\*\shell\SMCopy
  (Default) = "SM Copy"
  Icon = <path_to_exe>
  command\(Default) = "<path_to_exe>" --copy "%1"
```

**Folder context menu:**
```
HKEY_CLASSES_ROOT\Directory\shell\SMCopy
  (Default) = "SM Copy"
  Icon = <path_to_exe>
  command\(Default) = "<path_to_exe>" --copy "%1"
```

**Paste context menu:**
```
HKEY_CLASSES_ROOT\Directory\Background\shell\SMPaste
  (Default) = "SM Paste"
  Icon = <path_to_exe>
  command\(Default) = "<path_to_exe>" --paste "%V"
```

### Exit Codes

Robocopy exit codes (SM Copy interprets these):
- **0-7:** Success (various levels of success)
- **8+:** Failure (some files not copied)

Specific codes:
- **0:** No files copied (already exist)
- **1:** All files copied successfully
- **2:** Extra files in destination
- **3:** Some files copied, extra files present
- **8:** Some files or directories could not be copied

---

## FAQ

### Is SM Copy free?

Yes! SM Copy is open source. Feel free to use and modify.

### Does it replace Windows copy?

No, it coexists. You have both options:
- Windows Copy/Paste (Ctrl+C, Ctrl+V)
- SM Copy/Paste (right-click menu)

### Can I use both at the same time?

Yes! They use separate clipboards:
- Windows clipboard: Ctrl+C/V
- SM clipboard: SM Copy/Paste

### Does it move or copy files?

It copies. Original files remain unchanged. Delete manually if you want to move.

### What about file permissions?

SM Copy preserves:
- File attributes
- Timestamps
- Data

It does NOT copy (by default):
- NTFS permissions (ACLs)
- Owner information
- Audit information

### Can I customize settings?

Current version uses optimal defaults. Future versions may add:
- Thread count adjustment
- Retry count configuration
- Custom robocopy flags

### Does it work on Windows 11?

Yes! Tested on:
- Windows 10 (all versions)
- Windows 11
- Windows Server 2016+

### Is it safe?

Yes:
- Non-destructive (doesn't delete source)
- Uses Microsoft's robocopy (enterprise-proven)
- No network connections
- No data collection
- Open source code

### How do I update?

1. Uninstall old version
2. Build new version
3. Install new version

### Can I copy to network drives?

Yes! Works with:
- Mapped network drives (Z:, etc.)
- UNC paths (\\\\server\\share)
- Network locations

Speed limited by network, not SM Copy.

### What if I accidentally close the progress window?

The copy continues in the background! However:
- You won't see progress
- Can't cancel easily
- Check Task Manager for robocopy.exe

### Does it support drag-and-drop?

No, only context menu. For drag-and-drop, use Windows native copy.

### Can I use it from command line?

Yes!
```batch
SMCopy.exe --copy "C:\\path\\to\\file.txt"
SMCopy.exe --paste "D:\\destination"
```

### Does it support undo?

No. Copied files remain. Use Recycle Bin to remove unwanted copies.

### Maximum file size?

Limited by:
- File system (NTFS: 16 TB)
- Available disk space
- Not limited by SM Copy itself

### Can I pause and resume?

Current version:
- Cancel: Yes
- Pause: Not implemented (button present but disabled)

Robocopy's `/Z` flag could enable resume in future versions.

---

## Support & Feedback

### Getting Help

1. Check this guide
2. Read TESTING.md for scenarios
3. Check QUICKSTART.md for basics
4. Review logs in progress window

### Reporting Issues

Include:
1. What you were trying to do
2. What happened vs. what you expected
3. Error messages (screenshot)
4. Windows version
5. File types and sizes involved

### Contributing

SM Copy is open source:
- Report bugs
- Suggest features
- Submit code improvements
- Share with others!

---

## Appendix: Command Line Reference

### Installation
```batch
SMCopy.exe --register    # Install context menu (requires admin)
SMCopy.exe --unregister  # Remove context menu (requires admin)
```

### Usage
```batch
SMCopy.exe --copy <path> [<path2> ...]    # Copy file(s)/folder(s)
SMCopy.exe --paste <destination>          # Paste to destination
```

### Examples
```batch
# Copy single file
SMCopy.exe --copy "C:\Documents\report.pdf"

# Copy multiple files
SMCopy.exe --copy "C:\file1.txt" "C:\file2.txt" "C:\file3.txt"

# Copy folder
SMCopy.exe --copy "C:\Projects\MyProject"

# Paste to location
SMCopy.exe --paste "D:\Backup"
```

---

**Version:** 1.0  
**Last Updated:** November 2025  
**Platform:** Windows 10/11, Server 2016+  

**Thank you for using SM Copy! 🚀**

