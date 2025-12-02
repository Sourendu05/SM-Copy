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
        private CancellationTokenSource _cancellationTokenSource = new();
        private DateTime _startTime;
        private long _totalBytes = 0;
        private long _copiedBytes = 0;
        private int _currentItemIndex = 0;
        private int _successfulItems = 0;
        private int _failedItems = 0;

        // UI Controls
        private ProgressBar progressBar = null!;
        private Label lblStatus = null!;
        private Label lblCurrentFile = null!;
        private Label lblSpeed = null!;
        private Label lblTimeRemaining = null!;
        private Label lblSize = null!;
        private Button btnPause = null!;
        private Button btnCancel = null!;
        private TextBox txtLog = null!;

        public ProgressWindow(List<CopyItem> items, string destination)
        {
            _items = items;
            _destination = destination;
            _totalBytes = items.Sum(i => i.Size);

            InitializeComponent();
            InitializeUI();
            
            // Start copying when form loads
            this.Load += async (s, e) => await StartCopyingAsync();
        }

        private void InitializeComponent()
        {
            this.Text = "SM Copy - Copying Files";
            this.Size = new Size(600, 450);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormClosing += ProgressWindow_FormClosing;
        }

        private void InitializeUI()
        {
            int padding = 15;
            int currentY = padding;

            // Status Label
            lblStatus = new Label
            {
                Text = "Preparing to copy...",
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
                Size = new Size(this.ClientSize.Width - padding * 2, 30),
                Style = ProgressBarStyle.Continuous
            };
            this.Controls.Add(progressBar);
            currentY += 40;

            // Current File Label
            lblCurrentFile = new Label
            {
                Text = "Current file: None",
                Location = new Point(padding, currentY),
                Size = new Size(this.ClientSize.Width - padding * 2, 20),
                Font = new Font("Segoe UI", 9)
            };
            this.Controls.Add(lblCurrentFile);
            currentY += 25;

            // Speed Label
            lblSpeed = new Label
            {
                Text = "Speed: 0 MB/s",
                Location = new Point(padding, currentY),
                Size = new Size(250, 20),
                Font = new Font("Segoe UI", 9)
            };
            this.Controls.Add(lblSpeed);

            // Time Remaining Label
            lblTimeRemaining = new Label
            {
                Text = "Time remaining: Calculating...",
                Location = new Point(padding + 250, currentY),
                Size = new Size(this.ClientSize.Width - padding * 2 - 250, 20),
                Font = new Font("Segoe UI", 9)
            };
            this.Controls.Add(lblTimeRemaining);
            currentY += 25;

            // Size Label
            lblSize = new Label
            {
                Text = $"Total: {CopyManager.FormatBytes(_totalBytes)}",
                Location = new Point(padding, currentY),
                Size = new Size(this.ClientSize.Width - padding * 2, 20),
                Font = new Font("Segoe UI", 9)
            };
            this.Controls.Add(lblSize);
            currentY += 30;

            // Log TextBox
            txtLog = new TextBox
            {
                Location = new Point(padding, currentY),
                Size = new Size(this.ClientSize.Width - padding * 2, 150),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Font = new Font("Consolas", 8)
            };
            this.Controls.Add(txtLog);
            currentY += 160;

            // Buttons
            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(this.ClientSize.Width - padding - 80, currentY),
                Size = new Size(80, 30)
            };
            btnCancel.Click += BtnCancel_Click;
            this.Controls.Add(btnCancel);

            btnPause = new Button
            {
                Text = "Pause",
                Location = new Point(this.ClientSize.Width - padding - 170, currentY),
                Size = new Size(80, 30),
                Enabled = false // Pause not implemented in this version
            };
            this.Controls.Add(btnPause);
        }

        private async Task StartCopyingAsync()
        {
            _startTime = DateTime.Now;
            AddLog($"Starting copy operation: {_items.Count} item(s)");
            AddLog($"Destination: {_destination}");
            AddLog("---");

            try
            {
                // Separate files and folders
                var files = _items.Where(i => i.Type == "file").ToList();
                var folders = _items.Where(i => i.Type == "folder").ToList();

                int totalOperations = folders.Count;
                
                // Group files by source directory to batch them
                var fileGroups = files.GroupBy(f => Path.GetDirectoryName(f.Path)).ToList();
                totalOperations += fileGroups.Count;

                int currentOpIndex = 0;

                // 1. Copy Folders (one by one as Robocopy handles recursion efficiently)
                foreach (var folder in folders)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested) break;

                    currentOpIndex++;
                    _currentItemIndex = currentOpIndex; // Approximate for UI
                    
                    await CopyFolderAsync(folder, currentOpIndex, totalOperations);
                }

                // 2. Copy Files (Batched by directory)
                foreach (var group in fileGroups)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested) break;

                    currentOpIndex++;
                    string sourceDir = group.Key ?? "";
                    var batchFiles = group.Select(i => Path.GetFileName(i.Path)).ToList();
                    long batchSize = group.Sum(i => i.Size);

                    await CopyFileBatchAsync(sourceDir, batchFiles, batchSize, currentOpIndex, totalOperations);
                }

                if (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    string message = $"Copy completed!\n\nSuccessful: {_successfulItems}\nFailed: {_failedItems}\nTime: {CopyManager.FormatTimeSpan(DateTime.Now - _startTime)}";
                    MessageBoxIcon icon = _failedItems > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information;
                    
                    UpdateStatus("Copy operation finished.", 100);
                    AddLog("---");
                    AddLog($"Summary: {_successfulItems} successful, {_failedItems} failed.");
                    
                    MessageBox.Show(message, "Copy Complete", MessageBoxButtons.OK, icon);
                    
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                AddLog($"ERROR: {ex.Message}");
                MessageBox.Show($"An error occurred:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task CopyFolderAsync(CopyItem item, int index, int total)
        {
            string folderName = Path.GetFileName(item.Path);
            AddLog($"Copying Folder: {folderName}");
            AddLog($"Source: {item.Path}");

            UpdateStatus($"Copying folder {folderName}... ({index}/{total})", 0);

            var wrapper = new RobocopyWrapper();
            wrapper.ProgressChanged += Wrapper_ProgressChanged;
            wrapper.OutputReceived += (s, output) => AddLog(output);

            string sourcePath = item.Path;
            string destPath = Path.Combine(_destination, folderName);
            
            AddLog($"Destination: {destPath}");

            // Try Robocopy first
            bool success = await wrapper.CopyAsync(
                sourcePath,
                destPath,
                null, 
                item.Size,
                _cancellationTokenSource.Token
            );

            if (!success)
            {
                AddLog("Using fallback copy method...");
                try
                {
                    var state = new CopyProgressState 
                    { 
                        TotalBytes = item.Size, 
                        StartTime = DateTime.Now 
                    };
                    await Task.Run(() => RecursiveCopy(sourcePath, destPath, _cancellationTokenSource.Token, state));
                    success = true;
                    AddLog("✓ Fallback copy successful.");
                }
                catch (Exception ex)
                {
                    AddLog($"✗ Fallback copy failed: {ex.Message}");
                    success = false;
                }
            }
            else
            {
                AddLog("✓ Robocopy completed successfully.");
            }

            if (success) 
            {
                AddLog($"Completed folder: {folderName}");
                _successfulItems++;
            }
            else 
            {
                AddLog($"Failed or incomplete folder: {folderName}");
                _failedItems++;
            }
        }

        private class CopyProgressState
        {
            private long _copiedBytes;
            public long TotalBytes { get; set; }
            public DateTime StartTime { get; set; }

            public long CopiedBytes
            {
                get => Interlocked.Read(ref _copiedBytes);
            }

            public void AddCopiedBytes(long bytes)
            {
                Interlocked.Add(ref _copiedBytes, bytes);
            }
        }

        private void RecursiveCopy(string sourceDir, string destDir, CancellationToken token, CopyProgressState state)
        {
            Directory.CreateDirectory(destDir);

            var files = Directory.GetFiles(sourceDir);
            
            // CRITICAL: Use sequential copy, NOT parallel
            // Parallel I/O causes disk seek thrashing and kills performance
            // Windows Explorer copies files sequentially for a reason
            foreach (var file in files)
            {
                if (token.IsCancellationRequested) return;
                
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destDir, fileName);
                
                // HIGH PERFORMANCE COPY:
                // - 8MB buffer (optimal for modern disks)
                // - NO WriteThrough (allows OS write caching like Windows Explorer)
                // - Asynchronous for better I/O pipeline
                // - SequentialScan hint for read-ahead optimization
                const int bufferSize = 8 * 1024 * 1024; // 8MB
                
                using (var sourceStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.SequentialScan | FileOptions.Asynchronous))
                using (var destStream = new FileStream(destFile, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.Asynchronous))
                {
                    byte[] buffer = new byte[bufferSize];
                    int bytesRead;
                    long fileProgress = 0;
                    
                    while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        if (token.IsCancellationRequested) return;
                        destStream.Write(buffer, 0, bytesRead);
                        
                        fileProgress += bytesRead;
                        state.AddCopiedBytes(bytesRead);
                        
                        // Update UI less frequently (every 100MB) to reduce overhead
                        if (state.CopiedBytes % (100 * 1024 * 1024) < bufferSize) 
                        {
                            UpdateProgressUI(state.TotalBytes, state.CopiedBytes, state.StartTime, fileName);
                        }
                    }
                }
                
                // Copy file attributes after content is written
                try
                {
                    File.SetAttributes(destFile, File.GetAttributes(file));
                    File.SetCreationTime(destFile, File.GetCreationTime(file));
                    File.SetLastWriteTime(destFile, File.GetLastWriteTime(file));
                }
                catch { /* Ignore attribute copy failures */ }
            }

            // Final UI update for this directory
            UpdateProgressUI(state.TotalBytes, state.CopiedBytes, state.StartTime, "Processing subdirectories...");

            foreach (var subDir in Directory.GetDirectories(sourceDir))
            {
                if (token.IsCancellationRequested) return;
                string destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                RecursiveCopy(subDir, destSubDir, token, state);
            }
        }

        private void UpdateProgressUI(long totalBytes, long currentCopied, DateTime startTime, string currentFile)
        {
             if (InvokeRequired)
            {
                // Use BeginInvoke to avoid blocking the copy threads
                BeginInvoke(new Action(() => UpdateProgressUI(totalBytes, currentCopied, startTime, currentFile)));
                return;
            }

            // Calculate progress, speed, and ETA
            double progressPercent = totalBytes > 0 ? (currentCopied * 100.0 / totalBytes) : 0;
            var elapsed = DateTime.Now - startTime;
            double speedBytesPerSec = elapsed.TotalSeconds > 0 ? currentCopied / elapsed.TotalSeconds : 0;
            TimeSpan eta = speedBytesPerSec > 0 ? TimeSpan.FromSeconds((totalBytes - currentCopied) / speedBytesPerSec) : TimeSpan.Zero;
            
            progressBar.Value = Math.Min(100, (int)progressPercent);
            lblCurrentFile.Text = $"Current file: {currentFile}";
            lblSpeed.Text = $"Speed: {CopyManager.FormatSpeed(speedBytesPerSec)}";
            lblTimeRemaining.Text = eta.TotalSeconds > 0 ? $"Time remaining: {CopyManager.FormatTimeSpan(eta)}" : "Time remaining: Calculating...";
            
            // Update status label to correct the "100%" issue
            lblStatus.Text = $"Copying... {(int)progressPercent}%";
        }

        private async Task CopyFileBatchAsync(string sourceDir, List<string> fileNames, long totalSize, int index, int total)
        {
            AddLog($"Copying Batch from: {sourceDir}");
            AddLog($"Files in batch: {fileNames.Count}");

            UpdateStatus($"Copying batch of {fileNames.Count} files... ({index}/{total})", 0);

            var wrapper = new RobocopyWrapper();
            wrapper.ProgressChanged += Wrapper_ProgressChanged;
            wrapper.OutputReceived += (s, output) => AddLog(output);

            string destPath = _destination;

            bool success = await wrapper.CopyAsync(
                sourceDir,
                destPath,
                fileNames,
                totalSize,
                _cancellationTokenSource.Token
            );

            if (!success)
            {
                AddLog("Robocopy batch failed. Attempting fallback copy for files...");
                int fallbackSuccessCount = 0;
                
                foreach (var fileName in fileNames)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested) break;
                    try
                    {
                        string sourceFile = Path.Combine(sourceDir, fileName);
                        string destFile = Path.Combine(destPath, fileName);
                        
                        // Ensure destination directory exists
                        Directory.CreateDirectory(destPath);
                        
                        await Task.Run(() => File.Copy(sourceFile, destFile, true));
                        fallbackSuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        AddLog($"Failed to copy file {fileName}: {ex.Message}");
                    }
                }

                if (fallbackSuccessCount > 0)
                {
                    success = true; // Consider partial success as success for the batch operation wrapper
                    _successfulItems += fallbackSuccessCount;
                    _failedItems += (fileNames.Count - fallbackSuccessCount);
                    AddLog($"Fallback copy: {fallbackSuccessCount} files copied, {fileNames.Count - fallbackSuccessCount} failed.");
                }
                else
                {
                    _failedItems += fileNames.Count;
                }
            }
            else
            {
                _successfulItems += fileNames.Count;
            }

            if (success) AddLog($"Completed batch from: {sourceDir}");
            else AddLog($"Failed or incomplete batch from: {sourceDir}");
        }

        private void Wrapper_ProgressChanged(object? sender, RobocopyProgress e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Wrapper_ProgressChanged(sender, e)));
                return;
            }

            // Update progress bar
            int progress = (int)Math.Min(100, Math.Max(0, e.ProgressPercentage));
            progressBar.Value = progress;

            // Update labels
            if (!string.IsNullOrEmpty(e.CurrentFile))
            {
                lblCurrentFile.Text = $"Current file: {e.CurrentFile}";
            }

            if (e.SpeedBytesPerSecond > 0)
            {
                lblSpeed.Text = $"Speed: {CopyManager.FormatSpeed(e.SpeedBytesPerSecond)}";
            }

            if (e.EstimatedTimeRemaining.TotalSeconds > 0)
            {
                lblTimeRemaining.Text = $"Time remaining: {CopyManager.FormatTimeSpan(e.EstimatedTimeRemaining)}";
            }

            // Update copied bytes for overall progress
            _copiedBytes += e.CopiedBytes;
            
            // Calculate overall progress across all items
            int overallProgress = _totalBytes > 0 
                ? (int)((_currentItemIndex - 1) * 100.0 / _items.Count + (e.ProgressPercentage / _items.Count))
                : 0;
            
            UpdateStatus($"Copying item {_currentItemIndex}/{_items.Count}... {progress}%", overallProgress);
        }

        private void UpdateStatus(string status, int progress)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateStatus(status, progress)));
                return;
            }

            lblStatus.Text = status;
            progressBar.Value = Math.Min(100, Math.Max(0, progress));
        }

        private void AddLog(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AddLog(message)));
                return;
            }

            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to cancel the copy operation?",
                "Confirm Cancel",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                _cancellationTokenSource.Cancel();
                AddLog("Cancelling operation...");
                btnCancel.Enabled = false;
                btnCancel.Text = "Cancelling...";
            }
        }

        private void ProgressWindow_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (progressBar.Value < 100 && progressBar.Value > 0)
            {
                var result = MessageBox.Show(
                    "Copy operation is still in progress. Are you sure you want to close?",
                    "Confirm Close",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                _cancellationTokenSource.Cancel();
            }
        }
    }
}

