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

namespace SAPIForVOICEVOX
{
    [Guid(guidString)]
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    public class VoiceVoxTTSEngine : ISpTTSEngine, ISpObjectWithToken, ISpTokenUI
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
            if (rguidFormatId == GetSPDFIDText())
            {
                return;
            }

            SPVTEXTFRAG currentTextList = pTextFragList;
            while (true)
            {
                pOutputSite.GetRate(out int tempInt);
                //SAPIは0が真ん中
                double speed;
                if (tempInt < 0)
                {
                    speed = Map(tempInt, -10, 0, 0.5, 1.0);
                }
                else
                {
                    speed = Map(tempInt, 0, 10, 1.0, 2.0);
                }
                pOutputSite.GetVolume(out ushort tempUshort);
                double volume = Map(tempUshort, 0, 100, 0.0, 1.0);
                //VOICEVOXへ送信
                //asyncメソッドにはref引数を指定できないらしいので、awaitも使用できない。awaitを使用しない実装にした。
                Task<byte[]> waveDataTask = SendToVoiceVox(currentTextList.pTextStart, SpeakerNumber, speed, 0, volume);
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
            pOutputFormatId = GetSPDFIDWaveFormatEx();

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
        const string regUI = "UI";
        const string regEngineProperties = "EngineProperties";

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
                registryKey.SetValue("Version", "11.0");
            }
            using (RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(regKey + regName1 + @"\" + regUI + @"\" + regEngineProperties))
            {
                //B書式指定子は中かっこ"{"で囲われる
                registryKey.SetValue("CLSID", CLSID.ToString("B"));
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
        /// <param name="speedScale">話速 0.5~2.0 中央=1</param>
        /// <param name="pitchScale">音高 -0.15~0.15 中央=0</param>
        /// <param name="volumeScale">音量 0.0~1.0</param>
        /// <returns>waveデータ</returns>
        async Task<byte[]> SendToVoiceVox(string text, int speakerNum, double speedScale, double pitchScale, double volumeScale)
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
                    //jsonの値変更
                    JObject jsonObj = JObject.Parse(resBodyStr);
                    jsonObj["speedScale"] = speedScale;
                    jsonObj["pitchScale"] = pitchScale;
                    jsonObj["volumeScale"] = volumeScale;
                    string jsonString = JsonConvert.SerializeObject(jsonObj, Formatting.None);

                    //jsonコンテンツに変換
                    var content = new StringContent(jsonString, Encoding.UTF8, @"application/json");
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

        #region UI関連

        private const string SPDUI_EngineProperties = "EngineProperties";
        const int S_OK = 0x00000000;


        /// <summary>
        /// UIサポートしているかどうかを返します。
        /// </summary>
        public int IsUISupported(string pszTypeOfUI, IntPtr pvExtraData, uint cbExtraData, IntPtr punkObject, out bool pfSupported)
        {
            pfSupported = false;

            if (pszTypeOfUI.Contains(SPDUI_EngineProperties))
            {
                pfSupported = true;
            }

            return S_OK;
        }

        /// <summary>
        /// 設定画面を表示します。
        /// </summary>
        /// <param name="hwndParent"></param>
        /// <param name="pszTitle"></param>
        /// <param name="pszTypeOfUI"></param>
        /// <param name="pvExtraData"></param>
        /// <param name="cbExtraData"></param>
        /// <param name="pToken"></param>
        /// <param name="punkObject"></param>
        /// <returns></returns>
        public int DisplayUI(IntPtr hwndParent, string pszTitle, string pszTypeOfUI, IntPtr pvExtraData, uint cbExtraData, ISpObjectToken pToken, IntPtr punkObject)
        {
            //関数内に処理が入る気配が無いままエラーメッセージが表示される、わけわからん。
            string dllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string dllDir = Path.GetDirectoryName(dllPath);
            string logFileName = Path.Combine(dllDir, "LOG.txt");
            using (StreamWriter streamWriter = new StreamWriter(logFileName, false))
            {
                streamWriter.Write("DisplayUI");
            }

            try
            {
                //System.Windows.Forms.MessageBox.Show(pszTypeOfUI);
            }
            catch (Exception ex)
            {
                using (StreamWriter streamWriter = new StreamWriter(logFileName, false))
                {
                    streamWriter.Write(ex.ToString());
                }
            }

            return S_OK;
        }

        #endregion
    }
}
