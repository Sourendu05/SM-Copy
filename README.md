<<<<<<< HEAD
# SM Copy - Fast File Copy for Windows

SM Copy is a Windows application that integrates robocopy's high-speed file copying capabilities directly into your right-click context menu.

**Status: ✅ Ready to Build and Install!** All critical issues have been resolved.

## Features

- **Fast Copying**: Uses robocopy backend with multi-threading (up to 32 threads) for maximum speed
- **Context Menu Integration**: Right-click "SM Copy" and "SM Paste" options
- **Progress Tracking**: Real-time display of copy speed, progress, and ETA
- **Smart Clipboard**: Separate from Windows clipboard, stores multiple items
- **Optimized Settings**: Automatically configured for best performance and reliability
- **Error Handling**: Automatic retry logic (3 retries with 1 second wait)
- **Safe Operations**: Non-destructive copying, source files remain intact

## Installation

1. Build the project or download the installer
2. Run `SMCopy.exe --register` as Administrator to register context menu
3. Start using SM Copy by right-clicking on files/folders!

## Usage

1. **Copy**: Right-click on file(s) or folder(s) → Select "SM Copy"
2. **Paste**: Navigate to destination → Right-click in folder → Select "SM Paste"
3. Watch the progress window with real-time stats!

## Uninstallation

Run `SMCopy.exe --unregister` as Administrator to remove context menu entries.

## System Requirements

- Windows 10/11 or Windows Server 2016+
- .NET 10.0 Runtime or later
- Administrator privileges (for registration only)

## Technical Details

SM Copy uses robocopy with optimized parameters:
- Multi-threaded copying (up to 32 threads)
- Smart retry logic
- Efficient memory usage
- Progress monitoring

## Important Notes

- **Multi-Select Limitation**: Currently copies one item at a time from context menu. See `KNOWN_LIMITATIONS.md` for details and workarounds.
- **No Custom Icon**: Uses default executable icon (can be added later)
- **Progress Display**: May update in chunks for very fast copies (this is normal)

For complete list of limitations and workarounds, see `KNOWN_LIMITATIONS.md`.

## Documentation

- **QUICKSTART.md** - Get started in 5 minutes
- **USER_GUIDE.md** - Complete user manual with FAQ
- **TESTING.md** - Comprehensive test scenarios
- **KNOWN_LIMITATIONS.md** - Current limitations and workarounds
- **PROJECT_SUMMARY.md** - Technical implementation details

## License

Open Source - Feel free to use and modify!

=======
# SM Copy

SM Copy brings the speed of Windows Robocopy to everyone through a simple right-click menu. No command line needed—just copy and paste like you normally would, but faster.

## Why SM Copy?

Windows' built-in copy is slow for large files and folders. Robocopy is fast but requires command-line knowledge. SM Copy bridges this gap with a familiar interface that anyone can use.

## Features

- **Faster Copying**: Multi-threaded copying (32 threads) via Robocopy
- **Familiar Interface**: Works like regular copy/paste with right-click
- **No Learning Curve**: If you can copy/paste, you can use SM Copy
- **Handles Everything**: Works with both files and folders
- **Zero Configuration**: Optimized settings work out of the box

## Installation

1. Download the latest release
2. Run installer as Administrator
3. Start using "SM Copy" and "SM Paste" from right-click menu

## How It Works

**To Copy:**
- Right-click any file or folder → **"SM Copy"**

**To Paste:**
- Right-click in destination folder → **"SM Paste"**

SM Copy uses these Robocopy parameters behind the scenes:
```
/E /MT:32 /R:0 /W:0 /NFL /NDL
```
- `/E` - Copies all subdirectories
- `/MT:32` - Uses 32 parallel threads
- `/R:0 /W:0` - No retry delays for speed
- `/NFL /NDL` - Minimal logging overhead

## Requirements

- Windows 10 or later (Robocopy included)
- Administrator rights for installation only

## Technical Notes

This is a straightforward wrapper around Windows Robocopy. The context menu integration uses Windows Registry to add the right-click options. File paths are stored temporarily to enable the copy-paste workflow.

## Contributing

Pull requests and suggestions welcome! 

**Development Note**: This project was rapidly prototyped with AI coding assistants to handle the Windows Registry integration and process management boilerplate, allowing focus on the user experience and Robocopy optimization.

## License

MIT License - Free to use and modify

## Acknowledgments

- Windows Robocopy for the heavy lifting
- The community for testing and feedback

## Resources

- [Robocopy Documentation](https://learn.microsoft.com/en-us/windows-server/administration/windows-commands/robocopy)
- [Report Issues](../../issues)

---

**Disclaimer**: Always backup important files. While Robocopy is reliable, unexpected interruptions can occur with any copy operation.
>>>>>>> 9f96593dc270078229dc12bab35e35f999ad67a3
