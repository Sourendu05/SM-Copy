using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SMCopy.Core;
using SMCopy.Models;

namespace SMCopy.Forms
{
    public partial class ProgressWindow : Form
    {
        private readonly List<CopyItem> _items;
        private readonly string _destination;
        private CancellationTokenSource _cts = new();
        private DateTime _startTime;
        private long _totalBytes = 0;
        private long _copiedBytes = 0;
        private int _successCount = 0;
        private int _failCount = 0;
        private bool _isCompleted = false;
        private FileCopier? _copier;

        // UI
        private ProgressBar progressBar = null!;
        private Label lblStatus = null!;
        private Label lblFile = null!;
        private Label lblSpeed = null!;
        private Label lblTime = null!;
        private Label lblSize = null!;
        private Button btnCancel = null!;
        private TextBox txtLog = null!;
        private CheckBox chkLog = null!;

        public ProgressWindow(List<CopyItem> items, string destination)
        {
            _items = items;
            _destination = destination;
            _totalBytes = items.Sum(i => i.Size);

            InitializeComponent();
            InitializeUI();
            this.Load += async (s, e) => await StartCopyAsync();
        }

        private void InitializeComponent()
        {
            Text = "SM Copy - Fast File Transfer";
            Size = new Size(550, 320);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            FormClosing += OnFormClosing;
            BackColor = Color.FromArgb(30, 30, 30);
            ForeColor = Color.White;
        }

        private void InitializeUI()
        {
            int p = 20, y = p;

            lblStatus = new Label
            {
                Text = "Starting...",
                Location = new Point(p, y),
                Size = new Size(ClientSize.Width - p * 2, 28),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 200, 255)
            };
            Controls.Add(lblStatus);
            y += 35;

            progressBar = new ProgressBar
            {
                Location = new Point(p, y),
                Size = new Size(ClientSize.Width - p * 2, 25),
                Style = ProgressBarStyle.Continuous
            };
            Controls.Add(progressBar);
            y += 35;

            lblFile = new Label
            {
                Text = "",
                Location = new Point(p, y),
                Size = new Size(ClientSize.Width - p * 2, 22),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.LightGray
            };
            Controls.Add(lblFile);
            y += 28;

            lblSpeed = new Label
            {
                Text = "Speed: --",
                Location = new Point(p, y),
                Size = new Size(200, 22),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.LightGreen
            };
            Controls.Add(lblSpeed);

            lblTime = new Label
            {
                Text = "Time: --",
                Location = new Point(p + 200, y),
                Size = new Size(ClientSize.Width - p * 2 - 200, 22),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.LightGray
            };
            Controls.Add(lblTime);
            y += 28;

            lblSize = new Label
            {
                Text = $"0 B / {CopyManager.FormatBytes(_totalBytes)}",
                Location = new Point(p, y),
                Size = new Size(ClientSize.Width - p * 2, 22),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray
            };
            Controls.Add(lblSize);
            y += 35;

            chkLog = new CheckBox
            {
                Text = "Show Details",
                Location = new Point(p, y),
                Size = new Size(120, 25),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9)
            };
            chkLog.CheckedChanged += (s, e) =>
            {
                txtLog.Visible = chkLog.Checked;
                Height = chkLog.Checked ? 480 : 320;
            };
            Controls.Add(chkLog);

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(ClientSize.Width - p - 90, y - 3),
                Size = new Size(90, 32),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderColor = Color.Gray;
            btnCancel.Click += OnCancelClick;
            Controls.Add(btnCancel);
            y += 40;

            txtLog = new TextBox
            {
                Location = new Point(p, y),
                Size = new Size(ClientSize.Width - p * 2, 150),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Font = new Font("Consolas", 8),
                BackColor = Color.FromArgb(20, 20, 20),
                ForeColor = Color.LightGreen,
                Visible = false
            };
            Controls.Add(txtLog);
        }

        private async Task StartCopyAsync()
        {
            _startTime = DateTime.Now;
            Log($"SM Copy - {_items.Count} item(s) to {_destination}");

            try
            {
                var files = _items.Where(i => i.Type == "file").ToList();
                var folders = _items.Where(i => i.Type == "folder").ToList();

                // Copy folders
                foreach (var folder in folders)
                {
                    if (_cts.Token.IsCancellationRequested) break;
                    await CopyFolderAsync(folder);
                }

                // Copy files (grouped by source directory)
                var groups = files.GroupBy(f => Path.GetDirectoryName(f.Path)).ToList();
                foreach (var group in groups)
                {
                    if (_cts.Token.IsCancellationRequested) break;
                    await CopyFilesAsync(group.Key ?? "", group.ToList());
                }

                if (!_cts.Token.IsCancellationRequested)
                    ShowComplete();
            }
            catch (Exception ex)
            {
                Log($"ERROR: {ex.Message}");
                ShowError(ex.Message);
            }
        }

        private async Task CopyFolderAsync(CopyItem folder)
        {
            string name = Path.GetFileName(folder.Path);
            SetStatus($"Copying: {name}");
            Log($"Folder: {folder.Path}");

            _copier = new FileCopier();
            long startBytes = _copiedBytes;

            _copier.ProgressChanged += (s, p) =>
            {
                _copiedBytes = startBytes + p.CopiedBytes;
                UpdateUI(p.CurrentFile, p.SpeedBytesPerSecond, p.EstimatedTimeRemaining);
            };
            _copier.LogMessage += (s, msg) => Log(msg);

            string dest = Path.Combine(_destination, name);
            bool ok = await _copier.CopyFolderAsync(folder.Path, dest, folder.Size, _cts.Token);

            if (ok) _successCount++;
            else _failCount++;
        }

        private async Task CopyFilesAsync(string sourceDir, List<CopyItem> files)
        {
            var names = files.Select(f => Path.GetFileName(f.Path)).ToList();
            long size = files.Sum(f => f.Size);

            SetStatus($"Copying {names.Count} file(s)...");
            Log($"Files: {sourceDir} ({names.Count} files)");

            _copier = new FileCopier();
            long startBytes = _copiedBytes;

            _copier.ProgressChanged += (s, p) =>
            {
                _copiedBytes = startBytes + p.CopiedBytes;
                UpdateUI(p.CurrentFile, p.SpeedBytesPerSecond, p.EstimatedTimeRemaining);
            };
            _copier.LogMessage += (s, msg) => Log(msg);

            bool ok = await _copier.CopyFilesAsync(sourceDir, _destination, names, size, _cts.Token);

            if (ok) _successCount += names.Count;
            else _failCount += names.Count;
        }

        private void UpdateUI(string file, double speed, TimeSpan eta)
        {
            if (InvokeRequired) { BeginInvoke(new Action(() => UpdateUI(file, speed, eta))); return; }

            double pct = _totalBytes > 0 ? (_copiedBytes * 100.0 / _totalBytes) : 0;
            progressBar.Value = Math.Min(100, Math.Max(0, (int)pct));

            if (!string.IsNullOrEmpty(file))
                lblFile.Text = file;

            if (speed > 0)
                lblSpeed.Text = $"Speed: {CopyManager.FormatSpeed(speed)}";

            if (eta.TotalSeconds > 0 && eta.TotalSeconds < 86400)
                lblTime.Text = $"Time: ~{CopyManager.FormatTimeSpan(eta)} left";

            lblSize.Text = $"{CopyManager.FormatBytes(_copiedBytes)} / {CopyManager.FormatBytes(_totalBytes)}";
        }

        private void SetStatus(string text)
        {
            if (InvokeRequired) { Invoke(new Action(() => SetStatus(text))); return; }
            lblStatus.Text = text;
        }

        private void ShowComplete()
        {
            _isCompleted = true;
            var elapsed = DateTime.Now - _startTime;
            double speed = elapsed.TotalSeconds > 0 ? _totalBytes / elapsed.TotalSeconds : 0;

            if (InvokeRequired) { Invoke(new Action(ShowComplete)); return; }

            lblStatus.Text = "Transfer Complete!";
            lblStatus.ForeColor = Color.LightGreen;
            progressBar.Value = 100;

            lblFile.Text = $"{_successCount} item(s) copied" + (_failCount > 0 ? $", {_failCount} failed" : "");
            lblFile.ForeColor = _failCount > 0 ? Color.Orange : Color.LightGreen;

            lblSpeed.Text = $"Speed: {CopyManager.FormatSpeed(speed)}";
            lblTime.Text = $"Time: {CopyManager.FormatTimeSpan(elapsed)}";
            lblSize.Text = $"Completed: {CopyManager.FormatBytes(_totalBytes)}";

            btnCancel.Text = "Close";
            btnCancel.BackColor = Color.FromArgb(40, 120, 80);

            Log($"Done: {_successCount} ok, {_failCount} failed, {CopyManager.FormatTimeSpan(elapsed)}, {CopyManager.FormatSpeed(speed)}");
        }

        private void ShowError(string msg)
        {
            _isCompleted = true;
            if (InvokeRequired) { Invoke(new Action(() => ShowError(msg))); return; }

            lblStatus.Text = "Error!";
            lblStatus.ForeColor = Color.Red;
            lblFile.Text = msg;
            lblFile.ForeColor = Color.Red;
            btnCancel.Text = "Close";
        }

        private void Log(string msg)
        {
            if (InvokeRequired) { BeginInvoke(new Action(() => Log(msg))); return; }
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}{Environment.NewLine}");
        }

        private void OnCancelClick(object? sender, EventArgs e)
        {
            if (_isCompleted) { Close(); return; }

            if (MessageBox.Show("Cancel?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _cts.Cancel();
                _copier?.Cancel();
                btnCancel.Enabled = false;
                btnCancel.Text = "Cancelling...";
            }
        }

        private void OnFormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_isCompleted) return;

            if (progressBar.Value > 0 && progressBar.Value < 100)
            {
                if (MessageBox.Show("Cancel and close?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
                _cts.Cancel();
                _copier?.Cancel();
            }
        }
    }
}
