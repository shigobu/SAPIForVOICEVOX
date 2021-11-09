using SFVvCommon;
using System;
using System.Collections.Generic;

namespace SFVvCommon
{
    /// <summary>
    /// SAPI用のスタイル。レジストリ登録に必要なものを保持します。
    /// </summary>
    public class SapiStyle
    {
        /// <summary>
        /// SAPIスタイルを初期化します。
        /// </summary>
        /// <param name="name">話者名</param>
        /// <param name="styleName">スタイル名</param>
        /// <param name="iD">ID</param>
        /// <param name="clsid">SAPIエンジンのクラスID</param>
        public SapiStyle(string name, string styleName, int iD, Guid clsid)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            StyleName = styleName ?? throw new ArgumentNullException(nameof(styleName));
            ID = iD;
            CLSID = clsid;
        }

        /// <summary>
        /// VOICEVOXスタイルとクラスIDを指定して、SAPIスタイルを初期化します。
        /// </summary>
        /// <param name="voicevoxStyle">VOICEVOXスタイル</param>
        /// <param name="clsid">SAPIエンジンのクラスID</param>
        public SapiStyle(VoicevoxStyle voicevoxStyle, Guid clsid) : this(voicevoxStyle.Name, voicevoxStyle.StyleName, voicevoxStyle.ID, clsid)
        {
        }

        /// <summary>
        /// 話者名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// スタイル
        /// </summary>
        public string StyleName { get; set; }

        /// <summary>
        /// sapiに表示される名前
        /// </summary>
        public string SpaiName
        {
            get
            {
                return "VOICEVOX " + Name + " " + StyleName;
            }
        }

        /// <summary>
        /// ID
        /// </summary>
        public int ID { get; set; }

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
