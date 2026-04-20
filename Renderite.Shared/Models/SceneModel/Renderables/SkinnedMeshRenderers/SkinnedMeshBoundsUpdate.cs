using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public struct SkinnedMeshBoundsUpdate
    {
        /// <summary>
        /// The ID of the skinned mesh renderer
        /// </summary>
        public int renderableIndex;

        /// <summary>
        /// The local bounds to assign to the skinned mesh
        /// </summary>
        public RenderBoundingBox localBounds;
    }
}
