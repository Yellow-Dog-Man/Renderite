using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public enum VertexAttributeFormat : short
    {
        Float32,
        Half16,

        UNorm8,
        UNorm16,

        SInt8,
        SInt16,
        SInt32,

        UInt8,
        UInt16,
        UInt32,
    }

    public static class VertexAttributeFormatHelper
    {
        public static unsafe int GetSize(this VertexAttributeFormat format)
        {
            return format switch
            {
                VertexAttributeFormat.Float32 => sizeof(float),
                VertexAttributeFormat.Half16 => sizeof(ushort),

                VertexAttributeFormat.UNorm8 => sizeof(byte),
                VertexAttributeFormat.UNorm16 => sizeof(ushort),

                VertexAttributeFormat.UInt8 => sizeof(byte),
                VertexAttributeFormat.UInt16 => sizeof(ushort),
                VertexAttributeFormat.UInt32 => sizeof(uint),

                VertexAttributeFormat.SInt8 => sizeof(sbyte),
                VertexAttributeFormat.SInt16 => sizeof(short),
                VertexAttributeFormat.SInt32 => sizeof(int),

                _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
            };
        }
    }
}
