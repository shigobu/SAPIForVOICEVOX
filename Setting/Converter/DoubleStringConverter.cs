using System;
using System.Globalization;
using System.Windows.Data;

namespace Setting
{
    /// <summary>
    /// 浮動小数点を文字列に変換するコンバーター。一桁版
    /// </summary>
    [ValueConversion(typeof(double), typeof(string))]
    class DoubleStringConverter : IValueConverter
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
