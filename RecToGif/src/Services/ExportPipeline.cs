using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RecToGif.Editor;
using RecToGif.Models;

namespace RecToGif.Services
{
    public class ExportPipeline : IExportPipeline
    {
        private readonly AppSettings _appSettings;

        public ExportPipeline(AppSettings? appSettings = null)
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

            // Estimate disk space needed: ~50 KB per rendered PNG frame + output file
            long estimatedBytesPerFrame = 50 * 1024;
            long estimatedTotal = frames.Count * estimatedBytesPerFrame + 10 * 1024 * 1024;

            string? outputDrive = Path.GetPathRoot(outputPath);
            if (!string.IsNullOrEmpty(outputDrive))
            {
                try
                {
                    var driveInfo = new DriveInfo(outputDrive);
                    if (driveInfo.IsReady && driveInfo.AvailableFreeSpace < estimatedTotal)
                    {
                        long neededMb = estimatedTotal / (1024 * 1024);
                        long availableMb = driveInfo.AvailableFreeSpace / (1024 * 1024);
                        throw new InvalidOperationException(
                            $"Not enough disk space on {driveInfo.Name}. Need ~{neededMb} MB but only {availableMb} MB available.");
                    }
                }
                catch (ArgumentException) { } // Root path unreadable on some systems — skip check
            }

            Directory.CreateDirectory(workDir);

            int total = frames.Count;
            int rendered = 0;
            // Cap parallelism at 4 to avoid saturating I/O on many-core machines
            int degreeOfParallelism = Math.Min(Math.Max(1, Environment.ProcessorCount), 4);
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = degreeOfParallelism,
                CancellationToken = token
            };

            try
            {
                await Parallel.ForAsync(0, total, parallelOptions, async (i, ct) =>
                {
                    ct.ThrowIfCancellationRequested();
                    await RenderFrameAsync(frames[i], settings, workDir, i);
                    int done = Interlocked.Increment(ref rendered);
                    progress.Report((int)((float)done / total * 90));
                });
            }
            catch (OperationCanceledException)
            {
                // Wait for any in-flight saves before deleting
                await Task.Delay(500, CancellationToken.None);
                try { Directory.Delete(workDir, true); } catch { }
                throw;
            }

            int fps = ComputeFps(frames);
            switch (format.ToLower())
            {
                case "gif":
                    await RenderGifAsync(workDir, outputPath, settings, progress, token, fps);
                    break;
                case "mp4":
                    await RenderVideoAsync(workDir, outputPath, "mp4", settings, progress, token, fps);
                    break;
                case "webm":
                    await RenderVideoAsync(workDir, outputPath, "webm", settings, progress, token, fps);
                    break;
                case "webp":
                    await RenderWebpAsync(workDir, outputPath, settings, progress, token, fps);
                    break;
            }

            try { Directory.Delete(workDir, true); } catch { }
        }

        private async Task<string?> RenderFrameAsync(FrameItem frame, ProjectSettings settings, string workDir, int index)
        {
            try
            {
                if (!File.Exists(frame.ImagePath))
                {
                    System.Diagnostics.Debug.WriteLine($"[Export] Frame {index} file not found: {frame.ImagePath}");
                    return null;
                }

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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Export] Frame {index} render failed: {ex.Message}");
                return null;
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
                using (var font = overlay.ToFont())
                {
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

        private static string? FindExecutable(string exeName, string localAppDataSubPath, string? userPath = null)
        {
            if (!string.IsNullOrWhiteSpace(userPath))
            {
                if (!File.Exists(userPath)) return null;
                string ext = Path.GetExtension(userPath);
                if (!string.Equals(ext, ".exe", StringComparison.OrdinalIgnoreCase))
                    return null;
                return userPath;
            }

            string bundled = Path.Combine(AppContext.BaseDirectory, "tools", exeName);
            if (File.Exists(bundled)) return bundled;

            string local = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), localAppDataSubPath, exeName);
            if (File.Exists(local)) return local;

            return null;
        }

        private static int ComputeFps(IReadOnlyList<FrameItem> frames)
        {
            if (frames.Count == 0) return 15;

            var delays = frames
                .Select(f => f.Metadata.DelayMs)
                .Where(d => d > 0)
                .OrderBy(d => d)
                .ToList();

            if (delays.Count == 0) return 15;

            int medianDelay = delays[delays.Count / 2];
            int fps = (int)Math.Round(1000.0 / medianDelay);
            return Math.Clamp(fps, 1, 120);
        }

        private async Task RenderGifAsync(string sourceDir, string outputPath, ProjectSettings settings, IProgress<int> progress, CancellationToken token, int fps)
        {
            string? gifskiPath = FindExecutable("gifski.exe", @"RecToGif\gifski", _appSettings.GifskiPath);
            if (gifskiPath == null)
                throw new FileNotFoundException("gifski.exe not found. Set the path in Settings → External Tools, place it in %LocalAppData%\\RecToGif\\gifski\\, or add it to PATH.");

            var psi = new ProcessStartInfo(gifskiPath)
            {
                Arguments = $"-o \"{outputPath}\" \"{sourceDir}\\*.png\" --fps {fps} --quality 90",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            ValidateExePath(gifskiPath, "gifski");
            ValidateOutputPath(outputPath);

            using (var process = Process.Start(psi))
            {
                if (process != null)
                {
                    try { await process.WaitForExitAsync(token); }
                    catch (OperationCanceledException) { process.Kill(); throw; }

                    if (process.ExitCode != 0)
                    {
                        string stderr = await process.StandardError.ReadToEndAsync();
                        throw new InvalidOperationException($"gifski exited with code {process.ExitCode}: {stderr}");
                    }
                }
                else
                {
                    throw new InvalidOperationException("Failed to start gifski process.");
                }
            }
            progress.Report(100);
        }

        private async Task RenderVideoAsync(string sourceDir, string outputPath, string format, ProjectSettings settings, IProgress<int> progress, CancellationToken token, int fps)
        {
            string? ffmpegPath = FindExecutable("ffmpeg.exe", @"RecToGif\ffmpeg", _appSettings.FfmpegPath);
            if (ffmpegPath == null)
                throw new FileNotFoundException("ffmpeg.exe not found. Set the path in Settings → External Tools, place it in %LocalAppData%\\RecToGif\\ffmpeg\\, or add it to PATH.");

            string codec = format switch
            {
                "mp4" => "libx264 -pix_fmt yuv420p",
                "webm" => "libvpx-vp9 -b:v 0 -crf 30",
                _ => "libx264 -pix_fmt yuv420p"
            };

            string args = $"-y -framerate {fps} -i \"{sourceDir}\\%05d.png\" -c:v {codec} \"{outputPath}\"";

            var psi = new ProcessStartInfo(ffmpegPath)
            {
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true
            };

            ValidateExePath(ffmpegPath, "ffmpeg");
            ValidateOutputPath(outputPath);

            using (var process = Process.Start(psi))
            {
                if (process != null)
                {
                    try { await process.WaitForExitAsync(token); }
                    catch (OperationCanceledException) { process.Kill(); throw; }

                    if (process.ExitCode != 0)
                    {
                        string stderr = await process.StandardError.ReadToEndAsync();
                        throw new InvalidOperationException($"ffmpeg exited with code {process.ExitCode}: {stderr}");
                    }
                }
                else
                {
                    throw new InvalidOperationException("Failed to start ffmpeg process.");
                }
            }
            progress.Report(100);
        }

        private static void ValidateOutputPath(string outputPath)
        {
            if (outputPath.Contains('\"') || outputPath.Contains('\''))
                throw new ArgumentException("Output path contains invalid characters.");
        }

        /// <summary>
        /// Sanity-check that a resolved executable path is an .exe whose file name
        /// starts with the expected tool name. This prevents a user-configured path
        /// from accidentally pointing to a different executable.
        /// </summary>
        private static void ValidateExePath(string exePath, string expectedToolName)
        {
            if (!File.Exists(exePath))
                throw new FileNotFoundException($"Executable not found: {exePath}");
            string fileName = Path.GetFileNameWithoutExtension(exePath);
            if (!fileName.StartsWith(expectedToolName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException(
                    $"Configured path for {expectedToolName} points to '{fileName}.exe', which does not appear to be {expectedToolName}.");
        }

        private async Task RenderWebpAsync(string sourceDir, string outputPath, ProjectSettings settings, IProgress<int> progress, CancellationToken token, int fps)
        {
            string? ffmpegPath = FindExecutable("ffmpeg.exe", @"RecToGif\ffmpeg", _appSettings.FfmpegPath);
            if (ffmpegPath == null)
                throw new FileNotFoundException("ffmpeg.exe not found. Set the path in Settings → External Tools, place it in %LocalAppData%\\RecToGif\\ffmpeg\\, or add it to PATH.");

            ValidateExePath(ffmpegPath, "ffmpeg");
            ValidateOutputPath(outputPath);

            string args = $"-y -framerate {fps} -i \"{sourceDir}\\%05d.png\" -c:v libwebp_anim -lossless 0 -quality 80 -loop 0 \"{outputPath}\"";

            var psi = new ProcessStartInfo(ffmpegPath)
            {
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true
            };

            using (var process = Process.Start(psi))
            {
                if (process != null)
                {
                    try { await process.WaitForExitAsync(token); }
                    catch (OperationCanceledException) { process.Kill(); throw; }

                    if (process.ExitCode != 0)
                    {
                        string stderr = await process.StandardError.ReadToEndAsync();
                        throw new InvalidOperationException($"ffmpeg exited with code {process.ExitCode}: {stderr}");
                    }
                }
                else
                {
                    throw new InvalidOperationException("Failed to start ffmpeg process.");
                }
            }
            progress.Report(100);
        }
    }
}
