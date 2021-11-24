using RomajiToHiraganaLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SAPIForVOICEVOX
{
    /// <summary>
    /// 英単語を含む文字列を英単語の読みと対応するカナに変換する関数を提供します。
    /// </summary>
    public class EnglishKanaDictionary : Dictionary<string, string>
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

        /// <summary>
        /// 指定の文字列に含まれている英単語を対応する読みのカナへ置換した文字列を返します。
        /// </summary>
        /// <param name="sourceString"></param>
        /// <returns></returns>
        public string ReplaceEnglishToKana(string sourceString)
        {
            //英単語の抽出
            Regex regex = new Regex(@"[a-zA-Z']+", RegexOptions.IgnoreCase);
            IEnumerable<Match> matchCollection = regex.Matches(sourceString).Cast<Match>();
            //Matchから文字列を取得。文字数が大きい順で並び替え。
            IEnumerable<string> englishWords = matchCollection.Select(match => match.Value.ToLowerInvariant()).OrderByDescending(x => x.Length);

            string returnString = sourceString;
            foreach (var english in englishWords)
            {
                string kana;
                if (this.ContainsKey(english))
                {
                    kana = this[english];
                }
                else
                {
                    //辞書に含まれていない場合、２つの単語を結合されているとみなして、検索
                    string temp = TwoWordEngToKana(english);
                    if (temp == null)
                    {
                        continue;
                    }
                    kana = temp;
                }
                returnString = returnString.ToLowerInvariant().Replace(english, kana);
            }

            //ローマ字辞書を全て走査し置換を行うため、最後に行う。
            returnString = RomajiToHiragana.Convert(returnString);

            return returnString;
        }

        /// <summary>
        /// ２つの単語を結合した英単語の対応する読みのカナを返します。
        /// </summary>
        /// <param name="Word">英単語</param>
        /// <returns></returns>
        private string TwoWordEngToKana(string word)
        {
            //6文字未満はスキップ
            if (word.Length < 6)
            {
                return null;
            }

            for (int i = 0; i < word.Length - 1; i++)
            {
                string eng1 = word.Substring(0, i + 1);
                string eng2 = word.Substring(i + 1);
                //対応するカナが無い場合、継続。
                if (!this.ContainsKey(eng1))
                {
                    continue;
                }
                if (!this.ContainsKey(eng2))
                {
                    continue;
                }
                return this[eng1] + this[eng2];
            }
            //見つからなかった場合nullを返す。
            return null;
        }
    }
}
