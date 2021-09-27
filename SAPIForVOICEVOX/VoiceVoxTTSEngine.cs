using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Setting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TTSEngineLib;

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

        #region ネイティブ

        const ushort WAVE_FORMAT_PCM = 1;

        //SPDFID_WaveFormatExの値は、ヘッダーファイルで定義さていなくて、不明である。
        //したがって、C++コードで値を返す関数を定義しDLLとして出力し、使用することにした。
        [DllImport("SAPIGetStaticValueLib.dll")]
        static extern Guid GetSPDFIDWaveFormatEx();
        //同上
        [DllImport("SAPIGetStaticValueLib.dll")]
        static extern Guid GetSPDFIDText();

        /// <summary>
        /// SPVESACTIONSは、ISpTTSEngineSite :: GetActions呼び出しによって返される値をリストします。これらの値から、TTSエンジンは、アプリケーションによって行われたリアルタイムのアクション要求を受信します。
        /// </summary>
        [Flags]
        enum SPVESACTIONS
        {
            SPVES_CONTINUE = 0,
            SPVES_ABORT = (1 << 0),
            SPVES_SKIP = (1 << 1),
            SPVES_RATE = (1 << 2),
            SPVES_VOLUME = (1 << 3)
        }

        /// <summary>
        /// SPEVENTENUMは、SAPIから可能なイベントを一覧表示します。
        /// </summary>
        enum SPEVENTENUM
        {
            SPEI_UNDEFINED = 0,
            SPEI_START_INPUT_STREAM = 1,
            SPEI_END_INPUT_STREAM = 2,
            SPEI_VOICE_CHANGE = 3,
            SPEI_TTS_BOOKMARK = 4,
            SPEI_WORD_BOUNDARY = 5,
            SPEI_PHONEME = 6,
            SPEI_SENTENCE_BOUNDARY = 7,
            SPEI_VISEME = 8,
            SPEI_TTS_AUDIO_LEVEL = 9,
            SPEI_TTS_PRIVATE = 15,
            SPEI_MIN_TTS = 1,
            SPEI_MAX_TTS = 15,
            SPEI_END_SR_STREAM = 34,
            SPEI_SOUND_START = 35,
            SPEI_SOUND_END = 36,
            SPEI_PHRASE_START = 37,
            SPEI_RECOGNITION = 38,
            SPEI_HYPOTHESIS = 39,
            SPEI_SR_BOOKMARK = 40,
            SPEI_PROPERTY_NUM_CHANGE = 41,
            SPEI_PROPERTY_STRING_CHANGE = 42,
            SPEI_FALSE_RECOGNITION = 43,
            SPEI_INTERFERENCE = 44,
            SPEI_REQUEST_UI = 45,
            SPEI_RECO_STATE_CHANGE = 46,
            SPEI_ADAPTATION = 47,
            SPEI_START_SR_STREAM = 48,
            SPEI_RECO_OTHER_CONTEXT = 49,
            SPEI_SR_AUDIO_LEVEL = 50,
            SPEI_SR_RETAINEDAUDIO = 51,
            SPEI_SR_PRIVATE = 52,
            SPEI_ACTIVE_CATEGORY_CHANGED = 53,
            SPEI_RESERVED5 = 54,
            SPEI_RESERVED6 = 55,
            SPEI_MIN_SR = 34,
            SPEI_MAX_SR = 55,
            SPEI_RESERVED1 = 30,
            SPEI_RESERVED2 = 33,
            SPEI_RESERVED3 = 63
        }

        //SPEVENTENUMはフラグを直接定義しているのではなく、フラグの位置を定義してるらしい？
        //SPFEIマクロを使用して変換する必要がある？
        const ulong SPFEI_FLAGCHECK = (1u << (int)SPEVENTENUM.SPEI_RESERVED1) | (1u << (int)SPEVENTENUM.SPEI_RESERVED2);
        const ulong SPFEI_ALL_TTS_EVENTS = 0x000000000000FFFEul | SPFEI_FLAGCHECK;
        const ulong SPFEI_ALL_SR_EVENTS = 0x003FFFFC00000000ul | SPFEI_FLAGCHECK;
        const ulong SPFEI_ALL_EVENTS = 0xEFFFFFFFFFFFFFFFul;
        ulong SPFEI(SPEVENTENUM SPEI_ord)
        {
            return (1ul << (int)SPEI_ord) | SPFEI_FLAGCHECK;
        }

        enum SPEVENTLPARAMTYPE
        {
            SPET_LPARAM_IS_UNDEFINED = 0,
            SPET_LPARAM_IS_TOKEN = (SPET_LPARAM_IS_UNDEFINED + 1),
            SPET_LPARAM_IS_OBJECT = (SPET_LPARAM_IS_TOKEN + 1),
            SPET_LPARAM_IS_POINTER = (SPET_LPARAM_IS_OBJECT + 1),
            SPET_LPARAM_IS_STRING = (SPET_LPARAM_IS_POINTER + 1)
        }

        #endregion


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
        /// 英語カナ辞書
        /// </summary>
        EnglishKanaDictionary engKanaDict;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VoiceVoxTTSEngine()
        {
            httpClient = new HttpClient();
            engKanaDict = new EnglishKanaDictionary();
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
                ulong writtenWavLength = 0;
                SPVTEXTFRAG currentTextList = pTextFragList;
                while (true)
                {
                    //分割
                    string[] splitedString = currentTextList.pTextStart.Split(charSeparators.ToArray(), StringSplitOptions.RemoveEmptyEntries);

                    foreach (string str in splitedString)
                    {
                        //アクションを確認し、アボートの場合は終了
                        SPVESACTIONS sPVESACTIONS = (SPVESACTIONS)pOutputSite.GetActions();
                        if (sPVESACTIONS.HasFlag(SPVESACTIONS.SPVES_ABORT))
                        {
                            return;
                        }

                        //SAPIイベント
                        pOutputSite.GetEventInterest(out ulong ulongValue);
                        List<SPEVENT> sPEVENTList = new List<SPEVENT>();
                        //プラットフォームのビット数に応じて、wParamとlParamの型が異なるので、分岐
#if x64
                        ulong wParam = (ulong)str.Length;
                        long lParam = currentTextList.pTextStart.IndexOf(str);
#else
                        uint wParam = (uint)str.Length;
                        int lParam = currentTextList.pTextStart.IndexOf(str);
#endif
                        //SPEI_SENTENCE_BOUNDARYとWORD_BOUNDARY_EVENTにのみ対応
                        if ((ulongValue & SPFEI(SPEVENTENUM.SPEI_SENTENCE_BOUNDARY)) == SPFEI(SPEVENTENUM.SPEI_SENTENCE_BOUNDARY))
                        {
                            SPEVENT SENTENCE_BOUNDARY_EVENT = new SPEVENT();
                            SENTENCE_BOUNDARY_EVENT.eEventId = (ushort)SPEVENTENUM.SPEI_SENTENCE_BOUNDARY;
                            SENTENCE_BOUNDARY_EVENT.elParamType = (ushort)SPEVENTLPARAMTYPE.SPET_LPARAM_IS_UNDEFINED;
                            SENTENCE_BOUNDARY_EVENT.wParam = wParam;
                            SENTENCE_BOUNDARY_EVENT.lParam = lParam;
                            SENTENCE_BOUNDARY_EVENT.ullAudioStreamOffset = writtenWavLength;

                            sPEVENTList.Add(SENTENCE_BOUNDARY_EVENT);
                        }
                        if ((ulongValue & SPFEI(SPEVENTENUM.SPEI_WORD_BOUNDARY)) == SPFEI(SPEVENTENUM.SPEI_WORD_BOUNDARY))
                        {
                            SPEVENT WORD_BOUNDARY_EVENT = new SPEVENT();
                            WORD_BOUNDARY_EVENT.eEventId = (ushort)SPEVENTENUM.SPEI_WORD_BOUNDARY;
                            WORD_BOUNDARY_EVENT.elParamType = (ushort)SPEVENTLPARAMTYPE.SPET_LPARAM_IS_UNDEFINED;
                            WORD_BOUNDARY_EVENT.wParam = wParam;
                            WORD_BOUNDARY_EVENT.lParam = lParam;
                            WORD_BOUNDARY_EVENT.ullAudioStreamOffset = writtenWavLength;
                            sPEVENTList.Add(WORD_BOUNDARY_EVENT);
                        }
                        if (sPEVENTList.Count > 0)
                        {
                            SPEVENT[] sPEVENTArr = sPEVENTList.ToArray();
                            pOutputSite.AddEvents(ref sPEVENTArr[0], (uint)sPEVENTArr.Length);
                        }
                        //英単語をカナへ置換
                        string replaceString = engKanaDict.ReplaceEnglishToKana(str);

                        //VOICEVOXへ送信
                        //asyncメソッドにはref引数を指定できないらしいので、awaitも使用できない。awaitを使用しない実装にした。
                        Task<byte[]> waveDataTask = SendToVoiceVox(replaceString, SpeakerNumber, speed, pitch, intonation, volume);
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
                        writtenWavLength += OutputSiteWriteSafe(pOutputSite, waveData);
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
        private uint OutputSiteWriteSafe(ISpTTSEngineSite pOutputSite, byte[] data)
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
                return written;
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

#region トークン関連

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

#endregion

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

        const string wavMediaType = "audio/wav";

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
