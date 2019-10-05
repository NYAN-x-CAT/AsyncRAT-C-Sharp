/*
 * 添加DecodeFormFile函数
 *   2015-07-14 16:31:32
 *   
 * 修复ForcePathObject查找不到子对象的bug,感谢(Putree  274638001<spiritring@gmail.com>)反馈
 *   2015-07-14 16:32:13 
 *   
 * 修复整数值为127时解码出来为0的情况,感谢(Putree  274638001<spiritring@gmail.com>)反馈
 *   2015-07-14 15:28:45
 *   
 *   Credit -> github.com/ymofen/SimpleMsgPack.Net
 */
using Server.Algorithm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Server.MessagePack
{
    public class MsgPackEnum : IEnumerator
    {
        List<MsgPack> children;
        int position = -1;

        public MsgPackEnum(List<MsgPack> obj)
        {
            children = obj;
        }
        object IEnumerator.Current
        {
            get { return children[position]; }
        }

        bool IEnumerator.MoveNext()
        {
            position++;
            return (position < children.Count);
        }

        void IEnumerator.Reset()
        {
            position = -1;
        }

    }

    public class MsgPackArray
    {
        List<MsgPack> children;
        MsgPack owner;

        public MsgPackArray(MsgPack msgpackObj, List<MsgPack> listObj)
        {
            owner = msgpackObj;
            children = listObj;
        }

        public MsgPack Add()
        {
            return owner.AddArrayChild();
        }

        public MsgPack Add(String value)
        {
            MsgPack obj = owner.AddArrayChild();
            obj.AsString = value;
            return obj;
        }

        public MsgPack Add(Int64 value)
        {
            MsgPack obj = owner.AddArrayChild();
            obj.SetAsInteger(value);
            return obj;
        }

        public MsgPack Add(Double value)
        {
            MsgPack obj = owner.AddArrayChild();
            obj.SetAsFloat(value);
            return obj;
        }

        public MsgPack this[int index]
        {
            get { return children[index]; }
        }

        public int Length
        {
            get { return children.Count; }
        }
    }

    public class MsgPack : IEnumerable
    {
        string name;
        string lowerName;
        object innerValue;
        MsgPackType valueType;
        MsgPack parent;
        List<MsgPack> children = new List<MsgPack>();
        MsgPackArray refAsArray = null;

        private void SetName(string value)
        {
            this.name = value;
            this.lowerName = name.ToLower();
        }

        private void Clear()
        {
            for (int i = 0; i < children.Count; i++)
            {
                ((MsgPack)children[i]).Clear();
            }
            children.Clear();
        }

        private MsgPack InnerAdd()
        {
            MsgPack r = new MsgPack();
            r.parent = this;
            this.children.Add(r);
            return r;
        }

        private int IndexOf(string name)
        {
            int i = -1;
            int r = -1;

            string tmp = name.ToLower();
            foreach (MsgPack item in children)
            {
                i++;
                if (tmp.Equals(item.lowerName))
                {
                    r = i;
                    break;
                }
            }
            return r;
        }

        public MsgPack FindObject(string name)
        {
            int i = IndexOf(name);
            if (i == -1)
            {
                return null;
            }
            else
            {
                return this.children[i];
            }
        }


        private MsgPack InnerAddMapChild()
        {
            if (valueType != MsgPackType.Map)
            {
                Clear();
                this.valueType = MsgPackType.Map;
            }
            return InnerAdd();
        }

        private MsgPack InnerAddArrayChild()
        {
            if (valueType != MsgPackType.Array)
            {
                Clear();
                this.valueType = MsgPackType.Array;
            }
            return InnerAdd();
        }

        public MsgPack AddArrayChild()
        {
            return InnerAddArrayChild();
        }

        private void WriteMap(Stream ms)
        {
            byte b;
            byte[] lenBytes;
            int len = children.Count;
            if (len <= 15)
            {
                b = (byte)(0x80 + (byte)len);
                ms.WriteByte(b);
            }
            else if (len <= 65535)
            {
                b = 0xDE;
                ms.WriteByte(b);

                lenBytes = BytesTools.SwapBytes(BitConverter.GetBytes((Int16)len));
                ms.Write(lenBytes, 0, lenBytes.Length);
            }
            else
            {
                b = 0xDF;
                ms.WriteByte(b);
                lenBytes = BytesTools.SwapBytes(BitConverter.GetBytes((Int32)len));
                ms.Write(lenBytes, 0, lenBytes.Length);
            }

            for (int i = 0; i < len; i++)
            {
                WriteTools.WriteString(ms, children[i].name);
                children[i].Encode2Stream(ms);
            }
        }

        private void WirteArray(Stream ms)
        {
            byte b;
            byte[] lenBytes;
            int len = children.Count;
            if (len <= 15)
            {
                b = (byte)(0x90 + (byte)len);
                ms.WriteByte(b);
            }
            else if (len <= 65535)
            {
                b = 0xDC;
                ms.WriteByte(b);

                lenBytes = BytesTools.SwapBytes(BitConverter.GetBytes((Int16)len));
                ms.Write(lenBytes, 0, lenBytes.Length);
            }
            else
            {
                b = 0xDD;
                ms.WriteByte(b);
                lenBytes = BytesTools.SwapBytes(BitConverter.GetBytes((Int32)len));
                ms.Write(lenBytes, 0, lenBytes.Length);
            }


            for (int i = 0; i < len; i++)
            {
                ((MsgPack)children[i]).Encode2Stream(ms);
            }
        }

        public void SetAsInteger(Int64 value)
        {
            this.innerValue = value;
            this.valueType = MsgPackType.Integer;
        }

        public void SetAsUInt64(UInt64 value)
        {
            this.innerValue = value;
            this.valueType = MsgPackType.UInt64;
        }

        public UInt64 GetAsUInt64()
        {
            switch (this.valueType)
            {
                case MsgPackType.Integer:
                    return Convert.ToUInt64((Int64)this.innerValue);
                case MsgPackType.UInt64:
                    return (UInt64)this.innerValue;
                case MsgPackType.String:
                    return UInt64.Parse(this.innerValue.ToString().Trim());
                case MsgPackType.Float:
                    return Convert.ToUInt64((Double)this.innerValue);
                case MsgPackType.Single:
                    return Convert.ToUInt64((Single)this.innerValue);
                case MsgPackType.DateTime:
                    return Convert.ToUInt64((DateTime)this.innerValue);
                default:
                    return 0;
            }

        }

        public Int64 GetAsInteger()
        {
            switch (this.valueType)
            {
                case MsgPackType.Integer:
                    return (Int64)this.innerValue;
                case MsgPackType.UInt64:
                    return Convert.ToInt64((Int64)this.innerValue);
                case MsgPackType.String:
                    return Int64.Parse(this.innerValue.ToString().Trim());
                case MsgPackType.Float:
                    return Convert.ToInt64((Double)this.innerValue);
                case MsgPackType.Single:
                    return Convert.ToInt64((Single)this.innerValue);
                case MsgPackType.DateTime:
                    return Convert.ToInt64((DateTime)this.innerValue);
                default:
                    return 0;
            }
        }

        public Double GetAsFloat()
        {
            switch (this.valueType)
            {
                case MsgPackType.Integer:
                    return Convert.ToDouble((Int64)this.innerValue);
                case MsgPackType.String:
                    return Double.Parse((String)this.innerValue);
                case MsgPackType.Float:
                    return (Double)this.innerValue;
                case MsgPackType.Single:
                    return (Single)this.innerValue;
                case MsgPackType.DateTime:
                    return Convert.ToInt64((DateTime)this.innerValue);
                default:
                    return 0;
            }
        }


        public void SetAsBytes(byte[] value)
        {
            this.innerValue = value;
            this.valueType = MsgPackType.Binary;
        }

        public byte[] GetAsBytes()
        {
            switch (this.valueType)
            {
                case MsgPackType.Integer:
                    return BitConverter.GetBytes((Int64)this.innerValue);
                case MsgPackType.String:
                    return BytesTools.GetUtf8Bytes(this.innerValue.ToString());
                case MsgPackType.Float:
                    return BitConverter.GetBytes((Double)this.innerValue);
                case MsgPackType.Single:
                    return BitConverter.GetBytes((Single)this.innerValue);
                case MsgPackType.DateTime:
                    long dateval = ((DateTime)this.innerValue).ToBinary();
                    return BitConverter.GetBytes(dateval);
                case MsgPackType.Binary:
                    return (byte[])this.innerValue;
                default:
                    return new byte[] { };
            }
        }

        public void Add(string key, String value)
        {
            MsgPack tmp = InnerAddArrayChild();
            tmp.name = key;
            tmp.SetAsString(value);
        }

        public void Add(string key, int value)
        {
            MsgPack tmp = InnerAddArrayChild();
            tmp.name = key;
            tmp.SetAsInteger(value);
        }

        public async Task<bool> LoadFileAsBytes(string fileName)
        {
            if (File.Exists(fileName))
            {
                byte[] value = null;
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                value = new byte[fs.Length];
                await fs.ReadAsync(value, 0, (int)fs.Length);
                await fs.FlushAsync();
                fs.Close();
                fs.Dispose();
                SetAsBytes(value);
                return true;
            }
            else
            {
                return false;
            }

        }

        public async Task<bool> SaveBytesToFile(string fileName)
        {
            if (this.innerValue != null)
            {
                FileStream fs = new FileStream(fileName, FileMode.Append);
                await fs.WriteAsync(((byte[])this.innerValue), 0, ((byte[])this.innerValue).Length);
                await fs.FlushAsync();
                fs.Close();
                fs.Dispose();
                return true;
            }
            else
            {
                return false;
            }
        }

        public MsgPack ForcePathObject(string path)
        {
            MsgPack tmpParent, tmpObject;
            tmpParent = this;
            string[] pathList = path.Trim().Split(new Char[] { '.', '/', '\\' });
            string tmp = null;
            if (pathList.Length == 0)
            {
                return null;
            }
            else if (pathList.Length > 1)
            {
                for (int i = 0; i < pathList.Length - 1; i++)
                {
                    tmp = pathList[i];
                    tmpObject = tmpParent.FindObject(tmp);
                    if (tmpObject == null)
                    {
                        tmpParent = tmpParent.InnerAddMapChild();
                        tmpParent.SetName(tmp);
                    }
                    else
                    {
                        tmpParent = tmpObject;
                    }
                }
            }
            tmp = pathList[pathList.Length - 1];
            int j = tmpParent.IndexOf(tmp);
            if (j > -1)
            {
                return tmpParent.children[j];
            }
            else
            {
                tmpParent = tmpParent.InnerAddMapChild();
                tmpParent.SetName(tmp);
                return tmpParent;
            }
        }

        public void SetAsNull()
        {
            Clear();
            this.innerValue = null;
            this.valueType = MsgPackType.Null;
        }

        public void SetAsString(String value)
        {
            this.innerValue = value;
            this.valueType = MsgPackType.String;
        }

        public String GetAsString()
        {
            if (this.innerValue == null)
            {
                return "";
            }
            else
            {
                return this.innerValue.ToString();
            }

        }

        public void SetAsBoolean(Boolean bVal)
        {
            this.valueType = MsgPackType.Boolean;
            this.innerValue = bVal;
        }

        public void SetAsSingle(Single fVal)
        {
            this.valueType = MsgPackType.Single;
            this.innerValue = fVal;
        }

        public void SetAsFloat(Double fVal)
        {
            this.valueType = MsgPackType.Float;
            this.innerValue = fVal;
        }



        public void DecodeFromBytes(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bytes = Zip.Decompress(bytes);
                ms.Write(bytes, 0, bytes.Length);
                ms.Position = 0;
                DecodeFromStream(ms);
            }
        }

        public void DecodeFromFile(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open);
            DecodeFromStream(fs);
            fs.Dispose();
        }



        public void DecodeFromStream(Stream ms)
        {
            byte lvByte = (byte)ms.ReadByte();
            byte[] rawByte = null;
            MsgPack msgPack = null;
            int len = 0;
            int i = 0;

            if (lvByte <= 0x7F)
            {   //positive fixint	0xxxxxxx	0x00 - 0x7f
                SetAsInteger(lvByte);
            }
            else if ((lvByte >= 0x80) && (lvByte <= 0x8F))
            {
                //fixmap	1000xxxx	0x80 - 0x8f
                this.Clear();
                this.valueType = MsgPackType.Map;
                len = lvByte - 0x80;
                for (i = 0; i < len; i++)
                {
                    msgPack = InnerAdd();
                    msgPack.SetName(ReadTools.ReadString(ms));
                    msgPack.DecodeFromStream(ms);
                }
            }
            else if ((lvByte >= 0x90) && (lvByte <= 0x9F))  //fixarray	1001xxxx	0x90 - 0x9f
            {
                //fixmap	1000xxxx	0x80 - 0x8f
                this.Clear();
                this.valueType = MsgPackType.Array;
                len = lvByte - 0x90;
                for (i = 0; i < len; i++)
                {
                    msgPack = InnerAdd();
                    msgPack.DecodeFromStream(ms);
                }
            }
            else if ((lvByte >= 0xA0) && (lvByte <= 0xBF))  // fixstr	101xxxxx	0xa0 - 0xbf
            {
                len = lvByte - 0xA0;
                SetAsString(ReadTools.ReadString(ms, len));
            }
            else if ((lvByte >= 0xE0) && (lvByte <= 0xFF))
            {   /// -1..-32
                //  negative fixnum stores 5-bit negative integer
                //  +--------+
                //  |111YYYYY|
                //  +--------+                
                SetAsInteger((sbyte)lvByte);
            }
            else if (lvByte == 0xC0)
            {
                SetAsNull();
            }
            else if (lvByte == 0xC1)
            {
                throw new Exception("(never used) type $c1");
            }
            else if (lvByte == 0xC2)
            {
                SetAsBoolean(false);
            }
            else if (lvByte == 0xC3)
            {
                SetAsBoolean(true);
            }
            else if (lvByte == 0xC4)
            {  // max 255
                len = ms.ReadByte();
                rawByte = new byte[len];
                ms.Read(rawByte, 0, len);
                SetAsBytes(rawByte);
            }
            else if (lvByte == 0xC5)
            {  // max 65535                
                rawByte = new byte[2];
                ms.Read(rawByte, 0, 2);
                rawByte = BytesTools.SwapBytes(rawByte);
                len = BitConverter.ToUInt16(rawByte, 0);

                // read binary
                rawByte = new byte[len];
                ms.Read(rawByte, 0, len);
                SetAsBytes(rawByte);
            }
            else if (lvByte == 0xC6)
            {  // binary max: 2^32-1                
                rawByte = new byte[4];
                ms.Read(rawByte, 0, 4);
                rawByte = BytesTools.SwapBytes(rawByte);
                len = BitConverter.ToInt32(rawByte, 0);

                // read binary
                rawByte = new byte[len];
                ms.Read(rawByte, 0, len);
                SetAsBytes(rawByte);
            }
            else if ((lvByte == 0xC7) || (lvByte == 0xC8) || (lvByte == 0xC9))
            {
                throw new Exception("(ext8,ext16,ex32) type $c7,$c8,$c9");
            }
            else if (lvByte == 0xCA)
            {  // float 32              
                rawByte = new byte[4];
                ms.Read(rawByte, 0, 4);
                rawByte = BytesTools.SwapBytes(rawByte);

                SetAsSingle(BitConverter.ToSingle(rawByte, 0));
            }
            else if (lvByte == 0xCB)
            {  // float 64              
                rawByte = new byte[8];
                ms.Read(rawByte, 0, 8);
                rawByte = BytesTools.SwapBytes(rawByte);
                SetAsFloat(BitConverter.ToDouble(rawByte, 0));
            }
            else if (lvByte == 0xCC)
            {  // uint8   
                //      uint 8 stores a 8-bit unsigned integer
                //      +--------+--------+
                //      |  0xcc  |ZZZZZZZZ|
                //      +--------+--------+
                lvByte = (byte)ms.ReadByte();
                SetAsInteger(lvByte);
            }
            else if (lvByte == 0xCD)
            {  // uint16      
                //    uint 16 stores a 16-bit big-endian unsigned integer
                //    +--------+--------+--------+
                //    |  0xcd  |ZZZZZZZZ|ZZZZZZZZ|
                //    +--------+--------+--------+
                rawByte = new byte[2];
                ms.Read(rawByte, 0, 2);
                rawByte = BytesTools.SwapBytes(rawByte);
                SetAsInteger(BitConverter.ToUInt16(rawByte, 0));
            }
            else if (lvByte == 0xCE)
            {
                //  uint 32 stores a 32-bit big-endian unsigned integer
                //  +--------+--------+--------+--------+--------+
                //  |  0xce  |ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ
                //  +--------+--------+--------+--------+--------+
                rawByte = new byte[4];
                ms.Read(rawByte, 0, 4);
                rawByte = BytesTools.SwapBytes(rawByte);
                SetAsInteger(BitConverter.ToUInt32(rawByte, 0));
            }
            else if (lvByte == 0xCF)
            {
                //  uint 64 stores a 64-bit big-endian unsigned integer
                //  +--------+--------+--------+--------+--------+--------+--------+--------+--------+
                //  |  0xcf  |ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|
                //  +--------+--------+--------+--------+--------+--------+--------+--------+--------+
                rawByte = new byte[8];
                ms.Read(rawByte, 0, 8);
                rawByte = BytesTools.SwapBytes(rawByte);
                SetAsUInt64(BitConverter.ToUInt64(rawByte, 0));
            }
            else if (lvByte == 0xDC)
            {
                //      +--------+--------+--------+~~~~~~~~~~~~~~~~~+
                //      |  0xdc  |YYYYYYYY|YYYYYYYY|    N objects    |
                //      +--------+--------+--------+~~~~~~~~~~~~~~~~~+
                rawByte = new byte[2];
                ms.Read(rawByte, 0, 2);
                rawByte = BytesTools.SwapBytes(rawByte);
                len = BitConverter.ToInt16(rawByte, 0);

                this.Clear();
                this.valueType = MsgPackType.Array;
                for (i = 0; i < len; i++)
                {
                    msgPack = InnerAdd();
                    msgPack.DecodeFromStream(ms);
                }
            }
            else if (lvByte == 0xDD)
            {
                //  +--------+--------+--------+--------+--------+~~~~~~~~~~~~~~~~~+
                //  |  0xdd  |ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|    N objects    |
                //  +--------+--------+--------+--------+--------+~~~~~~~~~~~~~~~~~+
                rawByte = new byte[4];
                ms.Read(rawByte, 0, 4);
                rawByte = BytesTools.SwapBytes(rawByte);
                len = BitConverter.ToInt16(rawByte, 0);

                this.Clear();
                this.valueType = MsgPackType.Array;
                for (i = 0; i < len; i++)
                {
                    msgPack = InnerAdd();
                    msgPack.DecodeFromStream(ms);
                }
            }
            else if (lvByte == 0xD9)
            {
                //  str 8 stores a byte array whose length is upto (2^8)-1 bytes:
                //  +--------+--------+========+
                //  |  0xd9  |YYYYYYYY|  data  |
                //  +--------+--------+========+
                SetAsString(ReadTools.ReadString(lvByte, ms));
            }
            else if (lvByte == 0xDE)
            {
                //    +--------+--------+--------+~~~~~~~~~~~~~~~~~+
                //    |  0xde  |YYYYYYYY|YYYYYYYY|   N*2 objects   |
                //    +--------+--------+--------+~~~~~~~~~~~~~~~~~+
                rawByte = new byte[2];
                ms.Read(rawByte, 0, 2);
                rawByte = BytesTools.SwapBytes(rawByte);
                len = BitConverter.ToInt16(rawByte, 0);

                this.Clear();
                this.valueType = MsgPackType.Map;
                for (i = 0; i < len; i++)
                {
                    msgPack = InnerAdd();
                    msgPack.SetName(ReadTools.ReadString(ms));
                    msgPack.DecodeFromStream(ms);
                }
            }
            else if (lvByte == 0xDE)
            {
                //    +--------+--------+--------+~~~~~~~~~~~~~~~~~+
                //    |  0xde  |YYYYYYYY|YYYYYYYY|   N*2 objects   |
                //    +--------+--------+--------+~~~~~~~~~~~~~~~~~+
                rawByte = new byte[2];
                ms.Read(rawByte, 0, 2);
                rawByte = BytesTools.SwapBytes(rawByte);
                len = BitConverter.ToInt16(rawByte, 0);

                this.Clear();
                this.valueType = MsgPackType.Map;
                for (i = 0; i < len; i++)
                {
                    msgPack = InnerAdd();
                    msgPack.SetName(ReadTools.ReadString(ms));
                    msgPack.DecodeFromStream(ms);
                }
            }
            else if (lvByte == 0xDF)
            {
                //    +--------+--------+--------+--------+--------+~~~~~~~~~~~~~~~~~+
                //    |  0xdf  |ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|   N*2 objects   |
                //    +--------+--------+--------+--------+--------+~~~~~~~~~~~~~~~~~+
                rawByte = new byte[4];
                ms.Read(rawByte, 0, 4);
                rawByte = BytesTools.SwapBytes(rawByte);
                len = BitConverter.ToInt32(rawByte, 0);

                this.Clear();
                this.valueType = MsgPackType.Map;
                for (i = 0; i < len; i++)
                {
                    msgPack = InnerAdd();
                    msgPack.SetName(ReadTools.ReadString(ms));
                    msgPack.DecodeFromStream(ms);
                }
            }
            else if (lvByte == 0xDA)
            {
                //      str 16 stores a byte array whose length is upto (2^16)-1 bytes:
                //      +--------+--------+--------+========+
                //      |  0xda  |ZZZZZZZZ|ZZZZZZZZ|  data  |
                //      +--------+--------+--------+========+
                SetAsString(ReadTools.ReadString(lvByte, ms));
            }
            else if (lvByte == 0xDB)
            {
                //  str 32 stores a byte array whose length is upto (2^32)-1 bytes:
                //  +--------+--------+--------+--------+--------+========+
                //  |  0xdb  |AAAAAAAA|AAAAAAAA|AAAAAAAA|AAAAAAAA|  data  |
                //  +--------+--------+--------+--------+--------+========+
                SetAsString(ReadTools.ReadString(lvByte, ms));
            }
            else if (lvByte == 0xD0)
            {
                //      int 8 stores a 8-bit signed integer
                //      +--------+--------+
                //      |  0xd0  |ZZZZZZZZ|
                //      +--------+--------+
                SetAsInteger((sbyte)ms.ReadByte());
            }
            else if (lvByte == 0xD1)
            {
                //    int 16 stores a 16-bit big-endian signed integer
                //    +--------+--------+--------+
                //    |  0xd1  |ZZZZZZZZ|ZZZZZZZZ|
                //    +--------+--------+--------+
                rawByte = new byte[2];
                ms.Read(rawByte, 0, 2);
                rawByte = BytesTools.SwapBytes(rawByte);
                SetAsInteger(BitConverter.ToInt16(rawByte, 0));
            }
            else if (lvByte == 0xD2)
            {
                //  int 32 stores a 32-bit big-endian signed integer
                //  +--------+--------+--------+--------+--------+
                //  |  0xd2  |ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|
                //  +--------+--------+--------+--------+--------+
                rawByte = new byte[4];
                ms.Read(rawByte, 0, 4);
                rawByte = BytesTools.SwapBytes(rawByte);
                SetAsInteger(BitConverter.ToInt32(rawByte, 0));
            }
            else if (lvByte == 0xD3)
            {
                //  int 64 stores a 64-bit big-endian signed integer
                //  +--------+--------+--------+--------+--------+--------+--------+--------+--------+
                //  |  0xd3  |ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|ZZZZZZZZ|
                //  +--------+--------+--------+--------+--------+--------+--------+--------+--------+
                rawByte = new byte[8];
                ms.Read(rawByte, 0, 8);
                rawByte = BytesTools.SwapBytes(rawByte);
                SetAsInteger(BitConverter.ToInt64(rawByte, 0));
            }
        }

        public byte[] Encode2Bytes()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Encode2Stream(ms);
                byte[] r = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(r, 0, (int)ms.Length);
                return Zip.Compress(r);
            }
        }

        public void Encode2Stream(Stream ms)
        {
            switch (this.valueType)
            {
                case MsgPackType.Unknown:
                case MsgPackType.Null:
                    WriteTools.WriteNull(ms);
                    break;
                case MsgPackType.String:
                    WriteTools.WriteString(ms, (String)this.innerValue);
                    break;
                case MsgPackType.Integer:
                    WriteTools.WriteInteger(ms, (Int64)this.innerValue);
                    break;
                case MsgPackType.UInt64:
                    WriteTools.WriteUInt64(ms, (UInt64)this.innerValue);
                    break;
                case MsgPackType.Boolean:
                    WriteTools.WriteBoolean(ms, (Boolean)this.innerValue);
                    break;
                case MsgPackType.Float:
                    WriteTools.WriteFloat(ms, (Double)this.innerValue);
                    break;
                case MsgPackType.Single:
                    WriteTools.WriteFloat(ms, (Single)this.innerValue);
                    break;
                case MsgPackType.DateTime:
                    WriteTools.WriteInteger(ms, GetAsInteger());
                    break;
                case MsgPackType.Binary:
                    WriteTools.WriteBinary(ms, (byte[])this.innerValue);
                    break;
                case MsgPackType.Map:
                    WriteMap(ms);
                    break;
                case MsgPackType.Array:
                    WirteArray(ms);
                    break;
                default:
                    WriteTools.WriteNull(ms);
                    break;
            }
        }

        public String AsString
        {
            get
            {
                return GetAsString();
            }
            set
            {
                SetAsString(value);
            }
        }

        public Int64 AsInteger
        {
            get { return GetAsInteger(); }
            set { SetAsInteger((Int64)value); }
        }

        public Double AsFloat
        {
            get { return GetAsFloat(); }
            set { SetAsFloat(value); }
        }
        public MsgPackArray AsArray
        {
            get
            {
                lock (this)
                {
                    if (refAsArray == null)
                    {
                        refAsArray = new MsgPackArray(this, children);
                    }
                }
                return refAsArray;
            }
        }


        public MsgPackType ValueType
        {
            get { return valueType; }
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return new MsgPackEnum(children);
        }
    }
}