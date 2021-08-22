﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TTSEngineLib;
using Microsoft.Win32;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace SAPIForVOICEVOX
{
    [Guid(guidString)]
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    public class VoiceVoxTTSEngine : ISpTTSEngine, ISpObjectWithToken
    {
        /// <summary>
        /// このクラスのGUID
        /// </summary>
        public const string guidString = "7A1BB9C4-DF39-4E01-A8DC-20DC1A0C03C6";

        const ushort WAVE_FORMAT_PCM = 1;

        readonly static Guid SPDFID_WaveFormatEx = new Guid("C31ADBAE-527F-4FF5-A230-F62BB61FF70C");
        readonly static Guid SPDFID_Text = new Guid("7CEEF9F9-3D13-11D2-9EE7-00C04F797396");


        /// <summary>
        /// キャラ番号
        /// </summary>
        int SpeakerNumber { get; set; } = 0;

        /// <summary>
        /// トークン
        /// </summary>
        ISpObjectToken Token { get; set; }

        /// <summary>
        /// 唯一のhttpクライアント
        /// </summary>
        HttpClient httpClient;

        /// <summary>
        /// コンストラクタ

        /// </summary>
        public VoiceVoxTTSEngine()
        {
            httpClient = new HttpClient();
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~VoiceVoxTTSEngine()
        {
            httpClient.Dispose();
        }

        /// <summary>
        /// スピークメソッド。
        /// 読み上げ指示が来ると呼ばれる。
        /// </summary>
        /// <param name="dwSpeakFlags"></param>
        /// <param name="rguidFormatId"></param>
        /// <param name="pWaveFormatEx"></param>
        /// <param name="pTextFragList"></param>
        /// <param name="pOutputSite"></param>
        public void Speak(uint dwSpeakFlags, ref Guid rguidFormatId, ref WAVEFORMATEX pWaveFormatEx, ref SPVTEXTFRAG pTextFragList, ISpTTSEngineSite pOutputSite)
        {
            //SPDFIDTextは非対応
            if (rguidFormatId == SPDFID_Text)
            {
                return;
            }

            SPVTEXTFRAG currentTextList = pTextFragList;
            while (true)
            {
                //VOICEVOXへ送信
                //asyncメソッドにはref引数を指定できないらしいので、awaitも使用できない。awaitを使用しない実装にした。
                Task<byte[]> waveDataTask = SendToVoiceVox(currentTextList.pTextStart, SpeakerNumber);
                waveDataTask.Wait();
                byte[] waveData = waveDataTask.Result;

                //受け取った音声データをpOutputSiteへ書き込む
                IntPtr pWavData = IntPtr.Zero;
                try
                {
                    //メモリが確実に確保され、確実に代入されるためのおまじない。
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try { }
                    finally
                    {
                        pWavData = Marshal.AllocCoTaskMem(waveData.Length);
                    }
                    Marshal.Copy(waveData, 0, pWavData, waveData.Length);
                    pOutputSite.Write(pWavData, (uint)waveData.Length, out uint written);
                }
                finally
                {
                    if (pWavData != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(pWavData);
                    }
                }

                //次のデータを設定
                if (pTextFragList.pNext == IntPtr.Zero)
                {
                    break;
                }
                else
                {
                    currentTextList = Marshal.PtrToStructure<SPVTEXTFRAG>(pTextFragList.pNext);
                }
            }
        }

        /// <summary>
        /// 読み上げ指示の前に呼ばれるはず。
        /// 音声データの形式を指定する。
        /// </summary>
        /// <param name="pTargetFmtId"></param>
        /// <param name="pTargetWaveFormatEx"></param>
        /// <param name="pOutputFormatId"></param>
        /// <param name="ppCoMemOutputWaveFormatEx"></param>
        public void GetOutputFormat(ref Guid pTargetFmtId, ref WAVEFORMATEX pTargetWaveFormatEx, out Guid pOutputFormatId, IntPtr ppCoMemOutputWaveFormatEx)
        {
            pOutputFormatId = SPDFID_WaveFormatEx;

            //comインターフェースのラップクラス自動生成がうまく行かなかったので、unsafeでポインタを直接使用する
            unsafe
            {
                WAVEFORMATEX wAVEFORMATEX = new WAVEFORMATEX();
                wAVEFORMATEX.wFormatTag = WAVE_FORMAT_PCM;
                wAVEFORMATEX.nChannels = 1;
                wAVEFORMATEX.nSamplesPerSec = 24000;
                wAVEFORMATEX.wBitsPerSample = 16;
                wAVEFORMATEX.nBlockAlign = (ushort)(wAVEFORMATEX.nChannels * wAVEFORMATEX.wBitsPerSample / 8);
                wAVEFORMATEX.nAvgBytesPerSec = wAVEFORMATEX.nSamplesPerSec * wAVEFORMATEX.nBlockAlign;
                wAVEFORMATEX.cbSize = 0;
                IntPtr intPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(wAVEFORMATEX));
                Marshal.StructureToPtr(wAVEFORMATEX, intPtr, false);

                WAVEFORMATEX** ppFormat = (WAVEFORMATEX**)ppCoMemOutputWaveFormatEx.ToPointer();
                *ppFormat = (WAVEFORMATEX*)intPtr.ToPointer();
            }
        }

        /// <summary>
        /// ここでトークンを使用し、初期化を行う。
        /// </summary>
        /// <param name="pToken"></param>
        public void SetObjectToken(ISpObjectToken pToken)
        {
            Token = pToken;
            //初期化
            //話者番号を取得し、プロパティに設定。
            Token.GetDWORD(regSpeakerNumber, out uint value);
            SpeakerNumber = (int)value;
        }

        /// <summary>
        /// トークンを取得します。
        /// </summary>
        /// <param name="ppToken"></param>
        public void GetObjectToken(out ISpObjectToken ppToken)
        {
            ppToken = Token;
        }

        #region レジストリ関連

        static Guid CLSID { get; } = new Guid(guidString);

        const string regKey = @"SOFTWARE\Microsoft\Speech\Voices\Tokens\";
        const string regName1 = "VOICEVOX1";
        const string regName2 = "VOICEVOX2";
        const string regAttributes = "Attributes";
        const string regSpeakerNumber = "SpeakerNumber";

        /// <summary>
        /// レジストリ登録されるときに呼ばれます。
        /// </summary>
        /// <param name="key">よくわからん。不使用。リファレンスに書いてあったから定義しただけ。</param>
        [ComRegisterFunction()]
        public static void RegisterClass(string key)
        {
            //四国めたん
            using (RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(regKey + regName1))
            {
                registryKey.SetValue("", "VOICEVOX 四国めたん");
                registryKey.SetValue("411", "VOICEVOX 四国めたん");
                //B書式指定子は中かっこ"{"で囲われる
                registryKey.SetValue("CLSID", CLSID.ToString("B"));
                registryKey.SetValue(regSpeakerNumber, 0);
            }
            using (RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(regKey + regName1 + @"\" + regAttributes))
            {
                registryKey.SetValue("Age", "Teen");
                registryKey.SetValue("Vendor", "Hiroshiba Kazuyuki");
                registryKey.SetValue("Language", "411");
                registryKey.SetValue("Gender", "Female");
                registryKey.SetValue("Name", "VOICEVOX Shikoku Metan");
            }

            //ずんだもん
            using (RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(regKey + regName2))
            {
                registryKey.SetValue("", "VOICEVOX ずんだもん");
                registryKey.SetValue("411", "VOICEVOX ずんだもん");
                //B書式指定子は中かっこ"{"で囲われる
                registryKey.SetValue("CLSID", CLSID.ToString("B"));
                registryKey.SetValue(regSpeakerNumber, 1);
            }
            using (RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(regKey + regName2 + @"\" + regAttributes))
            {
                registryKey.SetValue("Age", "Child");
                registryKey.SetValue("Vendor", "Hiroshiba Kazuyuki");
                registryKey.SetValue("Language", "411");
                registryKey.SetValue("Gender", "Female");
                registryKey.SetValue("Name", "VOICEVOX Zundamon");
            }
        }

        /// <summary>
        /// レジストリ解除されるときに呼ばれます。
        /// </summary>
        /// <param name="key">よくわからん。不使用。リファレンスに書いてあったから定義しただけ。</param>
        [ComUnregisterFunction()]
        public static void UnregisterClass(string key)
        {
            Registry.LocalMachine.DeleteSubKeyTree(regKey + regName1);
            Registry.LocalMachine.DeleteSubKeyTree(regKey + regName2);
        }

        #endregion

        /// <summary>
        /// VOICEVOXへ音声データ作成の指示を送ります。
        /// </summary>
        /// <param name="text">セリフ</param>
        /// <param name="speakerNum">話者番号</param>
        /// <returns>waveデータ</returns>
        async Task<byte[]> SendToVoiceVox(string text, int speakerNum)
        {
            //エンジンが起動中か確認を行う
            Process[] ps = Process.GetProcessesByName("run");
            if (ps.Length == 0)
            {
                Stream stream = Properties.Resources.ボイスボックスが見つかりません;
                byte[] wavData = new byte[stream.Length];
                stream.Read(wavData, 0, (int)stream.Length);
                return wavData;
            }

            string speakerString = speakerNum.ToString();

            //audio_queryのためのデータ
            var parameters = new Dictionary<string, string>()
            {
                { "text", text },
                { "speaker", speakerString },
            };
            //データのエンコード。日本語がある場合、エンコードが必要。
            string encodedParamaters = await new FormUrlEncodedContent(parameters).ReadAsStringAsync();

            try
            {
                //audio_queryを送る
                using (var resultAudioQuery = await httpClient.PostAsync(@"http://localhost:50021/audio_query?" + encodedParamaters, null))
                {
                    //戻り値を文字列にする
                    string resBodyStr = await resultAudioQuery.Content.ReadAsStringAsync();

                    //jsonコンテンツに変換
                    var content = new StringContent(resBodyStr, Encoding.UTF8, @"application/json");
                    //synthesis送信
                    using (var resultSynthesis = await httpClient.PostAsync(@"http://localhost:50021/synthesis?speaker=" + speakerString, content))
                    {
                        //戻り値をストリームで受け取る
                        Stream stream = await resultSynthesis.Content.ReadAsStreamAsync();
                        //byte配列に変換
                        byte[] wavData = new byte[stream.Length];
                        stream.Read(wavData, 0, (int)stream.Length);
                        return wavData;
                    }
                }
            }
            catch (Exception ex)
            {
                Stream stream = Properties.Resources.ボイスボックスと通信ができません;
                byte[] wavData = new byte[stream.Length];
                stream.Read(wavData, 0, (int)stream.Length);
                return wavData;
            }
        }

    }
}
