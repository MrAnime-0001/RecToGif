namespace RecToGif.Forms
{
    partial class EditorForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                _timeline?.Dispose();
                _previewPanel?.Dispose();
                _loopFinderPanel?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._toolStrip = new System.Windows.Forms.ToolStrip();
            this._btnDelete = new System.Windows.Forms.ToolStripButton();
            this._btnDuplicate = new System.Windows.Forms.ToolStripButton();
            this._sep1 = new System.Windows.Forms.ToolStripSeparator();
            this._btnMoveLeft = new System.Windows.Forms.ToolStripButton();
            this._btnMoveRight = new System.Windows.Forms.ToolStripButton();
            this._sep2 = new System.Windows.Forms.ToolStripSeparator();
            this._lblDelay = new System.Windows.Forms.ToolStripLabel();
            this._numDelay = new System.Windows.Forms.NumericUpDown();
            this._hostDelay = new System.Windows.Forms.ToolStripControlHost(this._numDelay);
            this._btnApplyDelay = new System.Windows.Forms.ToolStripButton();
            this._sep3 = new System.Windows.Forms.ToolStripSeparator();
            this._btnFindLoop = new System.Windows.Forms.ToolStripButton();
            this._sep4 = new System.Windows.Forms.ToolStripSeparator();
            this._btnCrop = new System.Windows.Forms.ToolStripButton();
            this._btnApplyCrop = new System.Windows.Forms.ToolStripButton();
            this._sep5 = new System.Windows.Forms.ToolStripSeparator();
            this._lblSize = new System.Windows.Forms.ToolStripLabel();
            this._numWidth = new System.Windows.Forms.NumericUpDown();
            this._hostWidth = new System.Windows.Forms.ToolStripControlHost(this._numWidth);
            this._lblX = new System.Windows.Forms.ToolStripLabel();
            this._numHeight = new System.Windows.Forms.NumericUpDown();
            this._hostHeight = new System.Windows.Forms.ToolStripControlHost(this._numHeight);
            this._btnResize = new System.Windows.Forms.ToolStripButton();
            this._sep6 = new System.Windows.Forms.ToolStripSeparator();
            this._btnExport = new System.Windows.Forms.ToolStripButton();
            this._previewPanel = new RecToGif.Controls.PreviewPanel();
            this._loopFinderPanel = new RecToGif.Controls.LoopFinderPanel();
            this._timeline = new RecToGif.Controls.FrameTimeline();
            this._toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numDelay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numHeight)).BeginInit();
            this.SuspendLayout();

            Color bgDark = Color.FromArgb(30, 30, 30);
            Color bgPanel = Color.FromArgb(45, 45, 45);
            Color fgWhite = Color.FromArgb(220, 220, 220);
            Color fgDim = Color.FromArgb(150, 150, 150);

            //
            // _toolStrip
            //
            this._toolStrip.BackColor = bgPanel;
            this._toolStrip.ForeColor = fgWhite;
            this._toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._btnDelete,
            this._btnDuplicate,
            this._sep1,
            this._btnMoveLeft,
            this._btnMoveRight,
            this._sep2,
            this._lblDelay,
            this._hostDelay,
            this._btnApplyDelay,
            this._sep3,
            this._btnFindLoop,
            this._sep4,
            this._btnCrop,
            this._btnApplyCrop,
            this._sep5,
            this._lblSize,
            this._hostWidth,
            this._lblX,
            this._hostHeight,
            this._btnResize,
            this._sep6,
            this._btnExport});
            this._toolStrip.Location = new System.Drawing.Point(0, 0);
            this._toolStrip.Name = "_toolStrip";
            this._toolStrip.Size = new System.Drawing.Size(1184, 28);
            this._toolStrip.TabIndex = 3;
            this._toolStrip.Text = "toolStrip1";
            //
            // _btnDelete
            //
            this._btnDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnDelete.ForeColor = fgWhite;
            this._btnDelete.Name = "_btnDelete";
            this._btnDelete.Size = new System.Drawing.Size(44, 25);
            this._btnDelete.Text = "Delete";
            this._btnDelete.ToolTipText = "Delete selected frames (Del)";
            //
            // _btnDuplicate
            //
            this._btnDuplicate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnDuplicate.ForeColor = fgWhite;
            this._btnDuplicate.Name = "_btnDuplicate";
            this._btnDuplicate.Size = new System.Drawing.Size(61, 25);
            this._btnDuplicate.Text = "Duplicate";
            this._btnDuplicate.ToolTipText = "Duplicate selected frames (Ctrl+D)";
            //
            // _sep1
            //
            this._sep1.Name = "_sep1";
            this._sep1.Size = new System.Drawing.Size(6, 28);
            //
            // _btnMoveLeft
            //
            this._btnMoveLeft.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnMoveLeft.ForeColor = fgWhite;
            this._btnMoveLeft.Name = "_btnMoveLeft";
            this._btnMoveLeft.Size = new System.Drawing.Size(64, 25);
            this._btnMoveLeft.Text = "Move Left";
            this._btnMoveLeft.ToolTipText = "Move selected frames left (Alt+Left)";
            //
            // _btnMoveRight
            //
            this._btnMoveRight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnMoveRight.ForeColor = fgWhite;
            this._btnMoveRight.Name = "_btnMoveRight";
            this._btnMoveRight.Size = new System.Drawing.Size(73, 25);
            this._btnMoveRight.Text = "Move Right";
            this._btnMoveRight.ToolTipText = "Move selected frames right (Alt+Right)";
            //
            // _sep2
            //
            this._sep2.Name = "_sep2";
            this._sep2.Size = new System.Drawing.Size(6, 28);
            //
            // _lblDelay
            //
            this._lblDelay.ForeColor = fgWhite;
            this._lblDelay.Name = "_lblDelay";
            this._lblDelay.Size = new System.Drawing.Size(64, 25);
            this._lblDelay.Text = "Delay (ms):";
            //
            // _numDelay
            //
            this._numDelay.BackColor = bgDark;
            this._numDelay.ForeColor = fgWhite;
            this._numDelay.Location = new System.Drawing.Point(0, 0);
            this._numDelay.Maximum = new decimal(new int[] { 5000, 0, 0, 0 });
            this._numDelay.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            this._numDelay.Name = "_numDelay";
            this._numDelay.Size = new System.Drawing.Size(60, 23);
            this._numDelay.Value = new decimal(new int[] { 100, 0, 0, 0 });
            //
            // _hostDelay
            //
            this._hostDelay.Name = "_hostDelay";
            this._hostDelay.Size = new System.Drawing.Size(60, 25);
            //
            // _btnApplyDelay
            //
            this._btnApplyDelay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnApplyDelay.ForeColor = fgWhite;
            this._btnApplyDelay.Name = "_btnApplyDelay";
            this._btnApplyDelay.Size = new System.Drawing.Size(42, 25);
            this._btnApplyDelay.Text = "Apply";
            this._btnApplyDelay.ToolTipText = "Apply delay to selected frames";
            //
            // _sep3
            //
            this._sep3.Name = "_sep3";
            this._sep3.Size = new System.Drawing.Size(6, 28);
            //
            // _btnFindLoop
            //
            this._btnFindLoop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnFindLoop.ForeColor = fgWhite;
            this._btnFindLoop.Name = "_btnFindLoop";
            this._btnFindLoop.Size = new System.Drawing.Size(64, 25);
            this._btnFindLoop.Text = "Find Loop";
            this._btnFindLoop.ToolTipText = "Find looping section in frames (Ctrl+L)";
            //
            // _sep4
            //
            this._sep4.Name = "_sep4";
            this._sep4.Size = new System.Drawing.Size(6, 28);
            //
            // _btnCrop
            //
            this._btnCrop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnCrop.ForeColor = fgWhite;
            this._btnCrop.Name = "_btnCrop";
            this._btnCrop.Size = new System.Drawing.Size(37, 25);
            this._btnCrop.Text = "Crop";
            this._btnCrop.ToolTipText = "Toggle crop mode (C)";
            //
            // _btnApplyCrop
            //
            this._btnApplyCrop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnApplyCrop.ForeColor = fgWhite;
            this._btnApplyCrop.Name = "_btnApplyCrop";
            this._btnApplyCrop.Size = new System.Drawing.Size(70, 25);
            this._btnApplyCrop.Text = "Apply Crop";
            this._btnApplyCrop.ToolTipText = "Apply crop selection";
            //
            // _sep5
            //
            this._sep5.Name = "_sep5";
            this._sep5.Size = new System.Drawing.Size(6, 28);
            //
            // _lblSize
            //
            this._lblSize.ForeColor = fgWhite;
            this._lblSize.Name = "_lblSize";
            this._lblSize.Size = new System.Drawing.Size(30, 25);
            this._lblSize.Text = "Size:";
            //
            // _numWidth
            //
            this._numWidth.BackColor = bgDark;
            this._numWidth.ForeColor = fgWhite;
            this._numWidth.Location = new System.Drawing.Point(0, 0);
            this._numWidth.Maximum = new decimal(new int[] { 4096, 0, 0, 0 });
            this._numWidth.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            this._numWidth.Name = "_numWidth";
            this._numWidth.Size = new System.Drawing.Size(50, 23);
            this._numWidth.Value = new decimal(new int[] { 800, 0, 0, 0 });
            //
            // _hostWidth
            //
            this._hostWidth.Name = "_hostWidth";
            this._hostWidth.Size = new System.Drawing.Size(50, 25);
            //
            // _lblX
            //
            this._lblX.ForeColor = fgWhite;
            this._lblX.Name = "_lblX";
            this._lblX.Size = new System.Drawing.Size(13, 25);
            this._lblX.Text = "x";
            //
            // _numHeight
            //
            this._numHeight.BackColor = bgDark;
            this._numHeight.ForeColor = fgWhite;
            this._numHeight.Location = new System.Drawing.Point(0, 0);
            this._numHeight.Maximum = new decimal(new int[] { 4096, 0, 0, 0 });
            this._numHeight.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            this._numHeight.Name = "_numHeight";
            this._numHeight.Size = new System.Drawing.Size(50, 23);
            this._numHeight.Value = new decimal(new int[] { 600, 0, 0, 0 });
            //
            // _hostHeight
            //
            this._hostHeight.Name = "_hostHeight";
            this._hostHeight.Size = new System.Drawing.Size(50, 25);
            //
            // _btnResize
            //
            this._btnResize.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnResize.ForeColor = fgWhite;
            this._btnResize.Name = "_btnResize";
            this._btnResize.Size = new System.Drawing.Size(43, 25);
            this._btnResize.Text = "Resize";
            this._btnResize.ToolTipText = "Resize output (Ctrl+R)";
            //
            // _sep6
            //
            this._sep6.Name = "_sep6";
            this._sep6.Size = new System.Drawing.Size(6, 28);
            //
            // _btnExport
            //
            this._btnExport.BackColor = Color.FromArgb(0, 120, 200);
            this._btnExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnExport.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this._btnExport.ForeColor = System.Drawing.Color.White;
            this._btnExport.Name = "_btnExport";
            this._btnExport.Size = new System.Drawing.Size(45, 25);
            this._btnExport.Text = "Export";
            this._btnExport.ToolTipText = "Export GIF or MP4 (Ctrl+E)";
            //
            // _previewPanel
            //
            this._previewPanel.BackColor = bgDark;
            this._previewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._previewPanel.Location = new System.Drawing.Point(0, 28);
            this._previewPanel.Name = "_previewPanel";
            this._previewPanel.Size = new System.Drawing.Size(884, 606);
            this._previewPanel.TabIndex = 0;
            //
            // _loopFinderPanel
            //
            this._loopFinderPanel.BackColor = bgPanel;
            this._loopFinderPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this._loopFinderPanel.Location = new System.Drawing.Point(884, 28);
            this._loopFinderPanel.Name = "_loopFinderPanel";
            this._loopFinderPanel.Size = new System.Drawing.Size(300, 606);
            this._loopFinderPanel.TabIndex = 1;
            this._loopFinderPanel.Visible = false;
            //
            // _timeline
            //
            this._timeline.BackColor = bgDark;
            this._timeline.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._timeline.Location = new System.Drawing.Point(0, 634);
            this._timeline.Name = "_timeline";
            this._timeline.Size = new System.Drawing.Size(1184, 127);
            this._timeline.TabIndex = 2;
            //
            // EditorForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = bgDark;
            this.ForeColor = fgWhite;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ClientSize = new System.Drawing.Size(1184, 761);
            this.Controls.Add(this._previewPanel);
            this.Controls.Add(this._loopFinderPanel);
            this.Controls.Add(this._toolStrip);
            this.Controls.Add(this._timeline);
            this.Name = "EditorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "RecToGif Editor";
            this._toolStrip.ResumeLayout(false);
            this._toolStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numDelay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numHeight)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private RecToGif.Controls.PreviewPanel _previewPanel;
        private RecToGif.Controls.LoopFinderPanel _loopFinderPanel;
        private RecToGif.Controls.FrameTimeline _timeline;
        private System.Windows.Forms.ToolStrip _toolStrip;
        private System.Windows.Forms.ToolStripButton _btnDelete;
        private System.Windows.Forms.ToolStripButton _btnDuplicate;
        private System.Windows.Forms.ToolStripSeparator _sep1;
        private System.Windows.Forms.ToolStripButton _btnMoveLeft;
        private System.Windows.Forms.ToolStripButton _btnMoveRight;
        private System.Windows.Forms.ToolStripSeparator _sep2;
        private System.Windows.Forms.ToolStripLabel _lblDelay;
        private System.Windows.Forms.NumericUpDown _numDelay;
        private System.Windows.Forms.ToolStripControlHost _hostDelay;
        private System.Windows.Forms.ToolStripButton _btnApplyDelay;
        private System.Windows.Forms.ToolStripSeparator _sep3;
        private System.Windows.Forms.ToolStripButton _btnFindLoop;
        private System.Windows.Forms.ToolStripSeparator _sep4;
        private System.Windows.Forms.ToolStripButton _btnCrop;
        private System.Windows.Forms.ToolStripButton _btnApplyCrop;
        private System.Windows.Forms.ToolStripSeparator _sep5;
        private System.Windows.Forms.ToolStripLabel _lblSize;
        private System.Windows.Forms.NumericUpDown _numWidth;
        private System.Windows.Forms.ToolStripControlHost _hostWidth;
        private System.Windows.Forms.ToolStripLabel _lblX;
        private System.Windows.Forms.NumericUpDown _numHeight;
        private System.Windows.Forms.ToolStripControlHost _hostHeight;
        private System.Windows.Forms.ToolStripButton _btnResize;
        private System.Windows.Forms.ToolStripSeparator _sep6 = null!;
        private System.Windows.Forms.ToolStripButton _btnExport = null!;
    }
}
