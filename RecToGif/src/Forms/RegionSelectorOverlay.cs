using System;
using System.Drawing;
using System.Windows.Forms;

namespace RecToGif.Forms
{
    public sealed partial class RegionSelectorOverlay : Form
    {
        private Point _startPos;
        private bool _isSelecting = false;
        public Rectangle SelectedRegion { get; private set; }
        private readonly Font _font = new Font("Segoe UI", 12, FontStyle.Bold);

        public RegionSelectorOverlay()
        {
            InitializeComponent();
            
            // Cover all screens
            this.Bounds = SystemInformation.VirtualScreen;
            
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            this.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    _isSelecting = true;
                    _startPos = e.Location;
                }
            };

            this.MouseMove += (s, e) =>
            {
                if (_isSelecting)
                {
                    SelectedRegion = GetRectangle(_startPos, e.Location);
                    this.Invalidate();
                }
            };

            this.MouseUp += (s, e) =>
            {
                if (_isSelecting)
                {
                    _isSelecting = false;
                    if (SelectedRegion.Width > 10 && SelectedRegion.Height > 10)
                    {
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
            };

            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_isSelecting || !SelectedRegion.IsEmpty)
            {
                // For now, just draw the rectangle and dimensions
                using var pen = new Pen(Color.Red, 2);
                e.Graphics.DrawRectangle(pen, SelectedRegion);

                string text = $"{SelectedRegion.Width} x {SelectedRegion.Height}";
                var size = e.Graphics.MeasureString(text, _font);
                
                var textRect = new Rectangle(
                    SelectedRegion.X, 
                    SelectedRegion.Y - (int)size.Height - 5, 
                    (int)size.Width + 10, 
                    (int)size.Height + 5);
                
                if (textRect.Y < 0) textRect.Y = SelectedRegion.Bottom + 5;

                e.Graphics.FillRectangle(Brushes.Red, textRect);
                e.Graphics.DrawString(text, _font, Brushes.White, textRect.X + 5, textRect.Y + 2);
            }
        }

        private Rectangle GetRectangle(Point p1, Point p2)
        {
            return new Rectangle(
                Math.Min(p1.X, p2.X),
                Math.Min(p1.Y, p2.Y),
                Math.Abs(p1.X - p2.X),
                Math.Abs(p1.Y - p2.Y));
        }
    }
}
