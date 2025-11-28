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
                    GameState.Exploring => Brushes.Green,
                    GameState.InCombat => Brushes.Red,
                    GameState.BossFight => Brushes.DarkRed,
                    GameState.Aiming => Brushes.Orange,
                    GameState.BowAiming => Brushes.OrangeRed,
                    GameState.Parrying => Brushes.Yellow,
                    GameState.Rolling => Brushes.YellowGreen,
                    GameState.MenuNavigation => Brushes.Blue,
                    GameState.Dialog => Brushes.Purple,
                    GameState.Dead => Brushes.Gray,
                    GameState.Loading => Brushes.LightGray,
                    _ => Brushes.Black
                };
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
