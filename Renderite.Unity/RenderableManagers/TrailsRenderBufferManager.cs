using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Unity
{
    public class TrailsBufferRendererManager : RenderableStateChangeManager<TrailsRenderBufferRenderer,
        TrailsRendererUpdate, TrailsRendererState, EmptyUpdateData>
    {
        public TrailsBufferRendererManager(RenderSpace space) : base(space)
        {
        }

        protected override void ApplyState(ref TrailsRendererState update, TrailsRenderBufferRenderer handler, ref EmptyUpdateData updateData, TrailsRendererUpdate batch)
        {
            handler.ApplyState(ref update);
        }

        protected override int GetRenderableIndex(ref TrailsRendererState state) => state.renderableIndex;

        protected override EmptyUpdateData InitUpdateData(TrailsRendererUpdate batch) => default;
    }
}
