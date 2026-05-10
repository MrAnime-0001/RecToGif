using System;
using System.Threading.Tasks;
using Windows.Graphics.Capture;
using WinRT.Interop;
using RecToGif.Models;
using RecToGif.Recorder;
using RecToGif.Services;

namespace RecToGif.Presenters
{
    public class RecorderPresenter
    {
        private readonly IRecorderView _view;
        private RecorderEngine? _engine;
        private CaptureSession? _currentSession;
        private readonly InputHook _globalHook = new();
        private readonly ShortcutService _shortcutService;

        private GraphicsCaptureItem? _selectedItem;
        private System.Drawing.Rectangle? _selectedRegion;
        private static GraphicsCaptureItem? _regionConsentItem;

        public string CurrentSourceDescription => _selectedItem != null
            ? $"Window: {_selectedItem.DisplayName}"
            : (_selectedRegion.HasValue ? $"Region: {_selectedRegion.Value.Width}x{_selectedRegion.Value.Height}" : "No source selected");

        public bool HasSource => _selectedItem != null || _selectedRegion.HasValue;
        public bool IsPaused { get; private set; }
        public bool IsRecording => _engine != null;

        public RecorderPresenter(IRecorderView view)
        {
            _view = view;
            _shortcutService = new ShortcutService(_globalHook);
            _globalHook.Start();

            _shortcutService.OnStartRecording += async (s, e) => await StartRecordingAsync();
            _shortcutService.OnPauseRecording += (s, e) => TogglePause();
            _shortcutService.OnStopRecording += async (s, e) => await StopRecordingAsync();
            _shortcutService.OnDiscardRecording += (s, e) => DiscardRecording();

            // Restore or create default region
            var settings = SettingsService.LoadSettings();
            if (settings.LastRegion.HasValue)
            {
                _selectedRegion = settings.LastRegion.Value;
            }
            else
            {
                // Default: 800x600 centered on primary screen
                var screen = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
                int w = Math.Min(800, screen.Width);
                int h = Math.Min(600, screen.Height);
                _selectedRegion = new System.Drawing.Rectangle(
                    (screen.Width - w) / 2,
                    (screen.Height - h) / 2,
                    w, h
                );
                settings.LastRegion = _selectedRegion;
                SettingsService.SaveSettings(settings);
            }

            _view.OnSourceChanged(CurrentSourceDescription!);
        }

        public void TogglePause()
        {
            if (_engine == null) return;

            if (IsPaused)
                ResumeRecording();
            else
                PauseRecording();
        }

        public async Task SelectWindowAsync()
        {
            var picker = new GraphicsCapturePicker();
            InitializeWithWindow.Initialize(picker, _view.Handle);
            var item = await picker.PickSingleItemAsync();
            if (item != null)
            {
                _selectedItem = item;
                _selectedRegion = null;
                _view.OnSourceChanged(CurrentSourceDescription);
            }
        }

        public void SetRegion(System.Drawing.Rectangle region)
        {
            _selectedRegion = region;
            _selectedItem = null;

            // Persist
            var settings = SettingsService.LoadSettings();
            settings.LastRegion = region;
            SettingsService.SaveSettings(settings);

            _view.OnSourceChanged(CurrentSourceDescription);
        }

        public async Task StartRecordingAsync()
        {
            if (_engine != null)
            {
                // Already recording — toggle pause
                if (HasSource)
                {
                    TogglePause();
                    return;
                }
                return;
            }

            if (!HasSource)
            {
                _view.ShowInlineMessage("Choose a region or window first");
                return;
            }

            GraphicsCaptureItem? item = _selectedItem;

            if (item == null && _selectedRegion.HasValue)
            {
                if (_regionConsentItem == null)
                {
                    var picker = new GraphicsCapturePicker();
                    InitializeWithWindow.Initialize(picker, _view.Handle);
                    _regionConsentItem = await picker.PickSingleItemAsync().AsTask().ConfigureAwait(true);

                    if (_regionConsentItem == null)
                    {
                        _view.ShowInlineMessage("Capture consent denied.");
                        return;
                    }
                }

                item = _regionConsentItem;
            }

            if (item == null) return;

            SettingsService.EnsureDirectoriesExist();
            string outputDir = SettingsService.CreateNewTempSessionFolder();

            var settings = SettingsService.LoadSettings();
            _currentSession = new CaptureSession
            {
                OutputDirectory = outputDir,
                TargetFps = settings.DefaultFps,
                CaptureCursor = settings.CaptureCursor
            };

            if (_selectedRegion.HasValue && _selectedItem == null)
            {
                var screen = System.Windows.Forms.Screen.FromRectangle(_selectedRegion.Value);
                _currentSession.TargetRegion = new System.Drawing.Rectangle(
                    _selectedRegion.Value.X - screen.Bounds.X,
                    _selectedRegion.Value.Y - screen.Bounds.Y,
                    _selectedRegion.Value.Width,
                    _selectedRegion.Value.Height
                );
            }
            else
            {
                _currentSession.TargetRegion = new System.Drawing.Rectangle(0, 0, item.Size.Width, item.Size.Height);
            }

            _engine = new RecorderEngine(_currentSession, _globalHook);
            _engine.Start(item);

            IsPaused = false;
            _view.OnRecordingStarted();
        }

        public void PauseRecording()
        {
            if (_engine == null) return;
            _engine.Pause();
            IsPaused = true;
            _view.OnRecordingPaused();
        }

        public void ResumeRecording()
        {
            if (_engine == null) return;
            _engine.Resume();
            IsPaused = false;
            _view.OnRecordingResumed();
        }

        public async Task StopRecordingAsync()
        {
            if (_engine == null) return;

            _engine.Stop();
            _engine.Dispose();
            _engine = null;

            if (_currentSession != null)
            {
                bool success = await EditorPresenter.OpenWithSession(_currentSession.OutputDirectory);
                if (!success)
                {
                    _view.OnRecordingStopped();
                    return;
                }
            }

            _view.OnRecordingStopped();
            _view.Close();
        }

        public void DiscardRecording()
        {
            if (_engine != null)
            {
                _engine.Stop();
                _engine.Dispose();
                _engine = null;
            }

            var dir = _currentSession?.OutputDirectory;
            _currentSession = null;

            if (dir != null && System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.Delete(dir, true);
            }
            _view.OnRecordingDiscarded();
        }

        public int GetFrameCount() => _engine?.FrameCount ?? 0;
    }

    public interface IRecorderView
    {
        IntPtr Handle { get; }
        int FormWidth { get; }
        int FormHeight { get; }
        Point GetFormPosition();
        void SetFormPosition(Point pos);
        void OnRecordingStarted();
        void OnRecordingPaused();
        void OnRecordingResumed();
        void OnRecordingStopped();
        void OnRecordingDiscarded();
        void Close();
        void OnSourceChanged(string description);
        void ShowInlineMessage(string message);
    }
}
