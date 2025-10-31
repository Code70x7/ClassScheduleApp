using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace ClassScheduleApp.Converters
{
    /// <summary>
    /// Returns a slightly different color for even/odd rows.
    /// Bind the index as the value (e.g., from a CollectionView's Alternation pattern),
    /// or pass it via ConverterParameter if you prefer.
    /// </summary>
    public sealed class AlternatingColorConverter : IValueConverter
    {
        // Light gray tints by default; tweak as you like.
        public Color Even { get; set; } = Color.FromArgb("#F7F8FC");
        public Color Odd { get; set; } = Colors.White;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Try to get an integer index from value or parameter
            int index = 0;
            if (value is int i) index = i;
            else if (parameter is string s && int.TryParse(s, out var p)) index = p;

            return (index % 2 == 0) ? Even : Odd;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
    }
}
