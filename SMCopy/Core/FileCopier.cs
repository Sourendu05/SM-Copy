using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace SMCopy.Core
{
    public class CopyProgress
    {
        public long TotalBytes { get; set; }
        public long CopiedBytes { get; set; }
        public double ProgressPercentage { get; set; }
        public double SpeedBytesPerSecond { get; set; }
        public TimeSpan EstimatedTimeRemaining { get; set; }
        public string CurrentFile { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Ultra high-performance file copier using Windows native CopyFileEx API.
    /// This is the same API Windows Explorer uses - achieves maximum disk speed.
    /// </summary>
    public class FileCopier
    {
        #region Native Windows API

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CopyFileExW(
            string lpExistingFileName,
            string lpNewFileName,
            CopyProgressRoutine? lpProgressRoutine,
            IntPtr lpData,
            ref int pbCancel,
            CopyFileFlags dwCopyFlags);

        private delegate CopyProgressResult CopyProgressRoutine(
            long TotalFileSize,
            long TotalBytesTransferred,
            long StreamSize,
            long StreamBytesTransferred,
            uint dwStreamNumber,
            CopyProgressCallbackReason dwCallbackReason,
            IntPtr hSourceFile,
            IntPtr hDestinationFile,
            IntPtr lpData);

        [Flags]
        private enum CopyFileFlags : uint
        {
            COPY_FILE_FAIL_IF_EXISTS = 0x00000001,
            COPY_FILE_NO_BUFFERING = 0x00001000,      // Bypass file system cache
            COPY_FILE_OPEN_SOURCE_FOR_WRITE = 0x00000004,
            COPY_FILE_RESTARTABLE = 0x00000002
        }

        private enum CopyProgressResult : uint
        {
            PROGRESS_CONTINUE = 0,
            PROGRESS_CANCEL = 1,
            PROGRESS_STOP = 2,
            PROGRESS_QUIET = 3
        }

        private enum CopyProgressCallbackReason : uint
        {
            CALLBACK_CHUNK_FINISHED = 0x00000000,
            CALLBACK_STREAM_SWITCH = 0x00000001
        }

        #endregion

        private DateTime _startTime;
        private long _totalCopied = 0;
        private long _currentFileCopied = 0;
        private long _lastBytes = 0;
        private DateTime _lastSpeedTime;
        private readonly Queue<double> _speedSamples = new();
        private int _cancelled = 0;  // Used by CopyFileExW
        private CopyProgress _progress = new();

        public event EventHandler<CopyProgress>? ProgressChanged;
        public event EventHandler<string>? LogMessage;

        /// <summary>
        /// Copy a single file using Windows native API for maximum speed.
        /// </summary>
        public async Task<bool> CopyFileAsync(string sourceFile, string destFile, long totalSize, CancellationToken ct = default)
        {
            var sourceInfo = new FileInfo(sourceFile);
            _progress.CurrentFile = sourceInfo.Name;
            long startOffset = _totalCopied;

            try
            {
                LogMessage?.Invoke(this, $"File: {sourceInfo.Name} ({FormatBytes(sourceInfo.Length)})");

                // Ensure destination directory exists
                string? destDir = Path.GetDirectoryName(destFile);
                if (!string.IsNullOrEmpty(destDir))
                    Directory.CreateDirectory(destDir);

                _currentFileCopied = 0;
                _progress.TotalBytes = totalSize;

                // Register cancellation
                using var reg = ct.Register(() => Interlocked.Exchange(ref _cancelled, 1));

                // Use native Windows copy with progress callback
                bool success = await Task.Run(() =>
                {
                    return CopyFileExW(
                        sourceFile,
                        destFile,
                        (total, transferred, streamSize, streamTransferred, streamNum, reason, hSrc, hDst, data) =>
                        {
                            if (_cancelled != 0)
                                return CopyProgressResult.PROGRESS_CANCEL;

                            _currentFileCopied = transferred;
                            _totalCopied = startOffset + transferred;
                            UpdateProgress();

                            return CopyProgressResult.PROGRESS_CONTINUE;
                        },
                        IntPtr.Zero,
                        ref _cancelled,
                        0);  // No special flags - let Windows optimize
                });

                if (!success)
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error == 1235) // ERROR_REQUEST_ABORTED
                        return false;
                    
                    LogMessage?.Invoke(this, $"Copy failed: error {error}");
                    return false;
                }

                // Update final progress for this file
                _totalCopied = startOffset + sourceInfo.Length;
                UpdateProgress();

                return true;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error: {sourceInfo.Name}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Copy multiple files from a directory.
        /// </summary>
        public async Task<bool> CopyFilesAsync(string sourceDir, string destDir, List<string> fileNames, long totalSize, CancellationToken ct = default)
        {
            InitProgress(totalSize);
            LogMessage?.Invoke(this, $"Copying {fileNames.Count} file(s)");

            int success = 0;

            foreach (var fileName in fileNames)
            {
                if (_cancelled != 0 || ct.IsCancellationRequested) break;

                if (await CopyFileAsync(Path.Combine(sourceDir, fileName), Path.Combine(destDir, fileName), totalSize, ct))
                    success++;
            }

            FinalizeProgress(totalSize);
            return success == fileNames.Count;
        }

        /// <summary>
        /// Copy an entire folder recursively using native Windows API.
        /// </summary>
        public async Task<bool> CopyFolderAsync(string sourceDir, string destDir, long totalSize, CancellationToken ct = default)
        {
            InitProgress(totalSize);

            try
            {
                var files = GetFilesToCopy(sourceDir);
                LogMessage?.Invoke(this, $"Copying {files.Count} file(s), {FormatBytes(totalSize)}");

                int success = 0;

                foreach (var srcFile in files)
                {
                    if (_cancelled != 0 || ct.IsCancellationRequested) break;

                    string relPath = srcFile.Substring(sourceDir.Length).TrimStart('\\', '/');
                    string dstFile = Path.Combine(destDir, relPath);

                    if (await CopyFileAsync(srcFile, dstFile, totalSize, ct))
                        success++;
                }

                FinalizeProgress(totalSize);
                return success == files.Count;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error: {ex.Message}");
                return false;
            }
        }

        private List<string> GetFilesToCopy(string sourceDir)
        {
            return Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories)
                .Where(f => !ShouldExclude(f))
                .ToList();
        }

        private void InitProgress(long totalSize)
        {
            _startTime = DateTime.Now;
            _lastSpeedTime = DateTime.Now;
            _totalCopied = 0;
            _lastBytes = 0;
            _speedSamples.Clear();
            Interlocked.Exchange(ref _cancelled, 0);
            _progress = new CopyProgress { TotalBytes = totalSize };
        }

        private void UpdateProgress()
        {
            _progress.CopiedBytes = _totalCopied;

            if (_progress.TotalBytes > 0)
                _progress.ProgressPercentage = Math.Min(99.9, (_totalCopied * 100.0) / _progress.TotalBytes);

            var now = DateTime.Now;
            var elapsed = now - _lastSpeedTime;

            // Update speed every 50ms for smooth display
            if (elapsed.TotalMilliseconds >= 50)
            {
                long delta = _totalCopied - _lastBytes;
                if (delta > 0)
                {
                    double speed = delta / elapsed.TotalSeconds;
                    _speedSamples.Enqueue(speed);
                    while (_speedSamples.Count > 10) _speedSamples.Dequeue();
                    _progress.SpeedBytesPerSecond = _speedSamples.Average();

                    if (_progress.SpeedBytesPerSecond > 0 && _progress.TotalBytes > _totalCopied)
                    {
                        double secs = (_progress.TotalBytes - _totalCopied) / _progress.SpeedBytesPerSecond;
                        _progress.EstimatedTimeRemaining = TimeSpan.FromSeconds(Math.Min(secs, 86400));
                    }
                }

                _lastBytes = _totalCopied;
                _lastSpeedTime = now;

                ProgressChanged?.Invoke(this, _progress);
            }
        }

        private void FinalizeProgress(long totalSize)
        {
            var elapsed = DateTime.Now - _startTime;
            double speed = elapsed.TotalSeconds > 0 ? totalSize / elapsed.TotalSeconds : 0;

            _progress.CopiedBytes = totalSize;
            _progress.ProgressPercentage = 100;
            _progress.SpeedBytesPerSecond = speed;
            _progress.EstimatedTimeRemaining = TimeSpan.Zero;
            _progress.IsCompleted = true;

            ProgressChanged?.Invoke(this, _progress);
            LogMessage?.Invoke(this, $"Completed: {FormatTime(elapsed)}, {FormatSpeed(speed)}");
        }

        private bool ShouldExclude(string path)
        {
            string name = Path.GetFileName(path).ToLower();
            if (name == "pagefile.sys" || name == "swapfile.sys" || name == "hiberfil.sys")
                return true;
            if (path.Contains("System Volume Information") || path.Contains("$RECYCLE.BIN"))
                return true;
            return false;
        }

        public void Cancel() => Interlocked.Exchange(ref _cancelled, 1);

        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1) { order++; len /= 1024; }
            return $"{len:0.##} {sizes[order]}";
        }

        private static string FormatSpeed(double bps) => $"{FormatBytes((long)bps)}/s";

        private static string FormatTime(TimeSpan ts)
        {
            if (ts.TotalHours >= 1) return $"{(int)ts.TotalHours}h {ts.Minutes}m";
            if (ts.TotalMinutes >= 1) return $"{ts.Minutes}m {ts.Seconds}s";
            return $"{ts.Seconds}s";
        }
    }
}
