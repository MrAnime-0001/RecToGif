using System.IO;
using System.Text.Json;

namespace RecToGif.Models
{
    public class AppSettings
    {
        public int DefaultFps { get; set; } = 15;
        public string ExportFormat { get; set; } = "gif";
        public string OutputDirectory { get; set; } = string.Empty;
        public bool CaptureCursor { get; set; } = true;
        public bool ShowInputOverlay { get; set; } = true;
        public string Theme { get; set; } = "Dark";
        public string GifskiPath { get; set; } = string.Empty;
        public string FfmpegPath { get; set; } = string.Empty;

        public void Save(string path)
        {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        public static AppSettings Load(string path)
        {
            if (!File.Exists(path)) return new AppSettings();
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
    }
}
