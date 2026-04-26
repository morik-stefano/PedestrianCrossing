using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PedestrianCrossing.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isGreen && isGreen)
                return new SolidColorBrush(Colors.Green);
            return new SolidColorBrush(Colors.Red);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}