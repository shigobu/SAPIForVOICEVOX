using System;
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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Setting;

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

        //SPDFID_WaveFormatExの値は、ヘッダーファイルで定義さていなくて、不明である。
        //したがって、C++コードで値を返す関数を定義しDLLとして出力し、使用することにした。
        [DllImport("SAPIGetStaticValueLib.dll")]
        static extern Guid GetSPDFIDWaveFormatEx();
        //同上
        [DllImport("SAPIGetStaticValueLib.dll")]
        static extern Guid GetSPDFIDText();


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
            if (rguidFormatId == GetSPDFIDText())
            {
                return;
            }

            //SAPIの情報取得
            pOutputSite.GetRate(out int tempInt);
            //SAPIは0が真ん中
            double SAPIspeed;
            if (tempInt < 0)
            {
                SAPIspeed = Map(tempInt, -10, 0, 0.5, 1.0);
            }
            else
            {
                SAPIspeed = Map(tempInt, 0, 10, 1.0, 2.0);
            }
            pOutputSite.GetVolume(out ushort tempUshort);
            double SAPIvolume = Map(tempUshort, 0, 100, 0.0, 1.0);

            //設定アプリのデータ取得
            GetSettingData(SpeakerNumber, out GeneralSetting generalSetting, out SynthesisParameter synthesisParameter);
            double speed;
            double volume;
            if (synthesisParameter.ValueMode == ParameterValueMode.SAPI)
            {
                speed = SAPIspeed;
                volume = SAPIvolume;
            }
            else
            {
                speed = synthesisParameter.Speed;
                volume = synthesisParameter.Volume;
            }
            double pitch = synthesisParameter.Pitch;
            double intonation = synthesisParameter.Intonation;

            //区切り文字設定
            List<char> charSeparators = new List<char>();
            if (generalSetting.isSplitKuten ?? false)
            {
                charSeparators.Add('。');
            }
            if (generalSetting.isSplitTouten ?? false)
            {
                charSeparators.Add('、');
            }

            try
            {
                SPVTEXTFRAG currentTextList = pTextFragList;
                while (true)
                {
                    //分割
                    string[] splitedString = currentTextList.pTextStart.Split(charSeparators.ToArray(), StringSplitOptions.RemoveEmptyEntries);

                    foreach (string str in splitedString)
                    {
                        //VOICEVOXへ送信
                        //asyncメソッドにはref引数を指定できないらしいので、awaitも使用できない。awaitを使用しない実装にした。
                        Task<byte[]> waveDataTask = SendToVoiceVox(str, SpeakerNumber, speed, pitch, intonation, volume);
                        byte[] waveData;
                        try
                        {
                            waveDataTask.Wait();
                            waveData = waveDataTask.Result;
                        }
                        catch (AggregateException ex) when (ex.InnerException is VoiceVoxEngineException)
                        {
                            //エンジンエラーを通知するかどうか
                            if (generalSetting.shouldNotifyEngineError ?? false)
                            {
                                VoiceVoxEngineException voiceNotification = ex.InnerException as VoiceVoxEngineException;
                                waveData = voiceNotification.ErrorVoice;
                            }
                            else
                            {
                                waveData = new byte[0];
                            }
                        }
                        waveData = DeleteHeaderFromWaveData(waveData);

                        //書き込み
                        OutputSiteWriteSafe(pOutputSite, waveData);
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
            //Task.Waitは例外をまとめてAggregateExceptionで投げる。
            catch (AggregateException ex) when (ex.InnerException is VoiceNotificationException)
            {
                VoiceNotificationException voiceNotification = ex.InnerException as VoiceNotificationException;
                byte[] waveData = voiceNotification.ErrorVoice;
                waveData = DeleteHeaderFromWaveData(waveData);

                //書き込み
                OutputSiteWriteSafe(pOutputSite, waveData);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// SAPI音声出力へ、安全な書き込みを行います。
        /// </summary>
        /// <param name="pOutputSite">TTSEngineSiteオブジェクト</param>
        /// <param name="data">音声データ</param>
        private void OutputSiteWriteSafe(ISpTTSEngineSite pOutputSite, byte[] data)
        {
            if (data is null)
            {
                data = new byte[0];
            }

            //受け取った音声データをpOutputSiteへ書き込む
            IntPtr pWavData = IntPtr.Zero;
            try
            {
                //メモリが確実に確保され、確実に代入されるためのおまじない。
                RuntimeHelpers.PrepareConstrainedRegions();
                try { }
                finally
                {
                    pWavData = Marshal.AllocCoTaskMem(data.Length);
                }
                Marshal.Copy(data, 0, pWavData, data.Length);
                pOutputSite.Write(pWavData, (uint)data.Length, out uint written);
            }
            finally
            {
                if (pWavData != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(pWavData);
                }
            }
        }

        const ushort channels = 1;
        const uint samplesPerSec = 24000;
        const ushort bitsPerSample = 16;

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
            pOutputFormatId = GetSPDFIDWaveFormatEx();

            //comインターフェースのラップクラス自動生成がうまく行かなかったので、unsafeでポインタを直接使用する
            unsafe
            {
                WAVEFORMATEX wAVEFORMATEX = new WAVEFORMATEX();
                wAVEFORMATEX.wFormatTag = WAVE_FORMAT_PCM;
                wAVEFORMATEX.nChannels = channels;
                wAVEFORMATEX.nSamplesPerSec = samplesPerSec;
                wAVEFORMATEX.wBitsPerSample = bitsPerSample;
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

        static string wavMediaType = "audio/wav";

        /// <summary>
        /// VOICEVOXへ音声データ作成の指示を送ります。
        /// </summary>
        /// <param name="text">セリフ</param>
        /// <param name="speakerNum">話者番号</param>
        /// <param name="speedScale">話速 0.5~2.0 中央=1</param>
        /// <param name="pitchScale">音高 -0.15~0.15 中央=0</param>
        /// <param name="intonation">抑揚 0~2 中央=1</param>
        /// <param name="volumeScale">音量 0.0~1.0</param>
        /// <returns>waveデータ</returns>
        async Task<byte[]> SendToVoiceVox(string text, int speakerNum, double speedScale, double pitchScale, double intonation, double volumeScale)
        {
            //エンジンが起動中か確認を行う
            Process[] ps = Process.GetProcessesByName("run");
            if (ps.Length == 0)
            {
                throw new VoiceVoxNotFoundException();
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
                using (var resultAudioQuery = await httpClient.PostAsync(@"http://127.0.0.1:50021/audio_query?" + encodedParamaters, null))
                {
                    //戻り値を文字列にする
                    string resBodyStr = await resultAudioQuery.Content.ReadAsStringAsync();

                    //jsonの値変更
                    JObject jsonObj = JObject.Parse(resBodyStr);
                    SetValueJObjectSafe(jsonObj, "speedScale", speedScale);
                    SetValueJObjectSafe(jsonObj, "pitchScale", pitchScale);
                    SetValueJObjectSafe(jsonObj, "intonationScale", intonation);
                    SetValueJObjectSafe(jsonObj, "volumeScale", volumeScale);

                    string jsonString = JsonConvert.SerializeObject(jsonObj, Formatting.None);

                    //jsonコンテンツに変換
                    var content = new StringContent(jsonString, Encoding.UTF8, @"application/json");
                    //synthesis送信
                    using (var resultSynthesis = await httpClient.PostAsync(@"http://127.0.0.1:50021/synthesis?speaker=" + speakerString, content))
                    {
                        HttpContent httpContent = resultSynthesis.Content;
                        //音声データで無い場合
                        if (httpContent.Headers.ContentType.MediaType != wavMediaType)
                        {
                            throw new VoiceVoxEngineException();
                        }
                        //戻り値をストリームで受け取る
                        Stream stream = await httpContent.ReadAsStreamAsync();
                        //byte配列に変換
                        byte[] wavData = new byte[stream.Length];
                        stream.Read(wavData, 0, (int)stream.Length);
                        return wavData;
                    }
                }
            }
            catch (VoiceVoxEngineException)
            {
                //エンジンエラーはそのまま呼び出し元へ投げる。
                throw;
            }
            catch (Exception ex)
            {
                throw new VoiceVoxConnectionException(ex);
            }
        }

        /// <summary>
        ///  数値をある範囲から別の範囲に変換します。
        /// </summary>
        /// <param name="x">変換したい数値</param>
        /// <param name="in_min">現在の範囲の下限</param>
        /// <param name="in_max">現在の範囲の上限</param>
        /// <param name="out_min">変換後の範囲の下限</param>
        /// <param name="out_max">変換後の範囲の上限</param>
        /// <returns>変換結果</returns>
        double Map(double x, double in_min, double in_max, double out_min, double out_max)
        {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }

        /// <summary>
        /// JObjectへ、プロパティの存在確認を行ってから、値を代入します。プロパティが存在しない場合は、代入されません。
        /// </summary>
        /// <param name="jobject">対象JObject</param>
        /// <param name="propertyName">プロパティ名</param>
        /// <param name="value">値</param>
        private void SetValueJObjectSafe(JObject jobject, string propertyName, double value)
        {
            if (jobject.ContainsKey(propertyName))
            {
                jobject[propertyName] = value;
            }
        }

        /// <summary>
        /// Wavデータからヘッダーを削除します。
        /// </summary>
        /// <param name="waveData">Wavデータ</param>
        /// <returns>
        /// ヘッダーの無いWavデータ。
        /// ただのPCMデータ。
        /// </returns>
        public static byte[] DeleteHeaderFromWaveData(byte[] waveData)
        {
            if (waveData is null)
            {
                throw new ArgumentNullException(nameof(waveData));
            }

            //先頭にWaveのヘッダーがあるかどうかの確認
            byte[] RIFF = { 0x52, 0x49, 0x46, 0x46 };
            if (waveData.Length < RIFF.Length)
            {
                return waveData;
            }
            for (int i = 0; i < RIFF.Length; i++)
            {
                if (waveData[i] != RIFF[i])
                {
                    //異なる場合、そのまま返す。
                    return waveData;
                }
            }
            int wavHeaderSize = 44;
            byte[] voiceData = new byte[waveData.Length - wavHeaderSize];
            //waveデータからヘッダー部分を削除
            Array.Copy(waveData, wavHeaderSize, voiceData, 0, voiceData.Length);
            return voiceData;
        }

        #region 設定データ取得関連

        /// <summary>
        /// 設定データを取得します。
        /// </summary>
        /// <param name="speakerNum">話者番号</param>
        /// <param name="generalSetting">全般設定</param>
        /// <param name="synthesisParameter">調声設定</param>
        private void GetSettingData(int speakerNum, out GeneralSetting generalSetting, out SynthesisParameter synthesisParameter)
        {
            generalSetting = ViewModel.LoadGeneralSetting();
            switch (generalSetting.synthesisSettingMode)
            {
                case SynthesisSettingMode.Batch:
                    synthesisParameter = ViewModel.LoadBatchSynthesisParameter();
                    break;
                case SynthesisSettingMode.EachCharacter:
                    synthesisParameter = ViewModel.LoadSpeakerSynthesisParameter()[speakerNum];
                    break;
                default:
                    synthesisParameter = new SynthesisParameter();
                    break;
            }
        }

        #endregion
    }
}
