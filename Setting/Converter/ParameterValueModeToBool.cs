using System;
using System.Globalization;
using System.Windows.Data;
using SFVvCommon;

namespace Setting
{
    class ParameterValueModeToBool : IValueConverter
    {
        // ConverterParameterをEnumに変換するメソッド
        private ParameterValueMode ConvertFromConverterParameter(object parameter)
        {
            string parameterString = parameter as string;
            return (ParameterValueMode)Enum.Parse(typeof(ParameterValueMode), parameterString);
        }

        #region IValueConverter メンバー
        // Enum → bool
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // XAMLに定義されたConverterParameterをEnumに変換する
            ParameterValueMode parameterValue = ConvertFromConverterParameter(parameter);

            // ConverterParameterとバインディングソースの値が等しいか？
            return parameterValue.Equals(value);
        }

        // bool → Enum
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // true→falseの変化は無視する
            // ※こうすることで、選択されたラジオボタンだけをデータに反映させる
            if (!(bool)value)
                return System.Windows.DependencyProperty.UnsetValue;

            // ConverterParameterをEnumに変換して返す
            return ConvertFromConverterParameter(parameter);
        }
        #endregion    
    }
}
