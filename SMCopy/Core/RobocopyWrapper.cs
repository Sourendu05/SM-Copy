using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
        private DateTime _startTime;
        private System.Timers.Timer? _progressTimer;
        private string _destinationPath = "";
        private long _totalSize = 0;
        private long _lastBytes = 0;
        private DateTime _lastSpeedTime;
        private readonly Queue<double> _speedSamples = new();

        public event EventHandler<RobocopyProgress>? ProgressChanged;
        public event EventHandler<string>? OutputReceived;

        public async Task<bool> CopyAsync(string sourcePath, string destinationPath, List<string>? fileNames, long totalSize, CancellationToken cancellationToken = default)
        {
            _startTime = DateTime.Now;
            _lastSpeedTime = DateTime.Now;
            _lastBytes = 0;
            _speedSamples.Clear();
            _destinationPath = destinationPath;
            _totalSize = totalSize;

            var progress = new RobocopyProgress { TotalBytes = totalSize };

            try
            {
                string arguments = BuildRobocopyArguments(sourcePath, destinationPath, fileNames);
                OutputReceived?.Invoke(this, $"[Robocopy] {arguments}");

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

                _process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        progress.ErrorMessage = e.Data;
                        OutputReceived?.Invoke(this, $"[Error] {e.Data}");
                    }
                };

                _process.Start();
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();

                // Set high priority for better performance
                try { _process.PriorityClass = ProcessPriorityClass.AboveNormal; } catch { }

                // Start progress monitoring timer - polls destination size every 200ms
                StartProgressMonitor(progress, fileNames);

                // Monitor for cancellation
                using var registration = cancellationToken.Register(() =>
                {
                    try
                    {
                        StopProgressMonitor();
                        if (_process != null && !_process.HasExited)
                            _process.Kill(true);
                    }
                    catch { }
                });

                await _process.WaitForExitAsync(cancellationToken);
                StopProgressMonitor();

                // Final update
                var elapsed = DateTime.Now - _startTime;
                double finalSpeed = elapsed.TotalSeconds > 0 ? totalSize / elapsed.TotalSeconds : 0;

                progress.IsCompleted = true;
                progress.ProgressPercentage = 100;
                progress.CopiedBytes = totalSize;
                progress.EstimatedTimeRemaining = TimeSpan.Zero;
                progress.SpeedBytesPerSecond = finalSpeed;
                ProgressChanged?.Invoke(this, progress);

                return _process.ExitCode < 8;
            }
            catch (OperationCanceledException)
            {
                StopProgressMonitor();
                progress.ErrorMessage = "Operation cancelled";
                progress.IsCompleted = true;
                ProgressChanged?.Invoke(this, progress);
                return false;
            }
            catch (Exception ex)
            {
                StopProgressMonitor();
                progress.ErrorMessage = ex.Message;
                progress.IsCompleted = true;
                ProgressChanged?.Invoke(this, progress);
                return false;
            }
        }

        private void StartProgressMonitor(RobocopyProgress progress, List<string>? fileNames)
        {
            _progressTimer = new System.Timers.Timer(200); // Update every 200ms
            _progressTimer.Elapsed += (s, e) =>
            {
                try
                {
                    // Get current size of destination
                    long currentBytes = GetDestinationSize(fileNames);
                    
                    if (currentBytes > 0)
                    {
                        progress.CopiedBytes = currentBytes;
                        
                        if (_totalSize > 0)
                        {
                            progress.ProgressPercentage = Math.Min(99.9, (currentBytes * 100.0) / _totalSize);
                        }

                        // Calculate speed
                        var now = DateTime.Now;
                        var elapsed = now - _lastSpeedTime;
                        if (elapsed.TotalMilliseconds >= 500)
                        {
                            long bytesDelta = currentBytes - _lastBytes;
                            if (bytesDelta > 0)
                            {
                                double speed = bytesDelta / elapsed.TotalSeconds;
                                _speedSamples.Enqueue(speed);
                                while (_speedSamples.Count > 5) _speedSamples.Dequeue();
                                progress.SpeedBytesPerSecond = _speedSamples.Average();

                                // Calculate ETA
                                if (progress.SpeedBytesPerSecond > 0 && _totalSize > currentBytes)
                                {
                                    double remainingSecs = (_totalSize - currentBytes) / progress.SpeedBytesPerSecond;
                                    progress.EstimatedTimeRemaining = TimeSpan.FromSeconds(Math.Min(remainingSecs, 86400));
                                }
                            }
                            _lastBytes = currentBytes;
                            _lastSpeedTime = now;
                        }

                        ProgressChanged?.Invoke(this, progress);
                    }
                }
                catch { }
            };
            _progressTimer.Start();
        }

        private void StopProgressMonitor()
        {
            _progressTimer?.Stop();
            _progressTimer?.Dispose();
            _progressTimer = null;
        }

        private long GetDestinationSize(List<string>? fileNames)
        {
            try
            {
                if (fileNames != null && fileNames.Count > 0)
                {
                    // Single files - check each file size
                    long total = 0;
                    foreach (var fileName in fileNames)
                    {
                        var destFile = Path.Combine(_destinationPath, fileName);
                        if (File.Exists(destFile))
                        {
                            total += new FileInfo(destFile).Length;
                        }
                    }
                    return total;
                }
                else
                {
                    // Directory copy - get total size of destination
                    if (Directory.Exists(_destinationPath))
                    {
                        return GetDirectorySize(_destinationPath);
                    }
                }
            }
            catch { }
            return 0;
        }

        private long GetDirectorySize(string path)
        {
            long size = 0;
            try
            {
                foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                {
                    try { size += new FileInfo(file).Length; } catch { }
                }
            }
            catch { }
            return size;
        }

        private string BuildRobocopyArguments(string sourcePath, string destinationPath, List<string>? fileNames)
        {
            var sb = new StringBuilder();

            string cleanSource = sourcePath.TrimEnd('\\', '/');
            string cleanDest = destinationPath.TrimEnd('\\', '/');

            sb.Append($"\"{cleanSource}\" \"{cleanDest}\" ");

            if (fileNames != null && fileNames.Count > 0)
            {
                foreach (var file in fileNames)
                    sb.Append($"\"{file}\" ");
            }
            else
            {
                sb.Append("/E ");  // Copy subdirectories including empty
            }

            // MAXIMUM SPEED PARAMETERS:
            // /MT:16    - 16 threads (good balance of speed and overhead)
            // /R:0 /W:0 - No retries (fail fast)
            // /NP       - No progress output (we track via file size)
            // /NFL /NDL - No file/dir listing (reduces output overhead)  
            // /NC /NS   - No class/size in output
            // /NJH /NJS - No job header/summary
            // /COPY:D   - Copy data only (fastest, skip attributes)
            //
            // NOT using /J (unbuffered) - slower on SSDs with cache
            // NOT using /NOOFFLOAD - want hardware acceleration
            
            sb.Append("/MT:16 ");           // 16 parallel threads
            sb.Append("/R:0 /W:0 ");        // No retries
            sb.Append("/NP ");              // No progress (we poll file size instead)
            sb.Append("/NFL /NDL ");        // No file/dir listing
            sb.Append("/NC /NS ");          // No class/size
            sb.Append("/NJH /NJS ");        // No header/summary
            sb.Append("/COPY:D ");          // Data only (fastest)

            // Exclusions
            sb.Append("/XD \"System Volume Information\" \"$RECYCLE.BIN\" ");
            sb.Append("/XF pagefile.sys swapfile.sys hiberfil.sys ");
            sb.Append("/XA:SH ");

            return sb.ToString();
        }

        public void Cancel()
        {
            StopProgressMonitor();
            try
            {
                if (_process != null && !_process.HasExited)
                    _process.Kill(true);
            }
            catch { }
        }
    }
}
