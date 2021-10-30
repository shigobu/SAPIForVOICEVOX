using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json.Linq;
using SAPIForVOICEVOX;
using StyleRegistrationTool.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
            MainWindow.Close();
        }

        /// <summary>
        /// 追加ボタンの処理
        /// </summary>
        private void AddCommandExecute()
        {
            foreach (var item in VoicevoxStyle_SelectedItems)
            {
                SapiStyle sapiStyle = new SapiStyle(item, VoiceVoxTTSEngine.CLSID);
                if (!SapiStyles.Contains(sapiStyle))
                {
                    SapiStyles.Add(sapiStyle);
                }
            }
            SapiStyles = new ObservableCollection<SapiStyle>(SortStyle(SapiStyles).OfType<SapiStyle>());
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
            SapiStyles.Clear();
            foreach (var item in VoicevoxStyles)
            {
                SapiStyle sapiStyle = new SapiStyle(item, VoiceVoxTTSEngine.CLSID);
                SapiStyles.Add(sapiStyle);
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
                Caption = "話者とスタイルの登録",
                InstructionText = "話者とスタイルの登録を行います。",
                Text = "後で登録することもできます。\n後で登録する場合、スタートの全てのプログラムから起動できます。"
            };

            var link1 = new TaskDialogCommandLink("link1", "登録する話者とスタイルを選択", "VOICEVOXの起動が必要");
            link1.Click += (sender1, e1) =>
            {
                dialog.Close();
                dialogResult = InstallerDialogResult.SelectStyle;
            };
            link1.Default = true;
            dialog.Controls.Add(link1);

            var link2 = new TaskDialogCommandLink("link2", "全ての話者とスタイルを登録", "VOICEVOXの起動が必要");
            link2.Click += (sender1, e1) =>
            {
                dialog.Close();
                dialogResult = InstallerDialogResult.AllStyle;
            };
            dialog.Controls.Add(link2);

            var link3 = new TaskDialogCommandLink("link3", "後で行う", "デフォルトの話者とスタイルが登録されます。");
            link3.Click += (sender1, e1) =>
            {
                dialog.Close();
                dialogResult = InstallerDialogResult.DefaultStyle;
            };
            dialog.Controls.Add(link3);

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
            dialog.Text = "話者とスタイル登録には、VOICEVOXの起動が必要です。";

            var link1 = new TaskDialogCommandLink("link1", "VOICEVOXを起動した");
            link1.Click += (sender1, e1) => dialog.Close();
            link1.Default = true;
            dialog.Controls.Add(link1);

            var link2 = new TaskDialogCommandLink("link2", "中止");
            link2.Click += (sender1, e1) =>
            {
                dialog.Close();
                shouldContinue = false;
            };
            dialog.Controls.Add(link2);

            dialog.Show();

            return shouldContinue;
        }

        /// <summary>
        /// VOICEVOXから話者とスタイル情報を取得します。
        /// </summary>
        /// <returns></returns>
        async Task<VoicevoxStyle[]> GetVoicevoxStyles()
        {
            List<VoicevoxStyle> voicevoxStyles = new List<VoicevoxStyle>();
            using (var resultSpeakers = await httpClient.GetAsync(@"http://127.0.0.1:50021/speakers"))
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
                        voicevoxStyles.Add(new VoicevoxStyle(name, styleName, id));
                    }
                }
            }
            return SortStyle(voicevoxStyles).OfType<VoicevoxStyle>().ToArray();
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

        /// <summary>
        /// スタイルの並び替えを行います。
        /// </summary>
        /// <param name="styles">スタイル配列</param>
        /// <returns>並び替えされた配列</returns>
        private IEnumerable SortStyle(IEnumerable styles)
        {
            IEnumerable<VoicevoxStyle> voicevoxStyles = styles.OfType<VoicevoxStyle>();
            IEnumerable<SapiStyle> sapiStyles = styles.OfType<SapiStyle>();
            if (voicevoxStyles.Count() > 0)
            {
                return voicevoxStyles.OrderBy(x => x.Name).ThenBy(x => x.ID);
            }
            else if (sapiStyles.Count() > 0)
            {
                return sapiStyles.OrderBy(x => x.Name).ThenBy(x => x.ID);
            }
            else { return new object[0]; }
        }

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
