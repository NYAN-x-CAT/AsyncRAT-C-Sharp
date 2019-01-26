using AsyncRAT_Sharp.Sockets;
using System.Collections.Generic;

namespace AsyncRAT_Sharp
{
    class Settings
    {
        public static List<Clients> Online = new List<Clients>();
        public static int Port = 8080;
        public static string Version = "0.2";
    }
}
