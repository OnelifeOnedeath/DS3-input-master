using System;
using System.Threading;
using WindowsInput;
using WindowsInput.Native;

namespace DS3InputMaster.Core.Emulation
{
    public class GamepadEmulator : IDisposable
    {
        private readonly InputSimulator _inputSimulator = new InputSimulator();
        private Thread _emulationThread;
        private bool _isRunning;
        private GamepadOutput _currentOutput;
        private readonly object _lockObject = new object();

        public void StartEmulation()
        {
            if (_isRunning) return;
            
            _isRunning = true;
            _emulationThread = new Thread(EmulationLoop);
            _emulationThread.IsBackground = true;
            _emulationThread.Start();
        }

        public void StopEmulation()
        {
            _isRunning = false;
            _emulationThread?.Join(1000);
        }

        public void UpdateState(GamepadOutput output)
        {
            lock (_lockObject)
            {
                _currentOutput = output;
            }
        }

        private void EmulationLoop()
        {
            while (_isRunning)
            {
                try
                {
                    GamepadOutput output;
                    lock (_lockObject)
                    {
                        output = _currentOutput;
                    }

                    if (output != null)
                    {
                        EmulateGamepad(output);
                    }
                    Thread.Sleep(16);
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // Log error
                }
            }
        }

        private void EmulateGamepad(GamepadOutput output)
        {
            // Emulate movement as WASD or arrow keys
            if (output.Movement.X > 0.1f)
                _inputSimulator.Keyboard.KeyDown(VirtualKeyCode.VK_D);
            else
                _inputSimulator.Keyboard.KeyUp(VirtualKeyCode.VK_D);

            if (output.Movement.X < -0.1f)
                _inputSimulator.Keyboard.KeyDown(VirtualKeyCode.VK_A);
            else
                _inputSimulator.Keyboard.KeyUp(VirtualKeyCode.VK_A);

            if (output.Movement.Y > 0.1f)
                _inputSimulator.Keyboard.KeyDown(VirtualKeyCode.VK_W);
            else
                _inputSimulator.Keyboard.KeyUp(VirtualKeyCode.VK_W);

            if (output.Movement.Y < -0.1f)
                _inputSimulator.Keyboard.KeyDown(VirtualKeyCode.VK_S);
            else
                _inputSimulator.Keyboard.KeyUp(VirtualKeyCode.VK_S);

            // Emulate actions
            foreach (var action in output.Actions)
            {
                EmulateAction(action);
            }
        }

        private void EmulateAction(GameAction action)
        {
            switch (action)
            {
                case GameAction.LightAttack:
                    _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.VK_J);
                    break;
                case GameAction.HeavyAttack:
                    _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.VK_K);
                    break;
                case GameAction.Block:
                    _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.VK_L);
                    break;
                case GameAction.Parry:
                    _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.VK_O);
                    break;
                case GameAction.UseItem:
                    _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.VK_Q);
                    break;
                case GameAction.Interact:
                    _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.VK_E);
                    break;
                case GameAction.Roll:
                    _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.VK_SPACE);
                    break;
                case GameAction.Jump:
                    _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.VK_SPACE);
                    break;
                case GameAction.Menu:
                    _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.ESCAPE);
                    break;
                case GameAction.SwitchSpell:
                    _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.VK_X);
                    break;
                case GameAction.SwitchItem:
                    _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.VK_Z);
                    break;
                case GameAction.TwoHand:
                    _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.VK_Y);
                    break;
                case GameAction.LockOn:
                    _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.VK_F);
                    break;
                case GameAction.MoveForward:
                case GameAction.MoveBackward:
                case GameAction.MoveLeft:
                case GameAction.MoveRight:
                    // These are handled by movement system
                    break;
            }
        }

        public void Dispose()
        {
            StopEmulation();
        }
    }

    public class GamepadOutput
    {
        public Vector2 Movement { get; set; }
        public Vector2 Camera { get; set; }
        public GameAction[] Actions { get; set; } = Array.Empty<GameAction>();
    }

    public struct Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static Vector2 Zero => new Vector2(0, 0);

        public Vector2 Normalized()
        {
            var length = (float)Math.Sqrt(X * X + Y * Y);
            return length > 0 ? new Vector2(X / length, Y / length) : Zero;
        }
    }
}
