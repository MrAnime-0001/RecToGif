using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RecToGif.Controls
{
    /// <summary>
    /// Thread-safe async thumbnail cache with LRU eviction and memory budget.
    /// Uses bicubic resampling for high-quality thumbnails.
    /// </summary>
    public sealed class ThumbnailCache : IDisposable
    {
        private readonly int _thumbWidth;
        private readonly int _thumbHeight;
        private readonly long _maxBytes;
        private long _currentBytes;

        private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
        // Tracks in-flight load tasks to avoid duplicate work for the same path
        private readonly ConcurrentDictionary<string, Task<Bitmap?>> _inflight = new();
        private readonly object _sync = new();

        private bool _disposed;

        private sealed class CacheEntry
        {
            public Bitmap? Bitmap { get; }
            public long SizeBytes { get; }
            public int AccessCounter { get; set; } // incremented on each access for LRU approximation

            public CacheEntry(Bitmap bitmap, long sizeBytes)
            {
                Bitmap = bitmap;
                SizeBytes = sizeBytes;
                AccessCounter = 0;
            }
        }

        public ThumbnailCache(int width, int height, long maxBytes)
        {
            _thumbWidth = width;
            _thumbHeight = height;
            _maxBytes = maxBytes;
        }

        /// <summary>
        /// Synchronously tries to get a thumbnail from cache. Updates access counter for LRU.
        /// Returns a CLONE so the caller owns an independent bitmap — the cache may evict
        /// and dispose its own copy at any time without affecting the returned image.
        /// </summary>
        public bool TryGet(string imagePath, out Bitmap? bitmap)
        {
            if (_cache.TryGetValue(imagePath, out var entry) && entry.Bitmap != null)
            {
                entry.AccessCounter++; // promote on access
                try { bitmap = new Bitmap(entry.Bitmap); return true; }
                catch { bitmap = null; return false; }
            }
            bitmap = null;
            return false;
        }

        /// <summary>
        /// Asynchronously loads a thumbnail, using cache if available or creating it.
        /// Callback is invoked on a background thread; the caller should BeginInvoke onto UI thread if needed.
        /// Deduplicates concurrent requests for the same uncached path.
        /// </summary>
        public void GetOrLoadAsync(string imagePath, Action<Bitmap?> onLoaded)
        {
            if (_disposed) { onLoaded(null); return; }

            // Fast path: already cached — update LRU and return a CLONE (see TryGet comment)
            if (_cache.TryGetValue(imagePath, out var existing) && existing.Bitmap != null)
            {
                existing.AccessCounter++;
                try { onLoaded(new Bitmap(existing.Bitmap)); }
                catch { onLoaded(null); }
                return;
            }

            // Deduplicate in-flight requests for the same path
            var task = _inflight.GetOrAdd(imagePath, _ => Task.Run(() => LoadThumbnail(imagePath)));
            task.ContinueWith(t =>
            {
                // Remove from in-flight map so future requests for the same path can re-load if needed
                _inflight.TryRemove(imagePath, out _);
                onLoaded(t.Result);
            }, TaskScheduler.Default);
        }

        private Bitmap? LoadThumbnail(string imagePath)
        {
            if (_disposed) return null;

            try
            {
                if (!File.Exists(imagePath)) return null;

                using var original = new Bitmap(imagePath);
                using var thumbnail = ResizeImage(original, _thumbWidth, _thumbHeight);

                if (thumbnail == null) return null;

                long sizeBytes = thumbnail.Width * thumbnail.Height * 4L;
                // Pass a CLONE to the cache so AddWithEviction owns it and can safely dispose
                // it if the path was already cached (deduplicated in-flight). The original
                // thumbnail stays valid for our own return clone below.
                AddWithEviction(imagePath, new Bitmap(thumbnail), sizeBytes);

                // Return a separate clone so the caller owns an independent bitmap
                return new Bitmap(thumbnail);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Pre-warms the cache with multiple thumbnails in parallel.
        /// Does not block the caller; completions are fire-and-forget.
        /// </summary>
        public async Task WarmAsync(IList<string> imagePaths, int maxDegreeOfParallelism = 4)
        {
            if (_disposed) return;

            var uniquePaths = imagePaths
                .Distinct()
                .Where(p => !_cache.ContainsKey(p) && !_inflight.ContainsKey(p))
                .ToList();

            if (uniquePaths.Count == 0) return;

            using var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
            var tasks = uniquePaths.Select(path => Task.Run(async () =>
            {
                await semaphore.WaitAsync().ConfigureAwait(false);
                try
                {
                    if (_disposed) return;
                    GetOrLoadAsync(path, _ => { });
                }
                finally
                {
                    semaphore.Release();
                }
            }));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Clears all cached thumbnails and releases memory.
        /// </summary>
        public void Clear()
        {
            lock (_sync)
            {
                foreach (var kvp in _cache)
                {
                    kvp.Value.Bitmap?.Dispose();
                }
                _cache.Clear();
                _currentBytes = 0;
            }
            // Clear in-flight without lock (it's a ConcurrentDictionary)
            _inflight.Clear();
        }

        private void AddWithEviction(string path, Bitmap bitmap, long sizeBytes)
        {
            if (sizeBytes > _maxBytes) return;

            lock (_sync)
            {
                if (_disposed) return;

                // Evict until we're under budget — evict lowest access-counter entry first (O(n) scan, no sort)
                while (_currentBytes + sizeBytes > _maxBytes && _cache.Count > 0)
                {
                    string? evictKey = null;
                    int minAccess = int.MaxValue;
                    foreach (var kvp in _cache)
                    {
                        if (kvp.Value.AccessCounter < minAccess)
                        {
                            minAccess = kvp.Value.AccessCounter;
                            evictKey = kvp.Key;
                        }
                    }
                    if (evictKey == null) break;
                    if (_cache.TryRemove(evictKey, out var evicted))
                    {
                        _currentBytes -= evicted.SizeBytes;
                        evicted.Bitmap?.Dispose();
                    }
                }

                if (_cache.TryGetValue(path, out var existing))
                {
                    // Already cached (e.g., deduplicated in-flight completed); update access counter
                    existing.AccessCounter++;
                    bitmap.Dispose(); // don't need the newly created one
                    return;
                }

                _cache[path] = new CacheEntry(bitmap, sizeBytes);
                _currentBytes += sizeBytes;
            }
        }

        private static Bitmap ResizeImage(Image original, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            destBitmap.SetResolution(original.HorizontalResolution, original.VerticalResolution);

            using var graphics = Graphics.FromImage(destBitmap);
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.SmoothingMode = SmoothingMode.HighQuality;

            graphics.DrawImage(original, destRect, 0, 0, original.Width, original.Height, GraphicsUnit.Pixel);

            return destBitmap;
        }

        public void Dispose()
        {
            lock (_sync)
            {
                if (_disposed) return;
                _disposed = true;

                foreach (var kvp in _cache)
                {
                    kvp.Value.Bitmap?.Dispose();
                }
                _cache.Clear();
                _currentBytes = 0;
            }
            _inflight.Clear();
        }
    }
}