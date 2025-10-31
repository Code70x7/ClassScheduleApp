using Microsoft.Maui.Controls;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;

namespace ClassScheduleApp.Converters
{
    public class AlternatingBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values is null || values.Length < 2 || values[0] is null || values[1] is null)
                return new SolidColorBrush(Colors.Transparent);

            var item = values[0];
            var itemsSource = values[1];

            // Find index of "item" in "itemsSource"
            int index = 0;
            if (itemsSource is IList list)
            {
                index = list.IndexOf(item);
            }
            else if (itemsSource is IEnumerable enumerable)
            {
                int i = 0;
                foreach (var it in enumerable)
                {
                    if (ReferenceEquals(it, item) || Equals(it, item))
                    {
                        index = i;
                        break;
                    }
                    i++;
                }
            }

            bool dark = Application.Current?.RequestedTheme == AppTheme.Dark;

            // soft alternating greys (light/dark aware)
            var lightA = Color.FromArgb("#F5F7FB");
            var lightB = Color.FromArgb("#ECEFF5");
            var darkA = Color.FromArgb("#15171B");
            var darkB = Color.FromArgb("#101216");

            var color = (index % 2 == 0)
                ? (dark ? darkA : lightA)
                : (dark ? darkB : lightB);

            return new SolidColorBrush(color);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
