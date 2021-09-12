using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace Setting
{
    class ViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChangedの実装
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
          => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public ViewModel()
        {
            LoadData();
        }

        #region プロパティとか

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


        private SynthesisParameter[] _SpeakerParameter = { new SynthesisParameter(), new SynthesisParameter() };
        /// <summary>
        /// 各キャラクター調声設定
        /// </summary>
        public SynthesisParameter[] SpeakerParameter
        {
            get => _SpeakerParameter;
            set
            {
                if (_SpeakerParameter == value) return;
                _SpeakerParameter = value;
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
            SpeakerParameter = new SynthesisParameter[2] { new SynthesisParameter(), new SynthesisParameter() };
        }

        #endregion

        #region 定数

        const string GeneralSettingXMLFileName = "GeneralSetting.xml";
        const string BatchParameterSettingXMLFileName = "BatchParameter.xml";
        const string SpeakerParameterSettingXMLFileName = "SpeakerParameter.xml";

        #endregion

        #region メソッド

        /// <summary>
        /// 実行中のコードを格納しているアセンブリのある場所を返します。
        /// </summary>
        /// <returns></returns>
        private string GetThisAppDirectory()
        {
            string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(appPath);
        }

        /// <summary>
        /// 全般設定ファイルの名前を取得します。
        /// </summary>
        /// <returns>全般設定ファイル名</returns>
        private string GetGeneralSettingFileName()
        {
            string directoryName = GetThisAppDirectory();
            return Path.Combine(directoryName, GeneralSettingXMLFileName);
        }

        /// <summary>
        /// 調声設定ファイル名を取得します。
        /// </summary>
        /// <returns>調声設定ファイル名</returns>
        private string GetBatchParameterSettingFileName()
        {
            string directoryName = GetThisAppDirectory();
            return Path.Combine(directoryName, BatchParameterSettingXMLFileName);
        }

        /// <summary>
        /// キャラ調声設定ファイルを取得します。
        /// </summary>
        /// <returns></returns>
        private string GetSpeakerParameterSettingFileName()
        {
            string directoryName = GetThisAppDirectory();
            return Path.Combine(directoryName, SpeakerParameterSettingXMLFileName);
        }
        
        /// <summary>
        /// 保存します。
        /// </summary>
        private void SaveData()
        {
            // シリアライズする
            var serializerGeneralSeting = new XmlSerializer(typeof(GeneralSetting));
            using (var streamWriter = new StreamWriter(GetGeneralSettingFileName(), false, Encoding.UTF8))
            {
                serializerGeneralSeting.Serialize(streamWriter, generalSetting);
            }

            var serializerBatchParameter = new XmlSerializer(typeof(SynthesisParameter));
            using (var streamWriter = new StreamWriter(GetBatchParameterSettingFileName(), false, Encoding.UTF8))
            {
                serializerBatchParameter.Serialize(streamWriter, BatchParameter);
            }

            var serializerSpeakerParameter = new XmlSerializer(typeof(SynthesisParameter[]));
            using (var streamWriter = new StreamWriter(GetSpeakerParameterSettingFileName(), false, Encoding.UTF8))
            {
                serializerSpeakerParameter.Serialize(streamWriter, SpeakerParameter);
            }
        }

        /// <summary>
        /// 設定を読み込みます。
        /// </summary>
        private void LoadData()
        {
            generalSetting = LoadGeneralSetting();
            BatchParameter = LoadBatchSynthesisParameter();
            SpeakerParameter = LoadSpeakerSynthesisParameter();
        }

        /// <summary>
        /// 一般設定を読み込みます。
        /// </summary>
        /// <returns>一般設定</returns>
        private GeneralSetting LoadGeneralSetting()
        {
            string settingFileName = GetGeneralSettingFileName();

            //ファイル存在確認
            if (!File.Exists(settingFileName))
            {
                //無い場合は新規でオブジェクト作成。
                return new GeneralSetting();
            }

            // デシリアライズする
            var serializerGeneralSetting = new XmlSerializer(typeof(GeneralSetting));
            GeneralSetting result;
            var xmlSettings = new XmlReaderSettings()
            {
                CheckCharacters = false,
            };
            using (var streamReader = new StreamReader(settingFileName, Encoding.UTF8))
            using (var xmlReader = XmlReader.Create(streamReader, xmlSettings))
            {
                result = (GeneralSetting)serializerGeneralSetting.Deserialize(xmlReader);
            }

            return result;
        }

        /// <summary>
        /// 一括の調声設定を取得します。
        /// </summary>
        /// <returns>調声設定</returns>
        private SynthesisParameter LoadBatchSynthesisParameter()
        {
            string settingFileName = GetBatchParameterSettingFileName();

            //ファイル存在確認
            if (!File.Exists(settingFileName))
            {
                //無い場合は新規でオブジェクト作成。
                return new SynthesisParameter();
            }

            // デシリアライズする
            var serializerSynthesisParameter = new XmlSerializer(typeof(SynthesisParameter));
            SynthesisParameter result;
            var xmlSettings = new XmlReaderSettings()
            {
                CheckCharacters = false,
            };
            using (var streamReader = new StreamReader(settingFileName, Encoding.UTF8))
            using (var xmlReader = XmlReader.Create(streamReader, xmlSettings))
            {
                result = (SynthesisParameter)serializerSynthesisParameter.Deserialize(xmlReader);
            }

            return result;
        }

        /// <summary>
        /// キャラ調声設定を読み込みます。
        /// </summary>
        /// <returns>キャラ調声設定配列</returns>
        private SynthesisParameter[] LoadSpeakerSynthesisParameter()
        {
            string settingFileName = GetSpeakerParameterSettingFileName();

            //ファイル存在確認
            if (!File.Exists(settingFileName))
            {
                //無い場合は新規でオブジェクト作成。
                return new SynthesisParameter[2] { new SynthesisParameter(), new SynthesisParameter()};
            }

            // デシリアライズする
            var serializerSynthesisParameter = new XmlSerializer(typeof(SynthesisParameter[]));
            SynthesisParameter[] result;
            var xmlSettings = new XmlReaderSettings()
            {
                CheckCharacters = false,
            };
            using (var streamReader = new StreamReader(settingFileName, Encoding.UTF8))
            using (var xmlReader = XmlReader.Create(streamReader, xmlSettings))
            {
                result = (SynthesisParameter[])serializerSynthesisParameter.Deserialize(xmlReader);
            }

            return result;
        }

        #endregion
    }
}
