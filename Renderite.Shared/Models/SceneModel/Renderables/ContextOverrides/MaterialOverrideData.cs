using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public struct MaterialOverrideState
    {
        /// <summary>
        /// Index of the material slot that's being overriden
        /// </summary>
        public int materialSlotIndex;

        /// <summary>
        /// ID of the material to override with
        /// </summary>
        public int materialAssetId;
    }
}
