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

