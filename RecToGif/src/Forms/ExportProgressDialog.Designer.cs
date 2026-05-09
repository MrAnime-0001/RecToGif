namespace RecToGif.Forms
{
    partial class ExportProgressDialog
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
            this._progressBar = new System.Windows.Forms.ProgressBar();
            this._btnCancel = new System.Windows.Forms.Button();
            this._lblStatus = new System.Windows.Forms.Label();
            this._lblFormat = new System.Windows.Forms.Label();
            this.SuspendLayout();

            Color bgDark = Color.FromArgb(30, 30, 30);
            Color bgPanel = Color.FromArgb(45, 45, 45);
            Color fgWhite = Color.FromArgb(220, 220, 220);
            Color fgDim = Color.FromArgb(150, 150, 150);

            //
            // _lblFormat
            //
            this._lblFormat.AutoSize = true;
            this._lblFormat.BackColor = Color.Transparent;
            this._lblFormat.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this._lblFormat.ForeColor = fgWhite;
            this._lblFormat.Location = new System.Drawing.Point(12, 14);
            this._lblFormat.Name = "_lblFormat";
            this._lblFormat.Size = new System.Drawing.Size(100, 19);
            this._lblFormat.TabIndex = 0;
            this._lblFormat.Text = "Exporting GIF...";
            //
            // _progressBar
            //
            this._progressBar.BackColor = bgDark;
            this._progressBar.ForeColor = Color.FromArgb(0, 140, 220);
            this._progressBar.Location = new System.Drawing.Point(12, 42);
            this._progressBar.Margin = new System.Windows.Forms.Padding(0);
            this._progressBar.Name = "_progressBar";
            this._progressBar.Size = new System.Drawing.Size(316, 24);
            this._progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this._progressBar.TabIndex = 0;
            //
            // _lblStatus
            //
            this._lblStatus.AutoSize = true;
            this._lblStatus.BackColor = Color.Transparent;
            this._lblStatus.ForeColor = fgDim;
            this._lblStatus.Location = new System.Drawing.Point(12, 74);
            this._lblStatus.Name = "_lblStatus";
            this._lblStatus.Size = new System.Drawing.Size(100, 15);
            this._lblStatus.TabIndex = 1;
            this._lblStatus.Text = "Initializing...";
            //
            // _btnCancel
            //
            this._btnCancel.BackColor = Color.FromArgb(180, 40, 40);
            this._btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnCancel.FlatAppearance.BorderColor = Color.FromArgb(200, 60, 60);
            this._btnCancel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this._btnCancel.ForeColor = System.Drawing.Color.White;
            this._btnCancel.Location = new System.Drawing.Point(253, 68);
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(75, 28);
            this._btnCancel.TabIndex = 2;
            this._btnCancel.Text = "Cancel";
            this._btnCancel.UseVisualStyleBackColor = false;
            //
            // ExportProgressDialog
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = bgDark;
            this.ForeColor = fgWhite;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ClientSize = new System.Drawing.Size(340, 108);
            this.ControlBox = false;
            this.Controls.Add(this._btnCancel);
            this.Controls.Add(this._lblStatus);
            this.Controls.Add(this._progressBar);
            this.Controls.Add(this._lblFormat);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExportProgressDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Exporting...";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.ProgressBar _progressBar;
        private System.Windows.Forms.Button _btnCancel;
        private System.Windows.Forms.Label _lblStatus;
        private System.Windows.Forms.Label _lblFormat;
    }
}
