using System;
using System.Runtime.InteropServices;
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

        public string CurrentSourceDescription => _selectedItem != null 
            ? $"Window: {_selectedItem.DisplayName}" 
            : (_selectedRegion.HasValue ? $"Region: {_selectedRegion.Value.Width}x{_selectedRegion.Value.Height}" : "No source selected");

        public bool HasSource => _selectedItem != null || _selectedRegion.HasValue;

        public bool IsPaused { get; private set; }

        public RecorderPresenter(IRecorderView view)
        {
            _view = view;
            _shortcutService = new ShortcutService(_globalHook);
            _globalHook.Start();

            _shortcutService.OnStartRecording += async (s, e) => await StartRecordingAsync();
            _shortcutService.OnPauseRecording += (s, e) => TogglePause();
            _shortcutService.OnStopRecording += async (s, e) => await StopRecordingAsync();
            _shortcutService.OnDiscardRecording += (s, e) => DiscardRecording();
        }

        public void TogglePause()
        {
            if (_engine == null) return;

            if (IsPaused)
            {
                ResumeRecording();
            }
            else
            {
                PauseRecording();
            }
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
            _view.OnSourceChanged(CurrentSourceDescription);
        }

        public async Task StartRecordingAsync()
        {
            if (HasSource && _engine != null)
            {
                TogglePause();
                return;
            }

            if (!HasSource)
            {
                _view.ShowInlineMessage("Choose a region or window first");
                return;
            }

            if (_engine != null) return; // Already recording

            GraphicsCaptureItem? item = _selectedItem;

            if (item == null && _selectedRegion.HasValue)
            {
                // Try to get monitor from region
                var rect = new RECT 
                { 
                    Left = _selectedRegion.Value.Left, 
                    Top = _selectedRegion.Value.Top, 
                    Right = _selectedRegion.Value.Right, 
                    Bottom = _selectedRegion.Value.Bottom 
                };
                IntPtr hMonitor = MonitorFromRect(ref rect, 2); // MONITOR_DEFAULTTONEAREST

                if (hMonitor != IntPtr.Zero)
                {
                    try
                    {
                        item = CreateItemForMonitor(hMonitor);
                    }
                    catch (Exception ex)
                    {
                        _view.ShowInlineMessage("Failed to capture monitor: " + ex.Message);
                        return;
                    }
                }
            }
            
            if (item == null) return;

            SettingsService.EnsureDirectoriesExist();
            string outputDir = SettingsService.CreateNewTempSessionFolder();

            _currentSession = new CaptureSession
            {
                OutputDirectory = outputDir,
                TargetFps = 15
            };

            if (_selectedRegion.HasValue && _selectedItem == null)
            {
                // We are capturing a monitor but only want a region
                // We need to find the monitor's coordinates to offset the region
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

            if (_currentSession != null && System.IO.Directory.Exists(_currentSession.OutputDirectory))
            {
                System.IO.Directory.Delete(_currentSession.OutputDirectory, true);
            }
            _view.OnRecordingDiscarded();
        }

        public int GetFrameCount() => _engine?.FrameCount ?? 0;

        #region Interop
        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromRect([In] ref RECT lprc, uint dwFlags);

        [DllImport("combase.dll", PreserveSig = true, CharSet = CharSet.Unicode)]
        private static extern int RoGetActivationFactory(string activatableClassId, ref Guid iid, out IntPtr factory);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left; public int Top; public int Right; public int Bottom; }

        [ComImport]
        [Guid("3628E81B-3CAC-4C60-B7F4-23CE0E0C3356")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IGraphicsCaptureItemInterop
        {
            void CreateForWindow(
                [In] IntPtr window,
                [In] ref Guid iid,
                [Out] out IntPtr result);

            void CreateForMonitor(
                [In] IntPtr monitor,
                [In] ref Guid iid,
                [Out] out IntPtr result);
        }

        private GraphicsCaptureItem CreateItemForMonitor(IntPtr hMonitor)
        {
            Guid interopIid = new Guid("3628E81B-3CAC-4C60-B7F4-23CE0E0C3356");
            int hr = RoGetActivationFactory("Windows.Graphics.Capture.GraphicsCaptureItem", ref interopIid, out IntPtr factoryPtr);
            Marshal.ThrowExceptionForHR(hr);

            var interop = (IGraphicsCaptureItemInterop)Marshal.GetObjectForIUnknown(factoryPtr);
            Marshal.Release(factoryPtr);

            Guid iid = new Guid("79C3F95B-31F7-4EC2-A464-632EF5D30760");
            interop.CreateForMonitor(hMonitor, ref iid, out IntPtr itemPtr);
            Marshal.ReleaseComObject(interop);
            try
            {
                return WinRT.MarshalInspectable<GraphicsCaptureItem>.FromAbi(itemPtr);
            }
            finally
            {
                Marshal.Release(itemPtr);
            }
        }
        #endregion
    }

    public interface IRecorderView
    {
        IntPtr Handle { get; }
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

