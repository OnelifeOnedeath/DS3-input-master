using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Input;

namespace DS3InputMaster.Core.InputCapture
{
    /// <summary>
    /// Перехватчик низкоуровневого ввода с клавиатуры и мыши
    /// </summary>
    public class InputCapture : IDisposable
    {
        private readonly LowLevelMouseProc _mouseProc;
        private readonly LowLevelKeyboardProc _keyboardProc;
        private IntPtr _mouseHookId = IntPtr.Zero;
        private IntPtr _keyboardHookId = IntPtr.Zero;
        private bool _isCapturing = false;

        public event Action<RawInputData> RawInputReceived;

        public InputCapture()
        {
            _mouseProc = MouseHookCallback;
            _keyboardProc = KeyboardHookCallback;
        }

        public void StartCapture()
        {
            if (_isCapturing) return;

            using (Process process = Process.GetCurrentProcess())
            using (ProcessModule module = process.MainModule)
            {
                _keyboardHookId = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardProc, GetModuleHandle(module.ModuleName), 0);
                _mouseHookId = SetWindowsHookEx(WH_MOUSE_LL, _mouseProc, GetModuleHandle(module.ModuleName), 0);
            }

            _isCapturing = true;
        }

        public void StopCapture()
        {
            if (!_isCapturing) return;

            UnhookWindowsHookEx(_keyboardHookId);
            UnhookWindowsHookEx(_mouseHookId);
            
            _isCapturing = false;
        }

        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var keyboardStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                var keyAction = GetKeyAction(wParam);
                
                var keyboardEvent = new KeyboardEvent
                {
                    Key = (VirtualKey)keyboardStruct.vkCode,
                    Action = keyAction,
                    IsShiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift),
                    IsCtrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl),
                    IsAltPressed = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)
                };

                var inputData = new RawInputData
                {
                    Keyboard = keyboardEvent,
                    Mouse = new MouseEvent(),
                    Timestamp = DateTime.Now
                };

                RawInputReceived?.Invoke(inputData);
            }

            return CallNextHookEx(_keyboardHookId, nCode, wParam, lParam);
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var mouseStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                var mouseEvent = new MouseEvent
                {
                    Position = new Point(mouseStruct.pt.x, mouseStruct.pt.y),
                    Movement = new Point(mouseStruct.pt.x, mouseStruct.pt.y), // TODO: Вычислять относительное движение
                    Buttons = GetMouseButtons(wParam),
                    WheelDelta = GetWheelDelta(wParam, mouseStruct)
                };

                var inputData = new RawInputData
                {
                    Keyboard = new KeyboardEvent(),
                    Mouse = mouseEvent,
                    Timestamp = DateTime.Now
                };

                RawInputReceived?.Invoke(inputData);
            }

            return CallNextHookEx(_mouseHookId, nCode, wParam, lParam);
        }

        private KeyAction GetKeyAction(IntPtr wParam)
        {
            return wParam switch
            {
                (IntPtr)WM_KEYDOWN => KeyAction.Pressed,
                (IntPtr)WM_KEYUP => KeyAction.Released,
                (IntPtr)WM_SYSKEYDOWN => KeyAction.Pressed,
                (IntPtr)WM_SYSKEYUP => KeyAction.Released,
                _ => KeyAction.None
            };
        }

        private MouseButton GetMouseButtons(IntPtr wParam)
        {
            return wParam switch
            {
                (IntPtr)WM_LBUTTONDOWN => MouseButton.Left,
                (IntPtr)WM_RBUTTONDOWN => MouseButton.Right,
                (IntPtr)WM_MBUTTONDOWN => MouseButton.Middle,
                (IntPtr)WM_XBUTTONDOWN => MouseButton.X1 | MouseButton.X2,
                _ => MouseButton.None
            };
        }

        private int GetWheelDelta(IntPtr wParam, MSLLHOOKSTRUCT mouseStruct)
        {
            if (wParam == (IntPtr)WM_MOUSEWHEEL)
            {
                return (short)((mouseStruct.mouseData >> 16) & 0xFFFF);
            }
            return 0;
        }

        public void Dispose()
        {
            StopCapture();
        }

        // WinAPI импорты
        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_XBUTTONDOWN = 0x020B;

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int x; public int y; }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
