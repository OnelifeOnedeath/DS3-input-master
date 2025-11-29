using System;
using System.Collections.Generic;
using System.Linq;
using DS3InputMaster.Models;
using DS3InputMaster.Models.InputProfiles;

namespace DS3InputMaster.Core.Interpretation
{
    public class InputInterpreter
    {
        private readonly InputHistory _history = new();
        private readonly IntentBuilder _intentBuilder = new();

        public PlayerIntent Interpret(RawInputData rawInput, GameState gameState, ControlProfile profile)
        {
            _history.AddInput(rawInput);
            
            var context = new InterpretationContext
            {
                RawInput = rawInput,
                GameState = gameState,
                Profile = profile,
                InputHistory = _history,
                CurrentTime = DateTime.Now
            };

            return _intentBuilder.BuildIntent(context);
        }

        public void ResetHistory()
        {
            _history.Clear();
        }
    }

    public class IntentBuilder
    {
        public PlayerIntent BuildIntent(InterpretationContext context)
        {
            var movement = CalculateMovement(context);
            var camera = CalculateCamera(context);
            var actions = DetectActions(context);

            return new PlayerIntent(movement, camera, actions);
        }

        private Vector2 CalculateMovement(InterpretationContext context)
        {
            var movement = new Vector2(0, 0);
            var profile = context.Profile.Movement;

            if (IsKeyPressed(context, GameAction.MoveForward))
                movement = new Vector2(movement.X, movement.Y + 1.0f);
            if (IsKeyPressed(context, GameAction.MoveBackward))
                movement = new Vector2(movement.X, movement.Y - 1.0f);
            if (IsKeyPressed(context, GameAction.MoveLeft))
                movement = new Vector2(movement.X - 1.0f, movement.Y);
            if (IsKeyPressed(context, GameAction.MoveRight))
                movement = new Vector2(movement.X + 1.0f, movement.Y);

            return ApplyResponseCurve(movement.Normalized(), profile.AnalogResponseCurve);
        }

        private Vector2 CalculateCamera(InterpretationContext context)
        {
            var mouse = context.RawInput.Mouse;
            var profile = context.Profile.Mouse;
            var sensitivity = GetContextSensitivity(context.GameState, profile);

            var cameraMovement = new Vector2(mouse.Movement.X, mouse.Movement.Y);
            cameraMovement = new Vector2(
                cameraMovement.X * sensitivity, 
                cameraMovement.Y * sensitivity
            );

            if (profile.InvertY)
                cameraMovement = new Vector2(cameraMovement.X, -cameraMovement.Y);

            return ApplySmoothing(cameraMovement, profile.Smoothing, context.InputHistory);
        }

        private IReadOnlyList<GameAction> DetectActions(InterpretationContext context)
        {
            var actions = new List<GameAction>();

            foreach (var binding in context.Profile.Bindings.Actions)
            {
                if (IsBindingActive(binding.Key, binding.Value, context))
                {
                    actions.Add(binding.Key);
                }
            }

            DetectComboActions(actions, context);

            return actions;
        }

        private bool IsBindingActive(GameAction action, InputBinding binding, InterpretationContext context)
        {
            if (binding.PrimaryKey != VirtualKey.None && 
                IsKeyActive(binding.PrimaryKey, binding.Modifiers, context))
                return true;

            if (binding.SecondaryKey != VirtualKey.None && 
                IsKeyActive(binding.SecondaryKey, binding.Modifiers, context))
                return true;

            if (binding.MouseButton != MouseButton.None && 
                IsMouseButtonActive(binding.MouseButton, context))
                return true;

            return false;
        }

        private bool IsKeyActive(VirtualKey key, InputModifier modifiers, InterpretationContext context)
        {
            var keyEvent = context.InputHistory.GetRecentKeyEvent(key);
            if (keyEvent.Action != KeyAction.Pressed) return false;

            return CheckModifiers(modifiers, context.RawInput.Keyboard);
        }

        private void DetectComboActions(List<GameAction> actions, InterpretationContext context)
        {
            if (actions.Contains(GameAction.Roll) && context.InputHistory.WasActionRecently(GameAction.LightAttack, 0.2f))
            {
                actions.Add(GameAction.LightAttack);
            }

            if (actions.Contains(GameAction.Parry) && IsInParryWindow(context))
            {
                actions.Add(GameAction.Parry);
            }
        }

        private bool IsKeyPressed(InterpretationContext context, GameAction action) 
        {
            var binding = context.Profile.Bindings.GetBinding(action);
            return IsBindingActive(action, binding, context);
        }

        private Vector2 ApplyResponseCurve(Vector2 input, float curve) 
        {
            return input;
        }

        private Vector2 ApplySmoothing(Vector2 input, float smoothing, InputHistory history) 
        {
            return input;
        }

        private float GetContextSensitivity(GameState state, MouseSettings mouse) 
        {
            return state switch
            {
                GameState.Aiming => mouse.AimingSensitivity,
                GameState.BowAiming => mouse.BowSensitivity,
                _ => mouse.CameraSensitivity
            };
        }

        private bool IsMouseButtonActive(MouseButton button, InterpretationContext context) 
        {
            return false;
        }

        private bool CheckModifiers(InputModifier required, KeyboardEvent actual) 
        {
            return false;
        }

        private bool IsInParryWindow(InterpretationContext context) 
        {
            return false;
        }
    }

    public class InterpretationContext
    {
        public required RawInputData RawInput { get; set; }
        public required GameState GameState { get; set; }
        public required ControlProfile Profile { get; set; }
        public required InputHistory InputHistory { get; set; }
        public required DateTime CurrentTime { get; set; }
    }

    public class InputHistory
    {
        public void AddInput(RawInputData input) { }
        public void Clear() { }
        public KeyboardEvent GetRecentKeyEvent(VirtualKey key) => new KeyboardEvent();
        public bool WasActionRecently(GameAction action, float seconds) => false;
    }
}
