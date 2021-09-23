using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPIForVOICEVOX
{
    class EnglishKanaDictionary : Dictionary<string, string>
    {
        /// <summary>
        /// 区切り文字列
        /// </summary>
        const string delimiterString = "  ";

        /// <summary>
        /// 区切り文字の配列
        /// </summary>
        readonly string[] delimiterStringArr = { delimiterString };

        /// <summary>
        /// 英語カナ辞書を初期化します。
        /// </summary>
        public EnglishKanaDictionary() : base(120000)
        {
            //改行で分割し、行ごとのデータにする。
            string[] newLineString = { "\r\n" };
            string[] lines = Properties.Resources.eng2kanaDict.Split(newLineString, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                //英語とカナを分割
                string[] engKana = line.Split(delimiterStringArr, StringSplitOptions.RemoveEmptyEntries);
                if (engKana.Length < 2)
                {
                    continue;
                }
                //内部辞書に追加
                string eng = engKana[0];
                string kana = engKana[1];
                this.Add(eng, kana);
            }
        }

        /// <summary>
        /// 指定の英単語の読みに対応するカナを取得します。
        /// </summary>
        /// <param name="key">英単語</param>
        /// <returns>カナ</returns>
        public new string this[string key] 
        { 
            get
            {
                return base[key];
            }
            private set
            {
                base[key] = value;
            }
        }
    }
}
