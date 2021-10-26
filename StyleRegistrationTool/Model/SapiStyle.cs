using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyleRegistrationTool.Model
{
    /// <summary>
    /// SAPI用のスタイル。レジストリ登録に必要なものを保持します。
    /// </summary>
    class SapiStyle
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
        public SapiStyle(VoicevoxStyle voicevoxStyle, Guid clsid)
        {
            if (voicevoxStyle == null)
            {
                throw new ArgumentNullException(nameof(voicevoxStyle));
            }

            Name = voicevoxStyle.Name;
            StyleName = voicevoxStyle.StyleName;
            ID = voicevoxStyle.ID;
            CLSID = clsid;
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

    }
}
