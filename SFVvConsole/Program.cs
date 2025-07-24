using SFVvCommon;
using System;
using System.IO;
using System.IO.Pipes;

namespace SFVvConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                using (var pipeClient = new NamedPipeClientStream(Common.PipeName))
                {
                    pipeClient.Connect();
                    using (StreamReader reader = new StreamReader(pipeClient))
                    {
                        if (pipeClient.CanRead)
                        {
                            // 有効な値が読み込めるまでループ
                            string readText = null;
                            while (pipeClient.IsConnected)
                            {
                                readText = reader.ReadLine();
                                if (readText != null)
                                {
                                    break;
                                }
                            }
                            Console.WriteLine(readText);
                        }
                    }
                }
            }
        }
    }
}
