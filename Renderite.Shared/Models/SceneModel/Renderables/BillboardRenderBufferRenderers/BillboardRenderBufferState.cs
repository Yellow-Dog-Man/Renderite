using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Renderite.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 24)]
    public struct BillboardRenderBufferState
    {
        /// <summary>
        /// Th index of the renderable this is for
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
        /// Min size of the billboards on the screen
        /// </summary>
        [FieldOffset(12)]
        public float minBillboardScreenSize;

        /// <summary>
        /// Max size of the billboards on the screen
        /// </summary>
        [FieldOffset(16)]
        public float maxBillboardScreenSize;

        /// <summary>
        /// Alignment mode for the renered billboards
        /// </summary>
        [FieldOffset(20)]
        public BillboardAlignment alignment;

        /// <summary>
        /// Motion vectors for the billboards
        /// </summary>
        [FieldOffset(21)]
        public MotionVectorMode motionVectorMode;
    }
}
