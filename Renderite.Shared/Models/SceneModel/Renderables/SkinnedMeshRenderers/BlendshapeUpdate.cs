using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Renderite.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct BlendshapeUpdate
    {
        /// <summary>
        /// The index of the blendshape on the mesh that's being updated
        /// </summary>
        [FieldOffset(0)]
        public int blendshapeIndex;

        /// <summary>
        /// Weight of the blendshape to set it to
        /// </summary>
        [FieldOffset(4)]
        public float weight;
    }
}
