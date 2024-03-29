﻿namespace SFVvCommon
{
    /// <summary>
    /// VOICEVOX側のスタイル情報を表します。
    /// </summary>
    public class VoicevoxStyle : StyleBase
    {
        /// <summary>
        /// スタイル情報を初期化します。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="styleName"></param>
        /// <param name="iD"></param>
        public VoicevoxStyle(string appName, string name, string styleName, int iD, int port) : base(appName, name, styleName, iD, port)
        {
        }
    }
}
