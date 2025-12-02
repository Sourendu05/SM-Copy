# SM Copy

SM Copy brings blazing-fast file transfers to everyone through a simple right-click menu. No command line needed—just copy and paste like you normally would, but significantly faster.

## Why SM Copy?

Windows' built-in copy is slow for large files and folders. SM Copy uses Windows' native high-performance APIs to deliver maximum transfer speeds with a familiar interface that anyone can use.

## Features

- **Maximum Performance**: Direct Windows Kernel API for optimal I/O speed
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

SM Copy leverages the Windows Native Copy API (CopyFileEx) to achieve direct, high-performance file transfers at the kernel level, bypassing the overhead of traditional copy methods.

## Requirements

- Windows 10 or later
- Administrator rights for installation only

## Technical Notes

SM Copy uses the Windows Kernel API for direct high-performance I/O operations. The context menu integration uses Windows Registry to add the right-click options. File paths are stored temporarily to enable the copy-paste workflow.

This approach provides optimal transfer speeds by working directly with Windows' native file system APIs rather than wrapping command-line tools.

## Contributing

Pull requests and suggestions welcome! 

**Development Note**: This project was rapidly prototyped with AI coding assistants to handle the Windows Registry integration and process management boilerplate, allowing focus on the user experience and performance optimization.

## License

MIT License - Free to use and modify

## Acknowledgments

- Windows Native API for the performance foundation
- The community for testing and feedback

## Resources

- [Report Issues](../../issues)

---

**Disclaimer**: Always backup important files. While Windows' native copy APIs are reliable, unexpected interruptions can occur with any copy operation.
