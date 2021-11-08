using System;

namespace SFVvCommon
{
    /// <summary>
    /// VOICEVOX側のスタイル情報を表します。
    /// </summary>
    public class VoicevoxStyle
    {
        /// <summary>
        /// スタイル情報を初期化します。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="styleName"></param>
        /// <param name="iD"></param>
        public VoicevoxStyle(string name, string styleName, int iD)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            StyleName = styleName ?? throw new ArgumentNullException(nameof(styleName));
            ID = iD;
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
        /// ID
        /// </summary>
        public int ID { get; set; }
    }
}
