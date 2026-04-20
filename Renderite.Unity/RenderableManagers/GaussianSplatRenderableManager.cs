using System;
using System.Collections.Generic;
using System.Text;
using Renderite.Shared;
using UnityEngine;

namespace Renderite.Unity
{
    public class GaussianSplatRenderableManager : RenderableStateChangeManager<GaussianSplatRenderable,
        GaussianSplatRenderablesUpdate, GaussianSplatRendererState, EmptyUpdateData>
    {
        public GaussianSplatRenderableManager(RenderSpace space) : base(space)
        {
        }

        protected override void ApplyState(ref GaussianSplatRendererState update, GaussianSplatRenderable handler, ref EmptyUpdateData updateData, GaussianSplatRenderablesUpdate batch)
        {
            handler.ApplyState(ref update);
        }

        protected override int GetRenderableIndex(ref GaussianSplatRendererState state) => state.renderableIndex;
        protected override EmptyUpdateData InitUpdateData(GaussianSplatRenderablesUpdate batch) => default;
    }
}
