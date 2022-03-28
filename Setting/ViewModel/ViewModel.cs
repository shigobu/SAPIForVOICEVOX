using System;
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
using System.Reflection;

namespace Setting
{
    public class ViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChangedの実装
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
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


        private SynthesisParameter[] _SpeakerParameter = new SynthesisParameter[CharacterCount];
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
            SpeakerParameter = new SynthesisParameter[CharacterCount];
            for (int i = 0; i < SpeakerParameter.Length; i++)
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

        const string GeneralSettingXMLFileName = "GeneralSetting.xml";
        const string BatchParameterSettingXMLFileName = "BatchParameter.xml";
        const string SpeakerParameterSettingXMLFileName = "SpeakerParameter.xml";

        const int CharacterCount = 100;

#if x64
        const string MutexName = "SAPIForVOICEVOX64bit";
#else
        const string MutexName = "SAPIForVOICEVOX32bit";
#endif

        #endregion

        #region メソッド
        /// <summary>
        /// 現在実行中のコードを含むアセンブリを返します。
        /// </summary>
        /// <returns></returns>
        static public Assembly GetThisAssembly()
        {
            return Assembly.GetExecutingAssembly();
        }

        /// <summary>
        /// 実行中のコードを格納しているアセンブリのある場所を返します。
        /// </summary>
        /// <returns></returns>
        static public string GetThisAppDirectory()
        {
            string appPath = GetThisAssembly().Location;
            return Path.GetDirectoryName(appPath);
        }

        /// <summary>
        /// 実行中のコードを格納しているアセンブリのバージョンは返します。
        /// </summary>
        /// <returns>アセンブリバーション</returns>
        static public Version GetThisAppVersion()
        {
            return GetThisAssembly().GetName().Version;
        }

        /// <summary>
        /// 全般設定ファイルの名前を取得します。
        /// </summary>
        /// <returns>全般設定ファイル名</returns>
        static public string GetGeneralSettingFileName()
        {
            string directoryName = GetThisAppDirectory();
            return Path.Combine(directoryName, GeneralSettingXMLFileName);
        }

        /// <summary>
        /// 調声設定ファイル名を取得します。
        /// </summary>
        /// <returns>調声設定ファイル名</returns>
        static public string GetBatchParameterSettingFileName()
        {
            string directoryName = GetThisAppDirectory();
            return Path.Combine(directoryName, BatchParameterSettingXMLFileName);
        }

        /// <summary>
        /// キャラ調声設定ファイル名を取得します。
        /// </summary>
        /// <returns></returns>
        static public string GetSpeakerParameterSettingFileName()
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

            //適応ボタン有効化のための、プロパティ変更通知登録
            BatchParameter.PropertyChanged += ViewModel_PropertyChanged;
            foreach (var item in SpeakerParameter)
            {
                item.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        /// <summary>
        /// 一般設定を読み込みます。
        /// </summary>
        /// <returns>一般設定</returns>
        static public GeneralSetting LoadGeneralSetting()
        {
            GeneralSetting result = new GeneralSetting();
            string settingFileName = GetGeneralSettingFileName();

            //ファイル存在確認
            if (!File.Exists(settingFileName))
            {
                //無い場合はそのまま返す。
                return result;
            }

            // デシリアライズする
            Mutex mutex = new Mutex(false, MutexName);
            try
            {
                //ミューテックス取得
                mutex.WaitOne();

                var serializerGeneralSetting = new XmlSerializer(typeof(GeneralSetting));
                var xmlSettings = new XmlReaderSettings()
                {
                    CheckCharacters = false,
                };
                using (var streamReader = new StreamReader(settingFileName, Encoding.UTF8))
                using (var xmlReader = XmlReader.Create(streamReader, xmlSettings))
                {
                    //結果上書き
                    result = (GeneralSetting)serializerGeneralSetting.Deserialize(xmlReader);
                }
                return result;
            }
            catch (Exception)
            {
                return result;
            }
            finally
            {
                //ミューテックス開放
                mutex.Dispose();
            }
        }

        /// <summary>
        /// 一括の調声設定を取得します。
        /// </summary>
        /// <returns>調声設定</returns>
        static public SynthesisParameter LoadBatchSynthesisParameter()
        {
            SynthesisParameter result = new SynthesisParameter();
            string settingFileName = GetBatchParameterSettingFileName();

            //ファイル存在確認
            if (!File.Exists(settingFileName))
            {
                //無い場合はそのまま返す。
                return result;
            }

            // デシリアライズする
            Mutex mutex = new Mutex(false, MutexName);
            try
            {
                //ミューテックス取得
                mutex.WaitOne();

                var serializerSynthesisParameter = new XmlSerializer(typeof(SynthesisParameter));
                var xmlSettings = new XmlReaderSettings()
                {
                    CheckCharacters = false,
                };
                using (var streamReader = new StreamReader(settingFileName, Encoding.UTF8))
                using (var xmlReader = XmlReader.Create(streamReader, xmlSettings))
                {
                    //結果上書き
                    result = (SynthesisParameter)serializerSynthesisParameter.Deserialize(xmlReader);
                }
                return result;
            }
            catch (Exception)
            {
                return result;
            }
            finally
            {
                //ミューテックス開放
                mutex.Dispose();
            }
        }

        /// <summary>
        /// キャラ調声設定を読み込みます。
        /// </summary>
        /// <returns>キャラ調声設定配列</returns>
        static public SynthesisParameter[] LoadSpeakerSynthesisParameter()
        {
            string settingFileName = GetSpeakerParameterSettingFileName();

            //戻り値を作成、初期化
            SynthesisParameter[] result = new SynthesisParameter[0];

            //ファイル存在確認
            if (!File.Exists(settingFileName))
            {
                return result;
            }

            // デシリアライズする
            Mutex mutex = new Mutex(false, MutexName);
            try
            {
                //同じファイルを同時に操作しないために、ミューテックスを使用
                mutex.WaitOne();

                var serializerSynthesisParameter = new XmlSerializer(typeof(SynthesisParameter[]));
                var xmlSettings = new XmlReaderSettings()
                {
                    CheckCharacters = false,
                };
                using (var streamReader = new StreamReader(settingFileName, Encoding.UTF8))
                using (var xmlReader = XmlReader.Create(streamReader, xmlSettings))
                {
                    SynthesisParameter[] resultTemp = (SynthesisParameter[])serializerSynthesisParameter.Deserialize(xmlReader);
                    if (resultTemp.Length == 0)
                    {
                        //要素数0の場合、初期値返す
                        return result;
                    }
                    else if (resultTemp.First().Version.Major == 1)
                    {
                        return result;
                    }
                    else
                    {
                        return resultTemp;
                    }
                }
            }
            catch (Exception)
            {
                return result;
            }
            finally
            {
                //ミューテックス開放
                mutex.Dispose();
            }
        }

        #endregion
    }
}
