using System;
using System.Globalization;
using System.Windows.Data;

namespace Setting
{
    class DoubleStringConverter2 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double doubleValue))
            {
                return "";
            }
            return doubleValue.ToString("F2");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0.0;
        }
    }
}
