using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using RecToGif.Editor;
using RecToGif.Models;

namespace RecToGif.Services
{
    public class ExportPipeline
    {
        private readonly AppSettings _appSettings;

        public ExportPipeline(AppSettings appSettings = null)
        {
            _appSettings = appSettings ?? new AppSettings();
        }

        public async Task ExportAsync(
            string outputPath, 
            string format, 
            IReadOnlyList<FrameItem> frames, 
            ProjectSettings settings,
            IProgress<int> progress,
            CancellationToken token)
        {
            string workDir = Path.Combine(Path.GetTempPath(), "RecToGifExport_" + Guid.NewGuid());
            Directory.CreateDirectory(workDir);

            try
            {
                for (int i = 0; i < frames.Count; i++)
                {
                    token.ThrowIfCancellationRequested();
                    string renderedPath = await RenderFrameAsync(frames[i], settings, workDir, i);
                    progress.Report((int)((float)i / frames.Count * 50));
                }

                switch (format.ToLower())
                {
                    case "gif":
                        await RenderGifAsync(workDir, outputPath, settings, progress, token);
                        break;
                    case "mp4":
                        await RenderVideoAsync(workDir, outputPath, "mp4", settings, progress, token);
                        break;
                }
            }
            finally
            {
                Directory.Delete(workDir, true);
            }
        }

        private async Task<string> RenderFrameAsync(FrameItem frame, ProjectSettings settings, string workDir, int index)
        {
            using (var bmp = new Bitmap(frame.ImagePath))
            {
                Rectangle crop = settings.CropRegion.IsEmpty ? new Rectangle(0, 0, bmp.Width, bmp.Height) : settings.CropRegion;
                
                var targetSize = settings.TargetSize.IsEmpty ? crop.Size : settings.TargetSize;
                using (var output = new Bitmap(targetSize.Width, targetSize.Height))
                using (var g = Graphics.FromImage(output))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(bmp, new Rectangle(0, 0, targetSize.Width, targetSize.Height), crop, GraphicsUnit.Pixel);

                    foreach (var overlay in settings.Overlays)
                    {
                        DrawOverlay(g, overlay, 1.0f, 0, 0); // Simplified scaling
                    }

                    if (settings.BorderThickness > 0)
                    {
                        using (var pen = new Pen(settings.BorderColor, settings.BorderThickness))
                        {
                            pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Inset;
                            g.DrawRectangle(pen, 0, 0, targetSize.Width - 1, targetSize.Height - 1);
                        }
                    }

                    string path = Path.Combine(workDir, $"{index:D5}.png");
                    output.Save(path, System.Drawing.Imaging.ImageFormat.Png);
                    return path;
                }
            }
        }

        private void DrawOverlay(Graphics g, VisualOverlay overlay, float ratio, int offsetX, int offsetY)
        {
            var bounds = new Rectangle(
                (int)(overlay.Bounds.X * ratio) + offsetX,
                (int)(overlay.Bounds.Y * ratio) + offsetY,
                (int)(overlay.Bounds.Width * ratio),
                (int)(overlay.Bounds.Height * ratio));

            if (overlay.Type == OverlayType.Text)
            {
                using (var brush = new SolidBrush(Color.FromArgb((int)(overlay.Opacity * 255), overlay.Color)))
                {
                    var font = overlay.Font ?? new Font("Arial", 12);
                    g.DrawString(overlay.Content, font, brush, bounds.Location);
                }
            }
            else if (overlay.Type == OverlayType.Watermark)
            {
                if (File.Exists(overlay.Content))
                {
                    using (var img = Image.FromFile(overlay.Content))
                    {
                        var cm = new System.Drawing.Imaging.ColorMatrix { Matrix33 = overlay.Opacity };
                        var ia = new System.Drawing.Imaging.ImageAttributes();
                        ia.SetColorMatrix(cm);
                        g.DrawImage(img, bounds, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);
                    }
                }
            }
        }

        private static string FindExecutable(string exeName, string localAppDataSubPath, string userPath = null)
        {
            if (!string.IsNullOrWhiteSpace(userPath) && File.Exists(userPath)) return userPath;

            string bundled = Path.Combine(AppContext.BaseDirectory, "tools", exeName);
            if (File.Exists(bundled)) return bundled;

            string local = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), localAppDataSubPath, exeName);
            if (File.Exists(local)) return local;

            foreach (var dir in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                try
                {
                    var candidate = Path.Combine(dir.Trim(), exeName);
                    if (File.Exists(candidate)) return candidate;
                }
                catch { }
            }

            return null;
        }

        private async Task RenderGifAsync(string sourceDir, string outputPath, ProjectSettings settings, IProgress<int> progress, CancellationToken token)
        {
            string gifskiPath = FindExecutable("gifski.exe", @"RecToGif\gifski", _appSettings.GifskiPath);
            if (gifskiPath == null)
                throw new FileNotFoundException("gifski.exe not found. Set the path in Settings → External Tools, place it in %LocalAppData%\\RecToGif\\gifski\\, or add it to PATH.");

            var psi = new ProcessStartInfo(gifskiPath)
            {
                Arguments = $"-o \"{outputPath}\" \"{sourceDir}\\*.png\" --fps 15 --quality 90",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            using (var process = Process.Start(psi))
            {
                if (process != null)
                {
                    try { await process.WaitForExitAsync(token); }
                    catch (OperationCanceledException) { process.Kill(); throw; }
                }
            }
            progress.Report(100);
        }

        private async Task RenderVideoAsync(string sourceDir, string outputPath, string format, ProjectSettings settings, IProgress<int> progress, CancellationToken token)
        {
            string ffmpegPath = FindExecutable("ffmpeg.exe", @"RecToGif\ffmpeg", _appSettings.FfmpegPath);
            if (ffmpegPath == null)
                throw new FileNotFoundException("ffmpeg.exe not found. Set the path in Settings → External Tools, place it in %LocalAppData%\\RecToGif\\ffmpeg\\, or add it to PATH.");
            
            // FFmpeg arguments for encoding PNG sequence to MP4
            string args = $"-framerate 15 -i \"{sourceDir}\\%05d.png\" -c:v libx264 -pix_fmt yuv420p \"{outputPath}\"";
            
            var psi = new ProcessStartInfo(ffmpegPath)
            {
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(psi))
            {
                if (process != null)
                {
                    try { await process.WaitForExitAsync(token); }
                    catch (OperationCanceledException) { process.Kill(); throw; }
                }
            }
            progress.Report(100);
        }
    }
}
