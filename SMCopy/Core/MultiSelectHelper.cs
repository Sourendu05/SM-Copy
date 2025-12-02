using System;
using System.Collections.Generic;
using System.Linq;

namespace SMCopy.Core
{
    /// <summary>
    /// Helper class for handling multiple file/folder selections from context menu
    /// </summary>
    public static class MultiSelectHelper
    {
        /// <summary>
        /// Process command line arguments that may contain multiple paths
        /// Windows passes multiple selected items as separate arguments
        /// </summary>
        public static List<string> ParseMultiplePathsFromArgs(string[] args)
        {
            var paths = new List<string>();

            foreach (var arg in args)
            {
                // Skip command flags
                if (arg.StartsWith("--") || arg.StartsWith("-"))
                    continue;

                // Clean up the path (remove quotes if present)
                string cleanPath = arg.Trim('"', '\'');
                
                if (!string.IsNullOrWhiteSpace(cleanPath))
                {
                    paths.Add(cleanPath);
                }
            }

            return paths;
        }

        /// <summary>
        /// Validates that all paths exist
        /// </summary>
        public static (List<string> valid, List<string> invalid) ValidatePaths(IEnumerable<string> paths)
        {
            var valid = new List<string>();
            var invalid = new List<string>();

            foreach (var path in paths)
            {
                if (System.IO.File.Exists(path) || System.IO.Directory.Exists(path))
                {
                    valid.Add(path);
                }
                else
                {
                    invalid.Add(path);
                }
            }

            return (valid, invalid);
        }

        /// <summary>
        /// Gets summary information about selected items
        /// </summary>
        public static string GetSelectionSummary(IEnumerable<string> paths)
        {
            int fileCount = 0;
            int folderCount = 0;
            long totalSize = 0;

            foreach (var path in paths)
            {
                if (System.IO.File.Exists(path))
                {
                    fileCount++;
                    try
                    {
                        totalSize += new System.IO.FileInfo(path).Length;
                    }
                    catch { }
                }
                else if (System.IO.Directory.Exists(path))
                {
                    folderCount++;
                    totalSize += CopyManager.GetItemSize(path);
                }
            }

            var summary = new List<string>();
            if (fileCount > 0) summary.Add($"{fileCount} file(s)");
            if (folderCount > 0) summary.Add($"{folderCount} folder(s)");
            
            string itemsSummary = string.Join(", ", summary);
            string sizeSummary = CopyManager.FormatBytes(totalSize);

            return $"{itemsSummary} - Total size: {sizeSummary}";
        }
    }
}

