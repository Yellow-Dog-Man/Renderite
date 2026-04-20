using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public struct GaussianSplatRendererState
    {
        public int renderableIndex;

        public int gaussianSplatAssetId;

        public float sizeScale;
        public float opacityScale;

        public int maxSHOrder;

        public bool sphericalHamornicsOnly;
    }
}
