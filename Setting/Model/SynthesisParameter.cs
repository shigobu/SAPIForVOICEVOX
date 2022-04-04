using System;
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

        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
          => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion

        #region Equals
        public override bool Equals(object obj)
        {
            return obj is SynthesisParameter parameter &&
                   ValueMode == parameter.ValueMode &&
                   Volume == parameter.Volume &&
                   Speed == parameter.Speed &&
                   Pitch == parameter.Pitch &&
                   Intonation == parameter.Intonation;
        }

        public override int GetHashCode()
        {
            int hashCode = 1557109181;
            hashCode = hashCode * -1521134295 + ValueMode.GetHashCode();
            hashCode = hashCode * -1521134295 + Volume.GetHashCode();
            hashCode = hashCode * -1521134295 + Speed.GetHashCode();
            hashCode = hashCode * -1521134295 + Pitch.GetHashCode();
            hashCode = hashCode * -1521134295 + Intonation.GetHashCode();
            return hashCode;
        }

        #endregion

        /// <summary>
        /// 設定ファイルバージョン
        /// </summary>
        public Version Version { get; set; } = new Version(1, 0);

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

        private int _Port = 50021;
        /// <summary>
        /// ポートを取得、設定します。
        /// </summary>
        public int Port
        {
            get => _Port;
            set
            {
                if (_Port == value) return;
                _Port = value;
                RaisePropertyChanged();
            }
        }

        private int _ID = 0;
        /// <summary>
        /// 話者IDを取得、設定します。
        /// </summary>
        public int ID
        {
            get => _ID;
            set
            {
                if (_ID == value) return;
                _ID = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 設定ファイルのバージョンを所得、設定します。
        /// </summary>
        public string Version { get; set; } = new Version(1, 0, 0).ToString();
    }
}
