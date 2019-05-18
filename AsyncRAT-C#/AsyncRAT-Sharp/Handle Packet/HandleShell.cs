using AsyncRAT_Sharp.Forms;
using AsyncRAT_Sharp.MessagePack;
using AsyncRAT_Sharp.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AsyncRAT_Sharp.Handle_Packet
{
    public class HandleShell
    {
        public HandleShell(MsgPack unpack_msgpack, Clients client)
        {
            if (Program.form1.InvokeRequired)
            {
                Program.form1.BeginInvoke((MethodInvoker)(() =>
                {
                    FormShell shell = (FormShell)Application.OpenForms["shell:" + client.ID];
                    if (shell != null)
                    {
                        shell.richTextBox1.AppendText(unpack_msgpack.ForcePathObject("ReadInput").AsString);
                        shell.richTextBox1.SelectionStart = shell.richTextBox1.TextLength;
                        shell.richTextBox1.ScrollToCaret();
                    }
                }));
            }
        }
    }
}