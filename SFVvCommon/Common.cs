using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private const string tokensRegKey = @"SOFTWARE\Microsoft\Speech\Voices\Tokens\";
        private const string regClsid = "CLSID";

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

        /// <summary>
        /// スタイルの並び替えを行います。
        /// </summary>
        /// <param name="styles">スタイル配列</param>
        /// <returns>並び替えされた配列</returns>
        public static IEnumerable SortStyle(IEnumerable styles)
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
    }
}
