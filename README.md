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
