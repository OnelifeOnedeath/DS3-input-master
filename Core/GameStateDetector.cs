using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace DS3InputMaster.Core
{
    /// <summary>
    /// Детектор состояния игры Dark Souls 3 для контекстного управления
    /// </summary>
    public class GameStateDetector : IDisposable
    {
        private readonly ProcessMemoryReader _memoryReader;
        private readonly GameWindowObserver _windowObserver;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private Task _detectionTask;
        private GameState _currentState;
        private DateTime _lastStateChange;

        public event Action<GameState> StateChanged;

        public GameState CurrentState 
        { 
            get => _currentState;
            private set
            {
                if (_currentState != value)
                {
                    _currentState = value;
                    _lastStateChange = DateTime.Now;
                    StateChanged?.Invoke(value);
                }
            }
        }

        public GameStateDetector()
        {
            _memoryReader = new ProcessMemoryReader();
            _windowObserver = new GameWindowObserver();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void StartDetection()
        {
            if (_detectionTask != null && !_detectionTask.IsCompleted)
                return;

            _detectionTask = Task.Run(async () => await DetectionLoop(_cancellationTokenSource.Token));
        }

        public void StopDetection()
        {
            _cancellationTokenSource.Cancel();
            _detectionTask?.Wait(1000);
            _memoryReader.Dispose();
        }

        private async Task DetectionLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (!_memoryReader.IsConnected)
                    {
                        await TryFindGameProcess();
                        await Task.Delay(1000, cancellationToken);
                        continue;
                    }

                    var newState = await DetermineGameState();
                    CurrentState = newState;

                    await Task.Delay(50, cancellationToken); // 20Hz обновление
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // Логирование ошибок детекции
                    await Task.Delay(1000, cancellationToken);
                }
            }
        }

        private async Task TryFindGameProcess()
        {
            var processes = Process.GetProcessesByName("DarkSoulsIII");
            if (processes.Length == 0)
                processes = Process.GetProcessesByName("Dark Souls III");
                
            var gameProcess = processes.FirstOrDefault();
            if (gameProcess != null)
            {
                _memoryReader.Connect(gameProcess);
                _windowObserver.SetGameWindow(gameProcess.MainWindowHandle);
            }
        }

        private async Task<GameState> DetermineGameState()
        {
            // Проверяем активность окна
            if (!_windowObserver.IsGameWindowActive())
                return GameState.MenuNavigation;

            // Читаем состояние из памяти игры
            var gameData = await _memoryReader.ReadGameData();

            // Определяем состояние на основе данных
            if (gameData.IsPlayerDead)
                return GameState.Dead;

            if (gameData.IsInMenu)
                return GameState.MenuNavigation;

            if (gameData.IsInDialog)
                return GameState.Dialog;

            if (gameData.IsLoading)
                return GameState.Loading;

            if (gameData.IsAimingWithBow)
                return GameState.BowAiming;

            if (gameData.IsAiming)
                return GameState.Aiming;

            if (gameData.IsInBossFight)
                return GameState.BossFight;

            if (gameData.IsInCombat)
                return GameState.InCombat;

            if (gameData.IsParrying)
                return GameState.Parrying;

            if (gameData.IsRolling)
                return GameState.Rolling;

            return GameState.Exploring;
        }

        public void Dispose()
        {
            StopDetection();
            _cancellationTokenSource?.Dispose();
            _memoryReader?.Dispose();
        }
    }

    /// <summary>
    /// Читатель памяти процесса Dark Souls 3
    /// </summary>
    public class ProcessMemoryReader : IDisposable
    {
        private Process _gameProcess;
        private IntPtr _processHandle;

        public bool IsConnected => _gameProcess != null && !_gameProcess.HasExited;

        public void Connect(Process process)
        {
            _gameProcess = process;
            _processHandle = NativeMethods.OpenProcess(
                ProcessAccessFlags.VirtualMemoryRead, 
                false, 
                process.Id);
        }

        public async Task<GameData> ReadGameData()
        {
            if (!IsConnected)
                return new GameData();

            // Здесь будут реальные адреса памяти Dark Souls 3
            // Пока используем заглушку
            return await Task.Run(() => new GameData
            {
                IsPlayerDead = ReadBool(0x12345678),
                IsInMenu = ReadBool(0x12345679),
                IsInDialog = ReadBool(0x1234567A),
                IsLoading = ReadBool(0x1234567B),
                IsAiming = ReadBool(0x1234567C),
                IsAimingWithBow = ReadBool(0x1234567D),
                IsInCombat = ReadBool(0x1234567E),
                IsInBossFight = ReadBool(0x1234567F),
                IsParrying = ReadBool(0x12345680),
                IsRolling = ReadBool(0x12345681),
                Health = ReadFloat(0x12345682),
                Stamina = ReadFloat(0x12345683)
            });
        }

        private bool ReadBool(long address)
        {
            // Заглушка - в реальности чтение из памяти процесса
            return false;
        }

        private float ReadFloat(long address)
        {
            // Заглушка - в реальности чтение из памяти процесса
            return 0f;
        }

        public void Dispose()
        {
            if (_processHandle != IntPtr.Zero)
            {
                NativeMethods.CloseHandle(_processHandle);
                _processHandle = IntPtr.Zero;
            }
        }
    }

    /// <summary>
    /// Наблюдатель за окном игры
    /// </summary>
    public class GameWindowObserver
    {
        private IntPtr _gameWindowHandle;

        public void SetGameWindow(IntPtr handle)
        {
            _gameWindowHandle = handle;
        }

        public bool IsGameWindowActive()
        {
            if (_gameWindowHandle == IntPtr.Zero)
                return false;

            return NativeMethods.GetForegroundWindow() == _gameWindowHandle;
        }

        public bool IsGameWindowMinimized()
        {
            if (_gameWindowHandle == IntPtr.Zero)
                return false;

            return NativeMethods.IsIconic(_gameWindowHandle);
        }
    }

    /// <summary>
    /// Данные игры, читаемые из памяти
    /// </summary>
    public struct GameData
    {
        public bool IsPlayerDead { get; set; }
        public bool IsInMenu { get; set; }
        public bool IsInDialog { get; set; }
        public bool IsLoading { get; set; }
        public bool IsAiming { get; set; }
        public bool IsAimingWithBow { get; set; }
        public bool IsInCombat { get; set; }
        public bool IsInBossFight { get; set; }
        public bool IsParrying { get; set; }
        public bool IsRolling { get; set; }
        public float Health { get; set; }
        public float Stamina { get; set; }
    }

    // WinAPI импорты для работы с памятью и окнами
    internal static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr hWnd);
    }

    [Flags]
    public enum ProcessAccessFlags : uint
    {
        VirtualMemoryRead = 0x0010,
        VirtualMemoryWrite = 0x0020,
        VirtualMemoryOperation = 0x0008,
        QueryInformation = 0x0400
    }
}
