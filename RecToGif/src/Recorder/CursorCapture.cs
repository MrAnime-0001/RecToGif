using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace RecToGif.Recorder
{
    public class CursorCapture
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CURSORINFO
        {
            public int cbSize;
            public int flags;
            public IntPtr hCursor;
            public POINT ptScreenPos;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ICONINFO
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        private const int CURSOR_SHOWING = 0x00000001;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetCursorInfo(out CURSORINFO pci);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetIconInfo(IntPtr hIcon, out ICONINFO piconinfo);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DrawIconEx(IntPtr hdc, int xLeft, int yTop, IntPtr hIcon, int cxWidth, int cyWidth, int istepIfAniCur, IntPtr hbrFlickerFreeDraw, int diFlags);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        private const int DI_NORMAL = 0x0003;

        public struct CurrentCursorState
        {
            public bool IsVisible;
            public Point Position;
            public Point Hotspot;
            public IntPtr Handle;
        }

        public static CurrentCursorState GetCurrentState()
        {
            CURSORINFO ci = new CURSORINFO { cbSize = Marshal.SizeOf(typeof(CURSORINFO)) };
            if (GetCursorInfo(out ci))
            {
                var state = new CurrentCursorState
                {
                    IsVisible = (ci.flags & CURSOR_SHOWING) != 0,
                    Position = new Point(ci.ptScreenPos.x, ci.ptScreenPos.y),
                    Handle = ci.hCursor
                };

                if (state.IsVisible && ci.hCursor != IntPtr.Zero)
                {
                    if (GetIconInfo(ci.hCursor, out ICONINFO ii))
                    {
                        state.Hotspot = new Point(ii.xHotspot, ii.yHotspot);
                        if (ii.hbmColor != IntPtr.Zero) DeleteObject(ii.hbmColor);
                        if (ii.hbmMask != IntPtr.Zero) DeleteObject(ii.hbmMask);
                    }
                }

                return state;
            }

            return default;
        }

        public static void DrawCursor(Graphics g, CurrentCursorState state, Point offset)
        {
            if (!state.IsVisible || state.Handle == IntPtr.Zero) return;

            IntPtr hdc = g.GetHdc();
            try
            {
                if (!DrawIconEx(hdc,
                    state.Position.X - offset.X - state.Hotspot.X,
                    state.Position.Y - offset.Y - state.Hotspot.Y,
                    state.Handle, 0, 0, 0, IntPtr.Zero, DI_NORMAL))
                {
                    System.Diagnostics.Debug.WriteLine("[CursorCapture] DrawIconEx failed");
                }
            }
            finally
            {
                g.ReleaseHdc(hdc);
            }
        }
    }
}
