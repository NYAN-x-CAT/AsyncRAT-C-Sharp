using AsyncRAT_Sharp.Sockets;
using System.Windows.Forms;
using AsyncRAT_Sharp.MessagePack;
using System;
using System.Diagnostics;
using System.Drawing;

namespace AsyncRAT_Sharp.Handle_Packet
{
    class HandlePacket
    {
        public static void Read(Clients Client, byte[] Data)
        {
            try
            {
                    MsgPack unpack_msgpack = new MsgPack();
                    unpack_msgpack.DecodeFromBytes(Data);
                    switch (unpack_msgpack.ForcePathObject("Packet").AsString)
                    {
                        case "ClientInfo":
                        if (Program.form1.listView1.InvokeRequired)
                        {
                            Program.form1.listView1.BeginInvoke((MethodInvoker)(() =>
                            {
                                Client.LV = new ListViewItem();
                                Client.LV.Tag = Client;
                                Client.LV.Text = string.Format("{0}:{1}",Client.Client.RemoteEndPoint.ToString().Split(':')[0], Client.Client.LocalEndPoint.ToString().Split(':')[1]);
                                Client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("HWID").AsString);
                                Client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("User").AsString);
                                Client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("OS").AsString);
                                Program.form1.listView1.Items.Insert(0, Client.LV);
                                Settings.Online.Add(Client);
                            }));
                        }
                            break;

                        case "Ping":
                            {
                                Debug.WriteLine(unpack_msgpack.ForcePathObject("Message").AsString);
                            }
                            break;

                    case "Received":
                        {
                            if (Program.form1.listView1.InvokeRequired)
                            {
                                Program.form1.listView1.BeginInvoke((MethodInvoker)(() =>
                                {
                                    Client.LV.ForeColor = Color.Empty;
                                }));
                            }
                        }
                        break;
                }              
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}