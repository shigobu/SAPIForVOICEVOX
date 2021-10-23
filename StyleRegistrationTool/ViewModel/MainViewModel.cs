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

            var dialog = new TaskDialog();

            dialog.OwnerWindowHandle = mainWindow.Handle;

            dialog.Caption = "話者とスタイルの登録";
            dialog.InstructionText = "話者とスタイルの登録を行います。";
            dialog.Text = "後で登録することもできます。\nその場合、スタートの全てのプログラムから起動できます。";

            var link1 = new TaskDialogCommandLink("link1", "登録する話者とスタイルを選択", "VOICEVOXの起動が必要");
            link1.Click += (sender1, e1) => dialog.Close();
            link1.Default = true;
            dialog.Controls.Add(link1);

            var link2 = new TaskDialogCommandLink("link2", "全ての話者とスタイルを登録", "VOICEVOXの起動が必要");
            link2.Click += (sender1, e1) =>
            {
                dialog.Close();
                this.AllStyleRegistration();
                mainWindow.Close();
            };
            dialog.Controls.Add(link2);

            var link3 = new TaskDialogCommandLink("link3", "デフォルトの話者とスタイルを使用", "終了します");
            link3.Click += (sender1, e1) =>
            {
                dialog.Close();
                mainWindow.Close();
            };
            dialog.Controls.Add(link3);

            dialog.Show();

            //VOICEVOXから話者情報取得
            VoicevoxStyle[] voicevoxStyles = await GetVoicevoxStyles();
            foreach (var item in voicevoxStyles)
            {
                VoicevoxStyles.Add(item);
            }
        }

        #region メソッド

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
