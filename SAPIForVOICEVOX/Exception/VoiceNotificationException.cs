using System;

namespace SAPIForVOICEVOX
{
    /// <summary>
    /// 例外を音声で通知する、例外クラス。
    /// </summary>
    [Serializable]
    public class VoiceNotificationException : Exception
    {
        public VoiceNotificationException()
        {
        }

        public VoiceNotificationException(string message) : this(message, null)
        {
        }

        public VoiceNotificationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// エラー音声を取得、設定します。
        /// </summary>
        public byte[] ErrorVoice { get; protected set; } = null;
    }
}
