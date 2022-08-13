using SFVvCommon;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Setting
{
    class SynthesisSettingModeToBool : IValueConverter
    {
        // ConverterParameterをEnumに変換するメソッド
        private SynthesisSettingMode ConvertFromConverterParameter(object parameter)
        {
            string parameterString = parameter as string;
            return (SynthesisSettingMode)Enum.Parse(typeof(SynthesisSettingMode), parameterString);
        }

        #region IValueConverter メンバー
        // Enum → bool
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // XAMLに定義されたConverterParameterをEnumに変換する
            SynthesisSettingMode parameterValue = ConvertFromConverterParameter(parameter);

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
