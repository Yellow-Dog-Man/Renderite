using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public struct LODState
    {
        public float screenRelativeTransitionHeight;
        public float fadeTransitionWidth;

        /// <summary>
        /// The actual renderer indexes are stored in separate buffer
        /// </summary>
        public int rendererCount;
    }
}
