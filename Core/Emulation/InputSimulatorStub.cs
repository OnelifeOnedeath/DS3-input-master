namespace WindowsInput
{
    public class InputSimulator
    {
        public KeyboardSimulator Keyboard { get; } = new KeyboardSimulator();
        public MouseSimulator Mouse { get; } = new MouseSimulator();
    }

    public class KeyboardSimulator
    {
        public void KeyDown(object key) { }
        public void KeyUp(object key) { }
        public void KeyPress(object key) { }
    }

    public class MouseSimulator
    {
        public void MoveMouseBy(int x, int y) { }
    }
}

namespace WindowsInput.Native
{
    public enum VirtualKeyCode
    {
        VK_A = 0x41,
        VK_B = 0x42,
        VK_C = 0x43,
        VK_D = 0x44,
        VK_E = 0x45,
        VK_F = 0x46,
        VK_G = 0x47,
        VK_H = 0x48,
        VK_I = 0x49,
        VK_J = 0x4A,
        VK_K = 0x4B,
        VK_L = 0x4C,
        VK_M = 0x4D,
        VK_N = 0x4E,
        VK_O = 0x4F,
        VK_P = 0x50,
        VK_Q = 0x51,
        VK_R = 0x52,
        VK_S = 0x53,
        VK_T = 0x54,
        VK_U = 0x55,
        VK_V = 0x56,
        VK_W = 0x57,
        VK_X = 0x58,
        VK_Y = 0x59,
        VK_Z = 0x5A,
        VK_SPACE = 0x20,
        ESCAPE = 0x1B
    }
}
