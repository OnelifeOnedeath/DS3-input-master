using System;
using System.Collections.Generic;
using System.Linq;

namespace DS3InputMaster.Core
{
    /// <summary>
    /// Центральный контекст управления, связывающий физический ввод с игровыми действиями
    /// </summary>
    public class InputContext : IDisposable
    {
        private readonly InputCapture _inputCapture;
        private readonly GameStateDetector _gameStateDetector;
        private readonly ProfileManager _profileManager;
        private readonly InputInterpreter _inputInterpreter;
        
        // События для UI и отладки
        public event Action<InputEvent> InputProcessed;
        public event Action<GameState> GameStateChanged;
        
        public InputContext()
        {
            _profileManager = new ProfileManager();
            _gameStateDetector = new GameStateDetector();
            _inputInterpreter = new InputInterpreter();
            _inputCapture = new InputCapture();
            
            SetupEventHandlers();
        }
        
        private void SetupEventHandlers()
        {
            _inputCapture.RawInputReceived += OnRawInputReceived;
            _gameStateDetector.StateChanged += OnGameStateChanged;
        }
        
        private void OnRawInputReceived(RawInputData rawInput)
        {
            // Определяем контекст игры
            var gameState = _gameStateDetector.CurrentState;
            var activeProfile = _profileManager.GetActiveProfile();
            
            // Интерпретируем физический ввод в игровое намерение
            var playerIntent = _inputInterpreter.Interpret(rawInput, gameState, activeProfile);
            
            // Преобразуем в эмуляцию геймпада
            var gamepadOutput = GamepadEmulator.ConvertToGamepad(playerIntent);
            
            // Отправляем в игру
            GamepadEmulator.SendInput(gamepadOutput);
            
            InputProcessed?.Invoke(new InputEvent(rawInput, playerIntent, gamepadOutput));
        }
        
        private void OnGameStateChanged(GameState newState)
        {
            GameStateChanged?.Invoke(newState);
            
            // Автоматически применяем соответствующий профиль
            if (newState == GameState.InCombat)
                _profileManager.ApplyProfile("Combat");
            else if (newState == GameState.Exploring)
                _profileManager.ApplyProfile("Exploration");
        }
        
        public void ApplyProfile(string profileName)
        {
            _profileManager.ApplyProfile(profileName);
        }
        
        public void CalibrateMouseSensitivity()
        {
            // Метод для калибровки мыши под текущую аппаратную настройку
            var calibrationData = MouseCalibrator.PerformCalibration();
            _profileManager.UpdateActiveProfileSensitivity(calibrationData);
        }
        
        public void Dispose()
        {
            _inputCapture.Dispose();
            _gameStateDetector.Dispose();
        }
    }
    
    // Вспомогательные структуры для типизации
    public record InputEvent(RawInputData RawInput, PlayerIntent Intent, GamepadOutput Output);
    public record PlayerIntent(Vector2 Movement, Vector2 Camera, IReadOnlyList<GameAction> Actions);
}
