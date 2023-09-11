using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BG3.BagsOfSorting.Converter
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = value?.GetType();

            if (type == null)
            {
                return Visibility.Collapsed;
            }

            if (type == typeof(bool))
            {
                return (bool)value 
                    ? Visibility.Visible 
                    : Visibility.Collapsed;
            }

            //If not null, show it.
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
