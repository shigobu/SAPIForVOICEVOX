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
        public ChangePortWindow(string appName, int port)
        {
            InitializeComponent();

            DataContext = this;
            
            //プリセット作成
            portComboBox.Items.Add(new Model.NameAndPort("VOICEVOX", 50021));
            portComboBox.Items.Add(new Model.NameAndPort("VOICEVOX Nemo", 50121));
            portComboBox.Items.Add(new Model.NameAndPort("COEIROINK", 50031));
            portComboBox.Items.Add(new Model.NameAndPort("LMROID", 50073));
            portComboBox.Items.Add(new Model.NameAndPort("SHAREVOX", 50025));
            portComboBox.Items.Add(new Model.NameAndPort("ITVOICE", 49540));

            SelectedPreset = new Model.NameAndPort(appName, port);
        }
        
        #region INotifyPropertyChangedの実装
        public event PropertyChangedEventHandler PropertyChanged;

        private List<PropertyChangedEventArgs> propertyChangedEventArgsList = new List<PropertyChangedEventArgs>(3);

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            //eventArgsを使いまわしするための仕組み。newで逐一作成するよりも早い？？
            PropertyChangedEventArgs eventArgs = propertyChangedEventArgsList.FirstOrDefault(x => x.PropertyName == propertyName);
            if (eventArgs == null)
            {
                eventArgs = new PropertyChangedEventArgs(propertyName);
                propertyChangedEventArgsList.Add(eventArgs);
            }
            PropertyChanged?.Invoke(this, eventArgs);
        }
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
                RaisePropertyChanged();

                //SelectedIndexを-1にした場合、valueにnullが入るので確認
                if (value == null) return;
                Port = value.Port;
                AppName = value.Name;
                //入力の値がプリセットに含まれていない場合、未選択にする。
                if (!portComboBox.Items.Contains(SelectedPreset))
                {
                    portComboBox.SelectedIndex = -1;
                }
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
                SelectedPreset = new Model.NameAndPort(AppName, value);
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
                SelectedPreset = new Model.NameAndPort(value, Port);
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
