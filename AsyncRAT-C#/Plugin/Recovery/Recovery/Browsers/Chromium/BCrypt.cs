using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Plugin.Browsers.Chromium
{
    public static class BCrypt
    {
        public const uint ERROR_SUCCESS = 0x00000000;
        public const uint BCRYPT_PAD_PSS = 8;
        public const uint BCRYPT_PAD_OAEP = 4;

        public static readonly byte[] BCRYPT_KEY_DATA_BLOB_MAGIC = BitConverter.GetBytes(0x4d42444b);

        public static readonly string BCRYPT_OBJECT_LENGTH = "ObjectLength";
        public static readonly string BCRYPT_CHAIN_MODE_GCM = "ChainingModeGCM";
        public static readonly string BCRYPT_AUTH_TAG_LENGTH = "AuthTagLength";
        public static readonly string BCRYPT_CHAINING_MODE = "ChainingMode";
        public static readonly string BCRYPT_KEY_DATA_BLOB = "KeyDataBlob";
        public static readonly string BCRYPT_AES_ALGORITHM = "AES";

        public static readonly string MS_PRIMITIVE_PROVIDER = "Microsoft Primitive Provider";

        public static readonly int BCRYPT_AUTH_MODE_CHAIN_CALLS_FLAG = 0x00000001;
        public static readonly int BCRYPT_INIT_AUTH_MODE_INFO_VERSION = 0x00000001;

        public static readonly uint STATUS_AUTH_TAG_MISMATCH = 0xC000A002;

        [StructLayout(LayoutKind.Sequential)]
        public struct BCRYPT_PSS_PADDING_INFO
        {
            public BCRYPT_PSS_PADDING_INFO(string pszAlgId, int cbSalt)
            {
                this.pszAlgId = pszAlgId;
                this.cbSalt = cbSalt;
            }

            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszAlgId;
            public int cbSalt;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BCRYPT_AUTHENTICATED_CIPHER_MODE_INFO : IDisposable
        {
            public int cbSize;
            public int dwInfoVersion;
            public IntPtr pbNonce;
            public int cbNonce;
            public IntPtr pbAuthData;
            public int cbAuthData;
            public IntPtr pbTag;
            public int cbTag;
            public IntPtr pbMacContext;
            public int cbMacContext;
            public int cbAAD;
            public long cbData;
            public int dwFlags;

            public BCRYPT_AUTHENTICATED_CIPHER_MODE_INFO(byte[] iv, byte[] aad, byte[] tag) : this()
            {
                dwInfoVersion = BCRYPT_INIT_AUTH_MODE_INFO_VERSION;
                cbSize = Marshal.SizeOf(typeof(BCRYPT_AUTHENTICATED_CIPHER_MODE_INFO));

                if (iv != null)
                {
                    cbNonce = iv.Length;
                    pbNonce = Marshal.AllocHGlobal(cbNonce);
                    Marshal.Copy(iv, 0, pbNonce, cbNonce);
                }

                if (aad != null)
                {
                    cbAuthData = aad.Length;
                    pbAuthData = Marshal.AllocHGlobal(cbAuthData);
                    Marshal.Copy(aad, 0, pbAuthData, cbAuthData);
                }

                if (tag != null)
                {
                    cbTag = tag.Length;
                    pbTag = Marshal.AllocHGlobal(cbTag);
                    Marshal.Copy(tag, 0, pbTag, cbTag);

                    cbMacContext = tag.Length;
                    pbMacContext = Marshal.AllocHGlobal(cbMacContext);
                }
            }

            public void Dispose()
            {
                if (pbNonce != IntPtr.Zero) Marshal.FreeHGlobal(pbNonce);
                if (pbTag != IntPtr.Zero) Marshal.FreeHGlobal(pbTag);
                if (pbAuthData != IntPtr.Zero) Marshal.FreeHGlobal(pbAuthData);
                if (pbMacContext != IntPtr.Zero) Marshal.FreeHGlobal(pbMacContext);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BCRYPT_KEY_LENGTHS_STRUCT
        {
            public int dwMinLength;
            public int dwMaxLength;
            public int dwIncrement;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BCRYPT_OAEP_PADDING_INFO
        {
            public BCRYPT_OAEP_PADDING_INFO(string alg)
            {
                pszAlgId = alg;
                pbLabel = IntPtr.Zero;
                cbLabel = 0;
            }

            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszAlgId;
            public IntPtr pbLabel;
            public int cbLabel;
        }

        [DllImport("bcrypt.dll")]
        public static extern uint BCryptOpenAlgorithmProvider(out IntPtr phAlgorithm,
                                                              [MarshalAs(UnmanagedType.LPWStr)] string pszAlgId,
                                                              [MarshalAs(UnmanagedType.LPWStr)] string pszImplementation,
                                                              uint dwFlags);

        [DllImport("bcrypt.dll")]
        public static extern uint BCryptCloseAlgorithmProvider(IntPtr hAlgorithm, uint flags);

        [DllImport("bcrypt.dll", EntryPoint = "BCryptGetProperty")]
        public static extern uint BCryptGetProperty(IntPtr hObject, [MarshalAs(UnmanagedType.LPWStr)] string pszProperty, byte[] pbOutput, int cbOutput, ref int pcbResult, uint flags);

        [DllImport("bcrypt.dll", EntryPoint = "BCryptSetProperty")]
        internal static extern uint BCryptSetAlgorithmProperty(IntPtr hObject, [MarshalAs(UnmanagedType.LPWStr)] string pszProperty, byte[] pbInput, int cbInput, int dwFlags);


        [DllImport("bcrypt.dll")]
        public static extern uint BCryptImportKey(IntPtr hAlgorithm,
                                                         IntPtr hImportKey,
                                                         [MarshalAs(UnmanagedType.LPWStr)] string pszBlobType,
                                                         out IntPtr phKey,
                                                         IntPtr pbKeyObject,
                                                         int cbKeyObject,
                                                         byte[] pbInput, //blob of type BCRYPT_KEY_DATA_BLOB + raw key data = (dwMagic (4 bytes) | uint dwVersion (4 bytes) | cbKeyData (4 bytes) | data)
                                                         int cbInput,
                                                         uint dwFlags);

        [DllImport("bcrypt.dll")]
        public static extern uint BCryptDestroyKey(IntPtr hKey);

        [DllImport("bcrypt.dll")]
        public static extern uint BCryptEncrypt(IntPtr hKey,
                                                byte[] pbInput,
                                                int cbInput,
                                                ref BCRYPT_AUTHENTICATED_CIPHER_MODE_INFO pPaddingInfo,
                                                byte[] pbIV, int cbIV,
                                                byte[] pbOutput,
                                                int cbOutput,
                                                ref int pcbResult,
                                                uint dwFlags);

        [DllImport("bcrypt.dll")]
        internal static extern uint BCryptDecrypt(IntPtr hKey,
                                                  byte[] pbInput,
                                                  int cbInput,
                                                  ref BCRYPT_AUTHENTICATED_CIPHER_MODE_INFO pPaddingInfo,
                                                  byte[] pbIV,
                                                  int cbIV,
                                                  byte[] pbOutput,
                                                  int cbOutput,
                                                  ref int pcbResult,
                                                  int dwFlags);
    }

}
