using NAudio.Wave;
using System;
using System.IO;

namespace SAPIForVOICEVOX
{
    /// <summary>
    /// ボイスボックスと通信ができない場合に投げられます。
    /// </summary>
    [Serializable]
    public class VoiceVoxConnectionException : VoiceNotificationException
    {
        const string message = "ボイスボックスと通信ができません";

        public VoiceVoxConnectionException() : base(message)
        {
            Stream stream = Properties.Resources.ボイスボックスと通信ができません;
            ErrorVoice = new WaveFileReader(stream);
        }

        public VoiceVoxConnectionException(Exception innerException) : base(message, innerException) { }

    }
}
