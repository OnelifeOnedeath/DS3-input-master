using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using DS3InputMaster.Models;

namespace DS3InputMaster.Converters
{
    public class StringToColorConverter : IValueConverter
    {
        public static StringToColorConverter Instance { get; } = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GameState gameState)
            {
                return gameState switch
                {
                    GameState.Exploring => Avalonia.Media.Brushes.Green,
                    GameState.InCombat => Avalonia.Media.Brushes.Red,
                    GameState.BossFight => Avalonia.Media.Brushes.DarkRed,
                    GameState.Aiming => Avalonia.Media.Brushes.Orange,
                    GameState.BowAiming => Avalonia.Media.Brushes.OrangeRed,
                    GameState.Parrying => Avalonia.Media.Brushes.Yellow,
                    GameState.Rolling => Avalonia.Media.Brushes.YellowGreen,
                    GameState.MenuNavigation => Avalonia.Media.Brushes.Blue,
                    GameState.Dialog => Avalonia.Media.Brushes.Purple,
                    GameState.Dead => Avalonia.Media.Brushes.Gray,
                    GameState.Loading => Avalonia.Media.Brushes.LightGray,
                    _ => Avalonia.Media.Brushes.Black
                };
            }
            return Avalonia.Media.Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
