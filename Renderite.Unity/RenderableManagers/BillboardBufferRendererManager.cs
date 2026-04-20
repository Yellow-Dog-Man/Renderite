using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Unity
{
    public class BillboardBufferRendererManager : RenderableStateChangeManager<BillboardRenderBufferRenderer,
        BillboardRenderBufferUpdate, BillboardRenderBufferState, EmptyUpdateData>
    {
        public BillboardBufferRendererManager(RenderSpace space) : base(space)
        {
        }

        protected override void ApplyState(ref BillboardRenderBufferState update, BillboardRenderBufferRenderer handler, ref EmptyUpdateData updateData, BillboardRenderBufferUpdate batch)
        {
            handler.ApplyState(ref update);
        }

        protected override int GetRenderableIndex(ref BillboardRenderBufferState state) => state.renderableIndex;

        protected override EmptyUpdateData InitUpdateData(BillboardRenderBufferUpdate batch) => default;
    }
}
