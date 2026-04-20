using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public struct SkinnedMeshRealtimeBoundsUpdate
    {
        /// <summary>
        /// The ID of the skinned mesh renderer
        /// </summary>
        public int renderableIndex;

        /// <summary>
        /// The bounds computed by the renderer which should be filled in
        /// </summary>
        public RenderBoundingBox computedGlobalBounds;
    }
}
