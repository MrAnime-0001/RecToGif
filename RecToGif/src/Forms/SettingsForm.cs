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
        }

        private void InitializeComponent()
        {
            this.Text = "Settings";
            this.Size = new Size(430, 320);
            
            var tabControl = new TabControl { Dock = DockStyle.Fill };
            var captureTab = new TabPage("Capture");
            
            var lblFps = new Label { Text = "Capture FPS:", Location = new Point(10, 10) };
            var numFps = new NumericUpDown { Location = new Point(150, 10), Value = _settings.DefaultFps };
            numFps.ValueChanged += (s, e) => _settings.DefaultFps = (int)numFps.Value;
            
            captureTab.Controls.Add(lblFps);
            captureTab.Controls.Add(numFps);
            tabControl.TabPages.Add(captureTab);

            var toolsTab = new TabPage("External Tools");

            var lblGifski = new Label { Text = "gifski.exe path:", Location = new Point(10, 14), AutoSize = true };
            var txtGifski = new TextBox { Location = new Point(130, 10), Width = 200, Text = _settings.GifskiPath };
            var btnBrowseGifski = new Button { Text = "Browse", Location = new Point(335, 9), Width = 60 };
            btnBrowseGifski.Click += (s, e) =>
            {
                using var ofd = new OpenFileDialog { Filter = "gifski.exe|gifski.exe|All executables|*.exe", Title = "Locate gifski.exe" };
                if (ofd.ShowDialog() == DialogResult.OK) txtGifski.Text = ofd.FileName;
            };
            txtGifski.TextChanged += (s, e) => _settings.GifskiPath = txtGifski.Text;

            var lblFfmpeg = new Label { Text = "ffmpeg.exe path:", Location = new Point(10, 44), AutoSize = true };
            var txtFfmpeg = new TextBox { Location = new Point(130, 40), Width = 200, Text = _settings.FfmpegPath };
            var btnBrowseFfmpeg = new Button { Text = "Browse", Location = new Point(335, 39), Width = 60 };
            btnBrowseFfmpeg.Click += (s, e) =>
            {
                using var ofd = new OpenFileDialog { Filter = "ffmpeg.exe|ffmpeg.exe|All executables|*.exe", Title = "Locate ffmpeg.exe" };
                if (ofd.ShowDialog() == DialogResult.OK) txtFfmpeg.Text = ofd.FileName;
            };
            txtFfmpeg.TextChanged += (s, e) => _settings.FfmpegPath = txtFfmpeg.Text;

            toolsTab.Controls.AddRange(new Control[] { lblGifski, txtGifski, btnBrowseGifski, lblFfmpeg, txtFfmpeg, btnBrowseFfmpeg });
            tabControl.TabPages.Add(toolsTab);

            var btnSave = new Button { Text = "Save", Dock = DockStyle.Bottom };
            btnSave.Click += (s, e) => 
            {
                SettingsService.SaveSettings(_settings);
                SettingsService.SaveShortcuts(_shortcuts);
                this.Close();
            };

            this.Controls.Add(tabControl);
            this.Controls.Add(btnSave);
        }
    }
}
