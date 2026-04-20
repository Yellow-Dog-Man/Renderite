using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Renderite.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct TrailsRendererState
    {
        /// <summary>
        /// Th index of the renderable this is for
        /// </summary>
        [FieldOffset(0)]
        public int renderableIndex;

        /// <summary>
        /// The AssetID of the trails render buffer asset containing the data to render
        /// </summary>
        [FieldOffset(4)]
        public int trailsRenderBufferAssetId;

        /// <summary>
        /// The asset ID of the material to use to render this render buffer
        /// </summary>
        [FieldOffset(8)]
        public int materialAssetId;

        /// <summary>
        /// How is the texture UV mapped on the trails
        /// </summary>
        [FieldOffset(12)]
        public TrailTextureMode textureMode;

        /// <summary>
        /// Motion vectors for the billboards
        /// </summary>
        [FieldOffset(13)]
        public MotionVectorMode motionVectorMode;

        /// <summary>
        /// Whether to generate lighting data (like normals) for the trails
        /// </summary>
        [FieldOffset(14)]
        public bool generateLightingData;
    }
}
