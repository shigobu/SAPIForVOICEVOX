using System;
using System.IO;

namespace SAPIForVOICEVOX
{
    /// <summary>
    /// VOICEVOXのエンジンに関するエラーを表します。
    /// </summary>
    [Serializable]
    public class VoiceVoxEngineException : VoiceNotificationException
    {
        const string message = "エンジンエラーです";

        public VoiceVoxEngineException() : this(null) { }

        public VoiceVoxEngineException(Exception innerException) : base(message, innerException)
        {
            Stream stream = Properties.Resources.エンジンエラーです;
            ErrorVoice = new byte[stream.Length];
            stream.Read(ErrorVoice, 0, (int)stream.Length);
        }
    }
}
