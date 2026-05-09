namespace RecToGif.Forms
{
    partial class RecorderForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                _updateTimer?.Stop();
                _updateTimer?.Dispose();
                _recDotTimer?.Stop();
                _recDotTimer?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._btnSelectRegion = new System.Windows.Forms.Button();
            this._btnSelectWindow = new System.Windows.Forms.Button();
            this._btnSettings = new System.Windows.Forms.Button();
            this._btnRecord = new System.Windows.Forms.Button();
            this._btnPause = new System.Windows.Forms.Button();
            this._btnStop = new System.Windows.Forms.Button();
            this._btnDiscard = new System.Windows.Forms.Button();
            this._lblStatus = new System.Windows.Forms.Label();
            this._lblStats = new System.Windows.Forms.Label();
            this.SuspendLayout();

            Color bgDark = Color.FromArgb(30, 30, 30);
            Color bgPanel = Color.FromArgb(45, 45, 45);
            Color fgWhite = Color.FromArgb(220, 220, 220);
            Color fgDim = Color.FromArgb(150, 150, 150);

            this.BackColor = bgDark;
            this.ForeColor = fgWhite;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "RecorderForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "RecToGif Recorder";
            this.TopMost = true;

            //
            // _btnSelectRegion
            //
            this._btnSelectRegion.BackColor = bgPanel;
            this._btnSelectRegion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnSelectRegion.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 60);
            this._btnSelectRegion.ForeColor = fgWhite;
            this._btnSelectRegion.Location = new System.Drawing.Point(10, 10);
            this._btnSelectRegion.Name = "_btnSelectRegion";
            this._btnSelectRegion.Size = new System.Drawing.Size(120, 30);
            this._btnSelectRegion.TabIndex = 0;
            this._btnSelectRegion.Text = "Select Region";
            this._btnSelectRegion.UseVisualStyleBackColor = false;
            this._btnSelectRegion.Cursor = System.Windows.Forms.Cursors.Hand;
            //
            // _btnSelectWindow
            //
            this._btnSelectWindow.BackColor = bgPanel;
            this._btnSelectWindow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnSelectWindow.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 60);
            this._btnSelectWindow.ForeColor = fgWhite;
            this._btnSelectWindow.Location = new System.Drawing.Point(135, 10);
            this._btnSelectWindow.Name = "_btnSelectWindow";
            this._btnSelectWindow.Size = new System.Drawing.Size(120, 30);
            this._btnSelectWindow.TabIndex = 1;
            this._btnSelectWindow.Text = "Select Window";
            this._btnSelectWindow.UseVisualStyleBackColor = false;
            this._btnSelectWindow.Cursor = System.Windows.Forms.Cursors.Hand;
            //
            // _btnSettings
            //
            this._btnSettings.BackColor = bgPanel;
            this._btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnSettings.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 60);
            this._btnSettings.ForeColor = fgWhite;
            this._btnSettings.Location = new System.Drawing.Point(260, 10);
            this._btnSettings.Name = "_btnSettings";
            this._btnSettings.Size = new System.Drawing.Size(80, 30);
            this._btnSettings.TabIndex = 2;
            this._btnSettings.Text = "Settings";
            this._btnSettings.UseVisualStyleBackColor = false;
            this._btnSettings.Cursor = System.Windows.Forms.Cursors.Hand;
            //
            // _btnRecord
            //
            this._btnRecord.BackColor = Color.FromArgb(200, 50, 50);
            this._btnRecord.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnRecord.FlatAppearance.BorderColor = Color.FromArgb(220, 70, 70);
            this._btnRecord.ForeColor = System.Drawing.Color.White;
            this._btnRecord.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this._btnRecord.Location = new System.Drawing.Point(10, 55);
            this._btnRecord.Name = "_btnRecord";
            this._btnRecord.Size = new System.Drawing.Size(330, 45);
            this._btnRecord.TabIndex = 3;
            this._btnRecord.Text = "\U0001f534  Record";
            this._btnRecord.UseVisualStyleBackColor = false;
            this._btnRecord.Cursor = System.Windows.Forms.Cursors.Hand;
            //
            // _btnPause
            //
            this._btnPause.BackColor = Color.FromArgb(50, 50, 50);
            this._btnPause.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnPause.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 70);
            this._btnPause.ForeColor = fgWhite;
            this._btnPause.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this._btnPause.Location = new System.Drawing.Point(10, 55);
            this._btnPause.Name = "_btnPause";
            this._btnPause.Size = new System.Drawing.Size(105, 45);
            this._btnPause.TabIndex = 4;
            this._btnPause.Text = "Pause";
            this._btnPause.UseVisualStyleBackColor = false;
            this._btnPause.Enabled = false;
            this._btnPause.Visible = false;
            this._btnPause.Cursor = System.Windows.Forms.Cursors.Hand;
            //
            // _btnStop
            //
            this._btnStop.BackColor = Color.FromArgb(180, 40, 40);
            this._btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnStop.FlatAppearance.BorderColor = Color.FromArgb(200, 60, 60);
            this._btnStop.ForeColor = System.Drawing.Color.White;
            this._btnStop.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this._btnStop.Location = new System.Drawing.Point(122, 55);
            this._btnStop.Name = "_btnStop";
            this._btnStop.Size = new System.Drawing.Size(105, 45);
            this._btnStop.TabIndex = 5;
            this._btnStop.Text = "Stop";
            this._btnStop.UseVisualStyleBackColor = false;
            this._btnStop.Enabled = false;
            this._btnStop.Visible = false;
            this._btnStop.Cursor = System.Windows.Forms.Cursors.Hand;
            //
            // _btnDiscard
            //
            this._btnDiscard.BackColor = Color.FromArgb(50, 50, 50);
            this._btnDiscard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnDiscard.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 70);
            this._btnDiscard.ForeColor = fgWhite;
            this._btnDiscard.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this._btnDiscard.Location = new System.Drawing.Point(235, 55);
            this._btnDiscard.Name = "_btnDiscard";
            this._btnDiscard.Size = new System.Drawing.Size(105, 45);
            this._btnDiscard.TabIndex = 6;
            this._btnDiscard.Text = "Discard";
            this._btnDiscard.UseVisualStyleBackColor = false;
            this._btnDiscard.Enabled = false;
            this._btnDiscard.Visible = false;
            this._btnDiscard.Cursor = System.Windows.Forms.Cursors.Hand;
            //
            // _lblStatus
            //
            this._lblStatus.AutoSize = false;
            this._lblStatus.BackColor = Color.Transparent;
            this._lblStatus.ForeColor = fgDim;
            this._lblStatus.Location = new System.Drawing.Point(10, 110);
            this._lblStatus.Name = "_lblStatus";
            this._lblStatus.Size = new System.Drawing.Size(200, 22);
            this._lblStatus.TabIndex = 10;
            this._lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // _lblStats only shown during recording — no permanent placement needed
            this._lblStats = new System.Windows.Forms.Label();
            this._lblStats.AutoSize = false;
            this._lblStats.BackColor = Color.Transparent;
            this._lblStats.ForeColor = fgDim;
            this._lblStats.Location = new System.Drawing.Point(220, 110);
            this._lblStats.Name = "_lblStats";
            this._lblStats.Size = new System.Drawing.Size(120, 22);
            this._lblStats.TabIndex = 11;
            this._lblStats.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            //
            // RecorderForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(360, 140);
            this.Controls.Add(this._btnDiscard);
            this.Controls.Add(this._btnStop);
            this.Controls.Add(this._btnPause);
            this.Controls.Add(this._btnRecord);
            this.Controls.Add(this._lblStats);
            this.Controls.Add(this._lblStatus);
            this.Controls.Add(this._btnSettings);
            this.Controls.Add(this._btnSelectWindow);
            this.Controls.Add(this._btnSelectRegion);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Button _btnSelectRegion;
        private System.Windows.Forms.Button _btnSelectWindow;
        private System.Windows.Forms.Button _btnSettings;
        private System.Windows.Forms.Button _btnRecord;
        private System.Windows.Forms.Button _btnPause;
        private System.Windows.Forms.Button _btnStop;
        private System.Windows.Forms.Button _btnDiscard;
        private System.Windows.Forms.Label _lblStatus;
        private System.Windows.Forms.Label _lblStats;
    }
}
