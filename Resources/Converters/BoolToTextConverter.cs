// File: Resources/Converters/BoolToTextConverter.cs
using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace ClassScheduleApp.Converters
{
    /// <summary>
    /// Turns a bool into text.  Pass the two options with ConverterParameter
    /// like "On|Off".  If omitted, defaults to "✓|✗".
    /// </summary>
    public sealed class BoolToTextConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // parameter format: "TrueText|FalseText"
            var parts = (parameter as string)?.Split('|') ?? new[] { "✓", "✗" };

            var isTrue = value is bool b && b;
            return isTrue ? parts[0] : parts.Length > 1 ? parts[1] : string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => false; // one-way converter
    }
}
