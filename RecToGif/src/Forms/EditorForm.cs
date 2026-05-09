using System;
using System.Collections.Generic;
using System.Drawing;
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

        public EditorForm()
        {
            InitializeComponent();
            _presenter = new EditorPresenter(this);
            SubscribeEvents();
            SetupLoopFinderEvents();
        }

        public async Task LoadSession(string path) => await _presenter.LoadSessionAsync(path);

        private void SubscribeEvents()
        {
            _timeline.SelectionChanged += (s, e) => _presenter.OnSelectionChanged(_timeline.SelectedIndices);
            _timeline.FrameDoubleClicked += (s, index) => _presenter.OnFrameDoubleClicked(index);
            _timeline.DeleteRequested += (s, e) => _presenter.DeleteSelectedFrames();
            _timeline.UndoRequested += (s, e) => _presenter.Undo();
            _timeline.RedoRequested += (s, e) => _presenter.Redo();

            _btnDelete.Click += (s, e) => _presenter.DeleteSelectedFrames();
            _btnDuplicate.Click += (s, e) => _presenter.DuplicateSelectedFrames();
            _btnMoveLeft.Click += (s, e) => _presenter.MoveSelectedFrames(-1);
            _btnMoveRight.Click += (s, e) => _presenter.MoveSelectedFrames(1);
            _btnApplyDelay.Click += (s, e) => _presenter.ChangeSelectedFramesDelay((int)_numDelay.Value);
            _btnFindLoop.Click += (s, e) => _loopFinderPanel.Visible = !_loopFinderPanel.Visible;
            _btnCrop.Click += (s, e) => _presenter.ToggleCropMode();
            _btnApplyCrop.Click += (s, e) => _presenter.ApplyCrop();
            _btnResize.Click += (s, e) => _presenter.Resize((int)_numWidth.Value, (int)_numHeight.Value);

            _btnExport.Click += async (s, e) =>
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
            };
        }

        private void SetupLoopFinderEvents()
        {
            _loopFinderPanel.FindLoopsRequested += (min, max, threshold, size, progress, token) =>
                _presenter.FindLoopsAsync(min, max, threshold, size, progress, token);

            _loopFinderPanel.PreviewLoopRequested += (s, range) => _presenter.PreviewLoop(range.Start, range.End);
            _loopFinderPanel.ApplyLoopRequested += (s, range) => _presenter.ApplyLoop(range.Start, range.End);
            _loopFinderPanel.CancelRequested += (s, e) => _loopFinderPanel.Visible = false;
        }

        // IEditorView
        public void DisplayFrames(IEnumerable<FrameItem> frames) => _timeline.SetFrames(frames);

        public void ShowFramePreview(FrameItem frame) => _previewPanel.DisplayFrame(frame);

        public void SetPlaybackMode(bool playing) => _previewPanel.SetPlaybackMode(playing);

        public void InvokeIfRequired(Action action)
        {
            if (InvokeRequired) Invoke(action);
            else action();
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public bool IsCropping
        {
            get => _previewPanel.IsCropping;
            set => _previewPanel.IsCropping = value;
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
            set => _previewPanel.Overlays = value;
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Color BorderColor
        {
            get => _previewPanel.BorderColor;
            set => _previewPanel.BorderColor = value;
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int BorderThickness
        {
            get => _previewPanel.BorderThickness;
            set => _previewPanel.BorderThickness = value;
        }
    }
}
