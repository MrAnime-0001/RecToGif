using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;
using WinRT;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using RecToGif.Models;
using Device = SharpDX.Direct3D11.Device;

namespace RecToGif.Recorder
{
    public class RecorderEngine : IDisposable
    {
        private GraphicsCaptureItem? _captureItem;
        private Direct3D11CaptureFramePool? _framePool;
        private GraphicsCaptureSession? _captureSession;
        private Device _d3dDevice;
        private IDirect3DDevice _winrtDevice;

        private readonly FrameWriter _frameWriter;
        private readonly CaptureSession _session;
        private readonly InputHook _inputHook;
        private int _frameCount = 0;
        private volatile bool _isRecording = false;
        private volatile bool _isPaused = false;
        private long _lastFrameTimestampMs = 0;
        private readonly List<Task> _pendingSaveTasks = new();
        private readonly object _pendingLock = new();
        private byte[] _pixelBuffer = Array.Empty<byte>();
        private int _pixelBufferWidth = 0;
        private int _pixelBufferHeight = 0;
        private int _saveErrorCount = 0;

        public int FrameCount => _frameCount;

        public RecorderEngine(CaptureSession session, InputHook inputHook)
        {
            _session = session;
            _inputHook = inputHook;
            _frameWriter = new FrameWriter(session.OutputDirectory);

            _d3dDevice = new Device(DriverType.Hardware, DeviceCreationFlags.BgraSupport);
            using var dxgiDevice = _d3dDevice.QueryInterface<SharpDX.DXGI.Device3>();
            _winrtDevice = CreateDirect3DDevice(dxgiDevice.NativePointer);
        }

        [DllImport("d3d11.dll", EntryPoint = "CreateDirect3D11DeviceFromDXGIDevice", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern uint CreateDirect3D11DeviceFromDXGIDevice(IntPtr dxgiDevice, out IntPtr graphicsDevice);

        private static IDirect3DDevice CreateDirect3DDevice(IntPtr dxgiDevicePtr)
        {
            uint hr = CreateDirect3D11DeviceFromDXGIDevice(dxgiDevicePtr, out IntPtr graphicsDevicePtr);
            if (hr != 0) throw new COMException("Failed to create Direct3D11 device", (int)hr);

            var device = MarshalInterface<IDirect3DDevice>.FromAbi(graphicsDevicePtr);
            Marshal.Release(graphicsDevicePtr);
            return device;
        }

        public void Start(GraphicsCaptureItem item)
        {
            _captureItem = item;
            _captureItem.Closed += (s, e) => Stop();

            _framePool = Direct3D11CaptureFramePool.CreateFreeThreaded(
                _winrtDevice,
                DirectXPixelFormat.B8G8R8A8UIntNormalized,
                2,
                _captureItem.Size);

            _framePool.FrameArrived += OnFrameArrived;
            _captureSession = _framePool.CreateCaptureSession(_captureItem);

            _inputHook.Start();
            _lastFrameTimestampMs = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            _captureSession.StartCapture();
            _isRecording = true;
            _isPaused = false;
            _inputHook.ClearEvents();
        }

        public void Pause()
        {
            _isPaused = true;
        }

        public void Resume()
        {
            _isPaused = false;
            _lastFrameTimestampMs = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public void Stop()
        {
            _isRecording = false;
            _isPaused = false;
            _captureSession?.Dispose();
            _framePool?.Dispose();
            _captureItem = null;
            WaitForPendingSaves();
        }

        public void WaitForPendingSaves()
        {
            Task[] pending;
            lock (_pendingLock)
            {
                pending = _pendingSaveTasks.ToArray();
                _pendingSaveTasks.Clear();
            }
            try
            {
                Task.WaitAll(pending, TimeSpan.FromSeconds(10));
            }
            catch (AggregateException) { }
        }

        private void OnFrameArrived(Direct3D11CaptureFramePool sender, object args)
        {
            if (!_isRecording || _isPaused) return;

            using var frame = sender.TryGetNextFrame();
            if (frame == null) return;

            ProcessFrame(frame);
        }

        private void ProcessFrame(Direct3D11CaptureFrame frame)
        {
            int captureWidth = _captureItem!.Size.Width;
            int captureHeight = _captureItem!.Size.Height;

            int cropX = Math.Max(0, _session.TargetRegion.X);
            int cropY = Math.Max(0, _session.TargetRegion.Y);
            int outWidth = _session.TargetRegion.Width > 0 ? Math.Min(_session.TargetRegion.Width, captureWidth - cropX) : captureWidth;
            int outHeight = _session.TargetRegion.Height > 0 ? Math.Min(_session.TargetRegion.Height, captureHeight - cropY) : captureHeight;

            byte[] pixelData = CaptureSurfacePixels(frame.Surface, captureWidth, captureHeight, reuseBuffer: true);

            var bitmap = new Bitmap(outWidth, outHeight, PixelFormat.Format32bppPArgb);
            var bounds = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bitmapData = bitmap.LockBits(bounds, ImageLockMode.WriteOnly, bitmap.PixelFormat);

            try
            {
                int srcStride = captureWidth * 4;
                int destStride = bitmapData.Stride;
                for (int y = 0; y < outHeight; y++)
                {
                    int srcOffset = ((y + cropY) * captureWidth + cropX) * 4;
                    IntPtr destPtr = IntPtr.Add(bitmapData.Scan0, y * destStride);
                    Marshal.Copy(pixelData, srcOffset, destPtr, outWidth * 4);
                }
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }

            var cursorState = CursorCapture.GetCurrentState();
            long currentTimestampMs = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var meta = new FrameMeta
            {
                FrameIndex = Interlocked.Increment(ref _frameCount),
                TimestampMs = (long)frame.SystemRelativeTime.TotalMilliseconds,
                DelayMs = 1000 / _session.TargetFps,
                Cursor = new Models.CursorInfo
                {
                    Visible = cursorState.IsVisible,
                    X = cursorState.Position.X - _session.TargetRegion.X,
                    Y = cursorState.Position.Y - _session.TargetRegion.Y,
                    Type = "Default"
                },
                InputEvents = _inputHook.FlushEvents(_lastFrameTimestampMs, currentTimestampMs)
            };

            _lastFrameTimestampMs = currentTimestampMs;

            var saveTask = Task.Run(async () =>
            {
                try { await _frameWriter.SaveFrameAsync(bitmap, meta); }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref _saveErrorCount);
                    System.Diagnostics.Debug.WriteLine($"Save frame failed: {ex.Message}");
                }
            });
            lock (_pendingLock)
            {
                _pendingSaveTasks.Add(saveTask);
                _pendingSaveTasks.RemoveAll(t => t.IsCompleted);
            }
        }

        private byte[] CaptureSurfacePixels(IDirect3DSurface surface, int width, int height, bool reuseBuffer = false)
        {
            var nativeSurface = GetNativeSurface(surface);
            using var tex2D = nativeSurface.QueryInterface<SharpDX.Direct3D11.Texture2D>();

            var stagingDesc = new Texture2DDescription
            {
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.B8G8R8A8_UNorm,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Staging,
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.None
            };

            using var staging = new SharpDX.Direct3D11.Texture2D(_d3dDevice, stagingDesc);
            _d3dDevice.ImmediateContext.CopyResource(tex2D, staging);

            var mapSource = _d3dDevice.ImmediateContext.MapSubresource(staging, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);
            try
            {
                int stride = width * 4;
                int totalSize = height * stride;

                if (!reuseBuffer || totalSize > _pixelBuffer.Length)
                {
                    _pixelBuffer = new byte[totalSize];
                    _pixelBufferWidth = width;
                    _pixelBufferHeight = height;
                }
                var pixels = _pixelBuffer;

                for (int y = 0; y < height; y++)
                {
                    IntPtr srcPtr = IntPtr.Add(mapSource.DataPointer, y * mapSource.RowPitch);
                    Marshal.Copy(srcPtr, pixels, y * stride, stride);
                }
                return pixels;
            }
            finally
            {
                _d3dDevice.ImmediateContext.UnmapSubresource(staging, 0);
            }
        }

        private SharpDX.DXGI.Resource GetNativeSurface(IDirect3DSurface surface)
        {
            if (surface is not IWinRTObject winrtObj)
                throw new InvalidOperationException("Surface does not expose a native WinRT object");

            IntPtr inspectable = winrtObj.NativeObject.ThisPtr;
            Guid dxgiAccessGuid = new("A9B3D012-3DF2-4EE3-B8D1-8695F457D3C1");
            int hr = Marshal.QueryInterface(inspectable, ref dxgiAccessGuid, out IntPtr dxgiAccess);
            Marshal.ThrowExceptionForHR(hr);

            try
            {
                var getInterface = Marshal.GetDelegateForFunctionPointer<DxgiGetInterfaceDelegate>(
                    Marshal.ReadIntPtr(Marshal.ReadIntPtr(dxgiAccess), 3 * IntPtr.Size));
                Guid resourceGuid = typeof(SharpDX.DXGI.Resource).GUID;
                hr = getInterface(dxgiAccess, resourceGuid, out IntPtr resourcePtr);
                Marshal.ThrowExceptionForHR(hr);
                return new SharpDX.DXGI.Resource(resourcePtr);
            }
            finally
            {
                Marshal.Release(dxgiAccess);
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int DxgiGetInterfaceDelegate(IntPtr @this, Guid guid, out IntPtr ptr);

        public void Dispose()
        {
            Stop();
            _winrtDevice?.Dispose();
            _d3dDevice?.Dispose();
        }
    }
}
