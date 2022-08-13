using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using SFVvCommon;
using System.Reflection;

namespace Setting
{
    public class ViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChangedの実装
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
          => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public ViewModel(MainWindow mainWindow)
        {
            PropertyChanged += ViewModel_PropertyChanged;
            owner = mainWindow;
            LoadData();
        }

        #region プロパティとか

        public MainWindow owner { get; set; }

        /// <summary>
        /// Model
        /// </summary>
        private GeneralSetting generalSetting = null;

        /// <summary>
        /// 句点で分割するかどうかを取得、設定します。
        /// </summary>
        public bool? IsSplitKuten
        {
            get => generalSetting.isSplitKuten;
            set
            {
                if (generalSetting.isSplitKuten == value) return;
                generalSetting.isSplitKuten = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 読点で分割するかどうかを取得、設定します。
        /// </summary>
        public bool? IsSplitTouten
        {
            get => generalSetting.isSplitTouten;
            set
            {
                if (generalSetting.isSplitTouten == value) return;
                generalSetting.isSplitTouten = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 調声設定モードを取得、設定します。
        /// </summary>
        public SynthesisSettingMode SynthesisSettingMode
        {
            get => generalSetting.synthesisSettingMode;
            set
            {
                if (generalSetting.synthesisSettingMode == value) return;
                generalSetting.synthesisSettingMode = value;
                RaisePropertyChanged();
            }
        }


        private SynthesisParameter _BatchParameter = new SynthesisParameter();
        /// <summary>
        /// 一括調声設定
        /// </summary>
        public SynthesisParameter BatchParameter
        {
            get => _BatchParameter;
            set
            {
                if (_BatchParameter == value) return;
                _BatchParameter = value;
                RaisePropertyChanged();
            }
        }


        private List<SynthesisParameter> _SpeakerParameter = new List<SynthesisParameter>();
        /// <summary>
        /// 各キャラクター調声設定
        /// </summary>
        public List<SynthesisParameter> SpeakerParameter
        {
            get => _SpeakerParameter;
            set
            {
                if (_SpeakerParameter == value) return;
                _SpeakerParameter = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// エンジンエラーを通知するかどうかを取得、設定します。
        /// </summary>
        public bool? ShouldNotifyEngineError
        {
            get => generalSetting.shouldNotifyEngineError;
            set
            {
                if (generalSetting.shouldNotifyEngineError == value) return;
                generalSetting.shouldNotifyEngineError = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// SAPIイベントを通知するかどうかを取得、設定します。
        /// </summary>
        public bool? UseSapiEvent
        {
            get => generalSetting.useSspiEvent;
            set
            {
                if (generalSetting.useSspiEvent == value) return;
                generalSetting.useSspiEvent = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 疑問文を自動調声するかどうかを取得、設定します。
        /// </summary>
        public bool? UseInterrogativeAutoAdjustment
        {
            get => generalSetting.useInterrogativeAutoAdjustment;
            set
            {
                if (generalSetting.useInterrogativeAutoAdjustment == value) return;
                generalSetting.useInterrogativeAutoAdjustment = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region イベントとか

        //コマンドの使い方がいまいちわからないので、普通にイベントを使う。

        /// <summary>
        /// OKボタン押下イベント
        /// </summary>
        public void OkButton_Click(object sender, RoutedEventArgs e)
        {
            SaveData();
            Window.GetWindow((Button)sender).Close();
        }

        /// <summary>
        /// 適用ボタン押下イベント
        /// </summary>
        public void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            //ボタン連打防止ようにボタン無効化
            Button button = (Button)sender;
            button.IsEnabled = false;
            SaveData();
        }

        /// <summary>
        /// リセットボタン押下イベント
        /// </summary>
        public void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(Window.GetWindow((Button)sender), "各キャラクターの調声パラメータも含めて全て初期値にリセットします。" + Environment.NewLine + "よろしいですか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (result == MessageBoxResult.No)
            {
                return;
            }

            generalSetting = new GeneralSetting();
            //null指定で全てのプロパティ。
            //propertyName引数はオプション引数だがCallerMemberName属性が付いてるので、明示的に指定が必要。多分
            RaisePropertyChanged(null);

            BatchParameter = new SynthesisParameter();
            for (int i = 0; i < SpeakerParameter.Count; i++)
            {
                SpeakerParameter[i] = new SynthesisParameter();
            }
            RaisePropertyChanged(nameof(SpeakerParameter));

            //適応ボタン有効化のための、プロパティ変更通知登録
            BatchParameter.PropertyChanged += ViewModel_PropertyChanged;
            foreach (var item in SpeakerParameter)
            {
                item.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        /// <summary>
        /// バージョン情報ボタン押下イベント
        /// </summary>
        public void VersionInfoButton_Click(object sender, RoutedEventArgs e)
        {
            View.VersionInfoWindow versionInfoWindow = new View.VersionInfoWindow();
            versionInfoWindow.Owner = owner;
            versionInfoWindow.ShowDialog();
        }

        /// <summary>
        /// プロパティ変更の通知受取り
        /// </summary>
        public void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //適応ボタンの有効化
            owner.ApplyButton.IsEnabled = true;
        }

        #endregion

        #region 定数

        const int CharacterCount = 100;

        #endregion

        #region メソッド

        /// <summary>
        /// 保存します。
        /// </summary>
        private void SaveData()
        {
            // シリアライズする
            var serializerGeneralSeting = new XmlSerializer(typeof(GeneralSetting));
            using (var streamWriter = new StreamWriter(Common.GetGeneralSettingFileName(), false, Encoding.UTF8))
            {
                serializerGeneralSeting.Serialize(streamWriter, generalSetting);
            }

            BatchParameter.Version = Common.GetCurrentVersion().ToString();
            var serializerBatchParameter = new XmlSerializer(typeof(SynthesisParameter));
            using (var streamWriter = new StreamWriter(Common.GetBatchParameterSettingFileName(), false, Encoding.UTF8))
            {
                serializerBatchParameter.Serialize(streamWriter, BatchParameter);
            }

            foreach (var param in SpeakerParameter)
            {
                param.Version = Common.GetCurrentVersion().ToString();
            }
            var serializerSpeakerParameter = new XmlSerializer(typeof(List<SynthesisParameter>));
            using (var streamWriter = new StreamWriter(Common.GetSpeakerParameterSettingFileName(), false, Encoding.UTF8))
            {
                serializerSpeakerParameter.Serialize(streamWriter, SpeakerParameter);
            }
        }

        /// <summary>
        /// 設定を読み込みます。
        /// </summary>
        private void LoadData()
        {
            generalSetting = Common.LoadGeneralSetting();
            BatchParameter = Common.LoadBatchSynthesisParameter();
            SpeakerParameter = Common.LoadSpeakerSynthesisParameter();

            //適応ボタン有効化のための、プロパティ変更通知登録
            BatchParameter.PropertyChanged += ViewModel_PropertyChanged;
            foreach (var item in SpeakerParameter)
            {
                item.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        #endregion
    }
}
