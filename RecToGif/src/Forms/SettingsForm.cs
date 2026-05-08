using System;
using System.Drawing;
using System.Windows.Forms;
using RecToGif.Models;
using RecToGif.Services;

namespace RecToGif.Forms
{
    public partial class SettingsForm : Form
    {
        private AppSettings _settings;
        private ShortcutMap _shortcuts;

        public SettingsForm()
        {
            _settings = SettingsService.LoadSettings();
            _shortcuts = SettingsService.LoadShortcuts();
            
            InitializeComponent();
            LoadSettingsIntoControls();
            SubscribeEvents();
        }

        private void LoadSettingsIntoControls()
        {
            _numFps.Value = _settings.DefaultFps;
            _txtGifski.Text = _settings.GifskiPath;
            _txtFfmpeg.Text = _settings.FfmpegPath;
        }

        private void SubscribeEvents()
        {
            _numFps.ValueChanged += (s, e) => _settings.DefaultFps = (int)_numFps.Value;
            
            _btnBrowseGifski.Click += (s, e) =>
            {
                using var ofd = new OpenFileDialog { Filter = "gifski.exe|gifski.exe|All executables|*.exe", Title = "Locate gifski.exe" };
                if (ofd.ShowDialog() == DialogResult.OK) _txtGifski.Text = ofd.FileName;
            };
            _txtGifski.TextChanged += (s, e) => _settings.GifskiPath = _txtGifski.Text;

            _btnBrowseFfmpeg.Click += (s, e) =>
            {
                using var ofd = new OpenFileDialog { Filter = "ffmpeg.exe|ffmpeg.exe|All executables|*.exe", Title = "Locate ffmpeg.exe" };
                if (ofd.ShowDialog() == DialogResult.OK) _txtFfmpeg.Text = ofd.FileName;
            };
            _txtFfmpeg.TextChanged += (s, e) => _settings.FfmpegPath = _txtFfmpeg.Text;

            _btnSave.Click += (s, e) => 
            {
                SettingsService.SaveSettings(_settings);
                SettingsService.SaveShortcuts(_shortcuts);
                this.Close();
            };
        }
    }
}
