using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using RecToGif.Editor;
using RecToGif.Forms;
using RecToGif.Services;

namespace RecToGif.Presenters
{
    public class EditorPresenter
    {
        private readonly IEditorView _view;
        private readonly EditorModel _model = new();
        private List<int> _selectedIndices = new();
        public IReadOnlyList<int> SelectedIndices => _selectedIndices;
        
        private CancellationTokenSource? _playbackCts;
        private int _playbackIndex = 0;
        private readonly object _playbackLock = new();

        public EditorPresenter(IEditorView view)
        {
            _view = view;
        }

        public static async Task<bool> OpenWithSession(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                MessageBox.Show("Session directory not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Check if there are any frames
            if (!Directory.GetFiles(path, "*.png").Any())
            {
                MessageBox.Show("No frames captured in this session.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            var form = new EditorForm();
            await form.LoadSession(path);
            form.FormClosed += (s, e) => Application.Exit();
            form.Show();
            return true;
        }

        public async Task LoadSessionAsync(string path)
        {
            await _model.LoadSessionAsync(path);
            RefreshView();
        }

        public void OnSelectionChanged(IEnumerable<int> indices)
        {
            _selectedIndices = indices.ToList();
            if (_selectedIndices.Count > 0)
            {
                _view.ShowFramePreview(_model.Frames[_selectedIndices[0]]);
            }
        }

        public void OnFrameDoubleClicked(int index)
        {
            // Maybe open per-frame delay editor
        }

        public void DeleteSelectedFrames()
        {
            if (_selectedIndices.Count == 0) return;
            StopPlayback();
            var command = new DeleteFramesCommand(_selectedIndices);
            _model.ExecuteCommand(command);
            RefreshView();
        }

        public void DuplicateSelectedFrames()
        {
            if (_selectedIndices.Count == 0) return;
            StopPlayback();
            var command = new DuplicateFramesCommand(_selectedIndices);
            _model.ExecuteCommand(command);
            RefreshView();
        }

        public void MoveSelectedFrames(int direction)
        {
            if (_selectedIndices.Count == 0) return;
            StopPlayback();
            var command = new MoveFramesCommand(_selectedIndices, direction);
            _model.ExecuteCommand(command);
            RefreshView();
        }

        public void ChangeSelectedFramesDelay(int newDelay)
        {
            if (_selectedIndices.Count == 0) return;
            var command = new ChangeDelayCommand(_selectedIndices, newDelay);
            _model.ExecuteCommand(command);
            RefreshView();
        }

        public void Undo()
        {
            StopPlayback();
            _model.Undo();
            RefreshView();
        }

        public void Redo()
        {
            StopPlayback();
            _model.Redo();
            RefreshView();
        }

        private void RefreshView()
        {
            _view.DisplayFrames(_model.Frames);
            _view.Overlays = _model.ProjectSettings.Overlays;
            _selectedIndices.Clear();
            if (_model.Frames.Count > 0)
            {
                _view.ShowFramePreview(_model.Frames[0]);
            }
        }

        public void TogglePlayback()
        {
            if (_playbackCts != null)
            {
                StopPlayback();
            }
            else
            {
                StartPlayback();
            }
        }

        private void StartPlayback(int start = 0, int end = -1)
        {
            _playbackCts = new CancellationTokenSource();
            _playbackIndex = start;
            int endIndex = end == -1 ? _model.Frames.Count - 1 : end;
            Task.Run(() => PlaybackLoop(_playbackCts.Token, start, endIndex));
        }

        private void StopPlayback()
        {
            _playbackCts?.Cancel();
            _playbackCts = null;
            _view.InvokeIfRequired(() => _view.SetPlaybackMode(false));
        }

        private async Task PlaybackLoop(CancellationToken token, int start, int end)
        {
            try
            {
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                while (!token.IsCancellationRequested)
                {
                    if (_model.Frames.Count == 0) break;

                    int index;
                    lock (_playbackLock)
                    {
                        if (_playbackIndex > end || _playbackIndex < start) _playbackIndex = start;
                        index = _playbackIndex;
                    }

                    var frame = _model.Frames[index];
                    _view.InvokeIfRequired(() =>
                    {
                        _view.ShowFramePreview(frame);
                        _view.SetPlaybackMode(true);
                    });

                    int delayMs = frame.Metadata.DelayMs;
                    long elapsed = sw.ElapsedMilliseconds;
                    int expectedElapsed = (_playbackIndex - start) * delayMs;
                    int adjust = Math.Max(0, delayMs - (int)(elapsed - expectedElapsed));
                    await Task.Delay(adjust, token);
                    lock (_playbackLock)
                    {
                        _playbackIndex++;
                    }
                }
            }
            catch (TaskCanceledException) { }
        }

        // Loop Finder
        public async Task<List<LoopFinder.LoopCandidate>> FindLoopsAsync(int min, int max, double threshold, int size, IProgress<int> progress, CancellationToken token)
        {
            var finder = new LoopFinder();
            return await finder.FindLoopsAsync(_model.Frames, min, max, threshold, size, progress, token);
        }

        public void PreviewLoop(int start, int end)
        {
            StopPlayback();
            StartPlayback(start, end);
        }

        public void ApplyLoop(int start, int end)
        {
            StopPlayback();
            var command = new KeepRangeCommand(start, end);
            _model.ExecuteCommand(command);
            RefreshView();
        }

        // Crop
        public void ToggleCropMode()
        {
            _view.IsCropping = !_view.IsCropping;
            if (_view.IsCropping)
            {
                _view.CropRegion = _model.ProjectSettings.CropRegion;
            }
        }

        public void ApplyCrop()
        {
            var newCrop = _view.GetFinalCrop();
            var command = new CropCommand(newCrop);
            _model.ExecuteCommand(command);
            _view.IsCropping = false;
        }

        // Resize
        public void Resize(int width, int height)
        {
            var command = new ResizeCommand(new Size(width, height));
            _model.ExecuteCommand(command);
        }

        // Overlays
        public void AddTextOverlay(string text)
        {
            var overlay = new VisualOverlay
            {
                Type = OverlayType.Text,
                Content = text,
                Bounds = new Rectangle(10, 10, 200, 50),
                Color = Color.White,
                Opacity = 1.0f
            };
            var command = new AddOverlayCommand(overlay);
            _model.ExecuteCommand(command);
            RefreshView();
        }

        public void AddWatermark(string imagePath)
        {
            var overlay = new VisualOverlay
            {
                Type = OverlayType.Watermark,
                Content = imagePath,
                Bounds = new Rectangle(10, 10, 100, 100),
                Opacity = 0.5f
            };
            var command = new AddOverlayCommand(overlay);
            _model.ExecuteCommand(command);
            RefreshView();
        }

        // Export
        public async Task ExportAsync(string outputPath, string format, IProgress<int> progress, CancellationToken token)
        {
            var appSettings = SettingsService.LoadSettings();
            var pipeline = new ExportPipeline(appSettings);
            await pipeline.ExportAsync(outputPath, format, _model.Frames, _model.ProjectSettings, progress, token);
        }

        // Border
        public void UpdateBorder(Color color, int thickness)
        {
            var command = new UpdateBorderCommand(color, thickness);
            _model.ExecuteCommand(command);
            RefreshView();
        }
    }

    public interface IEditorView
    {
        void DisplayFrames(IEnumerable<FrameItem> frames);
        void ShowFramePreview(FrameItem frame);
        void InvokeIfRequired(Action action);
        void SetPlaybackMode(bool playing);
        
        bool IsCropping { get; set; }
        Rectangle CropRegion { get; set; }
        Rectangle GetFinalCrop();

        List<VisualOverlay> Overlays { get; set; }
        Color BorderColor { get; set; }
        int BorderThickness { get; set; }
    }
}
