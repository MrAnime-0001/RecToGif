using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace RecToGif.Forms
{
    public partial class ExportProgressDialog : Form
    {
        private readonly CancellationTokenSource _cts = new();

        public CancellationToken Token => _cts.Token;
        public IProgress<int> Progress => new Progress<int>(v => UpdateProgress(v));

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string FormatLabel
        {
            get => _lblFormat.Text;
            set => _lblFormat.Text = value;
        }

        public ExportProgressDialog()
        {
            InitializeComponent();
            _btnCancel.Click += (s, e) => _cts.Cancel();
        }

        private void UpdateProgress(int percent)
        {
            if (InvokeRequired)
            {
                if (IsDisposed) return;
                BeginInvoke(new Action<int>(UpdateProgress), percent);
                return;
            }
            if (IsDisposed) return;
            _progressBar.Value = Math.Clamp(percent, 0, 100);
            _lblStatus.Text = $"{percent}% complete";
        }

        public void SetStatus(string text)
        {
            if (InvokeRequired)
            {
                if (IsDisposed) return;
                BeginInvoke(new Action<string>(SetStatus), text);
                return;
            }
            if (IsDisposed) return;
            _lblStatus.Text = text;
        }
    }
}
