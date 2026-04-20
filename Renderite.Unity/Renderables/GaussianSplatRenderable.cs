using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public class GaussianSplatRenderable : Renderable
    {
        GaussianSplatRenderer renderer;

        protected override void Setup(Transform root)
        {
            renderer = root.gameObject.AddComponent<GaussianSplatRenderer>();
        }

        protected override void Cleanup()
        {
            UnityEngine.Object.Destroy(renderer);
        }

        public void ApplyState(ref GaussianSplatRendererState state)
        {
            renderer.Asset = RenderingManager.Instance.GaussianSplats.GetAsset(state.gaussianSplatAssetId);

            renderer.SplatScale = state.sizeScale;
            renderer.OpacityScale = state.opacityScale;
            renderer.SHOrder = Math.Min(state.maxSHOrder, 3);
            renderer.SHOnly = state.sphericalHamornicsOnly;
        }
    }
}
