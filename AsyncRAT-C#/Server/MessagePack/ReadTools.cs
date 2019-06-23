using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.MessagePack
{
    class ReadTools
    {
        public static String ReadString(Stream ms, int len)
        {
            byte[] rawBytes = new byte[len];
            ms.Read(rawBytes, 0, len);
            return BytesTools.GetString(rawBytes);
        }

        public static String ReadString(Stream ms)
        {
            byte strFlag = (byte)ms.ReadByte();
            return ReadString(strFlag, ms);
        }

        public static String ReadString(byte strFlag, Stream ms)
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

            byte[] rawBytes = null;
            int len = 0;
            if ((strFlag >= 0xA0) && (strFlag <= 0xBF))
            {
                len = strFlag - 0xA0;
            }
            else if (strFlag == 0xD9)
            {
                len = ms.ReadByte();
            }
            else if (strFlag == 0xDA)
            {
                rawBytes = new byte[2];
                ms.Read(rawBytes, 0, 2);
                rawBytes = BytesTools.SwapBytes(rawBytes);
                len = BitConverter.ToUInt16(rawBytes, 0);
            }
            else if (strFlag == 0xDB)
            {
                rawBytes = new byte[4];
                ms.Read(rawBytes, 0, 4);
                rawBytes = BytesTools.SwapBytes(rawBytes);
                len = BitConverter.ToInt32(rawBytes, 0);
            }
            rawBytes = new byte[len];
            ms.Read(rawBytes, 0, len);
            return BytesTools.GetString(rawBytes);
        }
    }
}