namespace Setting
{
    /// <summary>
    /// 全般設定を定義します。
    /// </summary>
    public class GeneralSetting
    {
        /// <summary>
        /// 句点で分割するかどうか。
        /// </summary>
        public bool? isSplitKuten = true;

        /// <summary>
        /// 読点で分割するかどうか。
        /// </summary>
        public bool? isSplitTouten = false;

        /// <summary>
        /// 改行で分割するかどうか。
        /// </summary>
        public bool? isSplitNewLine = false;

        /// <summary>
        /// 調声設定モード
        /// </summary>
        public SynthesisSettingMode synthesisSettingMode = SynthesisSettingMode.Batch;

        /// <summary>
        /// エンジンエラーを通知するかどうか
        /// </summary>
        public bool? shouldNotifyEngineError = true;

        /// <summary>
        /// SAPIイベントを使うかどうか
        /// </summary>
        public bool? useSspiEvent = true;

        /// <summary>
        /// 疑問文を自動調声するかどうか
        /// </summary>
        public bool? useInterrogativeAutoAdjustment = false;

    }
}
