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

namespace StyleRegistrationTool.ViewModel
{
    class MainViewModel : INotifyPropertyChanged
    {
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

        private ObservableCollection<VoicevoxStyle> _voicevoxStyles = new ObservableCollection<VoicevoxStyle>();
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


        #endregion

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
                ShowStartedInstallerDialog(mainWindow);
            }
            else
            {
                ShowVoicevoxConnectionDialog(mainWindow);
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
                    ShowVoicevoxConnectionDialog(mainWindow);
                    continue;
                }
            }
            //画面に表示
            if (voicevoxStyles == null)
            {
                mainWindow.Close();
            }
            foreach (var style in voicevoxStyles)
            {
                VoicevoxStyles.Add(style);
            }
        }

        #region メソッド

        /// <summary>
        /// インストーラから起動された時に表示するDialogを表示する。
        /// </summary>
        /// <param name="window">親ウィンドウ</param>
        private void ShowStartedInstallerDialog(MainWindow window)
        {
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
                window.Close();
            };
            dialog.Controls.Add(link2);

            var link3 = new TaskDialogCommandLink("link3", "デフォルトの話者とスタイルを使用", "終了します");
            link3.Click += (sender1, e1) =>
            {
                dialog.Close();
                window.Close();
            };
            dialog.Controls.Add(link3);

            dialog.Show();
        }

        /// <summary>
        /// VOICEVOXを起動したかどうかの確認ダイアログを表示します。
        /// </summary>
        /// <param name="window">親ウィンドウ</param>
        private void ShowVoicevoxConnectionDialog(MainWindow window)
        {
            var dialog = new TaskDialog();

            dialog.OwnerWindowHandle = window.Handle;
            //dialog.Icon = TaskDialogStandardIcon.Information;
            dialog.Caption = "VOICEVOX起動の確認";
            dialog.InstructionText = "VOICEVOXを起動しましたか？";
            dialog.Text = "話者とスタイル登録には、VOICEVOXの起動が必要です。";

            var link1 = new TaskDialogCommandLink("link1", "VOICEVOXを起動した", "VOICEVOXへの接続を試みます。");
            link1.Click += (sender1, e1) => dialog.Close();
            link1.Default = true;
            dialog.Controls.Add(link1);

            var link2 = new TaskDialogCommandLink("link2", "終了する", "話者とスタイルの登録を中止し、アプリを終了します。");
            link2.Click += (sender1, e1) =>
            {
                dialog.Close();
                window.Close();
            };
            dialog.Controls.Add(link2);

            dialog.Show();
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
    }
}
