using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Unity
{
    public abstract class Asset
    {
        public int AssetId { get; private set; } = -1;

        public void AssignId(int assetId)
        {
            if (AssetId >= 0)
                throw new InvalidOperationException("AssetId was already assigned");

            AssetId = assetId;
        }

        public AssetIntegrator AssetIntegrator => RenderingManager.Instance.AssetIntegrator;
    }
}
