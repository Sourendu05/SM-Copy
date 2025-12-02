using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SMCopy.Models;

namespace SMCopy.Core
{
    public static class CopyManager
    {
        private static readonly string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SMCopy"
        );
        private static readonly string ClipboardFile = Path.Combine(AppDataPath, "clipboard.json");

        static CopyManager()
        {
            // Ensure the AppData directory exists
            if (!Directory.Exists(AppDataPath))
            {
                Directory.CreateDirectory(AppDataPath);
            }
        }

        public static void SaveCopiedItems(string[] paths)
        {
            var items = new List<CopyItem>();

            foreach (var path in paths)
            {
                if (!File.Exists(path) && !Directory.Exists(path))
                {
                    continue; // Skip invalid paths
                }

                var item = new CopyItem
                {
                    Path = path,
                    Type = File.Exists(path) ? "file" : "folder",
                    Size = GetItemSize(path)
                };

                items.Add(item);
            }

            var clipboard = new CopyClipboard
            {
                Items = items,
                Timestamp = DateTime.Now
            };

            var json = JsonConvert.SerializeObject(clipboard, Formatting.Indented);
            File.WriteAllText(ClipboardFile, json);
        }

        public static List<CopyItem>? LoadCopiedItems()
        {
            if (!File.Exists(ClipboardFile))
            {
                return null;
            }

            try
            {
                var json = File.ReadAllText(ClipboardFile);
                var clipboard = JsonConvert.DeserializeObject<CopyClipboard>(json);
                return clipboard?.Items;
            }
            catch
            {
                return null;
            }
        }

        public static long GetItemSize(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    return new FileInfo(path).Length;
                }
                else if (Directory.Exists(path))
                {
                    return GetDirectorySize(path);
                }
            }
            catch
            {
                // Ignore errors (access denied, etc.)
            }
            return 0;
        }

        private static long GetDirectorySize(string path)
        {
            try
            {
                var dirInfo = new DirectoryInfo(path);
                long size = 0;

                // Get all files
                foreach (var file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                {
                    try
                    {
                        size += file.Length;
                    }
                    catch
                    {
                        // Skip files we can't access
                    }
                }

                return size;
            }
            catch
            {
                return 0;
            }
        }

        public static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        public static string FormatSpeed(double bytesPerSecond)
        {
            return $"{FormatBytes((long)bytesPerSecond)}/s";
        }

        public static string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours >= 1)
            {
                return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";
            }
            else if (timeSpan.TotalMinutes >= 1)
            {
                return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
            }
            else
            {
                return $"{timeSpan.Seconds}s";
            }
        }
    }
}

