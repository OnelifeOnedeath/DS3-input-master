using System.Collections.ObjectModel;
using System.Windows.Input;
using ReactiveUI;

namespace DS3InputMaster.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            StartCommand = ReactiveCommand.Create(() => { });
        }
        
        public string StatusMessage => "Готов к работе";
        public ObservableCollection<string> AvailableProfiles { get; } = new() { "Default" };
        public ObservableCollection<string> LogMessages { get; } = new() { "Запущено" };
        public ICommand StartCommand { get; }
    }

    public abstract class ViewModelBase : ReactiveUI.ReactiveObject { }
}
