using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RecToGif.Editor;

namespace RecToGif.Controls
{
    public class FrameTimeline : UserControl
    {
        private List<FrameItem> _frames = new();
        private readonly HashSet<int> _selectedIndices = new();
        private int _focusedIndex = -1;
        private const int ThumbnailWidth = 128;
        private const int ThumbnailHeight = 72; // 16:9 ratio approx
        private const int PaddingSize = 5;

        public event EventHandler? SelectionChanged;
        public event EventHandler<int>? FrameDoubleClicked;

        public FrameTimeline()
        {
            this.DoubleBuffered = true;
            this.AutoScroll = true;
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.Height = ThumbnailHeight + PaddingSize * 4;
        }

        public void SetFrames(IEnumerable<FrameItem> frames)
        {
            _frames = frames.ToList();
            _selectedIndices.Clear();
            _focusedIndex = -1;
            UpdateScrollRange();
            this.Invalidate();
        }

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

                // Draw background/border for selection
                if (_selectedIndices.Contains(i))
                {
                    e.Graphics.FillRectangle(Brushes.DodgerBlue, x - 2, y - 2, ThumbnailWidth + 4, ThumbnailHeight + 4);
                }
                else if (i == _focusedIndex)
                {
                    e.Graphics.DrawRectangle(Pens.Gray, x - 1, y - 1, ThumbnailWidth + 2, ThumbnailHeight + 2);
                }

                // Draw placeholder or thumbnail (actual loading should be async/cached)
                e.Graphics.FillRectangle(Brushes.Black, rect);

                // Draw index
                string indexStr = (i + 1).ToString();
                e.Graphics.DrawString(indexStr, this.Font, Brushes.White, x + 2, y + 2);

                // Draw delay
                string delayStr = $"{_frames[i].Metadata.DelayMs}ms";
                var size = e.Graphics.MeasureString(delayStr, this.Font);
                e.Graphics.DrawString(delayStr, this.Font, Brushes.Yellow, x + ThumbnailWidth - size.Width - 2, y + ThumbnailHeight - size.Height - 2);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
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
            int clientX = e.X - this.AutoScrollPosition.X;
            int index = clientX / (ThumbnailWidth + PaddingSize);
            if (index >= 0 && index < _frames.Count)
            {
                FrameDoubleClicked?.Invoke(this, index);
            }
        }
    }
}
