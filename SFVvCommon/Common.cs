using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace SFVvCommon
{
    public class Common
    {
        #region guid

        public const string guidString = "7A1BB9C4-DF39-4E01-A8DC-20DC1A0C03C6";
        /// <summary>
        /// TTSエンジンのcomで使用されるCLSID
        /// </summary>
        public static Guid CLSID { get; } = new Guid(guidString);

        /// <summary>
        /// レジストリ用Guid書式指定子
        /// </summary>
        /// <remarks>B書式指定子は中かっこ"{"で囲われる</remarks>
        public static string RegClsidFormatString { get; } = "B";

        #endregion

        #region レジストリ

        public const string tokensRegKey = @"SOFTWARE\Microsoft\Speech\Voices\Tokens\";
        public const string regSpeakerNumber = "SpeakerNumber";
        public const string regClsid = "CLSID";
        public const string regName = "Name";
        public const string regStyleName = "StyleName";
        public const string regPort = "Port";
        public const string regAttributes = "Attributes";
        public const string regAppName = "AppName";


        /// <summary>
        /// Windowsのレジストリから、SAPIForVOICEVOXのスピーカー情報を削除します。
        /// </summary>
        public static void ClearStyleFromWindowsRegistry()
        {
            //SAPIForVOICEVOXのトークンを表すキーの列挙
            using (RegistryKey regTokensKey = Registry.LocalMachine.OpenSubKey(tokensRegKey, true))
            {
                string[] tokenNames = regTokensKey.GetSubKeyNames();
                foreach (string tokenName in tokenNames)
                {
                    using (RegistryKey tokenKey = regTokensKey.OpenSubKey(tokenName))
                    {
                        string clsid = (string)tokenKey.GetValue(regClsid);
                        if (clsid == CLSID.ToString(RegClsidFormatString))
                        {
                            regTokensKey.DeleteSubKeyTree(tokenName);
                        }
                    }
                }
            }
        }

        #endregion

        const string GeneralSettingXMLFileName = "GeneralSetting.xml";
        const string BatchParameterSettingXMLFileName = "BatchParameter.xml";
        const string SpeakerParameterSettingXMLFileName = "SpeakerParameter.xml";
        const string StyleRegistrationSettingXMLFileName = "StyleRegistration.xml";

        /// <summary>
        /// スタイルの並び替えを行います。
        /// </summary>
        /// <param name="styles">スタイル配列</param>
        /// <returns>並び替えされた配列</returns>
        public static IEnumerable<StyleBase> SortStyle(IEnumerable<StyleBase> styles)
        {
            return styles.OrderBy(x => x.Port).ThenBy(x => x.Name, new StyleComparer()).ThenBy(x => x.ID);
        }

        /// <summary>
        /// 現在のバージョンを取得します。
        /// </summary>
        /// <returns></returns>
        public static Version GetCurrentVersion()
        {
            Assembly assembly = GetThisAssembly();
            AssemblyName asmName = assembly.GetName();
            return asmName.Version;
        }

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

        #region 設定

#if x64
        const string MutexName = "SAPIForVOICEVOX64bit";
#else
        const string MutexName = "SAPIForVOICEVOX32bit";
#endif


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
        /// スタイル登録設定ファイル名を取得します。
        /// </summary>
        /// <returns></returns>
        static public string GetStyleRegistrationSettingFileName()
        {
            string directoryName = GetThisAppDirectory();
            return Path.Combine(directoryName, StyleRegistrationSettingXMLFileName);
        }

        /// <summary>
        /// 一般設定を読み込みます。
        /// </summary>
        /// <returns>一般設定</returns>
        static public GeneralSetting LoadGeneralSetting()
        {
            GeneralSetting result = new GeneralSetting();
            string settingFileName = Common.GetGeneralSettingFileName();

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
            string settingFileName = Common.GetBatchParameterSettingFileName();

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
        static public List<SynthesisParameter> LoadSpeakerSynthesisParameter()
        {
            string settingFileName = Common.GetSpeakerParameterSettingFileName();

            //戻り値を作成、初期化
            List<SynthesisParameter> result = new List<SynthesisParameter>();

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

                var serializerSynthesisParameter = new XmlSerializer(typeof(List<SynthesisParameter>));
                var xmlSettings = new XmlReaderSettings()
                {
                    CheckCharacters = false,
                };
                using (var streamReader = new StreamReader(settingFileName, Encoding.UTF8))
                using (var xmlReader = XmlReader.Create(streamReader, xmlSettings))
                {
                    //結果上書き
                    result = (List<SynthesisParameter>)serializerSynthesisParameter.Deserialize(xmlReader);
                }
                //データがバージョン１の場合
                if (result.Count != 0 && new Version(result.First().Version).Major == 1)
                {
                    result = new List<SynthesisParameter>();
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

        #endregion
    }
}
