using System;
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

namespace RecToGif.Recorder
{
    public class RecorderEngine : IDisposable
    {
        private GraphicsCaptureItem? _captureItem;
        private Direct3D11CaptureFramePool? _framePool;
        private GraphicsCaptureSession? _captureSession;
        private SharpDX.Direct3D11.Device _d3dDevice;
        private SharpDX.Direct3D11.Texture2D? _stagingTexture;
        private IDirect3DDevice _winrtDevice;
        
        private readonly FrameWriter _frameWriter;
        private readonly CaptureSession _session;
        private readonly InputHook _inputHook;
        private int _frameCount = 0;
        private bool _isRecording = false;
        private bool _isPaused = false;
        private long _lastFrameTimestampMs = 0;

        public int FrameCount => _frameCount;

        public RecorderEngine(CaptureSession session, InputHook inputHook)
        {
            _session = session;
            _inputHook = inputHook;
            _frameWriter = new FrameWriter(session.OutputDirectory);

            // Initialize D3D11 Device
            _d3dDevice = new SharpDX.Direct3D11.Device(DriverType.Hardware, DeviceCreationFlags.BgraSupport);
            
            // Get the WinRT device
            using var dxgiDevice = _d3dDevice.QueryInterface<SharpDX.DXGI.Device3>();
            _winrtDevice = CreateDirect3DDevice(dxgiDevice.NativePointer);
        }

        [DllImport("d3d11.dll", EntryPoint = "CreateDirect3D11DeviceFromDXGIDevice", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern uint CreateDirect3D11DeviceFromDXGIDevice(IntPtr dxgiDevice, out IntPtr graphicsDevice);

        private static IDirect3DDevice CreateDirect3DDevice(IntPtr dxgiDevicePtr)
        {
            uint hr = CreateDirect3D11DeviceFromDXGIDevice(dxgiDevicePtr, out IntPtr graphicsDevicePtr);
            if (hr != 0) throw new COMException("Failed to create Direct3D11 device", (int)hr);
            
            var device = WinRT.MarshalInterface<IDirect3DDevice>.FromAbi(graphicsDevicePtr);
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

            // Use TargetRegion size if it's set and smaller than capture item, otherwise use item size
            int width = _session.TargetRegion.Width > 0 ? _session.TargetRegion.Width : _captureItem.Size.Width;
            int height = _session.TargetRegion.Height > 0 ? _session.TargetRegion.Height : _captureItem.Size.Height;

            _stagingTexture = new SharpDX.Direct3D11.Texture2D(_d3dDevice, new Texture2DDescription
            {
                Width = _captureItem.Size.Width,
                Height = _captureItem.Size.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.B8G8R8A8_UNorm,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Staging,
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.None
            });

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
            _stagingTexture?.Dispose();
            _captureItem = null;
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
            using var sourceTexture = GetSharpDXTexture(frame.Surface);
            _d3dDevice.ImmediateContext.CopyResource(sourceTexture, _stagingTexture);

            var mapSource = _d3dDevice.ImmediateContext.MapSubresource(_stagingTexture, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);
            try
            {
                int captureWidth = _captureItem!.Size.Width;
                int captureHeight = _captureItem!.Size.Height;
                
                int cropX = Math.Max(0, _session.TargetRegion.X);
                int cropY = Math.Max(0, _session.TargetRegion.Y);
                int width = _session.TargetRegion.Width > 0 ? Math.Min(_session.TargetRegion.Width, captureWidth - cropX) : captureWidth;
                int height = _session.TargetRegion.Height > 0 ? Math.Min(_session.TargetRegion.Height, captureHeight - cropY) : captureHeight;

                var bitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
                var bounds = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                var bitmapData = bitmap.LockBits(bounds, ImageLockMode.WriteOnly, bitmap.PixelFormat);

                for (int y = 0; y < height; y++)
                {
                    IntPtr sourcePtr = IntPtr.Add(mapSource.DataPointer, (y + cropY) * mapSource.RowPitch + (cropX * 4));
                    IntPtr destPtr = IntPtr.Add(bitmapData.Scan0, y * bitmapData.Stride);
                    CopyMemory(destPtr, sourcePtr, (uint)(width * 4));
                }

                bitmap.UnlockBits(bitmapData);

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

                Task.Run(async () =>
                {
                    try { await _frameWriter.SaveFrameAsync(bitmap, meta); }
                    finally { bitmap.Dispose(); }
                });
            }
            finally
            {
                _d3dDevice.ImmediateContext.UnmapSubresource(_stagingTexture, 0);
            }
        }

        private unsafe SharpDX.Direct3D11.Texture2D GetSharpDXTexture(IDirect3DSurface surface)
        {
            if (surface is not IWinRTObject winrtObj)
                throw new InvalidOperationException("Surface does not expose a native WinRT object");

            IntPtr inspectable = winrtObj.NativeObject.ThisPtr;

            // QI for IDirect3DDxgiInterfaceAccess (GUID: 37648AC8-...)
            Guid dxgiAccessGuid = new("A9B3D012-3DF2-4EE3-B8D1-8695F457D3C1");
            int hr = Marshal.QueryInterface(inspectable, ref dxgiAccessGuid, out IntPtr dxgiAccess);
            Marshal.ThrowExceptionForHR(hr);

            try
            {
                // vtable layout: [0]QI [1]AddRef [2]Release [3]GetInterface
                void** vtbl = *(void***)dxgiAccess;
                var getInterface = (delegate* unmanaged[Stdcall]<IntPtr, Guid*, void**, int>)vtbl[3];
                Guid resourceGuid = typeof(SharpDX.DXGI.Resource).GUID;
                void* resourcePtr;
                hr = getInterface(dxgiAccess, &resourceGuid, &resourcePtr);
                Marshal.ThrowExceptionForHR(hr);
                using var resource = new SharpDX.DXGI.Resource((IntPtr)resourcePtr);
                return resource.QueryInterface<SharpDX.Direct3D11.Texture2D>();
            }
            finally
            {
                Marshal.Release(dxgiAccess);
            }
        }

        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
        private static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        public void Dispose()
        {
            Stop();
            _winrtDevice?.Dispose();
            _d3dDevice?.Dispose();
        }
    }
}
