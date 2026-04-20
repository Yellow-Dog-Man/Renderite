using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Renderite.Shared
{
    public static class MathHelper
    {
        public static float FilterInvalid(float value, float fallback = default)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return fallback;

            return value;
        }

        public static int PixelsToBytes(int pixels, TextureFormat format) => (int)MathHelper.BitsToBytes(pixels * format.GetBitsPerPixel());

        public static double BitsToBytes(double bits) => bits / 8;
        public static double BytesToBits(double bytes) => bytes * 8;

        public static int BitsToBytes(int bits) => bits >> 3;
        public static int BytesToBits(int bytes) => bytes << 3;

        public static long BitsToBytes(long bits) => bits >> 3;
        public static long BytesToBits(long bytes) => bytes << 3;

        public static int AlignSize(int size, int blockSize) => size + ((blockSize - (size % blockSize)) % blockSize);
        public static RenderVector2i AlignSize(RenderVector2i size, RenderVector2i blockSize) =>
            new RenderVector2i(AlignSize(size.x, blockSize.x), AlignSize(size.y, blockSize.y));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CeilToInt(double val) => (int)Math.Ceiling(val);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RoundToInt(double val) => (int)(val + (val < 0 ? -0.5 : 0.5));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long RoundToLong(double val) => (long)(val + (val < 0 ? -0.5 : 0.5));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FloorToInt(double val) => (int)Math.Floor(val);

        public static int AlignToNextMultiple(int value, int alignment) => (int)(((value + alignment - 1) / alignment) * alignment);

        public static int NecessaryBits(ulong number)
        {
            int bits = 0;

            while (number != 0)
            {
                number >>= 1;
                bits++;
            }

            return bits;
        }

        public static bool HasFlag(this int flags, int index)
        {
            var mask = 1 << index;

            return (flags & mask) != 0;
        }

        public static void SetFlag(this ref int flags, int index, bool state)
        {
            var mask = 1 << index;

            if (state)
                flags |= mask;
            else
                flags &= ~mask;
        }

        public static bool HasFlag(this ushort flags, int index)
        {
            var mask = 1 << index;

            return (flags & mask) != 0;
        }

        public static void SetFlag(this ref ushort flags, int index, bool state)
        {
            var mask = 1 << index;

            if (state)
                flags = (ushort)(flags | mask);
            else
                flags = (ushort)((flags) & (~mask));
        }


        public static bool HasFlag(this byte flags, int index)
        {
            var mask = 1 << index;

            return (flags & mask) != 0;
        }

        public static void SetFlag(this ref byte flags, int index, bool state)
        {
            var mask = 1 << index;

            if (state)
                flags = (byte)(flags | mask);
            else
                flags = (byte)((flags) & (~mask));
        }
    }
}
