using System;
using System.Collections.Generic;

namespace SFVvCommon
{
    /// <summary>
    /// SAPI用のスタイル。レジストリ登録に必要なものを保持します。
    /// </summary>
    public class SapiStyle : StyleBase
    {
        public SapiStyle() : base()
        {
            CLSID = Common.CLSID;
        }

        /// <summary>
        /// SAPIスタイルを初期化します。
        /// </summary>
        /// <param name="name">話者名</param>
        /// <param name="styleName">スタイル名</param>
        /// <param name="iD">ID</param>
        /// <param name="clsid">SAPIエンジンのクラスID</param>
        public SapiStyle(string appName, string name, string styleName, int iD, int port, Guid clsid) : base(appName, name, styleName, iD, port)
        {
            CLSID = clsid;
        }

        /// <summary>
        /// VOICEVOXスタイルとクラスIDを指定して、SAPIスタイルを初期化します。
        /// </summary>
        /// <param name="voicevoxStyle">VOICEVOXスタイル</param>
        /// <param name="clsid">SAPIエンジンのクラスID</param>
        public SapiStyle(VoicevoxStyle voicevoxStyle, Guid clsid) : this(voicevoxStyle.AppName, voicevoxStyle.Name, voicevoxStyle.StyleName, voicevoxStyle.ID, voicevoxStyle.Port, clsid)
        {
        }

        /// <summary>
        /// sapiに表示される名前
        /// </summary>
        public string SpaiName
        {
            get
            {
                return AppName + " " + Name + " " + StyleName;
            }
        }

        /// <summary>
        /// SAPIForVOICEVOXモジュールのGuid
        /// </summary>
        public Guid CLSID { get; set; }

        #region Equalsの自動実装

        public override bool Equals(object obj)
        {
            var style = obj as SapiStyle;
            return style != null &&
                   Name == style.Name &&
                   StyleName == style.StyleName &&
                   ID == style.ID &&
                   CLSID.Equals(style.CLSID);
        }

        public override int GetHashCode()
        {
            var hashCode = 2064203553;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(StyleName);
            hashCode = hashCode * -1521134295 + ID.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Guid>.Default.GetHashCode(CLSID);
            return hashCode;
        }

        public static bool operator ==(SapiStyle style1, SapiStyle style2)
        {
            return EqualityComparer<SapiStyle>.Default.Equals(style1, style2);
        }

        public static bool operator !=(SapiStyle style1, SapiStyle style2)
        {
            return !(style1 == style2);
        }

        #endregion
    }
}
