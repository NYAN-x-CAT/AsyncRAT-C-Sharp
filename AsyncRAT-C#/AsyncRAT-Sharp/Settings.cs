using AsyncRAT_Sharp.Sockets;
using System.Collections.Generic;

namespace AsyncRAT_Sharp
{
    class Settings
    {
        public static readonly List<Clients> Online = new List<Clients>();
        public static readonly int Port = 8080;
        public static readonly string Version = "0.2.1";
        public static long Sent = 0;
        public static long Received = 0;
    }
}
