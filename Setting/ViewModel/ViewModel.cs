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
            generalSetting = LoadGeneralSetting();
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

        /// <summary>
        /// 一括設定
        /// </summary>
        /// <remarks>
        /// このプロパティを直接DataContextに入れるので、変更通知の仕組みは入れていない。
        /// </remarks>
        public SynthesisParameter BatchParameter { get; set; } = new SynthesisParameter();

        /// <summary>
        /// 話者１のパラメータ
        /// </summary>
        /// <remarks>
        /// このプロパティを直接DataContextに入れるので、変更通知の仕組みは入れていない。
        /// </remarks>
        public SynthesisParameter Speaker1Parameter { get; set; } = new SynthesisParameter();

        /// <summary>
        /// 話者２のパラメータ
        /// </summary>
        /// <remarks>
        /// このプロパティを直接DataContextに入れるので、変更通知の仕組みは入れていない。
        /// </remarks>
        public SynthesisParameter Speaker2Parameter { get; set; } = new SynthesisParameter();

        #endregion

        #region イベントとか

        //コマンドの使い方がいまいちわからないので、普通にイベントを使う。
        public void OkButton_Click(object sender, RoutedEventArgs e)
        {
            SaveData();
            Window.GetWindow((Button)sender).Close();
        }

        public void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            SaveData();
        }


        #endregion

        #region 定数

        const string GeneralSettingXMLFileName = "GeneralSetting.xml";

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
        /// 出力先を指定して、保存します。
        /// </summary>
        /// <param name="directoryName"></param>
        private void SaveData()
        {
            string directoryName = GetThisAppDirectory();
            string generalSettingFileName = Path.Combine(directoryName, GeneralSettingXMLFileName);

            // シリアライズする
            var serializerGeneralSeting = new XmlSerializer(typeof(GeneralSetting));
            using (var streamWriter = new StreamWriter(generalSettingFileName, false, Encoding.UTF8))
            {
                serializerGeneralSeting.Serialize(streamWriter, generalSetting);
            }
        }

        private GeneralSetting LoadGeneralSetting()
        {
            string directoryName = GetThisAppDirectory();
            string generalSettingFileName = Path.Combine(directoryName, GeneralSettingXMLFileName);

            //ファイル存在確認
            if (!File.Exists(generalSettingFileName))
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
            using (var streamReader = new StreamReader(generalSettingFileName, Encoding.UTF8))
            using (var xmlReader = XmlReader.Create(streamReader, xmlSettings))
            {
                result = (GeneralSetting)serializerGeneralSetting.Deserialize(xmlReader);
            }

            return result;
        }

        #endregion
    }
}
