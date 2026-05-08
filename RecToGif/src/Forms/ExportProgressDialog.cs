using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace RecToGif.Forms
{
    public partial class ExportProgressDialog : Form
    {
        private readonly CancellationTokenSource _cts = new();

        public CancellationToken Token => _cts.Token;
        public IProgress<int> Progress => new Progress<int>(v => _progressBar.Value = v);

        public ExportProgressDialog()
        {
            InitializeComponent();
            _btnCancel.Click += (s, e) => _cts.Cancel();
        }
    }
}
