using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;


namespace Renderite.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public readonly struct VertexAttributeDescriptor : IEquatable<VertexAttributeDescriptor>
    {
        [FieldOffset(0)]
        public readonly VertexAttributeType attribute;

        [FieldOffset(2)]
        public readonly VertexAttributeFormat format;

        [FieldOffset(4)]
        public readonly int dimensions;

        public int Size => format.GetSize() * dimensions;

        public VertexAttributeDescriptor(VertexAttributeType attribute, VertexAttributeFormat format, int dimensions)
        {
            this.attribute = attribute;
            this.format = format;
            this.dimensions = dimensions;
        }

        public bool Equals(VertexAttributeDescriptor other)
        {
            return attribute == other.attribute &&
                format == other.format &&
                dimensions == other.dimensions;
        }

        public static bool operator ==(VertexAttributeDescriptor left, VertexAttributeDescriptor right) => left.Equals(right);
        public static bool operator !=(VertexAttributeDescriptor left, VertexAttributeDescriptor right) => !(left == right);

        public override string ToString() => $"Attribute: {attribute}, Format: {format} x {dimensions}";
    }
}
