using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SMCopySetup
{
    public class SetupForm : Form
    {
        // Install paths
        private static readonly string InstallDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), 
            "SM Copy"
        );
        private static readonly string ExePath = Path.Combine(InstallDir, "SMCopy.exe");
        
        // UI Controls
        private Panel headerPanel = null!;
        private Label lblTitle = null!;
        private Label lblSubtitle = null!;
        private Label lblStatus = null!;
        private ProgressBar progressBar = null!;
        private TextBox txtLog = null!;
        private Button btnInstall = null!;
        private Button btnUninstall = null!;
        private Button btnClose = null!;
        private CheckBox chkDesktopShortcut = null!;

        public SetupForm()
        {
            InitializeUI();
            CheckExistingInstallation();
        }

        private void InitializeUI()
        {
            // Form settings
            this.Text = "SM Copy Setup";
            this.Size = new Size(520, 480);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(25, 25, 28);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9);

            int padding = 25;
            int y = 0;

            // Header panel with gradient
            headerPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(this.ClientSize.Width, 100),
                BackColor = Color.FromArgb(40, 100, 180)
            };
            headerPanel.Paint += (s, e) =>
            {
                using (var brush = new LinearGradientBrush(
                    headerPanel.ClientRectangle,
                    Color.FromArgb(30, 90, 170),
                    Color.FromArgb(50, 130, 220),
                    LinearGradientMode.Horizontal))
                {
                    e.Graphics.FillRectangle(brush, headerPanel.ClientRectangle);
                }
            };
            this.Controls.Add(headerPanel);

            // Title
            lblTitle = new Label
            {
                Text = "⚡ SM Copy",
                Location = new Point(padding, 20),
                Size = new Size(headerPanel.Width - padding * 2, 40),
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            headerPanel.Controls.Add(lblTitle);

            // Subtitle
            lblSubtitle = new Label
            {
                Text = "Lightning-fast file copying powered by Robocopy",
                Location = new Point(padding, 60),
                Size = new Size(headerPanel.Width - padding * 2, 25),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(200, 220, 255),
                BackColor = Color.Transparent
            };
            headerPanel.Controls.Add(lblSubtitle);

            y = headerPanel.Bottom + 25;

            // Status label
            lblStatus = new Label
            {
                Text = "Ready to install",
                Location = new Point(padding, y),
                Size = new Size(this.ClientSize.Width - padding * 2, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 200, 255)
            };
            this.Controls.Add(lblStatus);
            y += 30;

            // Progress bar
            progressBar = new ProgressBar
            {
                Location = new Point(padding, y),
                Size = new Size(this.ClientSize.Width - padding * 2, 20),
                Style = ProgressBarStyle.Continuous
            };
            this.Controls.Add(progressBar);
            y += 35;

            // Desktop shortcut checkbox
            chkDesktopShortcut = new CheckBox
            {
                Text = "Create desktop shortcut",
                Location = new Point(padding, y),
                Size = new Size(200, 25),
                ForeColor = Color.LightGray,
                Checked = false
            };
            this.Controls.Add(chkDesktopShortcut);
            y += 35;

            // Log textbox
            txtLog = new TextBox
            {
                Location = new Point(padding, y),
                Size = new Size(this.ClientSize.Width - padding * 2, 140),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(15, 15, 18),
                ForeColor = Color.FromArgb(100, 255, 150),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(txtLog);
            y += 155;

            // Buttons
            int btnWidth = 130;
            int btnHeight = 38;
            int btnSpacing = 15;
            int totalWidth = btnWidth * 3 + btnSpacing * 2;
            int btnX = (this.ClientSize.Width - totalWidth) / 2;

            btnInstall = new Button
            {
                Text = "📦 Install",
                Location = new Point(btnX, y),
                Size = new Size(btnWidth, btnHeight),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(40, 160, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnInstall.FlatAppearance.BorderSize = 0;
            btnInstall.Click += async (s, e) => await InstallAsync();
            this.Controls.Add(btnInstall);

            btnUninstall = new Button
            {
                Text = "🗑️ Uninstall",
                Location = new Point(btnX + btnWidth + btnSpacing, y),
                Size = new Size(btnWidth, btnHeight),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(180, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnUninstall.FlatAppearance.BorderSize = 0;
            btnUninstall.Click += async (s, e) => await UninstallAsync();
            this.Controls.Add(btnUninstall);

            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(btnX + (btnWidth + btnSpacing) * 2, y),
                Size = new Size(btnWidth, btnHeight),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(60, 60, 65),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);

            Log("SM Copy Setup ready.");
            Log($"Install location: {InstallDir}");
        }

        private void CheckExistingInstallation()
        {
            if (File.Exists(ExePath))
            {
                lblStatus.Text = "SM Copy is already installed";
                lblStatus.ForeColor = Color.FromArgb(255, 200, 100);
                btnInstall.Text = "🔄 Reinstall";
                Log("Existing installation detected.");
            }
        }

        private async Task InstallAsync()
        {
            SetButtonsEnabled(false);
            progressBar.Value = 0;

            try
            {
                // Step 1: Extract SMCopy.exe
                UpdateStatus("Extracting files...", 10);
                await Task.Run(() => ExtractApplication());
                progressBar.Value = 40;

                // Step 2: Register context menu
                UpdateStatus("Registering context menu...", 50);
                await Task.Run(() => RegisterContextMenu());
                progressBar.Value = 70;

                // Step 3: Create desktop shortcut if requested
                if (chkDesktopShortcut.Checked)
                {
                    UpdateStatus("Creating shortcut...", 80);
                    await Task.Run(() => CreateDesktopShortcut());
                }
                progressBar.Value = 90;

                // Step 4: Register uninstaller
                UpdateStatus("Finalizing...", 95);
                await Task.Run(() => RegisterUninstaller());
                progressBar.Value = 100;

                UpdateStatus("✓ Installation complete!", 100);
                Log("═══════════════════════════════════════");
                Log("SM Copy installed successfully!");
                Log("Right-click any file/folder to use SM Copy.");

                MessageBox.Show(
                    "SM Copy has been installed successfully!\n\n" +
                    "Right-click on any file or folder to see:\n" +
                    "• 'SM Copy' - to copy items\n" +
                    "• 'SM Paste' - to paste items (in folder background)\n\n" +
                    "Enjoy faster file transfers!",
                    "Installation Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                UpdateStatus("Installation failed!", 0);
                Log($"ERROR: {ex.Message}");
                MessageBox.Show($"Installation failed:\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetButtonsEnabled(true);
                CheckExistingInstallation();
            }
        }

        private async Task UninstallAsync()
        {
            var result = MessageBox.Show(
                "Are you sure you want to uninstall SM Copy?",
                "Confirm Uninstall",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result != DialogResult.Yes) return;

            SetButtonsEnabled(false);
            progressBar.Value = 0;

            try
            {
                // Step 1: Unregister context menu
                UpdateStatus("Removing context menu...", 20);
                await Task.Run(() => UnregisterContextMenu());
                progressBar.Value = 40;

                // Step 2: Remove files
                UpdateStatus("Removing files...", 60);
                await Task.Run(() => RemoveFiles());
                progressBar.Value = 80;

                // Step 3: Remove uninstaller entry
                UpdateStatus("Cleaning up...", 90);
                await Task.Run(() => RemoveUninstaller());
                progressBar.Value = 100;

                UpdateStatus("Uninstall complete", 100);
                Log("SM Copy has been uninstalled.");

                MessageBox.Show(
                    "SM Copy has been uninstalled successfully.",
                    "Uninstall Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                lblStatus.Text = "Ready to install";
                lblStatus.ForeColor = Color.FromArgb(100, 200, 255);
                btnInstall.Text = "📦 Install";
            }
            catch (Exception ex)
            {
                UpdateStatus("Uninstall failed!", 0);
                Log($"ERROR: {ex.Message}");
                MessageBox.Show($"Uninstall failed:\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetButtonsEnabled(true);
            }
        }

        #region Installation Methods

        private void ExtractApplication()
        {
            Log("Creating install directory...");
            Directory.CreateDirectory(InstallDir);

            Log("Extracting SMCopy.exe...");
            
            // Try to find embedded resource
            var assembly = Assembly.GetExecutingAssembly();
            string? resourceName = null;
            
            foreach (var name in assembly.GetManifestResourceNames())
            {
                if (name.EndsWith("SMCopy.exe", StringComparison.OrdinalIgnoreCase))
                {
                    resourceName = name;
                    break;
                }
            }

            if (resourceName != null)
            {
                // Extract embedded resource
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        using (var fileStream = new FileStream(ExePath, FileMode.Create, FileAccess.Write))
                        {
                            stream.CopyTo(fileStream);
                        }
                        Log($"Extracted: {ExePath}");
                        return;
                    }
                }
            }

            // Fallback: Look for SMCopy.exe in same directory as installer
            string localExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SMCopy.exe");
            if (File.Exists(localExe))
            {
                File.Copy(localExe, ExePath, true);
                Log($"Copied from: {localExe}");
                return;
            }

            // Fallback 2: Look in parent/sibling directories
            string? searchDir = AppDomain.CurrentDomain.BaseDirectory;
            for (int i = 0; i < 5; i++)
            {
                if (searchDir == null) break;
                
                string[] possiblePaths = {
                    Path.Combine(searchDir, "SMCopy.exe"),
                    Path.Combine(searchDir, "SMCopy", "bin", "Publish", "SMCopy.exe"),
                    Path.Combine(searchDir, "SMCopy", "bin", "Release", "net8.0-windows", "win-x64", "publish", "SMCopy.exe"),
                    Path.Combine(searchDir, "SMCopy", "bin", "Release", "net8.0-windows", "SMCopy.exe"),
                };

                foreach (var path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        File.Copy(path, ExePath, true);
                        Log($"Found and copied from: {path}");
                        return;
                    }
                }
                
                searchDir = Directory.GetParent(searchDir)?.FullName;
            }

            throw new FileNotFoundException(
                "SMCopy.exe not found. Please ensure SMCopy.exe is in the same folder as the setup, " +
                "or build the project first using: dotnet publish SMCopy -c Release"
            );
        }

        private void RegisterContextMenu()
        {
            Log("Registering context menu entries...");

            // Register for files: HKCR\*\shell\SMCopy
            using (var key = Registry.ClassesRoot.CreateSubKey(@"*\shell\SMCopy"))
            {
                key?.SetValue("", "SM Copy");
                key?.SetValue("Icon", $"\"{ExePath}\"");
                
                using (var cmdKey = key?.CreateSubKey("command"))
                {
                    cmdKey?.SetValue("", $"\"{ExePath}\" --copy \"%1\"");
                }
            }
            Log("  ✓ File context menu registered");

            // Register for folders: HKCR\Directory\shell\SMCopy
            using (var key = Registry.ClassesRoot.CreateSubKey(@"Directory\shell\SMCopy"))
            {
                key?.SetValue("", "SM Copy");
                key?.SetValue("Icon", $"\"{ExePath}\"");
                
                using (var cmdKey = key?.CreateSubKey("command"))
                {
                    cmdKey?.SetValue("", $"\"{ExePath}\" --copy \"%1\"");
                }
            }
            Log("  ✓ Folder context menu registered");

            // Register for folder background: HKCR\Directory\Background\shell\SMPaste
            using (var key = Registry.ClassesRoot.CreateSubKey(@"Directory\Background\shell\SMPaste"))
            {
                key?.SetValue("", "SM Paste");
                key?.SetValue("Icon", $"\"{ExePath}\"");
                
                using (var cmdKey = key?.CreateSubKey("command"))
                {
                    cmdKey?.SetValue("", $"\"{ExePath}\" --paste \"%V\"");
                }
            }
            Log("  ✓ Paste context menu registered");
        }

        private void UnregisterContextMenu()
        {
            Log("Removing context menu entries...");

            try { Registry.ClassesRoot.DeleteSubKeyTree(@"*\shell\SMCopy", false); }
            catch { }

            try { Registry.ClassesRoot.DeleteSubKeyTree(@"Directory\shell\SMCopy", false); }
            catch { }

            try { Registry.ClassesRoot.DeleteSubKeyTree(@"Directory\Background\shell\SMPaste", false); }
            catch { }

            Log("  ✓ Context menu entries removed");
        }

        private void CreateDesktopShortcut()
        {
            try
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string shortcutPath = Path.Combine(desktop, "SM Copy.lnk");

                // Use PowerShell to create shortcut (works without COM)
                string psScript = $@"
                    $WshShell = New-Object -ComObject WScript.Shell
                    $Shortcut = $WshShell.CreateShortcut('{shortcutPath}')
                    $Shortcut.TargetPath = '{ExePath}'
                    $Shortcut.WorkingDirectory = '{InstallDir}'
                    $Shortcut.Description = 'SM Copy - Fast File Transfer'
                    $Shortcut.Save()
                ";

                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -Command \"{psScript.Replace("\"", "\\\"")}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    process?.WaitForExit(5000);
                }

                Log("  ✓ Desktop shortcut created");
            }
            catch (Exception ex)
            {
                Log($"  ⚠ Could not create shortcut: {ex.Message}");
            }
        }

        private void RegisterUninstaller()
        {
            Log("Registering uninstaller...");

            string uninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SMCopy";
            using (var key = Registry.LocalMachine.CreateSubKey(uninstallKey))
            {
                if (key != null)
                {
                    // For single-file apps, use the current process path
                    string setupPath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName
                        ?? Path.Combine(AppContext.BaseDirectory, "SMCopySetup.exe");

                    key.SetValue("DisplayName", "SM Copy");
                    key.SetValue("DisplayVersion", "1.0.0");
                    key.SetValue("Publisher", "SM Copy");
                    key.SetValue("InstallLocation", InstallDir);
                    key.SetValue("DisplayIcon", ExePath);
                    key.SetValue("UninstallString", $"\"{setupPath}\"");
                    key.SetValue("NoModify", 1);
                    key.SetValue("NoRepair", 1);
                }
            }

            Log("  ✓ Uninstaller registered");
        }

        private void RemoveFiles()
        {
            Log("Removing application files...");

            // Remove app data
            string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SMCopy");
            if (Directory.Exists(appData))
            {
                Directory.Delete(appData, true);
                Log("  ✓ App data removed");
            }

            // Remove desktop shortcut
            string shortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "SM Copy.lnk");
            if (File.Exists(shortcut))
            {
                File.Delete(shortcut);
                Log("  ✓ Desktop shortcut removed");
            }

            // Remove install directory
            if (Directory.Exists(InstallDir))
            {
                try
                {
                    Directory.Delete(InstallDir, true);
                    Log("  ✓ Install directory removed");
                }
                catch
                {
                    Log("  ⚠ Could not remove install directory (may be in use)");
                }
            }
        }

        private void RemoveUninstaller()
        {
            try
            {
                Registry.LocalMachine.DeleteSubKeyTree(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SMCopy", false);
                Log("  ✓ Uninstaller entry removed");
            }
            catch { }
        }

        #endregion

        #region UI Helpers

        private void UpdateStatus(string text, int progress)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateStatus(text, progress)));
                return;
            }
            lblStatus.Text = text;
            progressBar.Value = progress;
        }

        private void Log(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Log(message)));
                return;
            }
            txtLog.AppendText(message + Environment.NewLine);
        }

        private void SetButtonsEnabled(bool enabled)
        {
            btnInstall.Enabled = enabled;
            btnUninstall.Enabled = enabled;
            btnClose.Enabled = enabled;
            chkDesktopShortcut.Enabled = enabled;
        }

        #endregion
    }
}

