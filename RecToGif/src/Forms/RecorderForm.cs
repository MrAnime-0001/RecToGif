using System;
using System.Windows.Forms;
using RecToGif.Forms;
using RecToGif.Presenters;

namespace RecToGif.Forms
{
    public partial class RecorderForm : Form, IRecorderView
    {
        private readonly RecorderPresenter _presenter;
        private System.Windows.Forms.Timer _updateTimer;
        private DateTime _startTime;
        private TimeSpan _elapsedTime;

        public RecorderForm()
        {
            InitializeComponent();
            _presenter = new RecorderPresenter(this);
            SubscribeEvents();
            
            _updateTimer = new System.Windows.Forms.Timer { Interval = 100 };
            _updateTimer.Tick += (s, e) => UpdateStatus();
        }

        private void SubscribeEvents()
        {
            _btnSelectRegion.Click += (s, e) => 
            {
                this.Hide();
                using (var overlay = new RegionSelectorOverlay())
                {
                    if (overlay.ShowDialog() == DialogResult.OK)
                    {
                        _presenter.SetRegion(overlay.SelectedRegion);
                    }
                }
                this.Show();
            };

            _btnSelectWindow.Click += async (s, e) => await _presenter.SelectWindowAsync();
            _btnRecord.Click += async (s, e) => await _presenter.StartRecordingAsync();
            _btnPause.Click += (s, e) => _presenter.TogglePause();
            _btnStop.Click += async (s, e) => await _presenter.StopRecordingAsync();
            _btnDiscard.Click += (s, e) => _presenter.DiscardRecording();
            _btnSettings.Click += (s, e) => new SettingsForm().ShowDialog(this);
        }

        private void UpdateStatus()
        {
            if (!_presenter.IsPaused) // Recording
            {
                _elapsedTime = DateTime.Now - _startTime;
            }
            
            _lblStatus.Text = $"Frames: {_presenter.GetFrameCount()} | Time: {_elapsedTime:mm\\:ss\\.f}";
        }

        public void OnRecordingStarted()
        {
            _btnSelectRegion.Enabled = false;
            _btnSelectWindow.Enabled = false;
            _btnRecord.Enabled = false;
            _btnPause.Enabled = true;
            _btnStop.Enabled = true;
            _btnDiscard.Enabled = true;
            _startTime = DateTime.Now;
            _updateTimer.Start();
        }

        public void OnRecordingStopped()
        {
            _btnSelectRegion.Enabled = true;
            _btnSelectWindow.Enabled = true;
            _btnRecord.Enabled = true;
            _btnPause.Enabled = false;
            _btnStop.Enabled = false;
            _btnDiscard.Enabled = false;
            _updateTimer.Stop();
        }

        public void OnRecordingPaused()
        {
            _btnPause.Text = "Resume";
        }

        public void OnRecordingResumed()
        {
            _btnPause.Text = "Pause";
        }

        public void OnRecordingDiscarded()
        {
            _btnSelectRegion.Enabled = true;
            _btnSelectWindow.Enabled = true;
            _btnRecord.Enabled = true;
            _btnPause.Enabled = false;
            _btnStop.Enabled = false;
            _btnDiscard.Enabled = false;
            _btnPause.Text = "Pause";
            _updateTimer.Stop();
            _lblStatus.Text = "Ready";
        }

        public void OnSourceChanged(string description)
        {
            _lblStatus.Text = description;
        }

        public void ShowInlineMessage(string message)
        {
            _lblStatus.Text = message;
        }
    }
}
