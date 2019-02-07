using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace StreamLibrary.src
{
    public unsafe class MurmurHash2Unsafe
    {
        const UInt32 m = 0x5bd1e995;
        const Int32 r = 24;

        public unsafe UInt32 Hash(Byte* data, int length)
        {
            if (length == 0)
                return 0;
            UInt32 h = 0xc58f1a7b ^ (UInt32)length;
            Int32 remainingBytes = length & 3; // mod 4
            Int32 numberOfLoops = length >> 2; // div 4
            UInt32* realData = (UInt32*)data;
            while (numberOfLoops != 0)
            {
                UInt32 k = *realData;
                k *= m;
                k ^= k >> r;
                k *= m;

                h *= m;
                h ^= k;
                numberOfLoops--;
                realData++;
            }
            switch (remainingBytes)
            {
                case 3:
                    h ^= (UInt16)(*realData);
                    h ^= ((UInt32)(*(((Byte*)(realData)) + 2))) << 16;
                    h *= m;
                    break;
                case 2:
                    h ^= (UInt16)(*realData);
                    h *= m;
                    break;
                case 1:
                    h ^= *((Byte*)realData);
                    h *= m;
                    break;
                default:
                    break;
            }

            // Do a few final mixes of the hash to ensure the last few
            // bytes are well-incorporated.

            h ^= h >> 13;
            h *= m;
            h ^= h >> 15;

            return h;
        }
    }
}