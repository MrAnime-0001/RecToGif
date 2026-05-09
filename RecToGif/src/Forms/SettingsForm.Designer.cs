namespace RecToGif.Forms
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
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
            this._hotkeysTable = new System.Windows.Forms.TableLayoutPanel();
            this._hotkeyCaptureLabel = new System.Windows.Forms.Label();
            this._btnResetHotkeys = new System.Windows.Forms.Button();
            this._tabControl.SuspendLayout();
            this._captureTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numFps)).BeginInit();
            this._toolsTab.SuspendLayout();
            this._hotkeysTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // _tabControl
            // 
            this._tabControl.Controls.Add(this._captureTab);
            this._tabControl.Controls.Add(this._toolsTab);
            this._tabControl.Controls.Add(this._hotkeysTab);
            this._tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tabControl.Location = new System.Drawing.Point(0, 0);
            this._tabControl.Name = "_tabControl";
            this._tabControl.SelectedIndex = 0;
            this._tabControl.Size = new System.Drawing.Size(464, 251);
            this._tabControl.TabIndex = 0;
            // 
            // _captureTab
            // 
            this._captureTab.Controls.Add(this._numFps);
            this._captureTab.Controls.Add(this._lblFps);
            this._captureTab.Location = new System.Drawing.Point(4, 24);
            this._captureTab.Name = "_captureTab";
            this._captureTab.Padding = new System.Windows.Forms.Padding(3);
            this._captureTab.Size = new System.Drawing.Size(406, 223);
            this._captureTab.TabIndex = 0;
            this._captureTab.Text = "Capture";
            this._captureTab.UseVisualStyleBackColor = true;
            // 
            // _numFps
            // 
            this._numFps.Location = new System.Drawing.Point(150, 10);
            this._numFps.Name = "_numFps";
            this._numFps.Size = new System.Drawing.Size(120, 23);
            this._numFps.TabIndex = 1;
            // 
            // _lblFps
            // 
            this._lblFps.AutoSize = true;
            this._lblFps.Location = new System.Drawing.Point(10, 10);
            this._lblFps.Name = "_lblFps";
            this._lblFps.Size = new System.Drawing.Size(75, 15);
            this._lblFps.TabIndex = 0;
            this._lblFps.Text = "Capture FPS:";
            // 
            // _toolsTab
            // 
            this._toolsTab.Controls.Add(this._btnBrowseFfmpeg);
            this._toolsTab.Controls.Add(this._txtFfmpeg);
            this._toolsTab.Controls.Add(this._lblFfmpeg);
            this._toolsTab.Controls.Add(this._btnBrowseGifski);
            this._toolsTab.Controls.Add(this._txtGifski);
            this._toolsTab.Controls.Add(this._lblGifski);
            this._toolsTab.Location = new System.Drawing.Point(4, 24);
            this._toolsTab.Name = "_toolsTab";
            this._toolsTab.Padding = new System.Windows.Forms.Padding(3);
            this._toolsTab.Size = new System.Drawing.Size(406, 223);
            this._toolsTab.TabIndex = 1;
            this._toolsTab.Text = "External Tools";
            this._toolsTab.UseVisualStyleBackColor = true;
            // 
            // _btnBrowseFfmpeg
            // 
            this._btnBrowseFfmpeg.Location = new System.Drawing.Point(335, 39);
            this._btnBrowseFfmpeg.Name = "_btnBrowseFfmpeg";
            this._btnBrowseFfmpeg.Size = new System.Drawing.Size(60, 23);
            this._btnBrowseFfmpeg.TabIndex = 5;
            this._btnBrowseFfmpeg.Text = "Browse";
            this._btnBrowseFfmpeg.UseVisualStyleBackColor = true;
            // 
            // _txtFfmpeg
            // 
            this._txtFfmpeg.Location = new System.Drawing.Point(130, 40);
            this._txtFfmpeg.Name = "_txtFfmpeg";
            this._txtFfmpeg.Size = new System.Drawing.Size(200, 23);
            this._txtFfmpeg.TabIndex = 4;
            // 
            // _lblFfmpeg
            // 
            this._lblFfmpeg.AutoSize = true;
            this._lblFfmpeg.Location = new System.Drawing.Point(10, 44);
            this._lblFfmpeg.Name = "_lblFfmpeg";
            this._lblFfmpeg.Size = new System.Drawing.Size(95, 15);
            this._lblFfmpeg.TabIndex = 3;
            this._lblFfmpeg.Text = "ffmpeg.exe path:";
            // 
            // _btnBrowseGifski
            // 
            this._btnBrowseGifski.Location = new System.Drawing.Point(335, 9);
            this._btnBrowseGifski.Name = "_btnBrowseGifski";
            this._btnBrowseGifski.Size = new System.Drawing.Size(60, 23);
            this._btnBrowseGifski.TabIndex = 2;
            this._btnBrowseGifski.Text = "Browse";
            this._btnBrowseGifski.UseVisualStyleBackColor = true;
            // 
            // _txtGifski
            // 
            this._txtGifski.Location = new System.Drawing.Point(130, 10);
            this._txtGifski.Name = "_txtGifski";
            this._txtGifski.Size = new System.Drawing.Size(200, 23);
            this._txtGifski.TabIndex = 1;
            // 
            // _lblGifski
            // 
            this._lblGifski.AutoSize = true;
            this._lblGifski.Location = new System.Drawing.Point(10, 14);
            this._lblGifski.Name = "_lblGifski";
            this._lblGifski.Size = new System.Drawing.Size(84, 15);
            this._lblGifski.TabIndex = 0;
            this._lblGifski.Text = "gifski.exe path:";
            // 
            // 
            // _hotkeysTab
            // 
            this._hotkeysTab.Controls.Add(this._btnResetHotkeys);
            this._hotkeysTab.Controls.Add(this._hotkeysTable);
            this._hotkeysTab.Location = new System.Drawing.Point(4, 24);
            this._hotkeysTab.Name = "_hotkeysTab";
            this._hotkeysTab.Padding = new System.Windows.Forms.Padding(3);
            this._hotkeysTab.Size = new System.Drawing.Size(456, 223);
            this._hotkeysTab.TabIndex = 2;
            this._hotkeysTab.Text = "Hotkeys";
            this._hotkeysTab.UseVisualStyleBackColor = true;
            // 
            // _hotkeysTable
            // 
            this._hotkeysTable.ColumnCount = 2;
            this._hotkeysTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160));
            this._hotkeysTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120));
            this._hotkeysTable.Dock = System.Windows.Forms.DockStyle.Top;
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
            // _hotkeyCaptureLabel
            // 
            this._hotkeyCaptureLabel.AutoSize = true;
            this._hotkeyCaptureLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._hotkeyCaptureLabel.Location = new System.Drawing.Point(8, 205);
            this._hotkeyCaptureLabel.Name = "_hotkeyCaptureLabel";
            this._hotkeyCaptureLabel.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this._hotkeyCaptureLabel.Size = new System.Drawing.Size(446, 18);
            this._hotkeyCaptureLabel.TabIndex = 1;
            this._hotkeyCaptureLabel.Text = "Click a text box, then press a key to rebind. Right-click to clear. Reset restores defaults.";
            this._hotkeyCaptureLabel.ForeColor = System.Drawing.Color.Gray;
            // 
            // _btnResetHotkeys
            // 
            this._btnResetHotkeys.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._btnResetHotkeys.Location = new System.Drawing.Point(3, 192);
            this._btnResetHotkeys.Name = "_btnResetHotkeys";
            this._btnResetHotkeys.Size = new System.Drawing.Size(450, 24);
            this._btnResetHotkeys.TabIndex = 2;
            this._btnResetHotkeys.Text = "Reset to Defaults";
            this._btnResetHotkeys.UseVisualStyleBackColor = true;
            // 
            // _btnSave
            // 
            this._btnSave.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._btnSave.Location = new System.Drawing.Point(0, 251);
            this._btnSave.Name = "_btnSave";
            this._btnSave.Size = new System.Drawing.Size(464, 30);
            this._btnSave.TabIndex = 1;
            this._btnSave.Text = "Save";
            this._btnSave.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(464, 281);
            this.Controls.Add(this._tabControl);
            this.Controls.Add(this._btnSave);
            this.Name = "SettingsForm";
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

        #endregion

        private System.Windows.Forms.TabControl _tabControl;
        private System.Windows.Forms.TabPage _captureTab;
        private System.Windows.Forms.TabPage _toolsTab;
        private System.Windows.Forms.TabPage _hotkeysTab;
        private System.Windows.Forms.TableLayoutPanel _hotkeysTable;
        private System.Windows.Forms.Label _hotkeyCaptureLabel;
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
    }
}
