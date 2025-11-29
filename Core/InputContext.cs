using System;
using System.Collections.Generic;
using System.Linq;
using DS3InputMaster.Configuration;
using DS3InputMaster.Core.Emulation;
using DS3InputMaster.Core.Interpretation;
using DS3InputMaster.Models;
using DS3InputMaster.Models.InputProfiles;

namespace DS3InputMaster.Core
{
    public class InputContext : IDisposable
    {
        private readonly InputCapture.InputCapture _inputCapture;
        private readonly GameStateDetector _gameStateDetector;
        private readonly ProfileManager _profileManager;
        private readonly InputInterpreter _inputInterpreter;
        private readonly GamepadEmulator _gamepadEmulator;
        
        private bool _isRunning;
        private GameState _currentGameState;

        public event Action<InputEvent> InputProcessed;
        public event Action<GameState> GameStateChanged;
        public event Action<string> ErrorOccurred;

        public InputContext(ProfileManager profileManager)
        {
            _profileManager = profileManager ?? throw new ArgumentNullException(nameof(profileManager));
            
            _gameStateDetector = new GameStateDetector();
            _inputInterpreter = new InputInterpreter();
            _gamepadEmulator = new GamepadEmulator();
            _inputCapture = new InputCapture.InputCapture();

            SetupEventHandlers();
        }

        private void SetupEventHandlers()
        {
            _inputCapture.RawInputReceived += OnRawInputReceived;
            _gameStateDetector.StateChanged += OnGameStateChanged;
            _profileManager.ProfileChanged += OnProfileChanged;
        }

        public void Start()
        {
            if (_isRunning) return;

            try
            {
                _gameStateDetector.StartDetection();
                _inputCapture.StartCapture();
                _gamepadEmulator.StartEmulation();
                
                _isRunning = true;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"Ошибка запуска: {ex.Message}");
            }
        }

        public void Stop()
        {
            if (!_isRunning) return;

            try
            {
                _inputCapture.StopCapture();
                _gameStateDetector.StopDetection();
                _gamepadEmulator.StopEmulation();
                
                _isRunning = false;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"Ошибка остановки: {ex.Message}");
            }
        }

        private void OnRawInputReceived(InputCapture.RawInputData rawInput)
        {
            if (!_isRunning) return;

            try
            {
                var gameState = _currentGameState;
                var activeProfile = _profileManager.ActiveProfile;

                var playerIntent = _inputInterpreter.Interpret(rawInput, gameState, activeProfile);

                var gamepadOutput = new GamepadOutput
                {
                    Movement = playerIntent.Movement,
                    Camera = playerIntent.Camera,
                    Actions = playerIntent.Actions
                };

                _gamepadEmulator.UpdateState(gamepadOutput);

                InputProcessed?.Invoke(new InputEvent(rawInput, playerIntent, gamepadOutput));
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"Ошибка обработки ввода: {ex.Message}");
            }
        }

        private void OnGameStateChanged(GameState newState)
        {
            _currentGameState = newState;
            GameStateChanged?.Invoke(newState);
            ApplyContextAwareProfile(newState);
        }

        private void OnProfileChanged(ControlProfile newProfile)
        {
            _inputInterpreter.ResetHistory();
        }

        private void ApplyContextAwareProfile(GameState gameState)
        {
            string profileName = gameState switch
            {
                GameState.InCombat or GameState.BossFight => "PvP",
                GameState.Aiming or GameState.BowAiming => "Magic", 
                GameState.Exploring => "Default",
                _ => null
            };

            if (profileName != null && _profileManager.LoadedProfiles.ContainsKey(profileName))
            {
                _profileManager.ApplyProfile(profileName);
            }
        }

        public void Dispose()
        {
            Stop();
            
            _inputCapture?.Dispose();
            _gameStateDetector?.Dispose();
            _gamepadEmulator?.Dispose();
        }

        public bool IsRunning => _isRunning;
        public GameState CurrentGameState => _currentGameState;
        public ControlProfile ActiveProfile => _profileManager.ActiveProfile;
    }

    public record InputEvent(InputCapture.RawInputData RawInput, PlayerIntent Intent, GamepadOutput Output);
    public record PlayerIntent
    {
        public Vector2 Movement { get; init; } = Vector2.Zero;
        public Vector2 Camera { get; init; } = Vector2.Zero;
        public IReadOnlyList<GameAction> Actions { get; init; } = Array.Empty<GameAction>();
    }
}
