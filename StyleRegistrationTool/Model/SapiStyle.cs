using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyleRegistrationTool.Model
{
    class SapiStyle
    {

        /// <summary>
        /// 話者名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// スタイル
        /// </summary>
        public string StyleName { get; set; }

        /// <summary>
        /// sapiに表示される名前
        /// </summary>
        public string SpaiName
        {
            get
            {
                return Name + " " + StyleName;
            }
        }

        /// <summary>
        /// ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// SAPIForVOICEVOXモジュールのGuid
        /// </summary>
        public Guid SAPIForModuleGuid { get; set; }

    }
}
