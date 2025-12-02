using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SMCopyInstaller
{
    public partial class InstallerForm : Form
    {
        private Label lblTitle = null!;
        private Label lblStatus = null!;
        private ProgressBar progressBar = null!;
        private TextBox txtLog = null!;
        private Button btnInstall = null!;
        private Button btnUninstall = null!;
        private Button btnClose = null!;

        private readonly string _solutionDir;
        private readonly string _installerDir;
        private readonly string _exePath;

        public InstallerForm()
        {
            // Get paths
            _installerDir = AppDomain.CurrentDomain.BaseDirectory;
            
            // Find solution directory by walking up
            string? currentDir = _installerDir;
            string? solutionDir = null;
            
            while (currentDir != null)
            {
                if (File.Exists(Path.Combine(currentDir, "SMCopy.sln")))
                {
                    solutionDir = currentDir;
                    break;
                }
                currentDir = Directory.GetParent(currentDir)?.FullName;
            }
            
            // Fallback if not found (though it should be if running from bin)
            _solutionDir = solutionDir ?? Path.GetFullPath(Path.Combine(_installerDir, "..\\..\\..\\..\\.."));
            _exePath = Path.Combine(_solutionDir, "SMCopy\\bin\\Release\\net10.0-windows\\SMCopy.exe");

            InitializeComponent();
            InitializeUI();
            
            Log("SM Copy Installer ready.");
            Log($"Installer location: {_installerDir}");
            Log($"Solution directory: {_solutionDir}");
        }

        private void InitializeComponent()
        {
            this.Text = "SM Copy Installer";
            this.Size = new Size(700, 550);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void InitializeUI()
        {
            int padding = 20;
            int currentY = padding;

            // Title
            lblTitle = new Label
            {
                Text = "SM Copy Installer",
                Location = new Point(padding, currentY),
                Size = new Size(this.ClientSize.Width - padding * 2, 40),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblTitle);
            currentY += 50;

            // Description
            var lblDesc = new Label
            {
                Text = "Fast file copy tool for Windows using Robocopy",
                Location = new Point(padding, currentY),
                Size = new Size(this.ClientSize.Width - padding * 2, 25),
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray
            };
            this.Controls.Add(lblDesc);
            currentY += 35;

            // Status Label
            lblStatus = new Label
            {
                Text = "Ready to install",
                Location = new Point(padding, currentY),
                Size = new Size(this.ClientSize.Width - padding * 2, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            this.Controls.Add(lblStatus);
            currentY += 35;

            // Progress Bar
            progressBar = new ProgressBar
            {
                Location = new Point(padding, currentY),
                Size = new Size(this.ClientSize.Width - padding * 2, 25),
                Style = ProgressBarStyle.Continuous
            };
            this.Controls.Add(progressBar);
            currentY += 35;

            // Log TextBox
            txtLog = new TextBox
            {
                Location = new Point(padding, currentY),
                Size = new Size(this.ClientSize.Width - padding * 2, 200),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                BackColor = Color.Black,
                ForeColor = Color.LightGreen
            };
            this.Controls.Add(txtLog);
            currentY += 210;

            // Buttons
            int btnWidth = 150;
            int btnHeight = 40;
            int btnSpacing = 10;
            int totalBtnWidth = (btnWidth * 3) + (btnSpacing * 2);
            int btnStartX = (this.ClientSize.Width - totalBtnWidth) / 2;

            btnInstall = new Button
            {
                Text = "Install",
                Location = new Point(btnStartX, currentY),
                Size = new Size(btnWidth, btnHeight),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnInstall.Click += async (s, e) => await InstallAsync();
            this.Controls.Add(btnInstall);

            btnUninstall = new Button
            {
                Text = "Uninstall",
                Location = new Point(btnStartX + btnWidth + btnSpacing, currentY),
                Size = new Size(btnWidth, btnHeight),
                Font = new Font("Segoe UI", 10),
                FlatStyle = FlatStyle.Flat
            };
            btnUninstall.Click += async (s, e) => await UninstallAsync();
            this.Controls.Add(btnUninstall);

            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(btnStartX + (btnWidth + btnSpacing) * 2, currentY),
                Size = new Size(btnWidth, btnHeight),
                Font = new Font("Segoe UI", 10),
                FlatStyle = FlatStyle.Flat
            };
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }

        private async Task InstallAsync()
        {
            SetButtonsEnabled(false);
            UpdateStatus("Installing SM Copy...");
            progressBar.Value = 0;

            try
            {
                // Step 1: Uninstall previous
                Log("═══════════════════════════════════════");
                Log("Step 1/3: Removing previous installation...");
                progressBar.Value = 10;
                await UninstallSilentAsync();
                progressBar.Value = 30;

                // Step 2: Build
                Log("═══════════════════════════════════════");
                Log("Step 2/3: Building SM Copy...");
                bool buildSuccess = await BuildAsync();
                progressBar.Value = 60;

                if (!buildSuccess)
                {
                    UpdateStatus("Build failed!");
                    Log("ERROR: Build failed. Installation aborted.");
                    SetButtonsEnabled(true);
                    return;
                }

                // Step 3: Register
                Log("═══════════════════════════════════════");
                Log("Step 3/3: Registering context menu...");
                progressBar.Value = 80;
                bool registerSuccess = await RegisterContextMenuAsync();
                progressBar.Value = 100;

                if (registerSuccess)
                {
                    UpdateStatus("Installation completed successfully!");
                    Log("═══════════════════════════════════════");
                    Log("✓ SM Copy installed successfully!");
                    Log("You can now right-click files/folders to use SM Copy.");
                    MessageBox.Show(
                        "SM Copy has been installed successfully!\n\n" +
                        "Right-click on any file or folder to see the\n" +
                        "\"SM Copy\" and \"SM Paste\" options.",
                        "Installation Complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                else
                {
                    UpdateStatus("Registration failed!");
                    Log("ERROR: Context menu registration failed.");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus("Installation failed!");
                Log($"ERROR: {ex.Message}");
                MessageBox.Show($"Installation failed:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetButtonsEnabled(true);
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
            UpdateStatus("Uninstalling SM Copy...");
            progressBar.Value = 0;

            try
            {
                await UninstallSilentAsync();
                progressBar.Value = 100;
                UpdateStatus("Uninstallation completed!");
                Log("✓ SM Copy uninstalled successfully.");
                MessageBox.Show(
                    "SM Copy has been uninstalled.",
                    "Uninstall Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                UpdateStatus("Uninstallation failed!");
                Log($"ERROR: {ex.Message}");
            }
            finally
            {
                SetButtonsEnabled(true);
            }
        }

        private async Task UninstallSilentAsync()
        {
            if (File.Exists(_exePath))
            {
                Log("Removing context menu entries...");
                await RunProcessAsync(_exePath, "--unregister");
            }

            // Clean AppData
            string appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SMCopy");
            if (Directory.Exists(appDataDir))
            {
                Log("Removing application data...");
                Directory.Delete(appDataDir, true);
            }

            Log("Uninstall completed.");
        }

        private async Task<bool> BuildAsync()
        {
            Log("Checking for .NET SDK...");
            
            // Check dotnet
            var dotnetCheck = await RunProcessAsync("dotnet", "--version");
            if (!dotnetCheck)
            {
                Log("ERROR: .NET SDK not found!");
                Log("Please install .NET 10.0 SDK from:");
                Log("https://dotnet.microsoft.com/download");
                return false;
            }

            Log("Building solution...");
            string solutionFile = Path.Combine(_solutionDir, "SMCopy.sln");
            
            if (!File.Exists(solutionFile))
            {
                Log($"ERROR: Solution file not found: {solutionFile}");
                return false;
            }

            bool success = await RunProcessAsync("dotnet", $"build \"{solutionFile}\" -c Release", _solutionDir);
            
            if (success && File.Exists(_exePath))
            {
                Log($"✓ Build successful: {_exePath}");
                return true;
            }
            else
            {
                Log("✗ Build failed or output not found.");
                return false;
            }
        }

        private async Task<bool> RegisterContextMenuAsync()
        {
            if (!File.Exists(_exePath))
            {
                Log($"ERROR: SMCopy.exe not found at: {_exePath}");
                return false;
            }

            Log($"Registering: {_exePath}");
            return await RunProcessAsync(_exePath, "--register");
        }

        private async Task<bool> RunProcessAsync(string fileName, string arguments, string? workingDir = null)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDir ?? Environment.SystemDirectory
                };

                using var process = new Process { StartInfo = processInfo };
                
                process.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Log(e.Data);
                    }
                };

                process.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Log($"ERROR: {e.Data}");
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await process.WaitForExitAsync();

                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Log($"ERROR: {ex.Message}");
                return false;
            }
        }

        private void UpdateStatus(string status)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateStatus(status)));
                return;
            }
            lblStatus.Text = status;
        }

        private void Log(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Log(message)));
                return;
            }
            txtLog.AppendText($"{message}{Environment.NewLine}");
        }

        private void SetButtonsEnabled(bool enabled)
        {
            btnInstall.Enabled = enabled;
            btnUninstall.Enabled = enabled;
            btnClose.Enabled = enabled;
        }
    }
}
