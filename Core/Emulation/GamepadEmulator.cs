using System;
using System.Threading;
using System.Windows.Input; // Встроенный .NET 8.0 API для ввода

namespace DS3InputMaster.Core.Emulation
{
    public class GamepadEmulator : IDisposable
    {
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
            // Эмуляция клавиш через встроенный .NET API
            // В реальном приложении здесь будет вызов WinAPI или использование UI Automation
            // Сейчас это заглушка для компиляции
            
            // Логируем действия для отладки
            if (output.Actions.Length > 0)
            {
                System.Diagnostics.Debug.WriteLine($"Actions: {string.Join(", ", output.Actions)}");
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
