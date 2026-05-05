using System.Drawing;

namespace RecToGif.Models
{
    public class CaptureSession
    {
        public string OutputDirectory { get; set; } = string.Empty;
        public Rectangle TargetRegion { get; set; }
        public int TargetFps { get; set; } = 15;
        public bool CaptureCursor { get; set; } = true;
        public bool ShowInputOverlay { get; set; } = true;
    }
}
