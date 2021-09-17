using System;
using System.IO;

namespace SAPIForVOICEVOX
{
    /// <summary>
    /// ボイスボックスが起動中プロセス一覧から見つからなかった場合に投げられます。
    /// </summary>
    [Serializable]
    public class VoiceVoxNotFoundException : VoiceNotificationException
    {
        const string message = "VOICEVOXが見つかりません";

        public VoiceVoxNotFoundException() : base(message)
        {
            Stream stream = Properties.Resources.ボイスボックスが見つかりません;
            ErrorVoice = new byte[stream.Length];
            stream.Read(ErrorVoice, 0, (int)stream.Length);
        }

        public VoiceVoxNotFoundException(Exception innerException) : base(message, innerException) { }
    }
}
