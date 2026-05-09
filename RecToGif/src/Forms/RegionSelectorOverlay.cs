using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RecToGif.Forms
{
    public sealed partial class RegionSelectorOverlay : Form
    {
        private Point _startPos;
        private bool _isDragging;
        private bool _isResizing;
        private ResizeEdge _activeEdge;
        private Point _resizeStart;
        private Rectangle _resizeStartRect;

        public Rectangle SelectedRegion { get; private set; }
        private readonly Font _font;
        private readonly Brush _bgBrush;

        // Overlay dim brush
        private static readonly Color DimColor = Color.FromArgb(120, 0, 0, 0);
        private static readonly Color BorderColor = Color.FromArgb(0, 180, 255);
        private static readonly Color HandleColor = Color.FromArgb(0, 180, 255);
        private const int HandleSize = 8;

        private enum ResizeEdge { None, N, S, E, W, NE, NW, SE, SW }

        public RegionSelectorOverlay()
        {
            InitializeComponent();
            Bounds = SystemInformation.VirtualScreen;
            _font = new Font("Segoe UI", 11, FontStyle.Bold);
            _bgBrush = new SolidBrush(DimColor);

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

            // Load last saved region
            var settings = Services.SettingsService.LoadSettings();
            if (settings.LastRegion.HasValue)
            {
                SelectedRegion = settings.LastRegion.Value;
                // Ensure it's within bounds
                var screen = SystemInformation.VirtualScreen;
                var clipped = Rectangle.Intersect(SelectedRegion, screen);
                if (clipped.Width < 50) clipped.Width = 50;
                if (clipped.Height < 50) clipped.Height = 50;
                SelectedRegion = clipped;
            }
            else
            {
                // Default centered 800x600
                var screen = SystemInformation.VirtualScreen;
                int w = Math.Min(800, screen.Width);
                int h = Math.Min(600, screen.Height);
                SelectedRegion = new Rectangle(
                    (screen.Width - w) / 2,
                    (screen.Height - h) / 2,
                    w, h);
            }

            this.MouseDown += OnMouseDown;
            this.MouseMove += OnMouseMove;
            this.MouseUp += OnMouseUp;
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    DialogResult = DialogResult.Cancel;
                    Close();
                }
                if (e.KeyCode == Keys.Enter)
                {
                    ConfirmRegion();
                }
            };
            this.Paint += OnOverlayPaint;
        }

        private void OnMouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            _activeEdge = HitTestEdge(e.Location);

            if (_activeEdge != ResizeEdge.None)
            {
                _isResizing = true;
                _resizeStart = e.Location;
                _resizeStartRect = SelectedRegion;
                return;
            }

            if (SelectedRegion.Contains(e.Location))
            {
                // Start drag
                _isDragging = true;
                _startPos = e.Location;
                return;
            }

            // Start new selection
            _isDragging = true;
            _startPos = e.Location;
            SelectedRegion = new Rectangle(e.Location.X, e.Location.Y, 0, 0);
        }

        private void OnMouseMove(object? sender, MouseEventArgs e)
        {
            if (_isResizing && _activeEdge != ResizeEdge.None)
            {
                int dx = e.X - _resizeStart.X;
                int dy = e.Y - _resizeStart.Y;
                SelectedRegion = ApplyResize(_resizeStartRect, _activeEdge, dx, dy);
                Invalidate();
                return;
            }

            if (_isDragging)
            {
                int dx = e.X - _startPos.X;
                int dy = e.Y - _startPos.Y;
                SelectedRegion = new Rectangle(
                    SelectedRegion.X + dx,
                    SelectedRegion.Y + dy,
                    SelectedRegion.Width,
                    SelectedRegion.Height);
                _startPos = e.Location;
                Invalidate();
                return;
            }

            // Update cursor
            var edge = HitTestEdge(e.Location);
            Cursor = edge switch
            {
                ResizeEdge.NW or ResizeEdge.SE => Cursors.SizeNWSE,
                ResizeEdge.NE or ResizeEdge.SW => Cursors.SizeNESW,
                ResizeEdge.N or ResizeEdge.S => Cursors.SizeNS,
                ResizeEdge.E or ResizeEdge.W => Cursors.SizeWE,
                _ when SelectedRegion.Contains(e.Location) => Cursors.SizeAll,
                _ => Cursors.Cross
            };
        }

        private void OnMouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            _isDragging = false;
            _isResizing = false;
            _activeEdge = ResizeEdge.None;
        }

        private void OnOverlayPaint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var screen = Bounds;

            // Draw dim overlay with cutout
            using (var rgn = new Region(screen))
            {
                rgn.Exclude(SelectedRegion);
                g.FillRegion(_bgBrush, rgn);
            }

            // Cross-hatch on dimmed area
            using (var hatchBrush = new HatchBrush(HatchStyle.LargeGrid,
                Color.FromArgb(25, 255, 255, 255), Color.Transparent))
            {
                using (var rgn = new Region(screen))
                {
                    rgn.Exclude(SelectedRegion);
                    g.FillRegion(hatchBrush, rgn);
                }
            }

            // Bright border around selected region
            using (var pen = new Pen(BorderColor, 2))
            {
                g.DrawRectangle(pen, SelectedRegion);
            }

            // Outer glow
            using (var glowPen = new Pen(Color.FromArgb(60, BorderColor), 4))
            {
                var glow = new Rectangle(
                    SelectedRegion.X - 1, SelectedRegion.Y - 1,
                    SelectedRegion.Width + 2, SelectedRegion.Height + 2);
                g.DrawRectangle(glowPen, glow);
            }

            // Draw resize handles
            DrawHandles(g);

            // Draw dimension label
            string label = $"{SelectedRegion.Width} x {SelectedRegion.Height}";
            var labelSize = g.MeasureString(label, _font);
            int labelX = SelectedRegion.X;
            int labelY = SelectedRegion.Y - (int)labelSize.Height - 8;

            if (labelY < 0)
                labelY = SelectedRegion.Bottom + 8;

            var labelRect = new Rectangle(labelX, labelY, (int)labelSize.Width + 16, (int)labelSize.Height + 6);

            using (var fillBrush = new SolidBrush(Color.FromArgb(200, 0, 0, 0)))
            using (var borderPen = new Pen(Color.FromArgb(100, 255, 255, 255)))
            {
                g.FillRectangle(fillBrush, labelRect);
                g.DrawRectangle(borderPen, labelRect);
            }

            using (var textBrush = new SolidBrush(Color.White))
            {
                g.DrawString(label, _font, textBrush,
                    labelRect.X + 8, labelRect.Y + 3);
            }

            // Instructions at top of screen
            string help = "Drag to select region | Drag edges/corners to resize | Drag inside to move | Enter to confirm | Esc to cancel";
            var helpSize = g.MeasureString(help, _font);
            int helpX = screen.Width / 2 - (int)helpSize.Width / 2;
            using (var helpBg = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
            using (var helpText = new SolidBrush(Color.FromArgb(200, 255, 255, 255)))
            {
                g.FillRectangle(helpBg, helpX - 10, 10, (int)helpSize.Width + 20, (int)helpSize.Height + 10);
                g.DrawString(help, _font, helpText, helpX, 15);
            }
        }

        private void DrawHandles(Graphics g)
        {
            using (var fillBrush = new SolidBrush(HandleColor))
            using (var borderPen = new Pen(Color.White, 1))
            {
                foreach (var rect in GetHandleRects())
                {
                    g.FillRectangle(fillBrush, rect);
                    g.DrawRectangle(borderPen, rect);
                }
            }
        }

        private Rectangle[] GetHandleRects()
        {
            var r = SelectedRegion;
            int hs = HandleSize;
            int half = hs / 2;
            return new[]
            {
                // Corners
                new Rectangle(r.X - half, r.Y - half, hs, hs),           // NW
                new Rectangle(r.Right - half, r.Y - half, hs, hs),       // NE
                new Rectangle(r.X - half, r.Bottom - half, hs, hs),      // SW
                new Rectangle(r.Right - half, r.Bottom - half, hs, hs),  // SE
                // Edges
                new Rectangle(r.X + r.Width/2 - half, r.Y - half, hs, hs),           // N
                new Rectangle(r.X + r.Width/2 - half, r.Bottom - half, hs, hs),       // S
                new Rectangle(r.X - half, r.Y + r.Height/2 - half, hs, hs),           // W
                new Rectangle(r.Right - half, r.Y + r.Height/2 - half, hs, hs),       // E
            };
        }

        private ResizeEdge HitTestEdge(Point p)
        {
            var r = SelectedRegion;
            int hs = HandleSize;
            int half = hs / 2;

            if (new Rectangle(r.X - half, r.Y - half, hs, hs).Contains(p)) return ResizeEdge.NW;
            if (new Rectangle(r.Right - half, r.Y - half, hs, hs).Contains(p)) return ResizeEdge.NE;
            if (new Rectangle(r.X - half, r.Bottom - half, hs, hs).Contains(p)) return ResizeEdge.SW;
            if (new Rectangle(r.Right - half, r.Bottom - half, hs, hs).Contains(p)) return ResizeEdge.SE;
            if (new Rectangle(r.X + r.Width/2 - half, r.Y - half, hs, hs).Contains(p)) return ResizeEdge.N;
            if (new Rectangle(r.X + r.Width/2 - half, r.Bottom - half, hs, hs).Contains(p)) return ResizeEdge.S;
            if (new Rectangle(r.X - half, r.Y + r.Height/2 - half, hs, hs).Contains(p)) return ResizeEdge.W;
            if (new Rectangle(r.Right - half, r.Y + r.Height/2 - half, hs, hs).Contains(p)) return ResizeEdge.E;

            return ResizeEdge.None;
        }

        private Rectangle ApplyResize(Rectangle original, ResizeEdge edge, int dx, int dy)
        {
            int x = original.X, y = original.Y, w = original.Width, h = original.Height;
            int minSize = 50;

            switch (edge)
            {
                case ResizeEdge.NW: x += dx; y += dy; w -= dx; h -= dy; break;
                case ResizeEdge.N: y += dy; h -= dy; break;
                case ResizeEdge.NE: y += dy; w += dx; h -= dy; break;
                case ResizeEdge.E: w += dx; break;
                case ResizeEdge.SE: w += dx; h += dy; break;
                case ResizeEdge.S: h += dy; break;
                case ResizeEdge.SW: x += dx; w -= dx; h += dy; break;
                case ResizeEdge.W: x += dx; w -= dx; break;
            }

            if (w < minSize) { if (x != original.X) x = original.X + original.Width - minSize; w = minSize; }
            if (h < minSize) { if (y != original.Y) y = original.Y + original.Height - minSize; h = minSize; }

            return new Rectangle(x, y, w, h);
        }

        private void ConfirmRegion()
        {
            if (SelectedRegion.Width > 10 && SelectedRegion.Height > 10)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);
            ConfirmRegion();
        }
    }
}
