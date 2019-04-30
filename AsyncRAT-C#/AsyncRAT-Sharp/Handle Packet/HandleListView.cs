using AsyncRAT_Sharp.MessagePack;
using AsyncRAT_Sharp.Sockets;
using cGeoIp;
using System.Drawing;
using System.Windows.Forms;

namespace AsyncRAT_Sharp.Handle_Packet
{
    public class HandleListView
    {
        public void AddToListview(Clients client, MsgPack unpack_msgpack)
        {
            if (Program.form1.listView1.InvokeRequired)
            {
                Program.form1.listView1.BeginInvoke((MethodInvoker)(() =>
                {
                    client.LV = new ListViewItem();
                    client.LV.Tag = client;
                    client.LV.Text = string.Format("{0}:{1}", client.ClientSocket.RemoteEndPoint.ToString().Split(':')[0], client.ClientSocket.LocalEndPoint.ToString().Split(':')[1]);
                    string[] ipinf = new cGeoMain().GetIpInf(client.ClientSocket.RemoteEndPoint.ToString().Split(':')[0]).Split(':');
                    client.LV.SubItems.Add(ipinf[1]);
                    client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("HWID").AsString);
                    client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("User").AsString);
                    client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("OS").AsString);
                    client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("Version").AsString);
                    client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("Performance").AsString);
                    client.LV.ToolTipText = unpack_msgpack.ForcePathObject("Path").AsString;
                    client.ID = unpack_msgpack.ForcePathObject("HWID").AsString;
                    Program.form1.listView1.Items.Insert(0, client.LV);
                    Program.form1.listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                }));
                lock (Settings.Online)
                {
                    Settings.Online.Add(client);
                }
               new HandleLogs().Addmsg($"Client {client.ClientSocket.RemoteEndPoint.ToString().Split(':')[0]} connected successfully", Color.Green);
            }
        }

        public void Received(Clients client)
        {
            if (Program.form1.listView1.InvokeRequired)
            {
                Program.form1.listView1.BeginInvoke((MethodInvoker)(() =>
                {
                    client.LV.ForeColor = Color.Empty;
                }));
            }
        }
    }
}
