using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using RecToGif.Editor;

namespace RecToGif.Controls
{
    public class LoopFinderPanel : UserControl
    {
        private readonly ProgressBar _progressBar;
        private readonly TrackBar _thresholdSlider;
        private readonly NumericUpDown _minSpanInput;
        private readonly NumericUpDown _maxSpanInput;
        private readonly ListBox _resultsList;
        private readonly Button _btnFind;
        private readonly Button _btnApply;
        private readonly Button _btnCancel;

        public event EventHandler<(int Start, int End)>? PreviewLoopRequested;
        public event EventHandler<(int Start, int End)>? ApplyLoopRequested;
        public event EventHandler? CancelRequested;
        public event Func<int, int, double, int, IProgress<int>, CancellationToken, Task<List<LoopFinder.LoopCandidate>>>? FindLoopsRequested;

        private CancellationTokenSource? _cts;

        public LoopFinderPanel()
        {
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.White;
            this.Padding = new Padding(10);
            this.Size = new Size(300, 500);

            var lblThreshold = new Label { Text = "Similarity Threshold:", Dock = DockStyle.Top };
            _thresholdSlider = new TrackBar { Minimum = 50, Maximum = 100, Value = 92, Dock = DockStyle.Top };
            
            var lblMinSpan = new Label { Text = "Min Span (frames):", Dock = DockStyle.Top };
            _minSpanInput = new NumericUpDown { Minimum = 2, Maximum = 1000, Value = 10, Dock = DockStyle.Top };
            
            var lblMaxSpan = new Label { Text = "Max Span (frames):", Dock = DockStyle.Top };
            _maxSpanInput = new NumericUpDown { Minimum = 2, Maximum = 1000, Value = 300, Dock = DockStyle.Top };

            _btnFind = new Button { Text = "Find Loops", Dock = DockStyle.Top, Height = 40, Margin = new Padding(0, 10, 0, 10) };
            _btnFind.Click += OnFindClick;

            _progressBar = new ProgressBar { Dock = DockStyle.Top, Height = 20, Visible = false };

            _resultsList = new ListBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White };
            _resultsList.SelectedIndexChanged += (s, e) => {
                if (_resultsList.SelectedItem is LoopFinder.LoopCandidate candidate)
                {
                    PreviewLoopRequested?.Invoke(this, (candidate.StartFrame, candidate.EndFrame));
                }
            };

            var btnPanel = new Panel { Dock = DockStyle.Bottom, Height = 40 };
            _btnApply = new Button { Text = "Apply", Dock = DockStyle.Left, Width = 140 };
            _btnApply.Click += (s, e) => {
                if (_resultsList.SelectedItem is LoopFinder.LoopCandidate candidate)
                {
                    ApplyLoopRequested?.Invoke(this, (candidate.StartFrame, candidate.EndFrame));
                }
            };
            
            _btnCancel = new Button { Text = "Cancel", Dock = DockStyle.Right, Width = 140 };
            _btnCancel.Click += (s, e) => CancelRequested?.Invoke(this, EventArgs.Empty);
            
            btnPanel.Controls.Add(_btnApply);
            btnPanel.Controls.Add(_btnCancel);

            this.Controls.Add(_resultsList);
            this.Controls.Add(_progressBar);
            this.Controls.Add(_btnFind);
            this.Controls.Add(_maxSpanInput);
            this.Controls.Add(lblMaxSpan);
            this.Controls.Add(_minSpanInput);
            this.Controls.Add(lblMinSpan);
            this.Controls.Add(_thresholdSlider);
            this.Controls.Add(lblThreshold);
            this.Controls.Add(btnPanel);
        }

        private async void OnFindClick(object? sender, EventArgs e)
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts = null;
                _btnFind.Text = "Find Loops";
                return;
            }

            if (FindLoopsRequested == null) return;

            _cts = new CancellationTokenSource();
            _btnFind.Text = "Stop";
            _progressBar.Visible = true;
            _progressBar.Value = 0;
            _resultsList.Items.Clear();

            var progress = new Progress<int>(v => _progressBar.Value = v);
            var token = _cts.Token;

            // Check if already cancelled (e.g., user clicked Find again immediately after Cancel)
            if (token.IsCancellationRequested)
            {
                _btnFind.Text = "Find Loops";
                _progressBar.Visible = false;
                _cts = null;
                return;
            }

            try
            {
                var results = await FindLoopsRequested((int)_minSpanInput.Value, (int)_maxSpanInput.Value, _thresholdSlider.Value, 64, progress, token);
                foreach (var res in results)
                {
                    _resultsList.Items.Add(res);
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                _btnFind.Text = "Find Loops";
                _progressBar.Visible = false;
                _cts = null;
            }
        }
    }
}
