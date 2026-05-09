using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RecToGif.Editor
{
    public class LoopFinder
    {
        public class LoopCandidate
        {
            public int StartFrame { get; set; }
            public int EndFrame { get; set; }
            public double Similarity { get; set; }
            public int Span => EndFrame - StartFrame;
        }

        public async Task<List<LoopCandidate>> FindLoopsAsync(
            IReadOnlyList<FrameItem> frames,
            int minSpan,
            int maxSpan,
            double threshold,
            int comparisonSize,
            IProgress<int> progress,
            CancellationToken token)
        {
            var results = new List<LoopCandidate>();
            int totalFrames = frames.Count;
            if (totalFrames < 2) return results;

            // Pre-process frames: downsample and grayscale
            var processedFrames = await Task.Run(() => 
                frames.Select(f => ProcessFrameForComparison(f.ImagePath, comparisonSize)).ToList(), 
                token);

            int totalPairs = 0;
            for (int i = 0; i < totalFrames - minSpan; i++)
            {
                for (int j = i + minSpan; j < Math.Min(i + maxSpan, totalFrames); j++)
                {
                    totalPairs++;
                }
            }

            int currentPair = 0;
            for (int i = 0; i < totalFrames - minSpan; i++)
            {
                token.ThrowIfCancellationRequested();
                byte[] frameA = processedFrames[i];

                for (int j = i + minSpan; j < Math.Min(i + maxSpan, totalFrames); j++)
                {
                    byte[] frameB = processedFrames[j];
                    double similarity = ComputeSimilarity(frameA, frameB);

                    if (similarity >= threshold)
                    {
                        results.Add(new LoopCandidate
                        {
                            StartFrame = i,
                            EndFrame = j,
                            Similarity = similarity
                        });
                    }

                    currentPair++;
                    progress.Report((int)((float)currentPair / totalPairs * 100));
                }
            }

            return results.OrderByDescending(r => r.Span).Take(5).ToList();
        }

        private byte[] ProcessFrameForComparison(string path, int size)
        {
            // Copy pixels into a managed array while the bitmap buffer is locked,
            // then dispose the bitmap — the unsafe pointer must not outlive the lock.
            using (var original = Image.FromFile(path))
            using (var resized = new Bitmap(original, new Size(size, size)))
            {
                var rect = new Rectangle(0, 0, size, size);
                var data = resized.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                try
                {
                    int byteCount = size * size;
                    var grayscale = new byte[byteCount];
                    unsafe
                    {
                        byte* ptr = (byte*)data.Scan0;
                        for (int i = 0; i < byteCount; i++)
                        {
                            // Simple average for grayscale — pointer is valid for the locked duration
                            grayscale[i] = (byte)((ptr[i * 4] + ptr[i * 4 + 1] + ptr[i * 4 + 2]) / 3);
                        }
                    }
                    return grayscale;
                }
                finally
                {
                    resized.UnlockBits(data);
                }
            }
        }

        private double ComputeSimilarity(byte[] a, byte[] b)
        {
            long diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff += Math.Abs(a[i] - b[i]);
            }

            double maxDiff = a.Length * 255.0;
            return (1.0 - (diff / maxDiff)) * 100.0;
        }
    }
}
