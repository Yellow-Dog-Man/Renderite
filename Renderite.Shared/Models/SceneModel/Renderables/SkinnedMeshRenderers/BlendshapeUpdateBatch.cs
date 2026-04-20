using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Renderite.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct BlendshapeUpdateBatch
    {
        /// <summary>
        /// The ID of the skinned mesh renderer
        /// </summary>
        [FieldOffset(0)]
        public int renderableIndex;

        /// <summary>
        /// Number of blendshapes being updated for this mesh
        /// </summary>
        [FieldOffset(4)]
        public int blendshapeUpdateCount;
    }
}
