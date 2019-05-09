using AsyncRAT_Sharp.Cryptography;
using AsyncRAT_Sharp.Sockets;
using System.Collections.Generic;

namespace AsyncRAT_Sharp
{
    class Settings
    {
        public static List<Clients> Online = new List<Clients>();
        public static List<string> Blocked = new List<string>();
        public static string Port { get; set; }
        public static long Sent { get; set; }
        public static long Received { get; set; }
        public static string Password { get; set; }
        public static Aes256 AES{ get; set; }

        public static readonly string Version = "AsyncRAT 0.4.7";
    }
}
