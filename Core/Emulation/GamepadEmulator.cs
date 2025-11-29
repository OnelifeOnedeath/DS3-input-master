using System;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

namespace DS3InputMaster.Core.Emulation
{
    public class GamepadEmulator
    {
        private readonly InputSimulator _inputSimulator = new InputSimulator();
        private Thread _emulationThread;
        private bool _isRunning;
        private GamepadOutput _currentOutput;

        public void Start()
        {
            if (_isRunning) return;
            
            _isRunning = true;
            _emulationThread = new Thread(EmulationLoop);
            _emulationThread.Start();
        }

        public void Stop()
        {
            _isRunning = false;
            _emulationThread?.Join(1000);
        }

        public void SetOutput(GamepadOutput output)
        {
            _currentOutput = output;
        }

        private void EmulationLoop()
        {
            while (_isRunning)
            {
                try
                {
                    if (_currentOutput != null)
                    {
                        EmulateGamepad(_currentOutput);
                    }
                    Thread.Sleep(16); // ~60Hz
                }
                catch (Exception ex)
                {
                    // Log error
                }
            }
        }

        private void EmulateGamepad(GamepadOutput output)
        {
            // Emulate movement
            if (output.Movement.X != 0 || output.Movement.Y != 0)
            {
                var normalized = output.Movement.Normalized();
                // Convert to gamepad input
            }

            // Emulate camera
            if (output.Camera.X != 0 || output.Camera.Y != 0)
            {
                // Convert mouse movement to right stick
            }

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
                // Add other actions...
            }
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

        public static Vector2 Zero => new Vector2 { X = 0, Y = 0 };

        public Vector2 Normalized()
        {
            var length = (float)Math.Sqrt(X * X + Y * Y);
            return length > 0 ? new Vector2 { X = X / length, Y = Y / length } : Zero;
        }
    }
}
