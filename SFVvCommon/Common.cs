using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public static IEnumerable<StyleBase> SortStyle(IEnumerable<StyleBase> styles)
        {
            return styles.OrderBy(x => x.Name, new StyleComparer()).ThenBy(x => x.ID);
        }
    }
}
