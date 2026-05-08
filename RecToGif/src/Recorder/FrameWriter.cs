using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using RecToGif.Models;

namespace RecToGif.Recorder
{
    public class FrameWriter
    {
        private readonly string _outputDirectory;

        public FrameWriter(string outputDirectory)
        {
            _outputDirectory = outputDirectory;
            if (!Directory.Exists(_outputDirectory))
            {
                Directory.CreateDirectory(_outputDirectory);
            }
        }

        public async Task SaveFrameAsync(Bitmap bitmap, FrameMeta meta)
        {
            string frameName = meta.FrameIndex.ToString("D5");
            string pngPath = Path.Combine(_outputDirectory, $"{frameName}.png");
            string metaPath = Path.Combine(_outputDirectory, $"{frameName}.meta");

            // Save PNG
            await Task.Run(() =>
            {
                bitmap.Save(pngPath, ImageFormat.Png);
                bitmap.Dispose();
            });

            // Save Metadata
            string json = JsonSerializer.Serialize(meta, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(metaPath, json);
        }
    }
}
