using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace DS3InputMaster.Core.Emulation
{
    public class GamepadEmulator : IDisposable
    {
        private readonly XInputState _currentState;
        private readonly XInputState _previousState;
        private bool _isEmulating;
        private Thread _emulationThread;
        private readonly object _stateLock = new object();

        public GamepadEmulator()
        {
            _currentState = new XInputState();
            _previousState = new XInputState();
        }

        public void StartEmulation()
        {
            if (_isEmulating) return;

            _isEmulating = true;
            _emulationThread = new Thread(EmulationLoop)
            {
                Name = "GamepadEmulation",
                IsBackground = true
            };
            _emulationThread.Start();
        }

        public void StopEmulation()
        {
            _isEmulating = false;
            _emulationThread?.Join(1000);
            ResetState();
        }

        public void UpdateState(GamepadOutput output)
        {
            lock (_stateLock)
            {
                _previousState.CopyFrom(_currentState);

                _currentState.LeftThumbX = ScaleAnalogValue(output.Movement.X);
                _currentState.LeftThumbY = ScaleAnalogValue(output.Movement.Y);
                _currentState.RightThumbX = ScaleAnalogValue(output.Camera.X);
                _currentState.RightThumbY = ScaleAnalogValue(output.Camera.Y);

                UpdateButtonsFromActions(output.Actions);
            }
        }

        private void UpdateButtonsFromActions(IReadOnlyList<GameAction> actions)
        {
            _currentState.Buttons = 0;

            foreach (var action in actions)
            {
                switch (action)
                {
                    case GameAction.LightAttack:
                        _currentState.Buttons |= XInputButtons.X;
                        break;
                    case GameAction.HeavyAttack:
                        _currentState.Buttons |= XInputButtons.Y;
                        break;
                    case GameAction.Parry:
                        _currentState.Buttons |= XInputButtons.LB;
                        break;
                    case GameAction.Roll:
                        _currentState.Buttons |= XInputButtons.B;
                        break;
                    case GameAction.UseItem:
                        _currentState.Buttons |= XInputButtons.RB;
                        break;
                    case GameAction.Jump:
                        _currentState.Buttons |= XInputButtons.A;
                        break;
                    case GameAction.Sprint:
                        _currentState.Buttons |= XInputButtons.LeftThumb;
                        break;
                    case GameAction.LockOn:
                        _currentState.Buttons |= XInputButtons.RightThumb;
                        break;
                    case GameAction.Menu:
                        _currentState.Buttons |= XInputButtons.Start;
                        break;
                }
            }
        }

        private short ScaleAnalogValue(float value)
        {
            return (short)(value * (value >= 0 ? 32767 : 32768));
        }

        private void EmulationLoop()
        {
            while (_isEmulating)
            {
                try
                {
                    lock (_stateLock)
                    {
                        EmulateStateChanges();
                    }
                    Thread.Sleep(8);
                }
                catch (ThreadAbortException) { break; }
                catch (Exception ex) { }
            }
        }

        private void EmulateStateChanges()
        {
            EmulateButtonPresses();
            EmulateThumbMovement();
            EmulateTriggers();
        }

        private void EmulateButtonPresses() { }
        private void EmulateThumbMovement() { }
        private void EmulateTriggers() { }

        private void ResetState()
        {
            lock (_stateLock)
            {
                _currentState.Reset();
                _previousState.Reset();
            }
        }

        public void Dispose()
        {
            StopEmulation();
        }
    }

    public class XInputState
    {
        public short LeftThumbX { get; set; }
        public short LeftThumbY { get; set; }
        public short RightThumbX { get; set; }
        public short RightThumbY { get; set; }
        public byte LeftTrigger { get; set; }
        public byte RightTrigger { get; set; }
        public XInputButtons Buttons { get; set; }

        public void Reset()
        {
            LeftThumbX = 0;
            LeftThumbY = 0;
            RightThumbX = 0;
            RightThumbY = 0;
            LeftTrigger = 0;
            RightTrigger = 0;
            Buttons = 0;
        }

        public void CopyFrom(XInputState other)
        {
            LeftThumbX = other.LeftThumbX;
            LeftThumbY = other.LeftThumbY;
            RightThumbX = other.RightThumbX;
            RightThumbY = other.RightThumbY;
            LeftTrigger = other.LeftTrigger;
            RightTrigger = other.RightTrigger;
            Buttons = other.Buttons;
        }
    }

    [Flags]
    public enum XInputButtons : ushort
    {
        None = 0,
        DPadUp = 0x0001,
        DPadDown = 0x0002,
        DPadLeft = 0x0004,
        DPadRight = 0x0008,
        Start = 0x0010,
        Back = 0x0020,
        LeftThumb = 0x0040,
        RightThumb = 0x0080,
        LB = 0x0100,
        RB = 0x0200,
        A = 0x1000,
        B = 0x2000,
        X = 0x4000,
        Y = 0x8000
    }

    public class GamepadOutput
    {
        public Vector2 Movement { get; set; }
        public Vector2 Camera { get; set; }
        public IReadOnlyList<GameAction> Actions { get; set; } = Array.Empty<GameAction>();
        public float LeftTrigger { get; set; }
        public float RightTrigger { get; set; }
    }

    public struct Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }
        
        public static Vector2 Zero => new Vector2 { X = 0, Y = 0 };
        
        public Vector2 Normalized()
        {
            var length = (float)Math.Sqrt(X * X + Y * Y);
            return length > 0 ? new Vector2 { X = X / length, Y = Y / length } : Zero;
        }
    }
}
