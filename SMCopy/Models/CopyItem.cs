using System;

namespace SMCopy.Models
{
    public class CopyItem
    {
        public string Path { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "file" or "folder"
        public long Size { get; set; } = 0;
    }

    public class CopyClipboard
    {
        public System.Collections.Generic.List<CopyItem> Items { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}

