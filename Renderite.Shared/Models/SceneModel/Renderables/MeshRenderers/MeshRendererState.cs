using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Renderite.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 24)]
    public struct MeshRendererState
    {
        /// <summary>
        /// This identifies the mesh renderer whose state is this for
        /// </summary>
        [FieldOffset(0)]
        public int renderableIndex;

        /// <summary>
        /// Index of the mesh asset that this mesh renderer renders
        /// </summary>
        [FieldOffset(4)]
        public int meshAssetId;

        /// <summary>
        /// Number of materials that this mesh renderer has. The actual material assignments are
        /// done through a separate buffer since they can be variable.
        /// </summary>
        [FieldOffset(8)]
        public int materialCount;

        /// <summary>
        /// Number of material property blocks that this mesh renderer has. The actual assigments
        /// are done through separate buffer as well.
        /// </summary>
        [FieldOffset(12)]
        public int materialPropertyBlockCount;

        /// <summary>
        /// Sorting order of this mesh within the render queue
        /// </summary>
        [FieldOffset(16)]
        public int sortingOrder;

        /// <summary>
        /// How are shadows rendered for this mesh
        /// </summary>
        [FieldOffset(20)]
        public ShadowCastMode shadowCastMode;

        /// <summary>
        /// How are motion vectors rendered for this mesh
        /// </summary>
        [FieldOffset(21)]
        public MotionVectorMode motionVectorMode;
    }
}
