using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DS3InputMaster.Core;
using DS3InputMaster.Configuration;
using DS3InputMaster.ViewModels;
using DS3InputMaster.Views;

namespace DS3InputMaster
{
    public class App : Application
    {
        private InputContext _inputContext;
        private ProfileManager _profileManager;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override async void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Инициализация ядра приложения
                await InitializeCoreSystems();
                
                // Создаем главное окно
                var mainWindow = new MainWindow
                {
                    DataContext = new MainViewModel(_inputContext, _profileManager)
                };
                
                desktop.MainWindow = mainWindow;
                
                // Запускаем систему ввода после загрузки UI
                desktop.MainWindow.Closed += (s, e) => Shutdown();
                _ = Task.Run(() => _inputContext?.Start());
            }

            base.OnFrameworkInitializationCompleted();
        }

        private async Task InitializeCoreSystems()
        {
            try
            {
                // Инициализация менеджера профилей
                _profileManager = new ProfileManager();
                await _profileManager.InitializeAsync();
                
                // Создание контекста ввода
                _inputContext = new InputContext(_profileManager);
                
                // Подписка на события для логирования
                _inputContext.InputProcessed += OnInputProcessed;
                _inputContext.GameStateChanged += OnGameStateChanged;
                _profileManager.ProfileChanged += OnProfileChanged;
                
            }
            catch (Exception ex)
            {
                // Обработка ошибок инициализации
                Console.WriteLine($"Ошибка инициализации: {ex.Message}");
            }
        }

        private void OnInputProcessed(InputEvent inputEvent)
        {
            // Логирование обработки ввода (для отладки)
            // Console.WriteLine($"Обработан ввод: {inputEvent.Intent.Actions.Count} действий");
        }

        private void OnGameStateChanged(GameState newState)
        {
            // Автоматическое переключение профилей по состоянию игры
            switch (newState)
            {
                case GameState.InCombat:
                case GameState.BossFight:
                    _profileManager.ApplyProfile("PvP");
                    break;
                case GameState.Exploring:
                    _profileManager.ApplyProfile("Default");
                    break;
                case GameState.Aiming:
                case GameState.BowAiming:
                    _profileManager.ApplyProfile("Magic");
                    break;
            }
        }

        private void OnProfileChanged(ControlProfile profile)
        {
            Console.WriteLine($"Активирован профиль: {profile.Name}");
        }

        private void Shutdown()
        {
            _inputContext?.Dispose();
            _profileManager = null;
        }
    }

    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Критическая ошибка: {ex}");
                // Здесь можно показать MessageBox с ошибкой
            }
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
    }
}
