using AsyncRAT_Sharp.Sockets;
using System.Windows.Forms;
using AsyncRAT_Sharp.MessagePack;

namespace AsyncRAT_Sharp.Handle_Packet
{
    class HandlePacket
    {
        public static Form1 Form;
        public delegate void UpdateListViewDelegatevoid(Clients Client, byte[] Data);
        public static void Read(Clients Client, byte[] Data)
        {
            MsgPack unpack_msgpack = new MsgPack();
            unpack_msgpack.DecodeFromBytes(Data);
            switch (unpack_msgpack.ForcePathObject("Packet").AsString)
            {
                case "ClientInfo":
                    if (Form.listView1.InvokeRequired)
                        Form.listView1.Invoke(new UpdateListViewDelegatevoid(Read), new object[] {Client, Data});
                    else
                    {
                        Client.LV = new ListViewItem();
                        Client.LV.Tag = Client;
                        Client.LV.Text = string.Concat(Client.client.RemoteEndPoint.ToString());
                        Client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("User").AsString);
                        Client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("OS").AsString);
                        Form.listView1.Items.Insert(0, Client.LV);
                        Form.listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                    }
                    break;
            }
        }
    }
}
