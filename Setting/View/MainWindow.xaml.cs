using Microsoft.Win32;
using SFVvCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Setting
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 最大化ボタン無効化

        /// <summary>
        /// ウィンドウに関するデータを取得
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="nIndex"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        /// <summary>
        /// ウィンドウの属性を変更
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="nIndex"></param>
        /// <param name="dwNewLong"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        /// <summary>
        /// ウィンドウスタイル
        /// </summary>
        private const int GWL_STYLE = -16;

        /// <summary>
        /// 最大化ボタン
        /// </summary>
        private const int WS_MAXIMIZEBOX = 0x0001_0000; // C#7より前の場合は 0x00010000

        /// <summary>
        /// 初期化時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper((Window)sender).Handle;
            int value = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, (int)(value & ~WS_MAXIMIZEBOX));
        }

        #endregion

        #region プロパティ

        /// <summary>
        /// メインのビューモデル
        /// </summary>
        ViewModel MainViewModel { get; set; }

        #endregion

        public MainWindow()
        {
            InitializeComponent();

#if x64
            string bitStr = "64bit版";
#else
            string bitStr = "32bit版";
#endif
            this.Title += bitStr;

            MainViewModel = new ViewModel(this);
            this.DataContext = MainViewModel;
            OkButton.Click += MainViewModel.OkButton_Click;
            ApplyButton.Click += MainViewModel.ApplyButton_Click;
            resetButton.Click += MainViewModel.ResetButton_Click;
            versionInfoButton.Click += MainViewModel.VersionInfoButton_Click;

            ApplyButton.IsEnabled = false;

            AddTabControl();
        }

        /// <summary>
        /// 各キャラクターやスタイルのタブを追加します。
        /// </summary>
        private void AddTabControl()
        {
            List<VoicevoxStyle> styles = new List<VoicevoxStyle>();

            using (RegistryKey regTokensKey = Registry.LocalMachine.OpenSubKey(Common.tokensRegKey))
            {
                string[] tokenNames = regTokensKey.GetSubKeyNames();
                foreach (string tokenName in tokenNames)
                {
                    using (RegistryKey tokenKey = regTokensKey.OpenSubKey(tokenName))
                    {
                        string clsid = (string)tokenKey.GetValue(Common.regClsid);
                        if (clsid != Common.CLSID.ToString(Common.RegClsidFormatString))
                        {
                            continue;
                        }

                        string name = (string)tokenKey.GetValue(Common.regName);
                        if (name == null)
                        {
                            AddTabDefault();
                            return;
                        }
                        else
                        {
                            string styleName = (string)tokenKey.GetValue(Common.regStyleName);
                            int id = (int)tokenKey.GetValue(Common.regSpeakerNumber, 0);
                            int port = (int)tokenKey.GetValue(Common.regPort, 50021);
                            styles.Add(new VoicevoxStyle(name, styleName, id, port));
                        }
                    }
                }
            }

            styles = Common.SortStyle(styles).OfType<VoicevoxStyle>().ToList();

            foreach (var style in styles)
            {
                IEnumerable<TabItem> tabItems = mainTab.Items.OfType<TabItem>().Where(x => x.Header.ToString() == style.Name);
                //スタイル名のタブ
                TabControl tabControl;
                if (tabItems.Count() == 0)
                {
                    Binding binding = new Binding("IsChecked");
                    binding.ElementName = nameof(parCharacterRadioButton);
                    binding.Converter = new BooleanToVisibilityConverter();
                    TabItem tabItem = new TabItem();
                    tabItem.Header = style.Name;
                    tabItem.SetBinding(TabItem.VisibilityProperty, binding);

                    mainTab.Items.Add(tabItem);

                    tabControl = new TabControl();
                    tabItem.Content = tabControl;
                }
                else
                {
                    tabControl = tabItems.First().Content as TabControl;
                }

                int index = Array.FindIndex(MainViewModel.SpeakerParameter, x => x.ID == style.ID && x.Port == style.Port);
                if (index < 0)
                {
                    throw new KeyNotFoundException();
                }

                VoicevoxParameterSlider parameterSlider = new VoicevoxParameterSlider();
                parameterSlider.SetBinding(VoicevoxParameterSlider.DataContextProperty, nameof(Setting.ViewModel.SpeakerParameter) + $"[{index}]");

                TabItem styleTabItem = new TabItem();
                styleTabItem.Header = style.StyleName;
                styleTabItem.Content = parameterSlider;

                tabControl.Items.Add(styleTabItem);
            }
        }

        private void AddTabDefault()
        {
            VoicevoxParameterSlider parameterSlider = new VoicevoxParameterSlider();
            parameterSlider.SetBinding(VoicevoxParameterSlider.DataContextProperty, nameof(Setting.ViewModel.SpeakerParameter) + "[0]");

            Binding binding = new Binding("IsChecked");
            binding.ElementName = nameof(parCharacterRadioButton);
            binding.Converter = new BooleanToVisibilityConverter();
            TabItem tabItem = new TabItem();
            tabItem.Header = "四国めたん";
            tabItem.SetBinding(TabItem.VisibilityProperty, binding);
            tabItem.Content = parameterSlider;

            mainTab.Items.Add(tabItem);

            parameterSlider = new VoicevoxParameterSlider();
            parameterSlider.SetBinding(VoicevoxParameterSlider.DataContextProperty, nameof(Setting.ViewModel.SpeakerParameter) + "[1]");

            tabItem = new TabItem();
            tabItem.Header = "ずんだもん";
            tabItem.SetBinding(TabItem.VisibilityProperty, binding);
            tabItem.Content = parameterSlider;

            mainTab.Items.Add(tabItem);
        }

        /// <summary>
        /// キャンセルボタン押下時のイベントハンドラ
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
