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
            switch (unpack_msgpack.ForcePathObject("Command").GetAsString())
            {
                case "started":
                    {
                        FormChat chat = (FormChat)Application.OpenForms["chat:" + unpack_msgpack.ForcePathObject("ID").AsString];
                        if (chat != null)
                        {
                            chat.Client = client;
                            chat.timer1.Start();
                            chat.textBox1.Enabled = true;
                            chat.richTextBox1.Enabled = true;
                        }
                        break;
                    }

                case "chat":
                    {
                        FormChat chat = (FormChat)Application.OpenForms["chat:" + unpack_msgpack.ForcePathObject("ID").AsString];
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
                        break;
                    }
            }
        }
    }
}
