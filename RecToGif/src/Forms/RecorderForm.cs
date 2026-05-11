using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using RecToGif.Presenters;

namespace RecToGif.Forms
{
    public partial class RecorderForm : Form, IRecorderView
    {
        private readonly RecorderPresenter _presenter;
        private System.Windows.Forms.Timer _updateTimer;
        private System.Windows.Forms.Timer _recDotTimer;
        private DateTime _startTime;
        private TimeSpan _elapsedTime;
        private bool _dotVisible;

        private static readonly Color FgDim = Color.FromArgb(150, 150, 150);

        public RecorderForm(
            RecorderPresenter presenter)
        {
            InitializeComponent();
            _presenter = presenter;
            _presenter.View = this;
            SubscribeEvents();

            _updateTimer = new System.Windows.Forms.Timer { Interval = 100 };
            _updateTimer.Tick += (s, e) => UpdateStats();

            _recDotTimer = new System.Windows.Forms.Timer { Interval = 500 };
            _recDotTimer.Tick += (s, e) => ToggleRecDot();
        }

        private void SubscribeEvents()
        {
            _btnSelectRegion.Click += (s, e) =>
            {
                this.Hide();
                using (var overlay = new RegionSelectorOverlay(Program.ServiceProvider.GetRequiredService<Services.ISettingsService>()))
                {
                    if (overlay.ShowDialog() == DialogResult.OK)
                    {
                        _presenter.SetRegion(overlay.SelectedRegion);
                        HighlightSelected(_btnSelectRegion);
                    }
                }
                this.Show();
            };

            _btnSelectWindow.Click += async (s, e) =>
            {
                await _presenter.SelectWindowAsync();
                HighlightSelected(_btnSelectWindow);
            };

            _btnRecord.Click += async (s, e) => await _presenter.StartRecordingAsync();
            _btnPause.Click += (s, e) => _presenter.TogglePause();
            _btnStop.Click += async (s, e) => await _presenter.StopRecordingAsync();
            _btnDiscard.Click += (s, e) => _presenter.DiscardRecording();
            _btnSettings.Click += (s, e) =>
            {
                this.TopMost = false;
                var settingsForm = Program.ServiceProvider.GetRequiredService<Forms.SettingsForm>();
                settingsForm.ShowDialog(this);
                this.TopMost = true;
            };
        }

        private void HighlightSelected(Button btn)
        {
            foreach (Button b in new[] { _btnSelectRegion, _btnSelectWindow })
                b.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 60);

            btn.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 215);
        }

        private void UpdateStats()
        {
            if (!_presenter.IsPaused)
                _elapsedTime = DateTime.Now - _startTime;

            _lblStats.Text = $"{_elapsedTime:mm\\:ss\\.f} | {_presenter.GetFrameCount()} frames";
        }

        private void ToggleRecDot()
        {
            _dotVisible = !_dotVisible;
            _btnRecord.Text = _dotVisible ? "\U0001f534  Recording..." : "  Recording...";
        }

        // --- View interface ---

        public void OnRecordingStarted()
        {
            _btnSelectRegion.Enabled = false;
            _btnSelectWindow.Enabled = false;

            _btnRecord.Visible = false;

            _btnPause.Enabled = true;
            _btnPause.Visible = true;
            _btnPause.Text = "Pause";
            _btnStop.Enabled = true;
            _btnStop.Visible = true;
            _btnDiscard.Enabled = true;
            _btnDiscard.Visible = true;

            _startTime = DateTime.Now;
            _dotVisible = true;
            _lblStatus.ForeColor = Color.FromArgb(255, 100, 100);
            _lblStats.ForeColor = Color.FromArgb(220, 220, 220);

            _updateTimer.Start();
            _recDotTimer.Start();
        }

        public void OnRecordingStopped()
        {
            _btnSelectRegion.Enabled = true;
            _btnSelectWindow.Enabled = true;

            _btnRecord.Visible = true;
            _btnRecord.Text = "\U0001f534  Record";

            _btnPause.Enabled = false;
            _btnPause.Visible = false;
            _btnStop.Enabled = false;
            _btnStop.Visible = false;
            _btnDiscard.Enabled = false;
            _btnDiscard.Visible = false;

            _updateTimer.Stop();
            _recDotTimer.Stop();
            _dotVisible = false;
            _lblStatus.ForeColor = FgDim;
            _lblStats.ForeColor = FgDim;
        }

        public void OnRecordingPaused()
        {
            _btnPause.Text = "Resume";
            _lblStatus.ForeColor = Color.FromArgb(255, 200, 50);
        }

        public void OnRecordingResumed()
        {
            _btnPause.Text = "Pause";
            _lblStatus.ForeColor = Color.FromArgb(255, 100, 100);
        }

        public void OnRecordingDiscarded()
        {
            _btnSelectRegion.Enabled = true;
            _btnSelectWindow.Enabled = true;

            _btnRecord.Visible = true;
            _btnRecord.Text = "\U0001f534  Record";

            _btnPause.Enabled = false;
            _btnPause.Visible = false;
            _btnPause.Text = "Pause";
            _btnStop.Enabled = false;
            _btnStop.Visible = false;
            _btnDiscard.Enabled = false;
            _btnDiscard.Visible = false;

            _updateTimer.Stop();
            _recDotTimer.Stop();
            _dotVisible = false;
            _lblStats.Text = "00:00.0 | 0 frames";
            _lblStatus.ForeColor = FgDim;
            _lblStats.ForeColor = FgDim;
            _elapsedTime = TimeSpan.Zero;
        }

        public void OnSourceChanged(string description)
        {
            _lblStatus.Text = description;
            _lblStatus.ForeColor = Color.FromArgb(100, 200, 255);
        }

        public void ShowInlineMessage(string message)
        {
            _lblStatus.Text = message;
            _lblStatus.ForeColor = Color.FromArgb(255, 200, 50);
        }

        public int FormWidth => Width;
        public int FormHeight => Height;
        public Point GetFormPosition() => Location;
        public void SetFormPosition(Point pos) => Location = pos;
    }
}
