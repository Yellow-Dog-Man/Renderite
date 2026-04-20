using EnumsNET;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Renderite.Shared
{
    public static class IdPacker<T>
        where T : struct, Enum
    {
        public static readonly int typeCount;
        public static readonly int typeBits;
        public static readonly int idBits;
        public static readonly int maxId;

        public static readonly int packTypeShift;
        public static readonly uint unpackMask;

        static IdPacker()
        {
            typeCount = EnumsNET.Enums.GetValues<T>().Count;
            typeBits = MathHelper.NecessaryBits((ulong)typeCount);
            idBits = 32 - typeBits;

            maxId = int.MaxValue >> typeBits;

            packTypeShift = 32 - typeBits;
            unpackMask = ~0U >> typeBits;
        }

        public static int Pack(int assetId, T type)
        {
            if (assetId > maxId)
                throw new NotSupportedException("AssetID exceeded maximum value");

            uint packed = (uint)assetId | (( Enums.ToUInt32Unsafe(type) ) << packTypeShift);

            return (int)packed;
        }

        public static void Unpack(int packed, out int id, out T type)
        {
            // Shift it back as unsigned - we don't want to extend the sign bit
            var p = (uint)packed;

            var typeInt = (p >> packTypeShift);

            type = Unsafe.As<uint, T>(ref typeInt);
            id = (int)(p & unpackMask);
        }
    }
}
