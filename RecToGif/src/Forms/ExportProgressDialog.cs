using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace RecToGif.Forms
{
    public class ExportProgressDialog : Form
    {
        private readonly ProgressBar _progressBar;
        private readonly CancellationTokenSource _cts = new();

        public CancellationToken Token => _cts.Token;
        public IProgress<int> Progress => new Progress<int>(v => _progressBar.Value = v);

        public ExportProgressDialog()
        {
            this.Text = "Exporting...";
            this.Size = new Size(300, 120);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ControlBox = false;

            _progressBar = new ProgressBar { Dock = DockStyle.Top, Height = 30, Margin = new Padding(10) };
            var btnCancel = new Button { Text = "Cancel", Dock = DockStyle.Bottom, Height = 30 };
            btnCancel.Click += (s, e) => _cts.Cancel();

            this.Controls.Add(_progressBar);
            this.Controls.Add(btnCancel);
        }
    }
}
