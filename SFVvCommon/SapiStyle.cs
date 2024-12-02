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

        public override bool Equals(object obj)
        {
            return obj is SapiStyle style &&
                   base.Equals(obj) &&
                   CLSID.Equals(style.CLSID);
        }

        public override int GetHashCode()
        {
            int hashCode = 2093058940;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + CLSID.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(SapiStyle left, SapiStyle right)
        {
            return EqualityComparer<SapiStyle>.Default.Equals(left, right);
        }

        public static bool operator !=(SapiStyle left, SapiStyle right)
        {
            return !(left == right);
        }
    }
}
