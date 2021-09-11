using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Setting
{
    /// <summary>
    /// VOICEVOXに必要なパラメータを定義します。
    /// </summary>
    public class SynthesisParameter : INotifyPropertyChanged
    {
        #region INotifyPropertyChangedの実装
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
          => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion


        private ParameterValueMode _ValueMode = ParameterValueMode.SAPI;
        /// <summary>
        /// SAPIの値を使用するか、設定アプリの値を使用するかを取得、設定します。
        /// </summary>
        public ParameterValueMode ValueMode
        {
            get => _ValueMode;
            set
            {
                if (_ValueMode == value) return;
                _ValueMode = value;
                RaisePropertyChanged();
            }
        }

        private double _Volume = 1;
        /// <summary>
        /// 音量を取得、設定します。
        /// </summary>
        public double Volume
        {
            get => _Volume;
            set
            {
                if (_Volume == value) return;
                _Volume = value;
                RaisePropertyChanged();
            }
        }

        private double _Speed = 1;
        /// <summary>
        /// 話速を取得、設定します。
        /// </summary>
        public double Speed
        {
            get => _Speed;
            set
            {
                if (_Speed == value) return;
                _Speed = value;
                RaisePropertyChanged();
            }
        }

        private double _Pitch = 0;
        /// <summary>
        /// 音高を取得、設定します。
        /// </summary>
        public double Pitch
        {
            get => _Pitch;
            set
            {
                if (_Pitch == value) return;
                _Pitch = value;
                RaisePropertyChanged();
            }
        }

        private double _Intonation = 1;
        /// <summary>
        /// 抑揚を取得、設定します。
        /// </summary>
        public double Intonation
        {
            get => _Intonation;
            set
            {
                if (_Intonation == value) return;
                _Intonation = value;
                RaisePropertyChanged();
            }
        }

    }

}
