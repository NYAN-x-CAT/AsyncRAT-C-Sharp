using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Client.Helper
{
   public static class HwidGen
    {
        public static string HWID()
        {
            try
            {
                return GetHash(string.Concat(Environment.ProcessorCount, Environment.UserName,
                    Environment.MachineName, Environment.OSVersion
                    , new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory)).TotalSize));
            }
            catch
            {
                return "Err HWID";
            }
        }

        public static string GetHash(string strToHash)
        {
            MD5CryptoServiceProvider md5Obj = new MD5CryptoServiceProvider();
            byte[] bytesToHash = Encoding.ASCII.GetBytes(strToHash);
            bytesToHash = md5Obj.ComputeHash(bytesToHash);
            StringBuilder strResult = new StringBuilder();
            foreach (byte b in bytesToHash)
                strResult.Append(b.ToString("x2"));
            return strResult.ToString().Substring(0, 20).ToUpper();
        }
    }
}
