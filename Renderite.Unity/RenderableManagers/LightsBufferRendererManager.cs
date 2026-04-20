using System;
using System.Collections.Generic;
using System.Text;
using Renderite.Shared;
using UnityEngine;

namespace Renderite.Unity
{
    public class LightsBufferRendererManager : RenderableStateChangeManager<LightsBufferRenderer, LightsBufferRendererUpdate, LightsBufferRendererState, EmptyUpdateData>
    {
        public LightsBufferRendererManager(RenderSpace space) : base(space)
        {
        }

        protected override void ApplyState(ref LightsBufferRendererState update, LightsBufferRenderer handler, ref EmptyUpdateData updateData, LightsBufferRendererUpdate batch)
        {
            handler.ApplyState(ref update);
        }

        protected override int GetRenderableIndex(ref LightsBufferRendererState state) => state.renderableIndex;
        protected override EmptyUpdateData InitUpdateData(LightsBufferRendererUpdate batch) => default;
    }
}
