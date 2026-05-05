using System;
using System.Windows.Forms;
using RecToGif.Forms;
using RecToGif.Presenters;

namespace RecToGif.Forms
{
    public partial class RecorderForm : Form, IRecorderView
    {
        private readonly RecorderPresenter _presenter;
        private Button _btnRecord;
        private Button _btnPause;
        private Button _btnStop;
        private Button _btnDiscard;
        private Button _btnSelectRegion;
        private Button _btnSelectWindow;
        private Label _lblStatus;
        private System.Windows.Forms.Timer _updateTimer;
        private DateTime _startTime;
        private TimeSpan _elapsedTime;

        public RecorderForm()
        {
            InitializeComponent();
            _presenter = new RecorderPresenter(this);
            SetupControls();
            
            _updateTimer = new System.Windows.Forms.Timer { Interval = 100 };
            _updateTimer.Tick += (s, e) => UpdateStatus();
        }

        private void InitializeComponent()
        {
            this.Text = "RecToGif Recorder";
            this.Size = new System.Drawing.Size(570, 120);
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void SetupControls()
        {
            _btnSelectRegion = new Button { Text = "Select Region", Location = new System.Drawing.Point(10, 10), Size = new System.Drawing.Size(100, 30) };
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

            _btnSelectWindow = new Button { Text = "Select Window", Location = new System.Drawing.Point(115, 10), Size = new System.Drawing.Size(100, 30) };
            _btnSelectWindow.Click += async (s, e) => await _presenter.SelectWindowAsync();

            _btnRecord = new Button { Text = "Record", Location = new System.Drawing.Point(220, 10), Size = new System.Drawing.Size(60, 30) };
            _btnRecord.Click += async (s, e) => await _presenter.StartRecordingAsync();

            _btnPause = new Button { Text = "Pause", Location = new System.Drawing.Point(285, 10), Size = new System.Drawing.Size(60, 30), Enabled = false };
            _btnPause.Click += (s, e) => _presenter.TogglePause();

            _btnStop = new Button { Text = "Done", Location = new System.Drawing.Point(350, 10), Size = new System.Drawing.Size(60, 30), Enabled = false };
            _btnStop.Click += async (s, e) => await _presenter.StopRecordingAsync();

            _btnDiscard = new Button { Text = "Discard", Location = new System.Drawing.Point(415, 10), Size = new System.Drawing.Size(60, 30), Enabled = false };
            _btnDiscard.Click += (s, e) => _presenter.DiscardRecording();

            var _btnSettings = new Button { Text = "Settings", Location = new System.Drawing.Point(480, 10), Size = new System.Drawing.Size(70, 30) };
            _btnSettings.Click += (s, e) => new SettingsForm().ShowDialog(this);

            _lblStatus = new Label { Text = "Ready", Location = new System.Drawing.Point(10, 50), Size = new System.Drawing.Size(530, 20) };

            this.Controls.Add(_btnSelectRegion);
            this.Controls.Add(_btnSelectWindow);
            this.Controls.Add(_btnRecord);
            this.Controls.Add(_btnPause);
            this.Controls.Add(_btnStop);
            this.Controls.Add(_btnDiscard);
            this.Controls.Add(_btnSettings);
            this.Controls.Add(_lblStatus);
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
            _btnPause.Text = "Pause";
            _btnStop.Enabled = true;
            _btnDiscard.Enabled = true;
            _startTime = DateTime.Now;
            _elapsedTime = TimeSpan.Zero;
            _updateTimer.Start();
        }

        public void OnRecordingPaused()
        {
            _btnPause.Text = "Resume";
        }

        public void OnRecordingResumed()
        {
            _btnPause.Text = "Pause";
            _startTime = DateTime.Now - _elapsedTime;
        }

        public void OnRecordingStopped()
        {
            ResetUI();
            _lblStatus.Text = "Recording saved.";
        }

        public void OnRecordingDiscarded()
        {
            ResetUI();
            _lblStatus.Text = "Recording discarded.";
        }

        private void ResetUI()
        {
            _updateTimer.Stop();
            _btnSelectRegion.Enabled = true;
            _btnSelectWindow.Enabled = true;
            _btnRecord.Enabled = true;
            _btnPause.Enabled = false;
            _btnPause.Text = "Pause";
            _btnStop.Enabled = false;
            _btnDiscard.Enabled = false;
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
