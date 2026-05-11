using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using RecToGif.Editor;
using RecToGif.Controls;

namespace RecToGif.Controls
{
    public class PreviewPanel : UserControl
    {
        private FrameItem? _currentFrame;
        private Image? _currentImage;
        private readonly InputOverlayRenderer _overlayRenderer = new();
        private bool _showOverlay = true;
        private bool _isPlaying = false;
        private Image? _cachedWatermark;
        private string _cachedWatermarkPath = string.Empty;
        
        // Crop State
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public bool IsCropping { get; set; }
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Rectangle CropRegion { get; set; }
        private int _draggingHandle = -1;
        private Point _lastMousePos;

        // Overlays
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public List<VisualOverlay> Overlays { get; set; } = new();

        // Border
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Color BorderColor { get; set; } = Color.Transparent;
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int BorderThickness { get; set; } = 0;

        public PreviewPanel()
        {
            this.DoubleBuffered = true;
            this.BackColor = Color.FromArgb(30, 30, 30);
            
            this.MouseDown += OnMouseDown;
            this.MouseMove += OnMouseMove;
            this.MouseUp += OnMouseUp;
        }

        public void DisplayFrame(FrameItem frame)
        {
            _currentFrame = frame;
            _currentImage?.Dispose();

            if (File.Exists(frame.ImagePath))
            {
                _currentImage = new Bitmap(frame.ImagePath);
            }
            else
            {
                _currentImage = null;
            }

            this.Invalidate();
        }

        public void SetPlaybackMode(bool playing)
        {
            _isPlaying = playing;
            this.Invalidate();
        }

        public Rectangle ApplyCrop()
        {
            if (CropRegion.IsEmpty && _currentImage != null)
            {
                return new Rectangle(0, 0, _currentImage.Width, _currentImage.Height);
            }
            return CropRegion;
        }

        private void OnMouseDown(object? sender, MouseEventArgs e)
        {
            if (!IsCropping || _currentImage == null) return;
            var screenCrop = MapToScreen(CropRegion);
            _draggingHandle = GetHandleAt(e.Location, screenCrop);
            _lastMousePos = e.Location;
        }

        private void OnMouseMove(object? sender, MouseEventArgs e)
        {
            if (!IsCropping || _currentImage == null) return;

            if (_draggingHandle != -1)
            {
                var dx = e.X - _lastMousePos.X;
                var dy = e.Y - _lastMousePos.Y;
                float ratio = GetScaleRatio();
                int idx = (int)(dx / ratio);
                int idy = (int)(dy / ratio);
                UpdateCrop(idx, idy);
                _lastMousePos = e.Location;
                this.Invalidate();
            }
            else
            {
                var screenCrop = MapToScreen(CropRegion);
                var handle = GetHandleAt(e.Location, screenCrop);
                this.Cursor = handle switch
                {
                    0 or 2 => Cursors.SizeNWSE,
                    1 or 3 => Cursors.SizeNESW,
                    4 => Cursors.SizeAll,
                    _ => Cursors.Default
                };
            }
        }

        private void OnMouseUp(object? sender, MouseEventArgs e)
        {
            _draggingHandle = -1;
        }

        private int GetHandleAt(Point p, Rectangle rect)
        {
            int hSize = 8;
            if (new Rectangle(rect.X - hSize, rect.Y - hSize, hSize * 2, hSize * 2).Contains(p)) return 0;
            if (new Rectangle(rect.Right - hSize, rect.Y - hSize, hSize * 2, hSize * 2).Contains(p)) return 1;
            if (new Rectangle(rect.Right - hSize, rect.Bottom - hSize, hSize * 2, hSize * 2).Contains(p)) return 2;
            if (new Rectangle(rect.X - hSize, rect.Bottom - hSize, hSize * 2, hSize * 2).Contains(p)) return 3;
            if (rect.Contains(p)) return 4;
            return -1;
        }

        private void UpdateCrop(int dx, int dy)
        {
            var r = CropRegion;
            switch (_draggingHandle)
            {
                case 0: r.X += dx; r.Width -= dx; r.Y += dy; r.Height -= dy; break;
                case 1: r.Width += dx; r.Y += dy; r.Height -= dy; break;
                case 2: r.Width += dx; r.Height += dy; break;
                case 3: r.X += dx; r.Width -= dx; r.Height += dy; break;
                case 4: r.X += dx; r.Y += dy; break;
            }
            if (r.Width < 10) r.Width = 10;
            if (r.Height < 10) r.Height = 10;
            if (r.X < 0) r.X = 0;
            if (r.Y < 0) r.Y = 0;
            if (_currentImage != null)
            {
                if (r.Right > _currentImage.Width) r.X = _currentImage.Width - r.Width;
                if (r.Bottom > _currentImage.Height) r.Y = _currentImage.Height - r.Height;
            }
            CropRegion = r;
        }

        private float GetScaleRatio()
        {
            if (_currentImage == null) return 1.0f;
            return Math.Min((float)this.Width / _currentImage.Width, (float)this.Height / _currentImage.Height);
        }

        private Rectangle MapToScreen(Rectangle imageRect)
        {
            if (_currentImage == null) return Rectangle.Empty;
            float ratio = GetScaleRatio();
            int w = (int)(_currentImage.Width * ratio);
            int h = (int)(_currentImage.Height * ratio);
            int offsetX = (this.Width - w) / 2;
            int offsetY = (this.Height - h) / 2;
            return new Rectangle(
                (int)(imageRect.X * ratio) + offsetX,
                (int)(imageRect.Y * ratio) + offsetY,
                (int)(imageRect.Width * ratio),
                (int)(imageRect.Height * ratio));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_currentImage == null) return;

            float ratio = GetScaleRatio();
            int w = (int)(_currentImage.Width * ratio);
            int h = (int)(_currentImage.Height * ratio);
            int x = (this.Width - w) / 2;
            int y = (this.Height - h) / 2;

            var destRect = new Rectangle(x, y, w, h);
            e.Graphics.InterpolationMode = _isPlaying
                ? System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear
                : System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            e.Graphics.DrawImage(_currentImage, destRect);

            if (_showOverlay && _currentFrame != null)
            {
                var state = e.Graphics.Save();
                e.Graphics.TranslateTransform(x, y);
                e.Graphics.ScaleTransform(ratio, ratio);
                _overlayRenderer.Render(e.Graphics, _currentFrame.Metadata.InputEvents, _currentImage.Size);
                e.Graphics.Restore(state);
            }

            if (IsCropping)
            {
                var screenCrop = MapToScreen(CropRegion);
                using (var pen = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawRectangle(pen, screenCrop);
                    int hSize = 4;
                    e.Graphics.FillRectangle(Brushes.White, screenCrop.X - hSize, screenCrop.Y - hSize, hSize * 2, hSize * 2);
                    e.Graphics.FillRectangle(Brushes.White, screenCrop.Right - hSize, screenCrop.Y - hSize, hSize * 2, hSize * 2);
                    e.Graphics.FillRectangle(Brushes.White, screenCrop.Right - hSize, screenCrop.Bottom - hSize, hSize * 2, hSize * 2);
                    e.Graphics.FillRectangle(Brushes.White, screenCrop.X - hSize, screenCrop.Bottom - hSize, hSize * 2, hSize * 2);
                }
            }

            foreach (var overlay in Overlays)
            {
                DrawOverlay(e.Graphics, overlay, ratio, x, y);
            }

            if (BorderThickness > 0 && BorderColor != Color.Transparent)
            {
                using (var pen = new Pen(BorderColor, BorderThickness * ratio))
                {
                    pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Inset;
                    e.Graphics.DrawRectangle(pen, destRect);
                }
            }
        }

        private void DrawOverlay(Graphics g, VisualOverlay overlay, float ratio, int offsetX, int offsetY)
        {
            var screenBounds = new Rectangle(
                (int)(overlay.Bounds.X * ratio) + offsetX,
                (int)(overlay.Bounds.Y * ratio) + offsetY,
                (int)(overlay.Bounds.Width * ratio),
                (int)(overlay.Bounds.Height * ratio));

            if (overlay.Type == OverlayType.Text)
            {
                using (var brush = new SolidBrush(Color.FromArgb((int)(overlay.Opacity * 255), overlay.Color)))
                using (var font = overlay.ToFont())
                {
                    g.DrawString(overlay.Content, font, brush, screenBounds.Location);
                }
            }
            else if (overlay.Type == OverlayType.Watermark)
            {
                if (string.IsNullOrEmpty(overlay.Content) || !File.Exists(overlay.Content)) return;

                if (_cachedWatermark == null || _cachedWatermarkPath != overlay.Content)
                {
                    _cachedWatermark?.Dispose();
                    var fs = new FileStream(overlay.Content, FileMode.Open, FileAccess.Read, FileShare.Read);
                    _cachedWatermark = Image.FromStream(fs);
                    _cachedWatermarkPath = overlay.Content;
                }

                var cm = new System.Drawing.Imaging.ColorMatrix { Matrix33 = overlay.Opacity };
                var ia = new System.Drawing.Imaging.ImageAttributes();
                ia.SetColorMatrix(cm);
                g.DrawImage(_cachedWatermark, screenBounds, 0, 0, _cachedWatermark.Width, _cachedWatermark.Height, GraphicsUnit.Pixel, ia);
            }
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public bool ShowOverlay
        {
            get => _showOverlay;
            set { _showOverlay = value; this.Invalidate(); }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _currentImage?.Dispose();
                _cachedWatermark?.Dispose();
                _overlayRenderer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
