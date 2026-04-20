using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public struct UnitySkinnedMeshBoundsUpdate
    {
        /// <summary>
        /// The ID of the skinned mesh renderer
        /// </summary>
        public int renderableIndex;

        /// <summary>
        /// The local bounds to assign to the skinned mesh
        /// </summary>
        public Bounds localBounds;
    }
}
