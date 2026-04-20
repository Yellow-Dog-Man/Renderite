using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public struct LODGroupState
    {
        /// <summary>
        /// Index of the renderable this state is for
        /// </summary>
        public int renderableIndex;

        /// <summary>
        /// How many LOD's does this one have. The actual LOD definitions are stored in another buffer
        /// </summary>
        public int lodCount;

        public bool crossFade;
        public bool animateCrossFading;
    }
}
