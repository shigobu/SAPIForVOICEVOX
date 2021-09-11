using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Setting
{
    class ViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChangedの実装
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
          => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public ViewModel()
        {
            generalSetting = new GeneralSetting();
        }

        /// <summary>
        /// Model
        /// </summary>
        private GeneralSetting generalSetting = null;

        /// <summary>
        /// 句点で分割するかどうかを取得、設定します。
        /// </summary>
        public bool? IsSplitKuten
        {
            get => generalSetting.isSplitKuten;
            set
            {
                if (generalSetting.isSplitKuten == value) return;
                generalSetting.isSplitKuten = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 読点で分割するかどうかを取得、設定します。
        /// </summary>
        public bool? IsSplitTouten
        {
            get => generalSetting.isSplitTouten;
            set
            {
                if (generalSetting.isSplitTouten == value) return;
                generalSetting.isSplitTouten = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 調声設定モードを取得、設定します。
        /// </summary>
        public SynthesisSettingMode SynthesisSettingMode
        {
            get => generalSetting.synthesisSettingMode;
            set
            {
                if (generalSetting.synthesisSettingMode == value) return;
                generalSetting.synthesisSettingMode = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 一括設定
        /// </summary>
        /// <remarks>
        /// このプロパティを直接DataContextに入れるので、変更通知の仕組みは入れていない。
        /// </remarks>
        public SynthesisParameter BatchParameter { get; set; } = new SynthesisParameter();

        /// <summary>
        /// 話者１のパラメータ
        /// </summary>
        /// <remarks>
        /// このプロパティを直接DataContextに入れるので、変更通知の仕組みは入れていない。
        /// </remarks>
        public SynthesisParameter Speaker1Parameter { get; set; } = new SynthesisParameter();

        /// <summary>
        /// 話者２のパラメータ
        /// </summary>
        /// <remarks>
        /// このプロパティを直接DataContextに入れるので、変更通知の仕組みは入れていない。
        /// </remarks>
        public SynthesisParameter Speaker2Parameter { get; set; } = new SynthesisParameter();

    }
}
