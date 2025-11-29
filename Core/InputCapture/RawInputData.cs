using System;
using System.Drawing;

namespace DS3InputMaster.Core.InputCapture
{
    public struct RawInputData
    {
        public KeyboardEvent Keyboard { get; set; }
        public MouseEvent Mouse { get; set; }
        public DateTime Timestamp { get; set; }
        
        public bool HasInput => Keyboard.IsActive || Mouse.IsActive;
    }

    public struct KeyboardEvent
    {
        public VirtualKey Key { get; set; }
        public KeyAction Action { get; set; }
        public bool IsShiftPressed { get; set; }
        public bool IsCtrlPressed { get; set; }
        public bool IsAltPressed { get; set; }
        
        public bool IsActive => Action != KeyAction.None;
    }

    public struct MouseEvent
    {
        public Point Position { get; set; }
        public Point Movement { get; set; }
        public MouseButton Buttons { get; set; }
        public int WheelDelta { get; set; }
        
        public bool IsActive => Movement.X != 0 || Movement.Y != 0 || Buttons != MouseButton.None || WheelDelta != 0;
    }

    public enum KeyAction
    {
        None,
        Pressed,
        Released,
        Repeated
    }

    [Flags]
    public enum MouseButton
    {
        None = 0,
        Left = 1,
        Right = 2,
        Middle = 4,
        X1 = 8,
        X2 = 16
    }
}
