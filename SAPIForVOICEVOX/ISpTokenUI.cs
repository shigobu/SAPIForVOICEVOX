using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TTSEngineLib;

namespace SAPIForVOICEVOX
{
    /// <summary>
    /// ISpObjectTokenに関連付けられたユーザーインターフェイスをプログラムで管理する手段を開発者に提供します。
    /// </summary>
    //[ComImport]
    [Guid("F8E690F0-39CB-4843-B8D7-C84696E1119D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ISpTokenUI
    {
        /// <summary>
        /// 指定されたUIタイプがトークンでサポートされているかどうかを判別します。
        /// </summary>
        /// <param name="pszTypeOfUI">オブジェクトのUIタイプを含むnullで終了する文字列のアドレス。</param>
        /// <param name="pvExtraData">オブジェクトに必要な追加情報へのポインター。ISpTokenUIのオブジェクト実装のおもむく提供されるデータのフォーマットおよび使用。</param>
        /// <param name="cbExtraData">ExtraDataのサイズ（バイト単位）。ISpTokenUIのオブジェクト実装のおもむく提供されるデータのフォーマットおよび使用。</param>
        /// <param name="punkObject">オブジェクトのIUnknownインターフェイスのアドレス。備考セクションを参照してください。</param>
        /// <param name="pfSupported">
        /// インターフェースのサポートを示す値を受け取る変数のアドレス。
        /// この値は、このインターフェイスがサポートされている場合はTRUEに設定され、サポートされていない場合はFALSEに設定されます。
        /// この値がTRUEであるが、戻りコードがS_FALSEの場合、UIタイプ（pszTypeOfUI）はサポートされますが、現在のパラメーターまたはランタイム環境ではサポートされません。
        /// UIオブジェクトの実装者に確認して、実行時の要件を確認してください。
        /// </param>
        void IsUISupported(string pszTypeOfUI, IntPtr pvExtraData, uint cbExtraData, IntPtr punkObject, out bool pfSupported);

        /// <summary>
        /// オブジェクトトークンに関連付けられたUIを表示します。
        /// </summary>
        /// <param name="hwndParent">親ウィンドウのハンドルを指定します。</param>
        /// <param name="pszTitle">
        /// UIに表示するウィンドウタイトルを含むnullで終了する文字列のアドレス。
        /// この値をNULLに設定して、TokenUIオブジェクトがデフォルトのウィンドウタイトルを使用する必要があることを示すことができます。
        /// </param>
        /// <param name="pszTypeOfUI">表示するUIタイプを含むnullで終了する文字列のアドレス。</param>
        /// <param name="pvExtraData">オブジェクトに必要な追加情報へのポインター。ISP TokenUIのオブジェクトの実装は、提供されたデータのフォーマット及び使用法を指示します。</param>
        /// <param name="cbExtraData">ExtraDataのサイズ（バイト単位）。ISP TokenUIのオブジェクトの実装は、提供されたデータのフォーマット及び使用法を指示します。</param>
        /// <param name="pToken">オブジェクトトークン識別子を含むISpObjectTokenのアドレス。備考セクションを参照してください。</param>
        /// <param name="punkObject">IUnknownインターフェイスポインタのアドレス。備考セクションを参照してください。</param>
        void DisplayUI(IntPtr hwndParent, string pszTitle, string pszTypeOfUI, IntPtr pvExtraData, uint cbExtraData, ISpObjectToken pToken, IntPtr punkObject);
    }
}
