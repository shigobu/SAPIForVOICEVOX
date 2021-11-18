using NAudio.Wave;
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

        public VoiceVoxEngineException() : base(message)
        {
            Stream stream = Properties.Resources.エンジンエラーです;
            ErrorVoice = new WaveFileReader(stream);
        }

        public VoiceVoxEngineException(Exception innerException) : base(message, innerException) { }
    }
}
