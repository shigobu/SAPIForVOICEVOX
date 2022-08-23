using System;
using System.Collections.Generic;

namespace SFVvCommon
{
    /// <summary>
    /// スタイルの並び替え情報を記したクラス。
    /// nameOrderの順に並び替える用にする。
    /// nameOrderに含まれていない場合は、nameOrderより後に標準の順番で並ぶ。
    /// </summary>
    class StyleComparer : IComparer<string>
    {
        private readonly string[] nameOrder = { "四国めたん", "ずんだもん", "春日部つむぎ", "波音リツ", "雨晴はう", "玄野武宏", "白上虎太郎", "青山龍星", "冥鳴ひまり", "九州そら", "もち子さん", "剣崎雌雄" };
        /// <summary>
        /// 大小関係を調べて、対応した値を返します。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>xがyより小さいときはマイナスの数、大きいときはプラスの数、同じときは0を返す</returns>
        public int Compare(string x, string y)
        {
            //nullが最も小さいとする
            if (x == null && y == null)
            {
                return 0;
            }
            if (x == null)
            {
                return -1;
            }
            if (y == null)
            {
                return 1;
            }

            //同じ場合は0
            if (x == y)
            {
                return 0;
            }

            //nameOrderの何番目か判定して、引き算。
            int xIndex = Array.IndexOf(nameOrder, x);
            int yIndex = Array.IndexOf(nameOrder, y);
            if (xIndex < 0 && yIndex < 0)
            {
                return string.Compare(x, y);
            }
            if (xIndex < 0)
            {
                return 1;
            }
            if (yIndex < 0)
            {
                return -1;
            }
            return xIndex - yIndex;
        }
    }
}
