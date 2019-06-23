using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.MessagePack
{
    class WriteTools
    {
        public static void WriteNull(Stream ms)
        {
            ms.WriteByte(0xC0);
        }

        public static void WriteString(Stream ms, String strVal)
        {
            //
            //fixstr stores a byte array whose length is upto 31 bytes:
            //+--------+========+
            //|101XXXXX|  data  |
            //+--------+========+
            //
            //str 8 stores a byte array whose length is upto (2^8)-1 bytes:
            //+--------+--------+========+
            //|  0xd9  |YYYYYYYY|  data  |
            //+--------+--------+========+
            //
            //str 16 stores a byte array whose length is upto (2^16)-1 bytes:
            //+--------+--------+--------+========+
            //|  0xda  |ZZZZZZZZ|ZZZZZZZZ|  data  |
            //+--------+--------+--------+========+
            //
            //str 32 stores a byte array whose length is upto (2^32)-1 bytes:
            //+--------+--------+--------+--------+--------+========+
            //|  0xdb  |AAAAAAAA|AAAAAAAA|AAAAAAAA|AAAAAAAA|  data  |
            //+--------+--------+--------+--------+--------+========+
            //
            //where
            //* XXXXX is a 5-bit unsigned integer which represents N
            //* YYYYYYYY is a 8-bit unsigned integer which represents N
            //* ZZZZZZZZ_ZZZZZZZZ is a 16-bit big-endian unsigned integer which represents N
            //* AAAAAAAA_AAAAAAAA_AAAAAAAA_AAAAAAAA is a 32-bit big-endian unsigned integer which represents N
            //* N is the length of data

            byte[] rawBytes = BytesTools.GetUtf8Bytes(strVal);
            byte[] lenBytes = null;
            int len = rawBytes.Length;
            byte b = 0;
            if (len <= 31)
            {
                b = (byte)(0xA0 + (byte)len);
                ms.WriteByte(b);
            }
            else if (len <= 255)
            {
                b = 0xD9;
                ms.WriteByte(b);
                b = (byte)len;
                ms.WriteByte(b);
            }
            else if (len <= 65535)
            {
                b = 0xDA;
                ms.WriteByte(b);

                lenBytes = BytesTools.SwapBytes(BitConverter.GetBytes((Int16)len));
                ms.Write(lenBytes, 0, lenBytes.Length);
            }
            else
            {
                b = 0xDB;
                ms.WriteByte(b);

                lenBytes = BytesTools.SwapBytes(BitConverter.GetBytes((Int32)len));
                ms.Write(lenBytes, 0, lenBytes.Length);
            }
            ms.Write(rawBytes, 0, rawBytes.Length);
        }
        public static void WriteBinary(Stream ms, byte[] rawBytes)
        {

            byte[] lenBytes = null;
            int len = rawBytes.Length;
            byte b = 0;
            if (len <= 255)
            {
                b = 0xC4;
                ms.WriteByte(b);
                b = (byte)len;
                ms.WriteByte(b);
            }
            else if (len <= 65535)
            {
                b = 0xC5;
                ms.WriteByte(b);

                lenBytes = BytesTools.SwapBytes(BitConverter.GetBytes((Int16)len));
                ms.Write(lenBytes, 0, lenBytes.Length);
            }
            else
            {
                b = 0xC6;
                ms.WriteByte(b);

                lenBytes = BytesTools.SwapBytes(BitConverter.GetBytes((Int32)len));
                ms.Write(lenBytes, 0, lenBytes.Length);
            }
            ms.Write(rawBytes, 0, rawBytes.Length);
        }

        public static void WriteFloat(Stream ms, Double fVal)
        {
            ms.WriteByte(0xCB);
            ms.Write(BytesTools.SwapDouble(fVal), 0, 8);
        }

        public static void WriteSingle(Stream ms, Single fVal)
        {
            ms.WriteByte(0xCA);
            ms.Write(BytesTools.SwapBytes(BitConverter.GetBytes(fVal)), 0, 4);
        }

        public static void WriteBoolean(Stream ms, Boolean bVal)
        {
            if (bVal)
            {
                ms.WriteByte(0xC3);
            }
            else
            {
                ms.WriteByte(0xC2);
            }
        }


        public static void WriteUInt64(Stream ms, UInt64 iVal)
        {
            ms.WriteByte(0xCF);
            byte[] dataBytes = BitConverter.GetBytes(iVal);
            ms.Write(BytesTools.SwapBytes(dataBytes), 0, 8);
        }

        public static void WriteInteger(Stream ms, Int64 iVal)
        {
            if (iVal >= 0)
            {   // 正数
                if (iVal <= 127)
                {
                    ms.WriteByte((byte)iVal);
                }
                else if (iVal <= 255)
                {  //UInt8
                    ms.WriteByte(0xCC);
                    ms.WriteByte((byte)iVal);
                }
                else if (iVal <= (UInt32)0xFFFF)
                {  //UInt16
                    ms.WriteByte(0xCD);
                    ms.Write(BytesTools.SwapInt16((Int16)iVal), 0, 2);
                }
                else if (iVal <= (UInt32)0xFFFFFFFF)
                {  //UInt32
                    ms.WriteByte(0xCE);
                    ms.Write(BytesTools.SwapInt32((Int32)iVal), 0, 4);
                }
                else
                {  //Int64
                    ms.WriteByte(0xD3);
                    ms.Write(BytesTools.SwapInt64(iVal), 0, 8);
                }
            }
            else
            {  // <0
                if (iVal <= Int32.MinValue)  //-2147483648  // 64 bit
                {
                    ms.WriteByte(0xD3);
                    ms.Write(BytesTools.SwapInt64(iVal), 0, 8);
                }
                else if (iVal <= Int16.MinValue)   // -32768    // 32 bit
                {
                    ms.WriteByte(0xD2);
                    ms.Write(BytesTools.SwapInt32((Int32)iVal), 0, 4);
                }
                else if (iVal <= -128)   // -32768    // 32 bit
                {
                    ms.WriteByte(0xD1);
                    ms.Write(BytesTools.SwapInt16((Int16)iVal), 0, 2);
                }
                else if (iVal <= -32)
                {
                    ms.WriteByte(0xD0);
                    ms.WriteByte((byte)iVal);
                }
                else
                {
                    ms.WriteByte((byte)iVal);
                }
            }  // end <0
        }
    }
}