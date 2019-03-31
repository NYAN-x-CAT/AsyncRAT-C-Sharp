using AsyncRAT_Sharp.Cryptography;
using AsyncRAT_Sharp.Sockets;
using System.Collections.Generic;

namespace AsyncRAT_Sharp
{
    class Settings
    {
        public static List<Clients> Online = new List<Clients>();
        public static List<string> Blocked = new List<string>();
        public static string Port = "6606,7707,8808";
        public static readonly string Version = "AsyncRAT 0.3.0";
        public static long Sent = 0;
        public static long Received = 0;
        public static string Password = "NYAN CAT";
        public static Aes256 aes256;
    }
}
