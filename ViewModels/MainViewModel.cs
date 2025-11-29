using System.Collections.ObjectModel;
using System.Windows.Input;
using ReactiveUI;

namespace DS3InputMaster.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private bool _isCapturingInput;
        private string _currentGameState = "Exploring";
        private string _selectedProfile = "Default";
        private string _statusMessage = "Готов к работе";

        public MainViewModel()
        {
            StartCommand = ReactiveCommand.Create(() => { });
            StopCommand = ReactiveCommand.Create(() => { });
            CalibrateCommand = ReactiveCommand.Create(() => { });
            CreateProfileCommand = ReactiveCommand.Create(() => { });
            SaveProfileCommand = ReactiveCommand.Create(() => { });
            DeleteProfileCommand = ReactiveCommand.Create(() => { });
        }
        
        public string StatusMessage 
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        public string CurrentGameState 
        {
            get => _currentGameState;
            set => this.RaiseAndSetIfChanged(ref _currentGameState, value);
        }

        public bool IsCapturingInput 
        {
            get => _isCapturingInput;
            set => this.RaiseAndSetIfChanged(ref _isCapturingInput, value);
        }

        public string SelectedProfile 
        {
            get => _selectedProfile;
            set => this.RaiseAndSetIfChanged(ref _selectedProfile, value);
        }

        public ObservableCollection<string> AvailableProfiles { get; } = new() { "Default", "PvP", "Magic" };
        public ObservableCollection<string> LogMessages { get; } = new() { "Система инициализирована" };
        
        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand CalibrateCommand { get; }
        public ICommand CreateProfileCommand { get; }
        public ICommand SaveProfileCommand { get; }
        public ICommand DeleteProfileCommand { get; }
    }

    public abstract class ViewModelBase : ReactiveUI.ReactiveObject { }
}
