using AsyncRAT_Sharp.Sockets;
using System.Collections.Generic;

namespace AsyncRAT_Sharp
{
    class Settings
    {
        public static List<Clients> Online = new List<Clients>();
        public static int Port = 6606;
        public static readonly string Version = "AsyncRAT 0.2.6";
        public static long Sent = 0;
        public static long Received = 0;
    }
}
