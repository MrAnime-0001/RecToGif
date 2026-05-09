namespace RecToGif.Forms
{
    partial class RecorderForm
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
            if (disposing)
            {
                components?.Dispose();
                _updateTimer?.Stop();
                _updateTimer?.Dispose();
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
            this._btnSelectRegion = new System.Windows.Forms.Button();
            this._btnSelectWindow = new System.Windows.Forms.Button();
            this._btnRecord = new System.Windows.Forms.Button();
            this._btnPause = new System.Windows.Forms.Button();
            this._btnStop = new System.Windows.Forms.Button();
            this._btnDiscard = new System.Windows.Forms.Button();
            this._btnSettings = new System.Windows.Forms.Button();
            this._lblStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _btnSelectRegion
            // 
            this._btnSelectRegion.Location = new System.Drawing.Point(10, 10);
            this._btnSelectRegion.Name = "_btnSelectRegion";
            this._btnSelectRegion.Size = new System.Drawing.Size(100, 30);
            this._btnSelectRegion.TabIndex = 0;
            this._btnSelectRegion.Text = "Select Region";
            this._btnSelectRegion.UseVisualStyleBackColor = true;
            // 
            // _btnSelectWindow
            // 
            this._btnSelectWindow.Location = new System.Drawing.Point(115, 10);
            this._btnSelectWindow.Name = "_btnSelectWindow";
            this._btnSelectWindow.Size = new System.Drawing.Size(100, 30);
            this._btnSelectWindow.TabIndex = 1;
            this._btnSelectWindow.Text = "Select Window";
            this._btnSelectWindow.UseVisualStyleBackColor = true;
            // 
            // _btnRecord
            // 
            this._btnRecord.Location = new System.Drawing.Point(220, 10);
            this._btnRecord.Name = "_btnRecord";
            this._btnRecord.Size = new System.Drawing.Size(60, 30);
            this._btnRecord.TabIndex = 2;
            this._btnRecord.Text = "Record";
            this._btnRecord.UseVisualStyleBackColor = true;
            // 
            // _btnPause
            // 
            this._btnPause.Enabled = false;
            this._btnPause.Location = new System.Drawing.Point(285, 10);
            this._btnPause.Name = "_btnPause";
            this._btnPause.Size = new System.Drawing.Size(60, 30);
            this._btnPause.TabIndex = 3;
            this._btnPause.Text = "Pause";
            this._btnPause.UseVisualStyleBackColor = true;
            // 
            // _btnStop
            // 
            this._btnStop.Enabled = false;
            this._btnStop.Location = new System.Drawing.Point(350, 10);
            this._btnStop.Name = "_btnStop";
            this._btnStop.Size = new System.Drawing.Size(60, 30);
            this._btnStop.TabIndex = 4;
            this._btnStop.Text = "Done";
            this._btnStop.UseVisualStyleBackColor = true;
            // 
            // _btnDiscard
            // 
            this._btnDiscard.Enabled = false;
            this._btnDiscard.Location = new System.Drawing.Point(415, 10);
            this._btnDiscard.Name = "_btnDiscard";
            this._btnDiscard.Size = new System.Drawing.Size(60, 30);
            this._btnDiscard.TabIndex = 5;
            this._btnDiscard.Text = "Discard";
            this._btnDiscard.UseVisualStyleBackColor = true;
            // 
            // _btnSettings
            // 
            this._btnSettings.Location = new System.Drawing.Point(480, 10);
            this._btnSettings.Name = "_btnSettings";
            this._btnSettings.Size = new System.Drawing.Size(70, 30);
            this._btnSettings.TabIndex = 6;
            this._btnSettings.Text = "Settings";
            this._btnSettings.UseVisualStyleBackColor = true;
            // 
            // _lblStatus
            // 
            this._lblStatus.Location = new System.Drawing.Point(10, 50);
            this._lblStatus.Name = "_lblStatus";
            this._lblStatus.Size = new System.Drawing.Size(530, 20);
            this._lblStatus.TabIndex = 7;
            this._lblStatus.Text = "Ready";
            // 
            // RecorderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(564, 81);
            this.Controls.Add(this._lblStatus);
            this.Controls.Add(this._btnSettings);
            this.Controls.Add(this._btnDiscard);
            this.Controls.Add(this._btnStop);
            this.Controls.Add(this._btnPause);
            this.Controls.Add(this._btnRecord);
            this.Controls.Add(this._btnSelectWindow);
            this.Controls.Add(this._btnSelectRegion);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "RecorderForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "RecToGif Recorder";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _btnRecord;
        private System.Windows.Forms.Button _btnPause;
        private System.Windows.Forms.Button _btnStop;
        private System.Windows.Forms.Button _btnDiscard;
        private System.Windows.Forms.Button _btnSelectRegion;
        private System.Windows.Forms.Button _btnSelectWindow;
        private System.Windows.Forms.Button _btnSettings;
        private System.Windows.Forms.Label _lblStatus;
    }
}
