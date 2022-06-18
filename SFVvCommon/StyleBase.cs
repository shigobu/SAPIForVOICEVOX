using System;

namespace SFVvCommon
{
    /// <summary>
    /// スタイル情報を保持する親クラス。
    /// </summary>
    public class StyleBase
    {
        public StyleBase()
        {
            AppName = "VOICEVOX";
            Name = "";
            StyleName = "";
            ID = 0;
            Port = 0;
        }

        public StyleBase(string appName, string name, string styleName, int iD, int port)
        {
            AppName = appName ?? throw new ArgumentNullException(nameof(appName));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            StyleName = styleName ?? throw new ArgumentNullException(nameof(styleName));
            ID = iD;
            Port = port;
        }

        /// <summary>
        /// アプリ名
        /// </summary>
        public string AppName { get; set; }

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

        /// <summary>
        /// ポート番号
        /// </summary>
        public int Port { get; set; }
    }
}
