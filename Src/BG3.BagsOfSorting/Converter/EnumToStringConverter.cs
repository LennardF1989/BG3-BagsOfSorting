using System.Globalization;
using System.Windows.Data;

namespace BG3.BagsOfSorting.Converter
{
    public class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            try
            {
                return Enum.GetName(value.GetType(), value);
            }
            catch
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            try
            {
                return Enum.Parse(targetType, value.ToString()!);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
