using System;
using System.Security.Cryptography;
using System.Text;

namespace Client.Helper
{
   static class Methods
    {
        public static string HWID()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Environment.UserDomainName);
            sb.Append(Environment.UserName);
            sb.Append(Environment.MachineName);
            sb.Append(Environment.Version);
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
            return strResult.ToString().Substring(0, 12).ToUpper();
        }
    }
}
