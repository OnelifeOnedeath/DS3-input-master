using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using DS3InputMaster.Core;
using DS3InputMaster.Configuration;
using DS3InputMaster.Models;
using DS3InputMaster.Models.InputProfiles;
using ReactiveUI;

namespace DS3InputMaster.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly InputContext _inputContext;
        private readonly ProfileManager _profileManager;
        
        private string _statusMessage = "Готов к работе";
        private bool _isConnected;
        private GameState _currentGameState;
        private ControlProfile _selectedProfile;
        private bool _isCapturingInput;

        public MainViewModel(InputContext inputContext, ProfileManager profileManager)
        {
            _inputContext = inputContext;
            _profileManager = profileManager;

            SetupEventHandlers();
            InitializeCommands();
            LoadProfiles();
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        public bool IsConnected
        {
            get => _isConnected;
            set => this.RaiseAndSetIfChanged(ref _isConnected, value);
        }

        public GameState CurrentGameState
        {
            get => _currentGameState;
            set => this.RaiseAndSetIfChanged(ref _currentGameState, value);
        }

        public ControlProfile SelectedProfile
        {
            get => _selectedProfile;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedProfile, value);
                if (value != null)
                {
                    _inputContext.ApplyCustomProfile(value.Name);
                }
            }
        }

        public bool IsCapturingInput
        {
            get => _isCapturingInput;
            set
            {
                this.RaiseAndSetIfChanged(ref _isCapturingInput, value);
                if (value) StartSystem(); else StopSystem();
            }
        }

        public ObservableCollection<ControlProfile> AvailableProfiles { get; } = new();
        public ObservableCollection<string> LogMessages { get; } = new();

        public ICommand StartCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand CalibrateCommand { get; private set; }
        public ICommand CreateProfileCommand { get; private set; }
        public ICommand SaveProfileCommand { get; private set; }
        public ICommand DeleteProfileCommand { get; private set; }

        private void SetupEventHandlers()
        {
            _inputContext.GameStateChanged += OnGameStateChanged;
            _inputContext.InputProcessed += OnInputProcessed;
            _inputContext.ErrorOccurred += OnErrorOccurred;

            _profileManager.ProfileChanged += OnProfileChanged;
            _profileManager.ProfileSaved += OnProfileSaved;
            _profileManager.ProfileLoaded += OnProfileLoaded;
        }

        private void InitializeCommands()
        {
            StartCommand = ReactiveCommand.Create(StartSystem);
            StopCommand = ReactiveCommand.Create(StopSystem);
            CalibrateCommand = ReactiveCommand.Create(CalibrateMouse);
            CreateProfileCommand = ReactiveCommand.Create(CreateNewProfile);
            SaveProfileCommand = ReactiveCommand.CreateFromTask(SaveCurrentProfile);
            DeleteProfileCommand = ReactiveCommand.CreateFromTask(DeleteCurrentProfile);
        }

        private void LoadProfiles()
        {
            AvailableProfiles.Clear();
            foreach (var profile in _profileManager.LoadedProfiles.Values)
            {
                AvailableProfiles.Add(profile);
            }
            
            SelectedProfile = _profileManager.ActiveProfile;
        }

        private void StartSystem()
        {
            try
            {
                _inputContext.Start();
                IsConnected = true;
                StatusMessage = "Система активна - Dark Souls 3 обнаружена";
                AddLog("Система управления запущена");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка запуска: {ex.Message}";
                AddLog($"Ошибка: {ex.Message}");
            }
        }

        private void StopSystem()
        {
            try
            {
                _inputContext.Stop();
                IsConnected = false;
                StatusMessage = "Система остановлена";
                AddLog("Система управления остановлена");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка остановки: {ex.Message}";
                AddLog($"Ошибка: {ex.Message}");
            }
        }

        private void CalibrateMouse()
        {
            _inputContext.CalibrateMouseSensitivity();
            AddLog("Калибровка мыши выполнена");
        }

        private void CreateNewProfile()
        {
            var newProfile = _profileManager.CreateNewProfile($"Профиль_{AvailableProfiles.Count + 1}");
            AvailableProfiles.Add(newProfile);
            SelectedProfile = newProfile;
            AddLog($"Создан новый профиль: {newProfile.Name}");
        }

        private async Task SaveCurrentProfile()
        {
            if (SelectedProfile != null)
            {
                await _inputContext.SaveCurrentProfileAsync();
                AddLog($"Профиль сохранен: {SelectedProfile.Name}");
            }
        }

        private async Task DeleteCurrentProfile()
        {
            if (SelectedProfile != null && SelectedProfile.Name != "Default")
            {
                var profileName = SelectedProfile.Name;
                await _profileManager.DeleteProfileAsync(profileName);
                AvailableProfiles.Remove(SelectedProfile);
                SelectedProfile = AvailableProfiles.FirstOrDefault();
                AddLog($"Профиль удален: {profileName}");
            }
        }

        private void OnGameStateChanged(GameState newState)
        {
            Dispatcher.UIThread.Post(() =>
            {
                CurrentGameState = newState;
                StatusMessage = $"Состояние: {GetGameStateDescription(newState)}";
            });
        }

        private void OnInputProcessed(InputEvent inputEvent)
        {
            // Для отладки - логируем интенсивный ввод
            if (inputEvent.Intent.Actions.Count > 0)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    AddLog($"Действия: {string.Join(", ", inputEvent.Intent.Actions)}");
                });
            }
        }

        private void OnErrorOccurred(string errorMessage)
        {
            Dispatcher.UIThread.Post(() =>
            {
                StatusMessage = $"Ошибка: {errorMessage}";
                AddLog($"Ошибка: {errorMessage}");
            });
        }

        private void OnProfileChanged(ControlProfile profile)
        {
            Dispatcher.UIThread.Post(() =>
            {
                SelectedProfile = profile;
                AddLog($"Применен профиль: {profile.Name}");
            });
        }

        private void OnProfileSaved(string profileName)
        {
            Dispatcher.UIThread.Post(() =>
            {
                AddLog($"Профиль сохранен: {profileName}");
            });
        }

        private void OnProfileLoaded(string profileName)
        {
            Dispatcher.UIThread.Post(() =>
            {
                AddLog($"Профиль загружен: {profileName}");
            });
        }

        private void AddLog(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            LogMessages.Insert(0, $"[{timestamp}] {message}");
            
            // Ограничиваем лог 100 сообщениями
            if (LogMessages.Count > 100)
            {
                LogMessages.RemoveAt(LogMessages.Count - 1);
            }
        }

        private string GetGameStateDescription(GameState state)
        {
            return state switch
            {
                GameState.Exploring => "Исследование",
                GameState.InCombat => "Бой",
                GameState.BossFight => "Босс",
                GameState.Aiming => "Прицеливание",
                GameState.BowAiming => "Прицеливание луком",
                GameState.Parrying => "Парирование",
                GameState.Rolling => "Уворот",
                GameState.MenuNavigation => "Меню",
                GameState.Dialog => "Диалог",
                GameState.Dead => "Смерть",
                GameState.Loading => "Загрузка",
                _ => "Неизвестно"
            };
        }

        public override void Dispose()
        {
            StopSystem();
            base.Dispose();
        }
    }

    public abstract class ViewModelBase : ReactiveObject, IDisposable
    {
        public virtual void Dispose() { }
    }
}
