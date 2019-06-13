using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.MessagePack
{
    public enum MsgPackType
    {
        Unknown = 0,
        Null = 1,
        Map = 2,
        Array = 3,
        String = 4,
        Integer = 5,
        UInt64 = 6,
        Boolean = 7,
        Float = 8,
        Single = 9,
        DateTime = 10,
        Binary = 11
    }
}