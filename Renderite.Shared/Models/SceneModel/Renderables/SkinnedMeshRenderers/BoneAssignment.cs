using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Renderite.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 12)]
    public struct BoneAssignment
    {
        /// <summary>
        /// The ID of the skinned mesh renderer
        /// </summary>
        [FieldOffset(0)]
        public int renderableIndex;

        /// <summary>
        /// This is the transform ID of the root bone. This doesn't have to be actual bone necessarily.
        /// It's stored separately in here since there's always one, so it's clear that it is there, rather
        /// than storing it as "first" bone - and also because skinned meshes that are blendshapes only will
        /// need one too, but won't have any actual bones
        /// </summary>
        [FieldOffset(4)]
        public int rootBoneTransformId;

        /// <summary>
        /// How many bones are being assigned. The actual bones are provided in separate buffer, consisting of
        /// list of transform indexes to assign to each bone slot
        /// </summary>
        [FieldOffset(8)]
        public int boneCount;
    }
}
