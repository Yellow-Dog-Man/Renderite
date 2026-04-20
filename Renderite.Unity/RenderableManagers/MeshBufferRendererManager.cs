using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Unity
{
    public class MeshBufferRendererManager : RenderableStateChangeManager<MeshRenderBufferRenderer,
        MeshRenderBufferUpdate, MeshRenderBufferState, EmptyUpdateData>
    {
        public MeshBufferRendererManager(RenderSpace space) : base(space)
        {
        }

        protected override void ApplyState(ref MeshRenderBufferState update, MeshRenderBufferRenderer handler, ref EmptyUpdateData updateData, MeshRenderBufferUpdate batch)
        {
            handler.ApplyState(ref update);
        }

        protected override int GetRenderableIndex(ref MeshRenderBufferState state) => state.renderableIndex;

        protected override EmptyUpdateData InitUpdateData(MeshRenderBufferUpdate batch) => default;
    }
}
