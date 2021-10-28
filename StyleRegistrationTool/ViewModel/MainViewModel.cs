using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json.Linq;
using StyleRegistrationTool.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
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
            if (commandline.Contains("/install"))
            {
                bool shouldContinue = ShowStartedInstallerDialog(mainWindow);
                if (!shouldContinue)
                {
                    return;
                }
            }
            else
            {
                bool shouldContinue = ShowVoicevoxConnectionDialog(mainWindow);
                if (!shouldContinue)
                {
                    return;
                }
            }


            //VOICEVOXから話者情報取得
            VoicevoxStyle[] voicevoxStyles = null;
            while(true)
            {
                try
                {
                    voicevoxStyles = await GetVoicevoxStyles();
                    break;
                }
                catch (HttpRequestException ex)
                {
                    bool shouldContinue = ShowVoicevoxConnectionDialog(mainWindow);
                    if (shouldContinue)
                    {
                        continue;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            if (voicevoxStyles == null)
            {
                mainWindow.Close();
                return;
            }

            //画面に表示
            //ここまで来たということは、VOICEVOXへ接続できたことになる。
            IsMainWindowEnabled = true;
            WaitCircleVisibility = Visibility.Collapsed;
            VoicevoxStyles = new ObservableCollection<VoicevoxStyle>(voicevoxStyles);
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
            
        }

        /// <summary>
        /// 削除ボタンの処理
        /// </summary>
        private void RemoveCommandExecute()
        {

        }

        /// <summary>
        /// 全て追加ボタンの処理
        /// </summary>
        private void AllAddCommandExecute()
        {

        }

        /// <summary>
        /// 全て削除ボタンの処理
        /// </summary>
        private void AllRemoveCommandExecute()
        {

        }

        #endregion

        #region メソッド

        /// <summary>
        /// インストーラから起動された時に表示するDialogを表示する。
        /// </summary>
        /// <param name="window">親ウィンドウ</param>
        /// <returns>
        /// true:メインウィンドウを閉じない
        /// false:メインウィンドウを閉じた
        /// </returns>
        private bool ShowStartedInstallerDialog(MainWindow window)
        {
            bool shouldContinue = true;

            var dialog = new TaskDialog
            {
                OwnerWindowHandle = window.Handle,
                Caption = "話者とスタイルの登録",
                InstructionText = "話者とスタイルの登録を行います。",
                Text = "後で登録することもできます。\n後で登録する場合、スタートの全てのプログラムから起動できます。"
            };

            var link1 = new TaskDialogCommandLink("link1", "登録する話者とスタイルを選択", "VOICEVOXの起動が必要");
            link1.Click += (sender1, e1) => dialog.Close();
            link1.Default = true;
            dialog.Controls.Add(link1);

            var link2 = new TaskDialogCommandLink("link2", "全ての話者とスタイルを登録", "VOICEVOXの起動が必要");
            link2.Click += (sender1, e1) =>
            {
                dialog.Close();
                this.AllStyleRegistration();
                shouldContinue = false;
                window.Close();
            };
            dialog.Controls.Add(link2);

            var link3 = new TaskDialogCommandLink("link3", "後で行う", "デフォルトの話者とスタイルが登録されます。");
            link3.Click += (sender1, e1) =>
            {
                dialog.Close();
                shouldContinue = false;
                window.Close();
            };
            dialog.Controls.Add(link3);

            dialog.Show();
            return shouldContinue;
        }

        /// <summary>
        /// VOICEVOXを起動したかどうかの確認ダイアログを表示します。中止が押された場合、親ウィンドウを閉じます。
        /// </summary>
        /// <param name="window">親ウィンドウ</param>
        /// <returns>
        /// true:メインウィンドウを閉じない
        /// false:メインウィンドウを閉じた
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
                window.Close();
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
            return voicevoxStyles.ToArray(); ;
        }


        /// <summary>
        /// 全てのスタイルを登録します。
        /// </summary>
        void AllStyleRegistration()
        {
            //未実装
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
    }
}
