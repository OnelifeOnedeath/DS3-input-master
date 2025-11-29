using Avalonia;
using System;
using System.Threading;

namespace DS3InputMaster
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Запуск Avalonia...");
                var app = BuildAvaloniaApp();
                app.StartWithClassicDesktopLifetime(args);
                Console.WriteLine("Приложение завершено");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА: {ex}");
                Console.ReadLine();
            }
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
    }
}
