using Server.MessagePack;
using Server.Sockets;
using System.Diagnostics;
using System.Windows.Forms;

namespace Server.Handle_Packet
{
    public class HandlePing
    {
        public HandlePing(Clients client, MsgPack unpack_msgpack)
        {
            if (Program.form1.listView1.InvokeRequired)
            {
                Program.form1.listView1.BeginInvoke((MethodInvoker)(() =>
                {
                    try
                    {

                        lock (Settings.Listview1Lock)
                            if (client.LV != null)
                                client.LV.SubItems[Program.form1.lv_prefor.Index].Text = unpack_msgpack.ForcePathObject("Message").AsString;
                            else
                                Debug.WriteLine("Temp socket pinged server");
                    }
                    catch { }
                }));
            }
        }
    }
}
