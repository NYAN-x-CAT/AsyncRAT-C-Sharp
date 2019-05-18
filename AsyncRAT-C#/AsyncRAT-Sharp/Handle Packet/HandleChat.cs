using AsyncRAT_Sharp.Forms;
using AsyncRAT_Sharp.MessagePack;
using AsyncRAT_Sharp.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AsyncRAT_Sharp.Handle_Packet
{
   public class HandleChat
    {
        public HandleChat(MsgPack unpack_msgpack, Clients client)
        {
            if (Program.form1.InvokeRequired)
            {
                Program.form1.BeginInvoke((MethodInvoker)(() =>
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
                }));
            }
        }
    }
}
