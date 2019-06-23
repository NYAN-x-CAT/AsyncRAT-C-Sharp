using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.MessagePack
{
    public class BytesTools
    {
        static UTF8Encoding utf8Encode = new UTF8Encoding();

        public static byte[] GetUtf8Bytes(String s)
        {

            return utf8Encode.GetBytes(s);
        }

        public static String GetString(byte[] utf8Bytes)
        {
            return utf8Encode.GetString(utf8Bytes);
        }

        public static String BytesAsString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(String.Format("{0:D3} ", b));
            }
            return sb.ToString();
        }


        public static String BytesAsHexString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(String.Format("{0:X2} ", b));
            }
            return sb.ToString();
        }

        /// <summary>
        ///   交换byte数组数据
        ///   可用于高低数据交换
        /// </summary>
        /// <param name="v">要交换的byte数组</param>
        /// <returns>返回交换后的数据</returns>
        public static byte[] SwapBytes(byte[] v)
        {
            byte[] r = new byte[v.Length];
            int j = v.Length - 1;
            for (int i = 0; i < r.Length; i++)
            {
                r[i] = v[j];
                j--;
            }
            return r;
        }

        public static byte[] SwapInt64(Int64 v)
        {
            //byte[] r = new byte[8];
            //r[7] = (byte)v;
            //r[6] = (byte)(v >> 8);
            //r[5] = (byte)(v >> 16);
            //r[4] = (byte)(v >> 24);
            //r[3] = (byte)(v >> 32);
            //r[2] = (byte)(v >> 40);
            //r[1] = (byte)(v >> 48);
            //r[0] = (byte)(v >> 56);            
            return SwapBytes(BitConverter.GetBytes(v));
        }

        public static byte[] SwapInt32(Int32 v)
        {
            byte[] r = new byte[4];
            r[3] = (byte)v;
            r[2] = (byte)(v >> 8);
            r[1] = (byte)(v >> 16);
            r[0] = (byte)(v >> 24);
            return r;
        }


        public static byte[] SwapInt16(Int16 v)
        {
            byte[] r = new byte[2];
            r[1] = (byte)v;
            r[0] = (byte)(v >> 8);
            return r;
        }

        public static byte[] SwapDouble(Double v)
        {
            return SwapBytes(BitConverter.GetBytes(v));
        }

    }
}