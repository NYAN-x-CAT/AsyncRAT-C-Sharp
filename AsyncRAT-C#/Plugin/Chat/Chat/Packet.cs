using MessagePackLib.MessagePack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Plugin
{
    public static class Packet
    {
        public static FormChat GetFormChat;

        public static void Read(object data)
        {
            try
            {
                MsgPack unpack_msgpack = new MsgPack();
                unpack_msgpack.DecodeFromBytes((byte[])data);
                switch (unpack_msgpack.ForcePathObject("Packet").AsString)
                {
                    case "chat":
                        {
                            new HandlerChat().CreateChat();
                            break;
                        }

                    case "chatWriteInput":
                        {
                            new HandlerChat().WriteInput(unpack_msgpack);
                            break;
                        }

                    case "chatExit":
                        {
                            new HandlerChat().ExitChat();
                            break;
                        }
                }
            }
            catch { }
        }
    }

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
                    Packet.GetFormChat?.Close();
                    Packet.GetFormChat?.Dispose();
                }));
            }
            Connection.Disconnected();
            GC.Collect();
        }
    }
}
