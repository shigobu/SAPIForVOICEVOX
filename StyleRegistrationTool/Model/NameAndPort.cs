using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyleRegistrationTool.Model
{
    /// <summary>
    /// 名前とポート番号
    /// </summary>
    public class NameAndPort
    {
        public NameAndPort()
        {

        }

        public NameAndPort(string name, int port)
        {
            Name = name ?? "";
            Port = port;
        }

        /// <summary>
        /// アプリ名
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// ポート番号
        /// </summary>
        public int Port { get; set; } = 50021;

        public override bool Equals(object obj)
        {
            var port = obj as NameAndPort;
            return port != null &&
                   Name == port.Name &&
                   Port == port.Port;
        }

        public static bool operator ==(NameAndPort port1, NameAndPort port2)
        {
            return EqualityComparer<NameAndPort>.Default.Equals(port1, port2);
        }

        public static bool operator !=(NameAndPort port1, NameAndPort port2)
        {
            return !(port1 == port2);
        }
    }
}
