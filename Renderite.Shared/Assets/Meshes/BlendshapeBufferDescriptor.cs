using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;


namespace Renderite.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public readonly struct BlendshapeBufferDescriptor : IEquatable<BlendshapeBufferDescriptor>
    {
        [FieldOffset(0)]
        public readonly int blendshapeIndex;

        [FieldOffset(4)]
        public readonly int frameIndex;

        [FieldOffset(8)]
        public readonly float frameWeight;

        [FieldOffset(12)]
        public readonly BlendshapeDataFlags dataFlags;

        public BlendshapeBufferDescriptor(BlendshapeDataFlags dataFlags, int blendshapeIndex, int frameIndex, float frameWeight)
        {
            this.dataFlags = dataFlags;
            this.blendshapeIndex = blendshapeIndex;
            this.frameIndex = frameIndex;
            this.frameWeight = frameWeight;
        }

        public bool Equals(BlendshapeBufferDescriptor other) =>
            dataFlags == other.dataFlags &&
            blendshapeIndex == other.blendshapeIndex &&
            frameIndex == other.frameIndex &&
            frameWeight == other.frameWeight;

        public static bool operator ==(BlendshapeBufferDescriptor left, BlendshapeBufferDescriptor right) => left.Equals(right);
        public static bool operator !=(BlendshapeBufferDescriptor left, BlendshapeBufferDescriptor right) => !(left == right);

        public override string ToString() => $"Flags: {dataFlags}, BlendshapeIndex: {blendshapeIndex}, FrameIndex: {frameIndex}, Weight: {frameWeight}";
    }
}
