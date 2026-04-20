using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Renderite.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct BoneWeight : IEquatable<BoneWeight>
    {
        [FieldOffset(0)]
        public float weight;

        [FieldOffset(4)]
        public int boneIndex;

        public BoneWeight(float weight, int boneIndex)
        {
            this.weight = weight;
            this.boneIndex = boneIndex;
        }

        public bool Equals(BoneWeight other) => weight == other.weight && boneIndex == other.boneIndex;

        public override bool Equals(object obj)
        {
            if (obj is BoneWeight weight)
                return this.Equals(weight);

            return false;
        }

        public override int GetHashCode()
        {
            int hashCode = 2127128854;
            hashCode = hashCode * -1521134295 + weight.GetHashCode();
            hashCode = hashCode * -1521134295 + boneIndex.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(BoneWeight left, BoneWeight right) => left.Equals(right);
        public static bool operator !=(BoneWeight left, BoneWeight right) => !(left == right);
    }
}
