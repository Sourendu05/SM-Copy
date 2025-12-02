using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SMCopy.Core;
using SMCopy.Forms;

namespace SMCopy
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Initialize Windows Forms application
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length == 0)
            {
                // No arguments - show help or settings
                MessageBox.Show(
                    "SM Copy - Fast File Copy Tool\n\n" +
                    "Usage:\n" +
                    "  SMCopy.exe --register    : Register context menu\n" +
                    "  SMCopy.exe --unregister  : Unregister context menu\n" +
                    "  SMCopy.exe --copy <paths>: Copy files/folders\n" +
                    "  SMCopy.exe --paste <dest>: Paste to destination\n\n" +
                    "Right-click on files/folders to use SM Copy!",
                    "SM Copy",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            try
            {
                string command = args[0].ToLower();

                switch (command)
                {
                    case "--register":
                        RegisterContextMenu();
                        break;

                    case "--unregister":
                        UnregisterContextMenu();
                        break;

                    case "--copy":
                        HandleCopy(args.Skip(1).ToArray());
                        break;

                    case "--paste":
                        HandlePaste(args.Skip(1).FirstOrDefault() ?? "");
                        break;

                    default:
                        MessageBox.Show($"Unknown command: {command}", "SM Copy", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "SM Copy Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void RegisterContextMenu()
        {
            try
            {
                ContextMenuHandler.Register();
                MessageBox.Show("SM Copy context menu registered successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Administrator privileges required to register context menu.\n\nPlease run as administrator.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to register context menu:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void UnregisterContextMenu()
        {
            try
            {
                ContextMenuHandler.Unregister();
                MessageBox.Show("SM Copy context menu unregistered successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Administrator privileges required to unregister context menu.\n\nPlease run as administrator.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to unregister context menu:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void HandleCopy(string[] paths)
        {
            if (paths.Length == 0)
            {
                MessageBox.Show("No files or folders selected to copy.", "SM Copy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Sanitize paths: remove quotes and fix escaping issues
            var sanitizedPaths = paths.Select(SanitizePath).ToArray();

            // Validate paths
            var (validPaths, invalidPaths) = MultiSelectHelper.ValidatePaths(sanitizedPaths);

            if (validPaths.Count == 0)
            {
                MessageBox.Show("No valid files or folders selected.", "SM Copy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Save valid paths
            CopyManager.SaveCopiedItems(validPaths.ToArray());

            // Removed annoying popup - files are now silently copied to clipboard
            // User will see the progress when they paste
            // string summary = MultiSelectHelper.GetSelectionSummary(validPaths);
            // MessageBox.Show($"Copied to SM Clipboard:\n\n{summary}", "SM Copy", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Warn about invalid paths if any
            if (invalidPaths.Count > 0)
            {
                MessageBox.Show($"Warning: {invalidPaths.Count} item(s) were skipped (not found).", "SM Copy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static string SanitizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            // Remove surrounding quotes
            path = path.Trim('"', '\'');

            // Windows sometimes adds \" at the end due to escaping
            // e.g., "C:\Folder\" becomes C:\" after quote removal
            // Fix this by removing trailing backslash-quote combinations
            while (path.EndsWith("\\\"") || path.EndsWith("\\'"))
            {
                path = path.Substring(0, path.Length - 2);
            }

            // Remove any remaining trailing quotes
            path = path.TrimEnd('"', '\'');

            return path;
        }

        private static void HandlePaste(string destination)
        {
            // Sanitize destination path
            destination = SanitizePath(destination);

            if (string.IsNullOrEmpty(destination) || !Directory.Exists(destination))
            {
                MessageBox.Show("Invalid destination folder.", "SM Copy", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var items = CopyManager.LoadCopiedItems();
            if (items == null || items.Count == 0)
            {
                MessageBox.Show("No items to paste. Please use SM Copy first.", "SM Copy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Show progress window
            var progressForm = new ProgressWindow(items, destination);
            Application.Run(progressForm);
        }
    }
}

