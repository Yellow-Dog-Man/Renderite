using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;


namespace Renderite.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct RenderableHandle
    {
        /// <summary>
        /// ID of the render space this renderable belongs to
        /// </summary>
        [FieldOffset(0)]
        public int renderSpaceId;

        /// <summary>
        /// Index of the renderable itself
        /// </summary>
        [FieldOffset(4)]
        public int renderableIndex;

        public RenderableHandle(int renderSpaceId, int renderableIndex)
        {
            this.renderSpaceId = renderSpaceId;
            this.renderableIndex = renderableIndex;
        }
    }
}
