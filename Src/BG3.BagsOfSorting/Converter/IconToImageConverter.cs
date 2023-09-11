using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;

namespace BG3.BagsOfSorting.Converter
{
    public class IconToImageConverter : IValueConverter
    {
        private static readonly ImageSourceConverter _imageSourceConverter = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fileName = $"{value}.png";
            var path = Path.Combine(Constants.ICONS_OUTPUT_PATH, fileName);

            if (File.Exists(path))
            {
                return ReadFile(path);
            }

            path = Path.Combine(Constants.CONTENT_CUSTOM_PATH, fileName);

            return File.Exists(path) ? ReadFile(path) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        private static object ReadFile(string path)
        {
            return _imageSourceConverter.ConvertFrom(File.ReadAllBytes(path));
        }
    }
}
