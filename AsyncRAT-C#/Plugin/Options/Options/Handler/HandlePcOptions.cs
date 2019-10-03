using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Plugin.Handler
{
    public class HandlePcOptions
    {
        public HandlePcOptions(string option)
        {
            switch (option)
            {
                case "restart":
                    {
                        Methods.ClientExit();
                        Process proc = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "cmd",
                                Arguments = "/c Shutdown /r /f /t 00",
                                WindowStyle = ProcessWindowStyle.Hidden,
                                CreateNoWindow = true,
                            }
                        };
                        proc.Start();
                        break;
                    }

                case "shutdown":
                    {
                        Methods.ClientExit();
                        Process proc = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "cmd",
                                Arguments = "/c Shutdown /s /f /t 00",
                                WindowStyle = ProcessWindowStyle.Hidden,
                                CreateNoWindow = true,
                            }
                        };
                        proc.Start();
                        break;
                    }

                case "logoff":
                    {
                        Methods.ClientExit();
                        Process proc = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "cmd",
                                Arguments = "/c Shutdown /l /f",
                                WindowStyle = ProcessWindowStyle.Hidden,
                                CreateNoWindow = true,
                            }
                        };
                        proc.Start();
                        break;
                    }
            }
        }
    }

}
