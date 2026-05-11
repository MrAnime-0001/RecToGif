namespace RecToGif.Forms
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._tabControl = new System.Windows.Forms.TabControl();
            this._captureTab = new System.Windows.Forms.TabPage();
            this._numFps = new System.Windows.Forms.NumericUpDown();
            this._lblFps = new System.Windows.Forms.Label();
            this._toolsTab = new System.Windows.Forms.TabPage();
            this._btnBrowseFfmpeg = new System.Windows.Forms.Button();
            this._txtFfmpeg = new System.Windows.Forms.TextBox();
            this._lblFfmpeg = new System.Windows.Forms.Label();
            this._btnBrowseGifski = new System.Windows.Forms.Button();
            this._txtGifski = new System.Windows.Forms.TextBox();
            this._lblGifski = new System.Windows.Forms.Label();
            this._btnSave = new System.Windows.Forms.Button();
            this._hotkeysTab = new System.Windows.Forms.TabPage();
            this._btnResetHotkeys = new System.Windows.Forms.Button();
            this._hotkeysTable = new System.Windows.Forms.TableLayoutPanel();
            this._tabControl.SuspendLayout();
            this._captureTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numFps)).BeginInit();
            this._toolsTab.SuspendLayout();
            this._hotkeysTab.SuspendLayout();
            this.SuspendLayout();

            Color bgDark = Color.FromArgb(30, 30, 30);
            Color bgPanel = Color.FromArgb(45, 45, 45);
            Color fgWhite = Color.FromArgb(220, 220, 220);
            Color fgDim = Color.FromArgb(150, 150, 150);

            //
            // _chkCaptureCursor
            //
            this._chkCaptureCursor = new System.Windows.Forms.CheckBox();
            this._chkCaptureCursor.AutoSize = true;
            this._chkCaptureCursor.ForeColor = fgWhite;
            this._chkCaptureCursor.Location = new System.Drawing.Point(10, 40);
            this._chkCaptureCursor.Name = "_chkCaptureCursor";
            this._chkCaptureCursor.Size = new System.Drawing.Size(220, 19);
            this._chkCaptureCursor.TabIndex = 2;
            this._chkCaptureCursor.Text = "Capture mouse cursor during recording";
            this._chkCaptureCursor.UseVisualStyleBackColor = false;
            //
            // _tabControl
            //
            this._tabControl.BackColor = bgPanel;
            this._tabControl.Controls.Add(this._captureTab);
            this._tabControl.Controls.Add(this._toolsTab);
            this._tabControl.Controls.Add(this._hotkeysTab);
            this._tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tabControl.ForeColor = fgWhite;
            this._tabControl.Location = new System.Drawing.Point(0, 0);
            this._tabControl.Name = "_tabControl";
            this._tabControl.SelectedIndex = 0;
            this._tabControl.Size = new System.Drawing.Size(464, 251);
            this._tabControl.TabIndex = 0;
            //
            // _captureTab
            //
            this._captureTab.BackColor = bgDark;
            this._captureTab.Controls.Add(this._chkCaptureCursor);
            this._captureTab.Controls.Add(this._numFps);
            this._captureTab.Controls.Add(this._lblFps);
            this._captureTab.ForeColor = fgWhite;
            this._captureTab.Location = new System.Drawing.Point(4, 24);
            this._captureTab.Name = "_captureTab";
            this._captureTab.Padding = new System.Windows.Forms.Padding(3);
            this._captureTab.Size = new System.Drawing.Size(406, 223);
            this._captureTab.TabIndex = 0;
            this._captureTab.Text = "Capture";
            //
            // _numFps
            //
            this._numFps.BackColor = Color.FromArgb(60, 60, 60);
            this._numFps.ForeColor = fgWhite;
            this._numFps.Location = new System.Drawing.Point(150, 10);
            this._numFps.Name = "_numFps";
            this._numFps.Size = new System.Drawing.Size(120, 23);
            this._numFps.TabIndex = 1;
            //
            // _lblFps
            //
            this._lblFps.AutoSize = true;
            this._lblFps.ForeColor = fgWhite;
            this._lblFps.Location = new System.Drawing.Point(10, 14);
            this._lblFps.Name = "_lblFps";
            this._lblFps.Size = new System.Drawing.Size(75, 15);
            this._lblFps.TabIndex = 0;
            this._lblFps.Text = "Capture FPS:";
            //
            // _toolsTab
            //
            this._toolsTab.BackColor = bgDark;
            this._toolsTab.Controls.Add(this._btnBrowseFfmpeg);
            this._toolsTab.Controls.Add(this._txtFfmpeg);
            this._toolsTab.Controls.Add(this._lblFfmpeg);
            this._toolsTab.Controls.Add(this._btnBrowseGifski);
            this._toolsTab.Controls.Add(this._txtGifski);
            this._toolsTab.Controls.Add(this._lblGifski);
            this._toolsTab.ForeColor = fgWhite;
            this._toolsTab.Location = new System.Drawing.Point(4, 24);
            this._toolsTab.Name = "_toolsTab";
            this._toolsTab.Padding = new System.Windows.Forms.Padding(3);
            this._toolsTab.Size = new System.Drawing.Size(406, 223);
            this._toolsTab.TabIndex = 1;
            this._toolsTab.Text = "External Tools";
            //
            // _btnBrowseFfmpeg
            //
            this._btnBrowseFfmpeg.BackColor = bgPanel;
            this._btnBrowseFfmpeg.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnBrowseFfmpeg.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 60);
            this._btnBrowseFfmpeg.ForeColor = fgWhite;
            this._btnBrowseFfmpeg.Location = new System.Drawing.Point(335, 39);
            this._btnBrowseFfmpeg.Name = "_btnBrowseFfmpeg";
            this._btnBrowseFfmpeg.Size = new System.Drawing.Size(60, 23);
            this._btnBrowseFfmpeg.TabIndex = 5;
            this._btnBrowseFfmpeg.Text = "Browse";
            this._btnBrowseFfmpeg.UseVisualStyleBackColor = false;
            this._btnBrowseFfmpeg.Cursor = System.Windows.Forms.Cursors.Hand;
            //
            // _txtFfmpeg
            //
            this._txtFfmpeg.BackColor = Color.FromArgb(60, 60, 60);
            this._txtFfmpeg.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._txtFfmpeg.ForeColor = fgWhite;
            this._txtFfmpeg.Location = new System.Drawing.Point(130, 40);
            this._txtFfmpeg.Name = "_txtFfmpeg";
            this._txtFfmpeg.Size = new System.Drawing.Size(200, 23);
            this._txtFfmpeg.TabIndex = 4;
            //
            // _lblFfmpeg
            //
            this._lblFfmpeg.AutoSize = true;
            this._lblFfmpeg.ForeColor = fgWhite;
            this._lblFfmpeg.Location = new System.Drawing.Point(10, 44);
            this._lblFfmpeg.Name = "_lblFfmpeg";
            this._lblFfmpeg.Size = new System.Drawing.Size(95, 15);
            this._lblFfmpeg.TabIndex = 3;
            this._lblFfmpeg.Text = "ffmpeg.exe path:";
            //
            // _btnBrowseGifski
            //
            this._btnBrowseGifski.BackColor = bgPanel;
            this._btnBrowseGifski.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnBrowseGifski.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 60);
            this._btnBrowseGifski.ForeColor = fgWhite;
            this._btnBrowseGifski.Location = new System.Drawing.Point(335, 9);
            this._btnBrowseGifski.Name = "_btnBrowseGifski";
            this._btnBrowseGifski.Size = new System.Drawing.Size(60, 23);
            this._btnBrowseGifski.TabIndex = 2;
            this._btnBrowseGifski.Text = "Browse";
            this._btnBrowseGifski.UseVisualStyleBackColor = false;
            this._btnBrowseGifski.Cursor = System.Windows.Forms.Cursors.Hand;
            //
            // _txtGifski
            //
            this._txtGifski.BackColor = Color.FromArgb(60, 60, 60);
            this._txtGifski.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._txtGifski.ForeColor = fgWhite;
            this._txtGifski.Location = new System.Drawing.Point(130, 10);
            this._txtGifski.Name = "_txtGifski";
            this._txtGifski.Size = new System.Drawing.Size(200, 23);
            this._txtGifski.TabIndex = 1;
            //
            // _lblGifski
            //
            this._lblGifski.AutoSize = true;
            this._lblGifski.ForeColor = fgWhite;
            this._lblGifski.Location = new System.Drawing.Point(10, 14);
            this._lblGifski.Name = "_lblGifski";
            this._lblGifski.Size = new System.Drawing.Size(84, 15);
            this._lblGifski.TabIndex = 0;
            this._lblGifski.Text = "gifski.exe path:";
            //
            // _hotkeysTab
            //
            this._hotkeysTab.BackColor = bgDark;
            this._hotkeysTab.Controls.Add(this._btnResetHotkeys);
            this._hotkeysTab.Controls.Add(this._hotkeysTable);
            this._hotkeysTab.ForeColor = fgWhite;
            this._hotkeysTab.Location = new System.Drawing.Point(4, 24);
            this._hotkeysTab.Name = "_hotkeysTab";
            this._hotkeysTab.Padding = new System.Windows.Forms.Padding(3);
            this._hotkeysTab.Size = new System.Drawing.Size(456, 223);
            this._hotkeysTab.TabIndex = 2;
            this._hotkeysTab.Text = "Hotkeys";
            //
            // _hotkeysTable
            //
            this._hotkeysTable.BackColor = bgDark;
            this._hotkeysTable.ColumnCount = 2;
            this._hotkeysTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160));
            this._hotkeysTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120));
            this._hotkeysTable.Dock = System.Windows.Forms.DockStyle.Top;
            this._hotkeysTable.ForeColor = fgWhite;
            this._hotkeysTable.Location = new System.Drawing.Point(3, 3);
            this._hotkeysTable.Name = "_hotkeysTable";
            this._hotkeysTable.Padding = new System.Windows.Forms.Padding(5);
            this._hotkeysTable.RowCount = 6;
            this._hotkeysTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30));
            this._hotkeysTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30));
            this._hotkeysTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30));
            this._hotkeysTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30));
            this._hotkeysTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30));
            this._hotkeysTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30));
            this._hotkeysTable.Size = new System.Drawing.Size(450, 180);
            this._hotkeysTable.TabIndex = 0;
            //
            // _btnResetHotkeys
            //
            this._btnResetHotkeys.BackColor = bgPanel;
            this._btnResetHotkeys.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnResetHotkeys.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 60);
            this._btnResetHotkeys.ForeColor = fgWhite;
            this._btnResetHotkeys.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._btnResetHotkeys.Location = new System.Drawing.Point(3, 192);
            this._btnResetHotkeys.Name = "_btnResetHotkeys";
            this._btnResetHotkeys.Size = new System.Drawing.Size(450, 24);
            this._btnResetHotkeys.TabIndex = 2;
            this._btnResetHotkeys.Text = "Reset to Defaults";
            this._btnResetHotkeys.UseVisualStyleBackColor = false;
            this._btnResetHotkeys.Cursor = System.Windows.Forms.Cursors.Hand;
            //
            // _btnSave
            //
            this._btnSave.BackColor = Color.FromArgb(0, 120, 200);
            this._btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnSave.FlatAppearance.BorderColor = Color.FromArgb(0, 140, 220);
            this._btnSave.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this._btnSave.ForeColor = System.Drawing.Color.White;
            this._btnSave.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._btnSave.Location = new System.Drawing.Point(0, 251);
            this._btnSave.Name = "_btnSave";
            this._btnSave.Size = new System.Drawing.Size(464, 30);
            this._btnSave.TabIndex = 1;
            this._btnSave.Text = "Save";
            this._btnSave.UseVisualStyleBackColor = false;
            this._btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            //
            // SettingsForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = bgDark;
            this.ForeColor = fgWhite;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ClientSize = new System.Drawing.Size(464, 281);
            this.Controls.Add(this._tabControl);
            this.Controls.Add(this._btnSave);
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this._tabControl.ResumeLayout(false);
            this._captureTab.ResumeLayout(false);
            this._captureTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numFps)).EndInit();
            this._toolsTab.ResumeLayout(false);
            this._toolsTab.PerformLayout();
            this._hotkeysTab.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.TabControl _tabControl;
        private System.Windows.Forms.TabPage _captureTab;
        private System.Windows.Forms.TabPage _toolsTab;
        private System.Windows.Forms.TabPage _hotkeysTab;
        private System.Windows.Forms.TableLayoutPanel _hotkeysTable;
        private System.Windows.Forms.Button _btnResetHotkeys;
        private System.Windows.Forms.Label _lblFps;
        private System.Windows.Forms.NumericUpDown _numFps;
        private System.Windows.Forms.Label _lblGifski;
        private System.Windows.Forms.TextBox _txtGifski;
        private System.Windows.Forms.Button _btnBrowseGifski;
        private System.Windows.Forms.Label _lblFfmpeg;
        private System.Windows.Forms.TextBox _txtFfmpeg;
        private System.Windows.Forms.Button _btnBrowseFfmpeg;
        private System.Windows.Forms.Button _btnSave;
        private System.Windows.Forms.CheckBox _chkCaptureCursor;
    }
}
