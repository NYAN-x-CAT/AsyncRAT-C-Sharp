using AsyncRAT_Sharp.MessagePack;
using AsyncRAT_Sharp.Sockets;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AsyncRAT_Sharp.Handle_Packet
{
   public class HandleThumbnails
    {
        public HandleThumbnails(Clients client, MsgPack unpack_msgpack)
        {
            if (Program.form1.listView3.InvokeRequired)
            {
                Program.form1.listView3.BeginInvoke((MethodInvoker)(() =>
                {
                    if (client.LV2 == null)
                    {
                        client.LV2 = new ListViewItem();
                        client.LV2.Text = string.Format("{0}:{1}", client.ClientSocket.RemoteEndPoint.ToString().Split(':')[0], client.ClientSocket.LocalEndPoint.ToString().Split(':')[1]);
                        client.LV2.ToolTipText = client.ID;
                        using (MemoryStream memoryStream = new MemoryStream(unpack_msgpack.ForcePathObject("Image").GetAsBytes()))
                        {
                            Program.form1.imageList1.Images.Add(client.ID, Bitmap.FromStream(memoryStream));
                            client.LV2.ImageKey = client.ID;
                            Program.form1.listView3.BeginUpdate();
                            Program.form1.listView3.Items.Insert(0, client.LV2);
                            Program.form1.listView3.EndUpdate();
                        }
                    }
                    else
                    {
                        using (MemoryStream memoryStream = new MemoryStream(unpack_msgpack.ForcePathObject("Image").GetAsBytes()))
                        {
                            Program.form1.listView3.BeginUpdate();
                            Program.form1.imageList1.Images.RemoveByKey(client.ID);
                            Program.form1.imageList1.Images.Add(client.ID, Bitmap.FromStream(memoryStream));
                            Program.form1.listView3.EndUpdate();
                        }
                    }
                }));
            }
        }
    }
}
