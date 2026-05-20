using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RecToGif.Editor;

namespace RecToGif.Controls
{
    public class FrameTimeline : UserControl, IDisposable
    {
        private List<FrameItem> _frames = new();
        private readonly HashSet<int> _selectedIndices = new();
        private int _focusedIndex = -1;
        private const int ThumbnailWidth = 128;
        private const int ThumbnailHeight = 72; // 16:9 ratio approx
        private const int PaddingSize = 5;

        // Async thumbnail cache: ~50 MB budget, bicubic resampling
        private readonly ThumbnailCache _thumbCache;
        // Tracks in-flight load requests to avoid duplicate submissions
        private readonly HashSet<string> _pendingPaths = new();
        private readonly object _pendingLock = new();

        public event EventHandler? SelectionChanged;
        public event EventHandler<int>? FrameDoubleClicked;

        public FrameTimeline()
        {
            this.DoubleBuffered = true;
            this.AutoScroll = true;
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.Height = ThumbnailHeight + PaddingSize * 4;

            _thumbCache = new ThumbnailCache(ThumbnailWidth, ThumbnailHeight, maxBytes: 50 * 1024 * 1024);
        }

        public void SetFrames(IEnumerable<FrameItem> frames)
        {
            _frames = frames.ToList();
            _selectedIndices.Clear();
            _focusedIndex = -1;
            _pendingPaths.Clear();
            _thumbCache.Clear(); // clear stale thumbnails on new session
            UpdateScrollRange();
            this.Invalidate();

            // Start background pre-warming of all thumbnails (does not block UI)
            var paths = _frames.Select(f => f.ImagePath).ToList();
            _ = _thumbCache.WarmAsync(paths, maxDegreeOfParallelism: 4);
        }

        /// <summary>
        /// Clears all cached thumbnails (e.g., when opening a new session).
        /// </summary>
        public void ClearThumbnails() => _thumbCache.Clear();

        public IEnumerable<int> SelectedIndices => _selectedIndices.OrderBy(i => i);

        private void UpdateScrollRange()
        {
            int totalWidth = _frames.Count * (ThumbnailWidth + PaddingSize) + PaddingSize;
            this.AutoScrollMinSize = new Size(totalWidth, this.Height - SystemInformation.HorizontalScrollBarHeight);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.TranslateTransform(this.AutoScrollPosition.X, this.AutoScrollPosition.Y);

            // Per-paint-pass clone cache: avoids cloning the same thumbnail twice in one
            // paint cycle when it's requested via both TryGet (sync) and GetOrLoadAsync (async
            // callback that arrives before the paint completes). Keys are ImagePath strings.
            var paintCloneCache = new Dictionary<string, Bitmap?>();

            int scrollX = -this.AutoScrollPosition.X;
            int visibleStartX = scrollX - (ThumbnailWidth + PaddingSize);
            int visibleEndX = scrollX + this.ClientSize.Width + (ThumbnailWidth + PaddingSize);

            int startIndex = Math.Max(0, visibleStartX / (ThumbnailWidth + PaddingSize));
            int endIndex = Math.Min(_frames.Count, (visibleEndX / (ThumbnailWidth + PaddingSize)) + 1);

            for (int i = startIndex; i < endIndex; i++)
            {
                int x = i * (ThumbnailWidth + PaddingSize) + PaddingSize;
                int y = PaddingSize;
                var rect = new Rectangle(x, y, ThumbnailWidth, ThumbnailHeight);

                // Draw selection / focus background
                if (_selectedIndices.Contains(i))
                {
                    e.Graphics.FillRectangle(Brushes.DodgerBlue, x - 2, y - 2, ThumbnailWidth + 4, ThumbnailHeight + 4);
                }
                else if (i == _focusedIndex)
                {
                    e.Graphics.DrawRectangle(Pens.Gray, x - 1, y - 1, ThumbnailWidth + 2, ThumbnailHeight + 2);
                }

                // Draw cached thumbnail or black placeholder if not yet loaded
                // Reuse a clone from the per-paint cache if available; ThumbnailCache.TryGet
                // returns a fresh clone each time so the cache's own copy can be evicted safely
                string imagePath = _frames[i].ImagePath;
                if (!paintCloneCache.TryGetValue(imagePath, out var bmp))
                {
                    bmp = _thumbCache.TryGet(imagePath, out var tmp) && tmp != null ? tmp : null;
                    paintCloneCache[imagePath] = bmp;
                }
                if (bmp != null)
                    e.Graphics.DrawImage(bmp, rect);
                else
                    e.Graphics.FillRectangle(Brushes.Black, rect);

                // Overlay: frame index + delay
                    // Frame index overlay — clip to thumbnail bounds with dark backing
                string indexStr = (i + 1).ToString();
                var indexSize = e.Graphics.MeasureString(indexStr, this.Font);
                int indexBgW = (int)Math.Ceiling(indexSize.Width) + 4;
                int indexBgH = (int)Math.Ceiling(indexSize.Height) + 2;
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(160, 0, 0, 0)), x + 1, y + 1, indexBgW, indexBgH);
                e.Graphics.DrawString(indexStr, this.Font, Brushes.White, x + 3, y + 2);

                // Delay overlay — bottom-right with dark backing
                string delayStr = $"{_frames[i].Metadata.DelayMs}ms";
                var delaySize = e.Graphics.MeasureString(delayStr, this.Font);
                int delayBgW = (int)Math.Ceiling(delaySize.Width) + 4;
                int delayBgH = (int)Math.Ceiling(delaySize.Height) + 2;
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(160, 0, 0, 0)),
                    x + ThumbnailWidth - delayBgW - 1, y + ThumbnailHeight - delayBgH - 1,
                    delayBgW, delayBgH);
                e.Graphics.DrawString(delayStr, this.Font, Brushes.Yellow,
                    x + ThumbnailWidth - delaySize.Width - 3, y + ThumbnailHeight - delaySize.Height - 2);

                // If not yet loaded (bmp == null), request async load; on completion, redraw on UI thread
                if (bmp == null)
                {
                    string path = imagePath;
                    int frameIdx = i;
                    bool wasAdded;
                    lock (_pendingPaths)
                    {
                        wasAdded = _pendingPaths.Add(path);
                    }
                    if (wasAdded)
                    {
                        _thumbCache.GetOrLoadAsync(path, bmp =>
                        {
                            lock (_pendingPaths) { _pendingPaths.Remove(path); }
                            if (bmp != null)
                            {
                                this.BeginInvoke(new Action(() =>
                                {
                                    int thumbX = frameIdx * (ThumbnailWidth + PaddingSize) + PaddingSize;
                                    this.Invalidate(new Rectangle(thumbX, PaddingSize, ThumbnailWidth, ThumbnailHeight));
                                    bmp.Dispose();
                                }));
                            }
                            else
                            {
                                bmp?.Dispose();
                            }
                        });
                    }
                }
            }

            foreach (var bmp in paintCloneCache.Values) bmp?.Dispose();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            this.Focus(); // ensure we receive keyboard events

            int clientX = e.X - this.AutoScrollPosition.X;
            int index = clientX / (ThumbnailWidth + PaddingSize);

            if (index >= 0 && index < _frames.Count)
            {
                HandleSelection(index, ModifierKeys);
                _focusedIndex = index;
                this.Invalidate();
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void HandleSelection(int index, Keys modifiers)
        {
            if ((modifiers & Keys.Control) == Keys.Control)
            {
                if (_selectedIndices.Contains(index)) _selectedIndices.Remove(index);
                else _selectedIndices.Add(index);
            }
            else if ((modifiers & Keys.Shift) == Keys.Shift && _focusedIndex != -1)
            {
                _selectedIndices.Clear();
                int start = Math.Min(_focusedIndex, index);
                int end = Math.Max(_focusedIndex, index);
                for (int i = start; i <= end; i++) _selectedIndices.Add(i);
            }
            else
            {
                _selectedIndices.Clear();
                _selectedIndices.Add(index);
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            this.Focus(); // ensure we receive keyboard events

            int clientX = e.X - this.AutoScrollPosition.X;
            int index = clientX / (ThumbnailWidth + PaddingSize);
            if (index >= 0 && index < _frames.Count)
            {
                FrameDoubleClicked?.Invoke(this, index);
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            // Route arrow keys, Up/Down, Home/End, Delete, Ctrl+A/Z/Y, Space/Enter to OnKeyDown
            return keyData == Keys.Left || keyData == Keys.Right ||
                   keyData == Keys.Up || keyData == Keys.Down ||
                   keyData == Keys.Home || keyData == Keys.End ||
                   keyData == Keys.Delete || keyData == Keys.Space || keyData == Keys.Enter ||
                   keyData == (Keys.Control | Keys.A) ||
                   keyData == (Keys.Control | Keys.Z) ||
                   keyData == (Keys.Control | Keys.Y) ||
                   base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (_frames.Count == 0) return;

            int newIndex = _focusedIndex;

            switch (e.KeyCode)
            {
                case Keys.Left:
                    newIndex = Math.Max(0, _focusedIndex - 1);
                    break;
                case Keys.Right:
                    newIndex = Math.Min(_frames.Count - 1, _focusedIndex + 1);
                    break;
                case Keys.Home:
                    newIndex = 0;
                    break;
                case Keys.End:
                    newIndex = _frames.Count - 1;
                    break;
                case Keys.A:
                    if (e.Control && _frames.Count > 0)
                    {
                        e.Handled = true;
                        _selectedIndices.Clear();
                        for (int i = 0; i < _frames.Count; i++) _selectedIndices.Add(i);
                        _focusedIndex = Math.Max(0, _focusedIndex);
                        this.Invalidate();
                        SelectionChanged?.Invoke(this, EventArgs.Empty);
                    }
                    return;

                case Keys.Z:
                    if (e.Control)
                    {
                        e.Handled = true;
                        UndoRequested?.Invoke(this, EventArgs.Empty);
                    }
                    return;

                case Keys.Y:
                    if (e.Control)
                    {
                        e.Handled = true;
                        RedoRequested?.Invoke(this, EventArgs.Empty);
                    }
                    return;

                case Keys.Delete:
                    e.Handled = true;
                    DeleteSelectedFramesWithConfirmation();
                    return;

                default:
                    return;
            }

            e.Handled = true;

            // Shift+Arrow = range selection; plain arrow = single selection
            if (e.Shift && _focusedIndex != -1)
            {
                _selectedIndices.Clear();
                int start = Math.Min(_focusedIndex, newIndex);
                int end = Math.Max(_focusedIndex, newIndex);
                for (int i = start; i <= end; i++) _selectedIndices.Add(i);
            }
            else
            {
                _selectedIndices.Clear();
                _selectedIndices.Add(newIndex);
            }

            _focusedIndex = newIndex;
            EnsureFocusedFrameVisible();
            this.Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void EnsureFocusedFrameVisible()
        {
            if (_focusedIndex < 0) return;

            int itemWidth = ThumbnailWidth + PaddingSize;
            int thumbStartX = _focusedIndex * itemWidth + PaddingSize;

            int viewStart = -AutoScrollPosition.X;
            int viewEnd = viewStart + ClientSize.Width;

            if (thumbStartX < viewStart)
            {
                // Frame is left of visible area — scroll left to show it
                AutoScrollPosition = new Point(thumbStartX, 0);
            }
            else if (thumbStartX + ThumbnailWidth > viewEnd)
            {
                // Frame is right of visible area — scroll so it's fully visible on right edge
                AutoScrollPosition = new Point(thumbStartX - ClientSize.Width + ThumbnailWidth, 0);
            }
        }

        private void DeleteSelectedFramesWithConfirmation()
        {
            if (_selectedIndices.Count == 0 && _focusedIndex >= 0)
            {
                _selectedIndices.Add(_focusedIndex);
            }

            if (_selectedIndices.Count == 0) return;

            string message = _selectedIndices.Count == 1
                ? "Delete 1 selected frame?"
                : $"Delete {_selectedIndices.Count} selected frames?";

            var result = MessageBox.Show(
                message + "\n\nThis cannot be undone.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            // Snapshot indices before firing so presenter sees the correct selection
            var indicesToDelete = _selectedIndices.ToList();
            DeleteRequested?.Invoke(this, EventArgs.Empty);
            // Don't call Invalidate here; the presenter will refresh via DisplayFrames

            // Also expose the deleted indices via a read-only property so the presenter
            // can retrieve them after the event fires
            _lastDeletedIndices = indicesToDelete;
        }

        private List<int> _lastDeletedIndices = new();

        /// <summary>Returns the indices that were just deleted via DeleteRequested, or empty list.</summary>
        public IReadOnlyList<int> LastDeletedIndices => _lastDeletedIndices;

        public event EventHandler? DeleteRequested;
        public event EventHandler? UndoRequested;
        public event EventHandler? RedoRequested;
        protected override void Dispose(bool disposing)
        {
            if (disposing) _thumbCache?.Dispose();
            base.Dispose(disposing);
        }
    }
}
