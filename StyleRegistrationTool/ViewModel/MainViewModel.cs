using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json.Linq;
using SFVvCommon;
using StyleRegistrationTool.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace StyleRegistrationTool.ViewModel
{
    class MainViewModel : INotifyPropertyChanged
    {

        public MainViewModel(MainWindow mainWindow)
        {
            MainWindow = mainWindow;
            OkCommand = new DelegateCommand(OkCommandExecute);
            CancelCommand = new DelegateCommand(() => MainWindow.Close());
            ChangePortCommand = new DelegateCommand(ChangePortCommandExecute);
            AddCommand = new DelegateCommand(AddCommandExecute);
            RemoveCommand = new DelegateCommand(RemoveCommandExecute);
            AllAddCommand = new DelegateCommand(AllAddCommandExecute);
            AllRemoveCommand = new DelegateCommand(AllRemoveCommandExecute);
        }

        #region INotifyPropertyChangedの実装
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
          => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        /// <summary>
        /// 唯一のhttpクライアント
        /// </summary>
        HttpClient httpClient = new HttpClient();

        #region プロパティ

        /// <summary>
        /// メインウィンドウを取得、設定します。
        /// </summary>
        public MainWindow MainWindow { get; set; }

        /// <summary>
        /// okボタンのコマンド
        /// </summary>
        public ICommand OkCommand { get; set; }

        /// <summary>
        /// キャンセルボタンのコマンド
        /// </summary>
        public ICommand CancelCommand { get; set; }

        /// <summary>
        /// ポート変更ボタンのコマンド
        /// </summary>
        public ICommand ChangePortCommand { get; set; }

        /// <summary>
        /// 追加コマンド
        /// </summary>
        public ICommand AddCommand { get; set; }

        /// <summary>
        /// 削除コマンド
        /// </summary>
        public ICommand RemoveCommand { get; set; }

        /// <summary>
        /// 全て追加コマンド
        /// </summary>
        public ICommand AllAddCommand { get; set; }

        /// <summary>
        /// 全て削除コマンド
        /// </summary>
        public ICommand AllRemoveCommand { get; set; }

        /// <summary>
        /// VOICEVOX側リストの選択されてるアイテム一覧
        /// </summary>
        internal IEnumerable<VoicevoxStyle> VoicevoxStyle_SelectedItems { get; set; } = Enumerable.Empty<VoicevoxStyle>();

        /// <summary>
        /// SAPI側リストの選択されているアイテム一覧
        /// </summary>
        internal IEnumerable<SapiStyle> SapiStyle_SelectedItems { get; set; } = Enumerable.Empty<SapiStyle>();

        /// <summary>
        /// ポート番号
        /// </summary>
        internal int Port { get; set; } = 50021;

        #region NotifyProperty

        private ObservableCollection<VoicevoxStyle> _voicevoxStyles = null;
        /// <summary>
        /// VOICEVOX側のスタイル一覧
        /// </summary>
        public ObservableCollection<VoicevoxStyle> VoicevoxStyles
        {
            get => _voicevoxStyles;
            set
            {
                if (_voicevoxStyles == value) return;
                _voicevoxStyles = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<SapiStyle> _sapiStyles = new ObservableCollection<SapiStyle>();
        /// <summary>
        /// SAPI側のスタイル一覧
        /// </summary>
        public ObservableCollection<SapiStyle> SapiStyles
        {
            get => _sapiStyles;
            set
            {
                if (_sapiStyles == value) return;
                _sapiStyles = value;
                RaisePropertyChanged();
            }
        }

        private bool _IsMainWindowEnabled = false;
        /// <summary>
        /// メイン画面が有効かどうか
        /// </summary>
        public bool IsMainWindowEnabled
        {
            get => _IsMainWindowEnabled;
            set
            {
                if (_IsMainWindowEnabled == value) return;
                _IsMainWindowEnabled = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _WaitCircleVisibility = Visibility.Visible;
        /// <summary>
        /// 待機ぐるぐる画面の表示状態
        /// </summary>
        public Visibility WaitCircleVisibility
        {
            get => _WaitCircleVisibility;
            set
            {
                if (_WaitCircleVisibility == value) return;
                _WaitCircleVisibility = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #endregion

        #region イベント

        /// <summary>
        /// mainWindowのLoadedイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async internal void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)sender;

            //コマンドラインを見て、インストーラから起動された場合、専用のダイアログを表示する。
            string[] commandline = Environment.GetCommandLineArgs();
            commandline = commandline.Select(str => str.ToLower()).ToArray();
            bool shouldContinue;
            if (commandline.Contains("/install"))
            {
                InstallerDialogResult dialogResult = ShowStartedInstallerDialog(mainWindow);
                switch (dialogResult)
                {
                    case InstallerDialogResult.SelectStyle:
                        //何もしない
                        break;
                    case InstallerDialogResult.AllStyle:
                        await AllStyleRegistration();
                        return;
                    case InstallerDialogResult.DefaultStyle:
                    default:
                        mainWindow.Close();
                        return;
                }
            }
            else
            {
                shouldContinue = ShowVoicevoxConnectionDialog(mainWindow);
                if (!shouldContinue)
                {
                    mainWindow.Close();
                    return;
                }
            }

            //VOICEVOXスタイルリストの更新
            shouldContinue = await UpdateVoicevoxStyles(false);
            if (!shouldContinue)
            {
                mainWindow.Close();
                return;
            }

            //SAPI側の情報取得
            SapiStyle[] sapiStyles = GetSapiStyles();
            SapiStyles = new ObservableCollection<SapiStyle>(sapiStyles);
        }

        /// <summary>
        /// VOICEVOXスタイルの更新を行います。
        /// </summary>
        /// <param name="isAllStyleRegistration">初回インストール時の全て登録ボタンが押されたときの処理を行うかどうか。</param>
        /// <returns></returns>
        async private Task<bool> UpdateVoicevoxStyles(bool isAllStyleRegistration)
        {
            //VOICEVOXから話者情報取得
            VoicevoxStyle[] voicevoxStyles = null;
            while (true)
            {
                try
                {
                    voicevoxStyles = await GetVoicevoxStyles();
                    break;
                }
                catch (HttpRequestException ex)
                {
                    bool shouldContinue = ShowVoicevoxConnectionDialog(MainWindow);
                    if (shouldContinue)
                    {
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            if (voicevoxStyles == null)
            {
                return false;
            }

            if (isAllStyleRegistration)
            {
                IsMainWindowEnabled = false;
                WaitCircleVisibility = Visibility.Visible;
            }
            else
            {
                IsMainWindowEnabled = true;
                WaitCircleVisibility = Visibility.Collapsed;
            }

            VoicevoxStyles = new ObservableCollection<VoicevoxStyle>(voicevoxStyles);
            return true;
        }

        private void OkCommandExecute()
        {
            RegistrationToWindowsRegistry();
            MainWindow.Close();
        }

        /// <summary>
        /// ポート変更ボタン
        /// </summary>
        private async void ChangePortCommandExecute()
        {
            int prevPort = Port;
            if (!ShowChangePortWindow())
            {
                return;
            }

            IsMainWindowEnabled = false;
            WaitCircleVisibility = Visibility.Visible;

            bool isSuccess = await UpdateVoicevoxStyles(false);
            if (!isSuccess)
            {
                Port = prevPort;
            }
            IsMainWindowEnabled = true;
            WaitCircleVisibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 追加ボタンの処理
        /// </summary>
        private void AddCommandExecute()
        {
            foreach (var item in VoicevoxStyle_SelectedItems)
            {
                SapiStyle sapiStyle = new SapiStyle(item, Common.CLSID);
                if (!SapiStyles.Contains(sapiStyle))
                {
                    SapiStyles.Add(sapiStyle);
                }
            }
            SapiStyles = new ObservableCollection<SapiStyle>(Common.SortStyle(SapiStyles).OfType<SapiStyle>());
        }

        /// <summary>
        /// 削除ボタンの処理
        /// </summary>
        private void RemoveCommandExecute()
        {
            List<SapiStyle> sapiStyles = new List<SapiStyle>(SapiStyle_SelectedItems);
            foreach (var item in sapiStyles)
            {
                SapiStyles.Remove(item);
            }
        }

        /// <summary>
        /// 全て追加ボタンの処理
        /// </summary>
        private void AllAddCommandExecute()
        {
            foreach (var item in VoicevoxStyles)
            {
                SapiStyle sapiStyle = new SapiStyle(item, Common.CLSID);
                if (!SapiStyles.Contains(sapiStyle))
                {
                    SapiStyles.Add(sapiStyle);
                }
            }
        }

        /// <summary>
        /// 全て削除ボタンの処理
        /// </summary>
        private void AllRemoveCommandExecute()
        {
            SapiStyles.Clear();
        }

        #endregion

        #region メソッド

        /// <summary>
        /// インストーラから起動された時に表示するDialogを表示する。
        /// </summary>
        /// <param name="window">親ウィンドウ</param>
        /// <returns>
        /// true:処理継続
        /// false:処理中止
        /// </returns>
        private InstallerDialogResult ShowStartedInstallerDialog(MainWindow window)
        {
            InstallerDialogResult dialogResult = InstallerDialogResult.SelectStyle;

            var dialog = new TaskDialog
            {
                OwnerWindowHandle = window.Handle,
                Caption = $"話者とスタイルの登録",
                InstructionText = "話者とスタイルの登録を行います。",
                Text = "後で登録することもできます。\n後で登録する場合、スタートの全てのプログラムから起動できます。"
            };

            var link1 = new TaskDialogCommandLink("link1", "登録する話者とスタイルを選択", "VOICEVOX(または派生アプリ)の起動が必要");
            link1.Click += (sender1, e1) =>
            {
                dialog.Close();
                dialogResult = InstallerDialogResult.SelectStyle;
            };
            link1.Default = true;
            dialog.Controls.Add(link1);

            var link2 = new TaskDialogCommandLink("link2", "全ての話者とスタイルを登録", "VOICEVOX(または派生アプリ)の起動が必要");
            link2.Click += (sender1, e1) =>
            {
                dialog.Close();
                dialogResult = InstallerDialogResult.AllStyle;
            };
            dialog.Controls.Add(link2);

            var link3 = new TaskDialogCommandLink("link3", "ポート変更", "COEIROINK等のVOICEVOX派生アプリを登録します");
            link3.Click += (sender1, e1) =>
            {
                ShowChangePortWindow();
            };
            dialog.Controls.Add(link3);

            var link4 = new TaskDialogCommandLink("link4", "後で行う", "デフォルトの話者とスタイルが登録されます。");
            link4.Click += (sender1, e1) =>
            {
                dialog.Close();
                dialogResult = InstallerDialogResult.DefaultStyle;
            };
            dialog.Controls.Add(link4);

            dialog.Show();
            return dialogResult;
        }

        /// <summary>
        /// VOICEVOXを起動したかどうかの確認ダイアログを表示します。中止が押された場合、親ウィンドウを閉じます。
        /// </summary>
        /// <param name="window">親ウィンドウ</param>
        /// <returns>
        /// true:処理継続
        /// false:処理中止
        /// </returns>
        private bool ShowVoicevoxConnectionDialog(MainWindow window)
        {
            bool shouldContinue = true;

            var dialog = new TaskDialog();

            dialog.OwnerWindowHandle = window.Handle;
            //dialog.Icon = TaskDialogStandardIcon.Information;
            dialog.Caption = "VOICEVOX起動の確認";
            dialog.InstructionText = "VOICEVOXを起動しましたか？";
            dialog.Text = "話者とスタイル登録には、VOICEVOX(または派生アプリ)の起動が必要です。";

            var link1 = new TaskDialogCommandLink("link1", "VOICEVOXを起動した");
            link1.Click += (sender1, e1) => dialog.Close();
            link1.Default = true;
            dialog.Controls.Add(link1);

            var link2 = new TaskDialogCommandLink("link2", "ポート変更");
            link2.Click += (sender1, e1) =>
            {
                dialog.Close();
                ShowChangePortWindow();
            };
            dialog.Controls.Add(link2);

            var link3 = new TaskDialogCommandLink("link3", "中止");
            link3.Click += (sender1, e1) =>
            {
                dialog.Close();
                shouldContinue = false;
            };
            dialog.Controls.Add(link3);

            dialog.Show();

            return shouldContinue;
        }

        /// <summary>
        /// ポート変更ダイアログを表示し、ユーザーの選択に応じて、Portプロパティを更新します。
        /// </summary>
        private bool ShowChangePortWindow()
        {
            ChangePortWindow portWindow = new ChangePortWindow(Port)
            {
                Owner = MainWindow
            };
            bool? portWindowResult = portWindow.ShowDialog();
            if (portWindowResult ?? false)
            {
                Port = portWindow.Port;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// VOICEVOXから話者とスタイル情報を取得します。
        /// </summary>
        /// <returns></returns>
        async Task<VoicevoxStyle[]> GetVoicevoxStyles()
        {
            List<VoicevoxStyle> voicevoxStyles = new List<VoicevoxStyle>();
            using (var resultSpeakers = await httpClient.GetAsync($"http://127.0.0.1:{Port}/speakers"))
            {
                //戻り値を文字列にする
                string resBodyStr = await resultSpeakers.Content.ReadAsStringAsync();
                JArray jsonObj = JArray.Parse(resBodyStr);
                foreach (var speaker in jsonObj)
                {
                    string name = speaker["name"].ToString();
                    foreach (var style in speaker["styles"])
                    {
                        string styleName = style["name"].ToString();
                        int id = style.Value<int>("id");
                        voicevoxStyles.Add(new VoicevoxStyle(name, styleName, id, Port));
                    }
                }
            }
            return Common.SortStyle(voicevoxStyles).OfType<VoicevoxStyle>().ToArray();
        }

        /// <summary>
        /// 全てのスタイルを登録します。
        /// </summary>
        private async Task AllStyleRegistration()
        {
            bool shouldContinue = await UpdateVoicevoxStyles(true);
            if (!shouldContinue)
            {
                MainWindow.Close();
                return;
            }
            AllAddCommandExecute();
            OkCommandExecute();
        }

        #region レジストリ関連

        /// <summary>
        /// Windowsのレジストリにスタイルを登録します。
        /// </summary>
        private void RegistrationToWindowsRegistry()
        {
            Common.ClearStyleFromWindowsRegistry();

            using (RegistryKey regTokensKey = Registry.LocalMachine.OpenSubKey(Common.tokensRegKey, true))
            {
                for (int i = 0; i < SapiStyles.Count(); i++)
                {
                    using (RegistryKey voiceVoxRegkey = regTokensKey.CreateSubKey("VOICEVOX" + i.ToString()))
                    {
                        voiceVoxRegkey.SetValue("", SapiStyles[i].SpaiName);
                        voiceVoxRegkey.SetValue("411", SapiStyles[i].SpaiName);
                        voiceVoxRegkey.SetValue(Common.regClsid, SapiStyles[i].CLSID.ToString(Common.RegClsidFormatString));
                        voiceVoxRegkey.SetValue(Common.regSpeakerNumber, SapiStyles[i].ID);
                        voiceVoxRegkey.SetValue(Common.regName, SapiStyles[i].Name);
                        voiceVoxRegkey.SetValue(Common.regStyleName, SapiStyles[i].StyleName);
                        voiceVoxRegkey.SetValue(Common.regPort, SapiStyles[i].Port);

                        using (RegistryKey AttributesRegkey = voiceVoxRegkey.CreateSubKey(Common.regAttributes))
                        {
                            AttributesRegkey.SetValue("Age", "Teen");
                            AttributesRegkey.SetValue("Vendor", "Hiroshiba Kazuyuki");
                            AttributesRegkey.SetValue("Language", "411");
                            AttributesRegkey.SetValue("Gender", "Female");
                            AttributesRegkey.SetValue("Name", SapiStyles[i].SpaiName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// レジストリに登録されているSAPIの話者情報を取得します。
        /// </summary>
        /// <returns></returns>
        private SapiStyle[] GetSapiStyles()
        {
            List<SapiStyle> sapiStyles = new List<SapiStyle>();

            using (RegistryKey regTokensKey = Registry.LocalMachine.OpenSubKey(Common.tokensRegKey, true))
            {
                string[] tokenNames = regTokensKey.GetSubKeyNames();
                foreach (string tokenName in tokenNames)
                {
                    using (RegistryKey tokenKey = regTokensKey.OpenSubKey(tokenName))
                    {
                        string clsid = (string)tokenKey.GetValue(Common.regClsid);
                        string name = (string)tokenKey.GetValue(Common.regName);
                        if (clsid == Common.CLSID.ToString(Common.RegClsidFormatString) &&
                            name != null)
                        {
                            string styleName = (string)tokenKey.GetValue(Common.regStyleName);
                            int id = (int)tokenKey.GetValue(Common.regSpeakerNumber, 0);
                            int port = (int)tokenKey.GetValue(Common.regPort, 50021);
                            SapiStyle sapiStyle = new SapiStyle(name, styleName, id, port, new Guid(clsid));
                            sapiStyles.Add(sapiStyle);
                        }
                    }
                }
            }

            return Common.SortStyle(sapiStyles).Cast<SapiStyle>().ToArray();
        }

        #endregion レジストリ関連

        #endregion

        #region コマンドクラス

        /// <summary>
        /// プリズムのコードを参考に、デリゲートコマンドを作成。
        /// </summary>
        class DelegateCommand : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public DelegateCommand(Action executeMethod)
            {
                ExecuteMethod = executeMethod;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                ExecuteMethod();
            }

            private Action ExecuteMethod { get; set; }
        }

        #endregion

        /// <summary>
        /// インストール時に表示されるダイアログの押されたボタンを表す列挙型
        /// </summary>
        private enum InstallerDialogResult
        {
            SelectStyle,
            AllStyle,
            DefaultStyle,
        }
    }
}
