using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DS3InputMaster.ViewModels;

namespace DS3InputMaster.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            DataContext = new MainViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
