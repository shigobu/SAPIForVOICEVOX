using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFVvCommon
{
    /// <summary>
    /// スタイル情報を保持する親クラス。
    /// </summary>
    public class StyleBase
    {
        public StyleBase(string name, string styleName, int iD)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            StyleName = styleName ?? throw new ArgumentNullException(nameof(styleName));
            ID = iD;
        }


        /// <summary>
        /// 話者名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// スタイル
        /// </summary>
        public string StyleName { get; set; }

        /// <summary>
        /// ID
        /// </summary>
        public int ID { get; set; }
    }
}
