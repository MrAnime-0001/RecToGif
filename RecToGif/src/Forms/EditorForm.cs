using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RecToGif.Editor;
using RecToGif.Controls;
using RecToGif.Presenters;

namespace RecToGif.Forms
{
    public partial class EditorForm : Form, IEditorView
    {
        private readonly EditorPresenter _presenter;
        private readonly PreviewPanel _previewPanel;
        private readonly FrameTimeline _timeline;
        private readonly ToolStrip _toolStrip;
        private readonly LoopFinderPanel _loopFinderPanel;

        public EditorForm()
        {
            InitializeComponent();
            
            _previewPanel = new PreviewPanel { Dock = DockStyle.Fill };
            _timeline = new FrameTimeline { Dock = DockStyle.Bottom };
            
            _toolStrip = new ToolStrip { Dock = DockStyle.Top };
            SetupToolStrip();

            _loopFinderPanel = new LoopFinderPanel { Dock = DockStyle.Right, Visible = false, Width = 300 };

            this.Controls.Add(_previewPanel);
            this.Controls.Add(_loopFinderPanel);
            this.Controls.Add(_timeline);
            this.Controls.Add(_toolStrip);

            _presenter = new EditorPresenter(this);

            _timeline.SelectionChanged += (s, e) => _presenter.OnSelectionChanged(_timeline.SelectedIndices);
            _timeline.FrameDoubleClicked += (s, index) => _presenter.OnFrameDoubleClicked(index);

            SetupLoopFinderEvents();
        }

        private void InitializeComponent()
        {
            this.Text = "RecToGif Editor";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(28, 28, 28);
        }

        private void SetupLoopFinderEvents()
        {
            _loopFinderPanel.FindLoopsRequested += (min, max, threshold, size, progress, token) => 
                _presenter.FindLoopsAsync(min, max, threshold, size, progress, token);
            
            _loopFinderPanel.PreviewLoopRequested += (s, range) => _presenter.PreviewLoop(range.Start, range.End);
            _loopFinderPanel.ApplyLoopRequested += (s, range) => _presenter.ApplyLoop(range.Start, range.End);
            _loopFinderPanel.CancelRequested += (s, e) => _loopFinderPanel.Visible = false;
        }

        private void SetupToolStrip()
        {
            _toolStrip.Items.Add(new ToolStripButton("Delete", null, (s, e) => _presenter.DeleteSelectedFrames()));
            _toolStrip.Items.Add(new ToolStripButton("Duplicate", null, (s, e) => _presenter.DuplicateSelectedFrames()));
            _toolStrip.Items.Add(new ToolStripSeparator());
            _toolStrip.Items.Add(new ToolStripButton("Move Left", null, (s, e) => _presenter.MoveSelectedFrames(-1)));
            _toolStrip.Items.Add(new ToolStripButton("Move Right", null, (s, e) => _presenter.MoveSelectedFrames(1)));
            _toolStrip.Items.Add(new ToolStripSeparator());
            
            _toolStrip.Items.Add(new ToolStripLabel("Delay (ms):"));
            var delayInput = new NumericUpDown { Minimum = 10, Maximum = 5000, Value = 100, Width = 60 };
            _toolStrip.Items.Add(new ToolStripControlHost(delayInput));
            _toolStrip.Items.Add(new ToolStripButton("Apply", null, (s, e) => _presenter.ChangeSelectedFramesDelay((int)delayInput.Value)));

            _toolStrip.Items.Add(new ToolStripSeparator());
            _toolStrip.Items.Add(new ToolStripButton("Find Loop", null, (s, e) => _loopFinderPanel.Visible = !_loopFinderPanel.Visible));
            _toolStrip.Items.Add(new ToolStripSeparator());

            _toolStrip.Items.Add(new ToolStripButton("Crop", null, (s, e) => _presenter.ToggleCropMode()));
            _toolStrip.Items.Add(new ToolStripButton("Apply Crop", null, (s, e) => _presenter.ApplyCrop()));
            _toolStrip.Items.Add(new ToolStripSeparator());

            _toolStrip.Items.Add(new ToolStripLabel("Size:"));
            var widthInput = new NumericUpDown { Minimum = 10, Maximum = 4096, Value = 800, Width = 50 };
            var heightInput = new NumericUpDown { Minimum = 10, Maximum = 4096, Value = 600, Width = 50 };
            _toolStrip.Items.Add(new ToolStripControlHost(widthInput));
            _toolStrip.Items.Add(new ToolStripLabel("x"));
            _toolStrip.Items.Add(new ToolStripControlHost(heightInput));
            _toolStrip.Items.Add(new ToolStripButton("Resize", null, (s, e) => _presenter.Resize((int)widthInput.Value, (int)heightInput.Value)));
            _toolStrip.Items.Add(new ToolStripSeparator());

            _toolStrip.Items.Add(new ToolStripButton("Text", null, (s, e) => 
            {
                string text = PromptForText("Enter text:", "Add Text Overlay");
                if (!string.IsNullOrEmpty(text)) _presenter.AddTextOverlay(text);
            }));

            _toolStrip.Items.Add(new ToolStripButton("Watermark", null, (s, e) => 
            {
                using (var ofd = new OpenFileDialog { Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK) _presenter.AddWatermark(ofd.FileName);
                }
            }));

            _toolStrip.Items.Add(new ToolStripButton("Border", null, (s, e) => 
            {
                using (var cd = new ColorDialog())
                {
                    if (cd.ShowDialog() == DialogResult.OK)
                    {
                        string input = PromptForText("Enter thickness (px):", "Border Thickness");
                        if (int.TryParse(input, out int thickness)) _presenter.UpdateBorder(cd.Color, thickness);
                    }
                }
            }));

            _toolStrip.Items.Add(new ToolStripSeparator());

            _toolStrip.Items.Add(new ToolStripButton("Export", null, async (s, e) => 
            {
                using (var sfd = new SaveFileDialog { Filter = "GIF|*.gif|MP4|*.mp4" })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        using (var dlg = new ExportProgressDialog())
                        {
                            dlg.Show();
                            string format = Path.GetExtension(sfd.FileName).TrimStart('.');
                            try
                            {
                                await _presenter.ExportAsync(sfd.FileName, format, dlg.Progress, dlg.Token);
                            }
                            catch (OperationCanceledException) { }
                            catch (System.IO.FileNotFoundException ex)
                            {
                                var result = MessageBox.Show(
                                    ex.Message + "\n\nOpen Settings to configure the path?",
                                    "Export Failed",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Error);
                                if (result == DialogResult.Yes)
                                    new SettingsForm().ShowDialog(this);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message, "Export Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            finally
                            {
                                dlg.Close();
                            }
                        }
                    }
                }
            }));

            _toolStrip.Items.Add(new ToolStripButton("Undo", null, (s, e) => _presenter.Undo()));
            _toolStrip.Items.Add(new ToolStripButton("Redo", null, (s, e) => _presenter.Redo()));
            _toolStrip.Items.Add(new ToolStripSeparator());
            _toolStrip.Items.Add(new ToolStripButton("Play", null, (s, e) => _presenter.TogglePlayback()));
            _toolStrip.Items.Add(new ToolStripSeparator());
            _toolStrip.Items.Add(new ToolStripButton("Settings", null, (s, e) => new SettingsForm().ShowDialog(this)));
        }

        private string PromptForText(string message, string title)
        {
            using (var dialog = new Form { Text = title, Size = new Size(300, 150), FormBorderStyle = FormBorderStyle.FixedDialog, StartPosition = FormStartPosition.CenterParent })
            {
                var lbl = new Label { Text = message, Dock = DockStyle.Top, Padding = new Padding(5) };
                var txt = new TextBox { Dock = DockStyle.Top, Margin = new Padding(10) };
                var btn = new Button { Text = "OK", Dock = DockStyle.Bottom, DialogResult = DialogResult.OK };
                dialog.Controls.Add(txt);
                dialog.Controls.Add(lbl);
                dialog.Controls.Add(btn);
                return dialog.ShowDialog() == DialogResult.OK ? txt.Text : string.Empty;
            }
        }

        public void DisplayFrames(IEnumerable<FrameItem> frames)
        {
            _timeline.SetFrames(frames);
        }

        public void ShowFramePreview(FrameItem frame)
        {
            _previewPanel.DisplayFrame(frame);
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public bool IsCropping 
        { 
            get => _previewPanel.IsCropping; 
            set { _previewPanel.IsCropping = value; _previewPanel.Invalidate(); } 
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Rectangle CropRegion 
        { 
            get => _previewPanel.CropRegion; 
            set => _previewPanel.CropRegion = value; 
        }

        public Rectangle GetFinalCrop() => _previewPanel.ApplyCrop();

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public List<VisualOverlay> Overlays
        {
            get => _previewPanel.Overlays;
            set { _previewPanel.Overlays = value; _previewPanel.Invalidate(); }
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Color BorderColor
        {
            get => _previewPanel.BorderColor;
            set { _previewPanel.BorderColor = value; _previewPanel.Invalidate(); }
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int BorderThickness
        {
            get => _previewPanel.BorderThickness;
            set { _previewPanel.BorderThickness = value; _previewPanel.Invalidate(); }
        }

        public void InvokeIfRequired(Action action)
        {
            if (this.InvokeRequired) this.BeginInvoke(action);
            else action();
        }

        public async Task LoadSession(string path)
        {
            await _presenter.LoadSessionAsync(path);
        }
    }
}
