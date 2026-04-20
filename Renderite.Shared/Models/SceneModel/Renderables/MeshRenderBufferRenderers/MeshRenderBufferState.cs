using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Renderite.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 20)]
    public struct MeshRenderBufferState
    {
        /// <summary>
        /// The index of the renderable this is for
        /// </summary>
        [FieldOffset(0)]
        public int renderableIndex;

        /// <summary>
        /// The AssetID of the point rendr buffer asset containing the data to render
        /// </summary>
        [FieldOffset(4)]
        public int pointRenderBufferAssetId;

        /// <summary>
        /// The asset ID of the material to use to render this render buffer
        /// </summary>
        [FieldOffset(8)]
        public int materialAssetId;

        /// <summary>
        /// The asset ID of the mesh used to render these particles
        /// </summary>
        [FieldOffset(12)]
        public int meshAssetId;

        /// <summary>
        /// Alignment mode for the renered meshes
        /// </summary>
        [FieldOffset(16)]
        public MeshAlignment alignment;
    }
}
