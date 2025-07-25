﻿using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace SetupCustomActions
{
    [RunInstaller(true)]
    public class CustomActions : Installer
    {
        /// <summary>
        /// インストールするときに呼ばれる。
        /// </summary>
        /// <param name="stateSaver"></param>
        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);

            RegisterDLL(RegisterType.Register);

            ExecuteStyleRegistrationTool();
        }

        /// <summary>
        /// アンインストールするときに呼ばれる。
        /// </summary>
        /// <param name="savedState"></param>
        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);
            try
            {
                RegisterDLL(RegisterType.UnRegister);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
        }

        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);

        }

        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);

        }

        private void RegisterDLL(RegisterType type)
        {
            // RegAsm のパスを取得
            string regAsmPath = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "RegAsm.exe");

            Process process = new Process();
            process.StartInfo.FileName = regAsmPath;

            //コマンドライン引数の作成
            string installDirectory = this.Context.Parameters["dir"];
            string targetDllName = "SAPIForVOICEVOX.dll";
            string dllPath = Path.Combine(installDirectory, targetDllName);
            string arguments = dllPath;
            if (type == RegisterType.UnRegister)
            {
                arguments += " /unregister ";
            }
            else
            {
                arguments += " /codebase";
            }
            process.StartInfo.Arguments = arguments;
            // ウィンドウを表示しない
            process.StartInfo.CreateNoWindow = true;
            // カレントディレクトリ指定
            process.StartInfo.WorkingDirectory = installDirectory;
            // シェルを使わない
            process.StartInfo.UseShellExecute = false;

            // 起動
            process.Start();

            // プロセス終了まで待機する
            process.WaitForExit();
            process.Close();
        }

        private void ExecuteStyleRegistrationTool()
        {
            string installDirectory = this.Context.Parameters["dir"];
            string targetExeName = "StyleRegistrationTool.exe";
            string targetExePath = Path.Combine(installDirectory, targetExeName);

            Process process = new Process();
            process.StartInfo.FileName = targetExePath;

            //コマンドライン引数の作成
            string arguments = "/install";
            process.StartInfo.Arguments = arguments;

            // 起動
            process.Start();
        }

        /// <summary>
        /// レジストリ登録するか。
        /// レジストリ解除するか。
        /// </summary>
        enum RegisterType
        {
            /// <summary>
            /// 登録
            /// </summary>
            Register,
            /// <summary>
            /// 解除
            /// </summary>
            UnRegister
        }
    }
}
