using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;

namespace StyleRegistrationTool.View
{
    /// <summary>
    /// ChangePortWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ChangePortWindow : Window , INotifyPropertyChanged
    {
        public ChangePortWindow(int port)
        {
            InitializeComponent();

            DataContext = this;
            
            //プリセット作成
            portComboBox.Items.Add(new Model.NameAndPort("VOICEVOX", 50021));
            portComboBox.Items.Add(new Model.NameAndPort("COEIROINK", 50031));
            portComboBox.Items.Add(new Model.NameAndPort("LMROID", 50073));

            SelectedPreset = new Model.NameAndPort("VOICEVOX", 50022);
        }
        
        #region INotifyPropertyChangedの実装
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
          => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        private Model.NameAndPort _nameAndPort = new Model.NameAndPort();
        /// <summary>
        /// アプリ名とポート名
        /// </summary>
        public Model.NameAndPort SelectedPreset
        {
            get => _nameAndPort;
            set
            {
                if (_nameAndPort == value) return;
                _nameAndPort = value;
                Port = _nameAndPort.Port;
                AppName = _nameAndPort.Name;
                RaisePropertyChanged();
            }
        }

        private int _port = 50021;
        /// <summary>
        /// ポート番号
        /// </summary>
        public int Port
        {
            get => _port;
            set
            {
                if (_port == value) return;
                _port = value;
                SelectedPreset = new Model.NameAndPort(AppName, _port);
                RaisePropertyChanged();
            }
        }

        private string _appName = "VOICEVOX";
        /// <summary>
        /// アプリ名
        /// </summary>
        public string AppName
        {
            get => _appName;
            set
            {
                if (_appName == value) return;
                _appName = value;
                SelectedPreset = new Model.NameAndPort(_appName, Port);
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// OKボタン押下イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }
    }

    /// <summary>
    /// boolを反転するコンバーター
    /// </summary>
    public class BoolNegativeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(value is bool && (bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(value is bool && (bool)value);
        }
    }
}
