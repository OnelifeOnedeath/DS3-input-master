using System.Collections.ObjectModel;
using System.Windows.Input;
using ReactiveUI;

namespace DS3InputMaster.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private string _statusMessage = "Готов к работе";
        
        public MainViewModel()
        {
            AvailableProfiles = new ObservableCollection<string> { "Default", "PvP", "Magic" };
            LogMessages = new ObservableCollection<string> { "Система инициализирована" };
            
            StartCommand = ReactiveCommand.Create(StartSystem);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        public ObservableCollection<string> AvailableProfiles { get; }
        public ObservableCollection<string> LogMessages { get; }
        public ICommand StartCommand { get; }

        private void StartSystem()
        {
            StatusMessage = "Система запущена!";
            LogMessages.Add("Система управления активирована");
        }
    }

    public abstract class ViewModelBase : ReactiveUI.ReactiveObject { }
}
