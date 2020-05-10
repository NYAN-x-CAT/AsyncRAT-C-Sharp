using Server.Algorithm;
using Server.Connection;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace Server
{
    public static class Settings
    {
        public static List<string> Blocked = new List<string>();
        public static object LockBlocked = new object();

        public static long SentValue { get; set; }
        public static long ReceivedValue { get; set; }
        public static object LockReceivedSendValue = new object();


        public static string CertificatePath = Application.StartupPath + "\\ServerCertificate.p12";
        public static X509Certificate2 ServerCertificate;
        public static readonly string Version = "AsyncRAT 0.5.7B";
        public static object LockListviewClients = new object();
        public static object LockListviewLogs = new object();
        public static object LockListviewThumb = new object();
        public static bool ReportWindow = false;
        public static List<Clients> ReportWindowClients = new List<Clients>();
        public static object LockReportWindowClients = new object();
    }

    public static class XmrSettings
    {
        public static string Pool = "";
        public static string Wallet = "";
        public static string Pass = "";
        public static string InjectTo = "";
        public static string Hash = "";
    }
}
