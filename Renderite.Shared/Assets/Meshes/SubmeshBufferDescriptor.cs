using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;


namespace Renderite.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 36)]
    public struct SubmeshBufferDescriptor : IEquatable<SubmeshBufferDescriptor>
    {
        [FieldOffset(0)]
        public SubmeshTopology topology;

        [FieldOffset(4)]
        public int indexStart;

        [FieldOffset(8)]
        public int indexCount;

        [FieldOffset(12)]
        public RenderBoundingBox bounds;

        public int EndIndex => indexStart + indexCount;

        public SubmeshBufferDescriptor(SubmeshTopology topology, int indexStart, int indexCount, RenderBoundingBox bounds)
        {
            this.topology = topology;
            this.indexStart = indexStart;
            this.indexCount = indexCount;
            this.bounds = bounds;
        }

        public bool Equals(SubmeshBufferDescriptor other)
        {
            return topology == other.topology &&
                indexStart == other.indexStart &&
                indexCount == other.indexCount &&
                bounds.Equals(other.bounds);
        }

        public static bool operator ==(SubmeshBufferDescriptor left, SubmeshBufferDescriptor right) => left.Equals(right);
        public static bool operator !=(SubmeshBufferDescriptor left, SubmeshBufferDescriptor right) => !(left == right);

        public override string ToString() => $"Topology: {topology}. Index Start: {indexStart}, Count: {indexCount},  EndIndex: {EndIndex}, Bounds: {bounds}";
    }
}
