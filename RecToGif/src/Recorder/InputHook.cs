using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using RecToGif.Models;

namespace RecToGif.Recorder
{
    public class InputHook : IInputHook
    {
        private readonly ConcurrentQueue<InputEvent> _eventBuffer = new();
        private Thread? _hookThread;
        private bool _isRunning;
        private IntPtr _keyboardHookId = IntPtr.Zero;
        private IntPtr _mouseHookId = IntPtr.Zero;
        private LowLevelKeyboardProc? _keyboardProc;
        private LowLevelMouseProc? _mouseProc;
        private ManualResetEvent _stopEvent = new(false);

        public event EventHandler<string>? ShortcutPressed;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;

        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_MBUTTONDOWN = 0x0207;

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public int mouseData;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        public void Start()
        {
            if (_isRunning) return;
            _isRunning = true;

            _hookThread = new Thread(RunHookLoop)
            {
                IsBackground = true,
                Name = "InputHookThread"
            };
            _hookThread.SetApartmentState(ApartmentState.STA);
            _hookThread.Start();
        }

        public void Stop()
        {
            if (!_isRunning) return;
            _isRunning = false;

            _stopEvent.Set();

            if (_hookThread != null && _hookThread.IsAlive)
            {
                _hookThread.Join(5000);
            }
            _stopEvent.Reset();
            _eventBuffer.Clear();
        }

        public void ClearEvents()
        {
            _eventBuffer.Clear();
        }

        private void RunHookLoop()
        {
            _keyboardProc = KeyboardHookCallback;
            _mouseProc = MouseHookCallback;

            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule!)
            {
                IntPtr moduleHandle = GetModuleHandle(curModule.ModuleName);
                _keyboardHookId = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardProc, moduleHandle, 0);
                _mouseHookId = SetWindowsHookEx(WH_MOUSE_LL, _mouseProc, moduleHandle, 0);
            }

            if (_keyboardHookId == IntPtr.Zero || _mouseHookId == IntPtr.Zero)
            {
                if (_keyboardHookId != IntPtr.Zero) UnhookWindowsHookEx(_keyboardHookId);
                if (_mouseHookId != IntPtr.Zero) UnhookWindowsHookEx(_mouseHookId);
                _keyboardHookId = IntPtr.Zero;
                _mouseHookId = IntPtr.Zero;
                _isRunning = false;
                return;
            }

            RunMessagePump();

            if (_keyboardHookId != IntPtr.Zero) UnhookWindowsHookEx(_keyboardHookId);
            if (_mouseHookId != IntPtr.Zero) UnhookWindowsHookEx(_mouseHookId);

            _keyboardHookId = IntPtr.Zero;
            _mouseHookId = IntPtr.Zero;
        }

        private void RunMessagePump()
        {
            while (_isRunning)
            {
                var safeHandle = _stopEvent.SafeWaitHandle;
                IntPtr handlePtr = safeHandle.DangerousGetHandle();
                int waitResult = MsgWaitForMultipleObjects(new IntPtr[] { handlePtr }, false, 100, 0xff);
                GC.KeepAlive(safeHandle); // ensure handle is not finalized during the native call
                if (waitResult == -1)
                {
                    // WAIT_FAILED (-1) means the wait handle became invalid — exit
                    break;
                }
                // Dispatch any pending WM messages (low-level hooks need a running message loop)
                PeekMessage(out var msg, IntPtr.Zero, 0, 0, 0x01); // PM_REMOVE
            }
        }

        [DllImport("user32.dll")]
        private static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

        [DllImport("user32.dll")]
        private static extern int MsgWaitForMultipleObjects(IntPtr[] handles, bool waitAll, int milliseconds, uint wakeMask);

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
        }

        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (!_isRunning) return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);

            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;
                
                // Skip modifier keys themselves from being the primary key in a combo event
                // but we still want to track them if they are part of a combo.
                if (IsModifierKey(key))
                {
                    return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
                }

                string keyStr = GetKeyComboString(key);

                ShortcutPressed?.Invoke(this, keyStr);
                
                _eventBuffer.Enqueue(new InputEvent
                {
                    Type = "KeyDown",
                    Key = keyStr,
                    TimestampMs = DateTimeOffset.Now.ToUnixTimeMilliseconds()
                });
            }
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private bool IsModifierKey(Keys key)
        {
            return key == Keys.ControlKey || key == Keys.LControlKey || key == Keys.RControlKey ||
                   key == Keys.ShiftKey || key == Keys.LShiftKey || key == Keys.RShiftKey ||
                   key == Keys.Menu || key == Keys.LMenu || key == Keys.RMenu;
        }

        private string GetKeyComboString(Keys key)
        {
            var modifiers = new List<string>();
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control) modifiers.Add("Ctrl");
            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) modifiers.Add("Shift");
            if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt) modifiers.Add("Alt");

            if (modifiers.Count > 0)
            {
                return string.Join("+", modifiers) + "+" + key.ToString();
            }
            return key.ToString();
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                string? button = null;
                if (wParam == (IntPtr)WM_LBUTTONDOWN) button = "Left";
                else if (wParam == (IntPtr)WM_RBUTTONDOWN) button = "Right";
                else if (wParam == (IntPtr)WM_MBUTTONDOWN) button = "Middle";

                if (button != null)
                {
                    var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                    _eventBuffer.Enqueue(new InputEvent
                    {
                        Type = "MouseClick",
                        Button = button,
                        X = hookStruct.pt.x,
                        Y = hookStruct.pt.y,
                        TimestampMs = DateTimeOffset.Now.ToUnixTimeMilliseconds()
                    });
                }
            }
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        public List<InputEvent> FlushEvents(long startTimestampMs, long endTimestampMs)
        {
            var events = new List<InputEvent>();
            while (_eventBuffer.TryDequeue(out var ev))
            {
                if (ev.TimestampMs <= endTimestampMs && ev.TimestampMs >= startTimestampMs)
                {
                    events.Add(ev);
                }
            }
            return events;
        }

        public void Dispose()
        {
            Stop();
            _stopEvent?.Dispose();
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, Delegate lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);


    }
}
