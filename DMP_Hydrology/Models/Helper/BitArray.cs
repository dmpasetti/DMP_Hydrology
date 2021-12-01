using System;
using System.Collections;

namespace USP_Hydrology
{
    public static partial class Helper
    {
        public static BitArray ToBitArray(this Byte value)
        {
            var b = BitConverter.GetBytes(value);
            var bs = new Byte[1] { b[0] };
            return new BitArray(bs);
        }
        public static BitArray ToBitArray(this Int16 value)
        {
            var b = BitConverter.GetBytes(value);
            var bs = new Byte[2] { b[0], b[1] };
            return new BitArray(bs);
        }
        public static BitArray ToBitArray(this Int32 value)
        {
            var b = BitConverter.GetBytes(value);
            var bs = new Byte[4] { b[0], b[1], b[2], b[3] };
            return new BitArray(bs);
        }
        public static BitArray ToBitArray(this Int64 value)
        {
            var b = BitConverter.GetBytes(value);
            var bs = new Byte[8] { b[0], b[1], b[2], b[3], b[4], b[5], b[6], b[7] };
            return new BitArray(bs);
        }

        public static Byte ToByte(this BitArray value)
        {
            var b = new Byte[1];
            value.CopyTo(b, 0);
            return b[0];
        }
        public static Int16 ToInt16(this BitArray value)
        {
            var b = new Byte[2];
            value.CopyTo(b, 0);
            return BitConverter.ToInt16(b, 0);
        }
        public static Int32 ToInt32(this BitArray value)
        {
            var b = new Byte[4];
            value.CopyTo(b, 0);
            return BitConverter.ToInt32(b, 0);
        }
        public static Int64 ToInt64(this BitArray value)
        {
            var b = new Byte[8];
            value.CopyTo(b, 0);
            return BitConverter.ToInt64(b, 0);
        }
    }
}
