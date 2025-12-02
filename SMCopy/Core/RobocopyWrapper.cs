using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SMCopy.Core
{
    public class RobocopyProgress
    {
        public long TotalBytes { get; set; }
        public long CopiedBytes { get; set; }
        public double ProgressPercentage { get; set; }
        public double SpeedBytesPerSecond { get; set; }
        public TimeSpan EstimatedTimeRemaining { get; set; }
        public string CurrentFile { get; set; } = string.Empty;
        public int FilesProcessed { get; set; }
        public int TotalFiles { get; set; }
        public bool IsCompleted { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class RobocopyWrapper
    {
        private Process? _process;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private DateTime _startTime;
        private long _lastCopiedBytes = 0;
        private DateTime _lastUpdateTime;
        
        // For smoothing speed calculation
        private readonly Queue<double> _speedSamples = new();
        private const int MaxSpeedSamples = 5;

        public event EventHandler<RobocopyProgress>? ProgressChanged;
        public event EventHandler<string>? OutputReceived;

        /// <summary>
        /// Copies files or a folder using Robocopy.
        /// </summary>
        /// <param name="sourcePath">Source directory path (for files) or source folder path (for folder copy)</param>
        /// <param name="destinationPath">Destination directory path</param>
        /// <param name="fileNames">List of filenames to copy. If null/empty, assumes sourcePath is a folder to be copied recursively.</param>
        /// <param name="totalSize">Total size of bytes to be copied (for progress calculation)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task<bool> CopyAsync(string sourcePath, string destinationPath, List<string>? fileNames, long totalSize, CancellationToken cancellationToken = default)
        {
            _startTime = DateTime.Now;
            _lastUpdateTime = DateTime.Now;
            _speedSamples.Clear();

            var progress = new RobocopyProgress
            {
                TotalBytes = totalSize
            };

            try
            {
                string arguments = BuildRobocopyArguments(sourcePath, destinationPath, fileNames);
                
                // Log the command for debugging
                OutputReceived?.Invoke(this, $"Command: robocopy.exe {arguments}");
                OutputReceived?.Invoke(this, $"Source: {sourcePath}");
                OutputReceived?.Invoke(this, $"Destination: {destinationPath}");
                OutputReceived?.Invoke(this, "---");
                
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "robocopy.exe",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                    WorkingDirectory = Environment.SystemDirectory
                };

                _process = new Process { StartInfo = processStartInfo };

                _process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        // Robocopy with progress outputs carriage returns to update the same line.
                        // We need to handle that if we want clean logs, but for parsing we just need the data.
                        OutputReceived?.Invoke(this, e.Data);
                        ParseRobocopyOutput(e.Data, progress);
                    }
                };

                _process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        progress.ErrorMessage = e.Data;
                        OutputReceived?.Invoke(this, $"ERROR: {e.Data}");
                    }
                };

                _process.Start();
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();

                // Monitor for cancellation
                using var registration = cancellationToken.Register(() =>
                {
                    try
                    {
                        if (_process != null && !_process.HasExited)
                        {
                            _process.Kill();
                        }
                    }
                    catch { }
                });

                await _process.WaitForExitAsync(cancellationToken);

                // Mark progress as completed and normalize final metrics
                progress.IsCompleted = true;
                progress.ProgressPercentage = 100;
                progress.EstimatedTimeRemaining = TimeSpan.Zero;
                progress.SpeedBytesPerSecond = 0;
                ProgressChanged?.Invoke(this, progress);

                // Log exit code - but only if it's a serious failure (>= 16)
                // Exit codes 8-15 will be handled by fallback, so don't confuse users with error messages
                if (_process.ExitCode >= 16)
                {
                    string exitCodeMessage = GetExitCodeMessage(_process.ExitCode);
                    // Suppress automatic logging of fatal errors to avoid confusion when fallback succeeds
                    // OutputReceived?.Invoke(this, "---");
                    // OutputReceived?.Invoke(this, $"Robocopy Exit Code: {_process.ExitCode} - {exitCodeMessage}");
                    progress.ErrorMessage = $"Copy failed: {exitCodeMessage}";
                }
                
                // Return success for exit codes < 8 (robocopy considers 0-7 as success)
                return _process.ExitCode < 8;
            }
            catch (OperationCanceledException)
            {
                progress.ErrorMessage = "Operation cancelled by user";
                progress.IsCompleted = true;
                ProgressChanged?.Invoke(this, progress);
                return false;
            }
            catch (Exception ex)
            {
                progress.ErrorMessage = ex.Message;
                progress.IsCompleted = true;
                ProgressChanged?.Invoke(this, progress);
                return false;
            }
        }

        private string BuildRobocopyArguments(string sourcePath, string destinationPath, List<string>? fileNames)
        {
            // Common options:
            // /MT:128 = multi-threaded copy with 128 threads
            // /R:0 = retry 0 times (fail fast) - User requested /R:0 /W:0
            // /W:0 = wait 0 seconds between retries
            // /J = unbuffered I/O (good for large files)
            // /ZB = restartable mode; if access denied use Backup mode (Fixes Issue 4)
            // /DCOPY:DAT = copy directory data, attributes, and timestamps
            // /COPY:DAT = copy file data, attributes, and timestamps
            // /XD ... = Exclude system directories
            
            // NOTE: Removed /NP (No Progress) to allow progress parsing (Fixes Issue 2)
            
            var sb = new StringBuilder();

            // Source and Destination
            // IMPORTANT: Handle trailing backslashes by escaping them.
            // If a path ends with \, and is followed by ", the \ escapes the ".
            // So "C:\" becomes "C:\" which escapes the quote. We need "C:\\"
            
            string cleanSource = sourcePath.TrimEnd('\\');
            string cleanDest = destinationPath.TrimEnd('\\');

            sb.Append($"\"{cleanSource}\" \"{cleanDest}\" ");

            // Files (if specified)
            if (fileNames != null && fileNames.Any())
            {
                foreach (var file in fileNames)
                {
                    sb.Append($"\"{file}\" ");
                }
            }
            else
            {
                // Folder copy mode
                sb.Append("/E "); // Copy subdirectories, including empty ones
            }

            // Options
            // /MT:64 = 64 threads (128 can cause overhead, 64 is sweet spot)
            // Removed /J (unbuffered I/O) - it can actually slow down on some systems
            // /ZB = Backup mode fallback
            // /DCOPY:DAT = Copy all directory metadata
            sb.Append("/MT:64 /R:0 /W:0 /ZB /DCOPY:DAT /COPY:DAT ");
            
            // Exclusions (Fixes Issue 4 - Access Denied on system folders)
            sb.Append("/XD \"System Volume Information\" \"$RECYCLE.BIN\" \"Pagefile.sys\" \"Swapfile.sys\" \"Hiberfil.sys\" \"Config.Msi\" ");

            return sb.ToString();
        }

        private void ParseRobocopyOutput(string output, RobocopyProgress progress)
        {
            try
            {
                // Robocopy output parsing strategy:
                // 1. Track completed files to estimate progress
                // 2. Parse "New File" lines to detect file starts and get sizes
                // 3. Update lastUpdateTime to show activity
                // 4. Calculate speed based on elapsed time and total bytes assumption
                
                _lastUpdateTime = DateTime.Now;

                // Pattern 1: Detect file completion - lines with "100%" or "100.0%"
                var percent100Match = Regex.Match(output, @"^\s*100(?:\.0)?%");
                if (percent100Match.Success)
                {
                    progress.FilesProcessed++;
                    
                    // If we know total files, we can estimate overall progress
                    if (progress.TotalFiles > 0)
                    {
                        progress.ProgressPercentage = (progress.FilesProcessed * 100.0) / progress.TotalFiles;
                    }
                }

                // Pattern 2: "New File" line to detect file start and parse size
                // Format: "  New File  \t\t   1234567\tfilename.ext"
                // or "             New File             1.2 m   filename.ext"
                if (output.Contains("New File"))
                {
                    // Try to parse filename from the line
                    var parts = output.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 2)
                    {
                        progress.CurrentFile = parts[parts.Length - 1];
                    }
                    
                    // Parse file size if present (format: 1.2k, 1.5m, 1.2g, or plain bytes)
                    var sizeMatch = Regex.Match(output, @"(\d+(?:\.\d+)?)\s*([kmg])?", RegexOptions.IgnoreCase);
                    if (sizeMatch.Success)
                    {
                        double size = double.Parse(sizeMatch.Groups[1].Value);
                        string unit = sizeMatch.Groups[2].Value.ToLower();
                        
                        long bytes = unit switch
                        {
                            "k" => (long)(size * 1024),
                            "m" => (long)(size * 1024 * 1024),
                            "g" => (long)(size * 1024 * 1024 * 1024),
                            _ => (long)size
                        };
                        
                        // Track as current file size (for future enhancement)
                    }
                }

                // Pattern 3: Parse summary statistics "Files : 123" at end
                var filesMatch = Regex.Match(output, @"Files\s*:\s*(\d+)");
                if (filesMatch.Success)
                {
                    int totalFiles = int.Parse(filesMatch.Groups[1].Value);
                    if (progress.TotalFiles == 0)
                    {
                        progress.TotalFiles = totalFiles;
                    }
                }

                // Pattern 4: Parse bytes summary "Bytes : 1234567890" at end
                var bytesMatch = Regex.Match(output, @"Bytes\s*:\s*([\d,\.]+\s*[kmg]?)", RegexOptions.IgnoreCase);
                if (bytesMatch.Success)
                {
                    string bytesStr = bytesMatch.Groups[1].Value.Replace(",", "").Replace(".", "").Trim();
                    
                    // Handle k/m/g suffix
                    var sizeMatch = Regex.Match(bytesStr, @"(\d+)\s*([kmg])?", RegexOptions.IgnoreCase);
                    if (sizeMatch.Success)
                    {
                        long bytes = long.Parse(sizeMatch.Groups[1].Value);
                        string unit = sizeMatch.Groups[2].Value.ToLower();
                        
                        bytes = unit switch
                        {
                            "k" => bytes * 1024,
                            "m" => bytes * 1024 * 1024,
                            "g" => bytes * 1024 * 1024 * 1024,
                            _ => bytes
                        };
                        
                        if (bytes > progress.CopiedBytes)
                        {
                            progress.CopiedBytes = bytes;
                        }
                    }
                }

                // Calculate speed and ETA based on elapsed time
                var elapsed = DateTime.Now - _startTime;
                if (elapsed.TotalSeconds > 0)
                {
                    // Estimate progress: assume linear progress over time
                    if (progress.TotalBytes > 0)
                    {
                        // If we have total bytes, calculate bytes copied based on files processed
                        if (progress.TotalFiles > 0 && progress.FilesProcessed > 0)
                        {
                            // Estimate: assume equal file sizes (imperfect but better than nothing)
                            long estimatedCopied = (long)((progress.FilesProcessed * 1.0 / progress.TotalFiles) * progress.TotalBytes);
                            progress.CopiedBytes = Math.Max(progress.CopiedBytes, estimatedCopied);
                        }
                        
                        // Calculate speed
                        progress.SpeedBytesPerSecond = progress.CopiedBytes / elapsed.TotalSeconds;
                        
                        // Add to speed samples for smoothing
                        _speedSamples.Enqueue(progress.SpeedBytesPerSecond);
                        if (_speedSamples.Count > MaxSpeedSamples)
                        {
                            _speedSamples.Dequeue();
                        }
                        
                        // Use average speed
                        if (_speedSamples.Count > 0)
                        {
                            progress.SpeedBytesPerSecond = _speedSamples.Average();
                        }
                        
                        // Calculate ETA
                        if (progress.SpeedBytesPerSecond > 0)
                        {
                            long remainingBytes = progress.TotalBytes - progress.CopiedBytes;
                            double remainingSeconds = remainingBytes / progress.SpeedBytesPerSecond;
                            progress.EstimatedTimeRemaining = TimeSpan.FromSeconds(Math.Max(0, remainingSeconds));
                        }
                        
                        // Update progress percentage
                        if (progress.CopiedBytes > 0)
                        {
                            progress.ProgressPercentage = Math.Min(100, (progress.CopiedBytes * 100.0) / progress.TotalBytes);
                        }
                    }
                }

                // Notify progress update
                ProgressChanged?.Invoke(this, progress);
            }
            catch
            {
                // Ignore parsing errors to prevent crashes
            }
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
            try
            {
                if (_process != null && !_process.HasExited)
                {
                    _process.Kill();
                }
            }
            catch { }
        }

        private string GetExitCodeMessage(int exitCode)
        {
            return exitCode switch
            {
                0 => "No files copied (no changes detected)",
                1 => "Files copied successfully",
                2 => "Extra files or directories detected",
                3 => "Files copied and extra files detected",
                4 => "Some mismatched files or directories detected",
                5 => "Some files copied, some mismatched",
                6 => "Extra files and mismatched files exist",
                7 => "Files copied, mismatches and extras exist",
                8 => "Copy errors - some files/directories could not be copied",
                16 => "Fatal error - robocopy did not copy any files",
                _ => $"Unknown exit code: {exitCode}"
            };
        }
    }
}

