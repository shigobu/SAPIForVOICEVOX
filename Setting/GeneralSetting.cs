using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// 調声設定モード
        /// </summary>
        public SynthesisSettingMode synthesisSettingMode = SynthesisSettingMode.Batch;

    }
}
