using System;
using System.Collections.Generic;

namespace RecToGif.Models
{
    public class FrameMeta
    {
        public int FrameIndex { get; set; }
        public long TimestampMs { get; set; }
        public int DelayMs { get; set; }
        public CursorInfo Cursor { get; set; } = new();
        public List<InputEvent> InputEvents { get; set; } = new();
    }

    public class CursorInfo
    {
        public bool Visible { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Type { get; set; } = "Arrow";
    }

    public class InputEvent
    {
        public string Type { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Button { get; set; } = string.Empty;
        public int X { get; set; }
        public int Y { get; set; }
        public long TimestampMs { get; set; }
    }
}
