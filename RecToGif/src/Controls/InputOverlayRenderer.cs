using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using RecToGif.Models;

namespace RecToGif.Controls
{
    public class InputOverlayRenderer : IDisposable
    {
        public Color PillColor { get; set; } = Color.FromArgb(180, 0, 0, 0);
        public Color TextColor { get; set; } = Color.White;
        public float Opacity { get; set; } = 0.7f;
        public Font Font { get; set; } = new Font("Segoe UI", 12, FontStyle.Bold);
        public int Padding { get; set; } = 10;
        public int CornerRadius { get; set; } = 15;

        private Rectangle _cachedRect;
        private int _cachedRadius;
        private GraphicsPath? _cachedPath;

        public void Dispose()
        {
            _cachedPath?.Dispose();
            Font?.Dispose();
        }

        public void Render(Graphics g, List<InputEvent> events, Size frameSize)
        {
            if (events == null || events.Count == 0) return;

            // Group events? Or just show the last one?
            // Usually we show events that happened "in" this frame.
            
            // Draw mouse clicks at their positions
            foreach (var ev in events.Where(e => e.Type == "MouseClick"))
            {
                DrawMouseClick(g, ev);
            }

            // Draw key combos in a corner (e.g. Bottom-Left)
            var keyEvents = events.Where(e => e.Type == "KeyDown").ToList();
            if (keyEvents.Any())
            {
                DrawKeyCombos(g, keyEvents, frameSize);
            }
        }

        private void DrawMouseClick(Graphics g, InputEvent ev)
        {
            int radius = 20;
            using (var brush = new SolidBrush(Color.FromArgb((int)(Opacity * 255), Color.Yellow)))
            using (var pen = new Pen(Color.Orange, 2))
            {
                g.FillEllipse(brush, ev.X - radius, ev.Y - radius, radius * 2, radius * 2);
                g.DrawEllipse(pen, ev.X - radius, ev.Y - radius, radius * 2, radius * 2);
            }
        }

        private void DrawKeyCombos(Graphics g, List<InputEvent> events, Size frameSize)
        {
            string text = string.Join("  ", events.Select(e => e.Key));
            var textSize = g.MeasureString(text, Font);

            int width = (int)textSize.Width + Padding * 2;
            int height = (int)textSize.Height + Padding * 2;

            int x = Padding;
            int y = frameSize.Height - height - Padding;

            var rect = new Rectangle(x, y, width, height);

            using (var path = GetRoundedRect(rect, CornerRadius))
            using (var brush = new SolidBrush(Color.FromArgb((int)(Opacity * PillColor.A), PillColor)))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.FillPath(brush, path);
                g.DrawString(text, Font, Brushes.White, x + Padding, y + Padding);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            if (_cachedPath != null && _cachedRect == rect && _cachedRadius == radius)
                return _cachedPath;

            _cachedPath?.Dispose();
            _cachedPath = new GraphicsPath();
            _cachedRect = rect;
            _cachedRadius = radius;

            int diameter = radius * 2;
            _cachedPath.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            _cachedPath.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            _cachedPath.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            _cachedPath.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            _cachedPath.CloseFigure();
            return _cachedPath;
        }
    }
}
