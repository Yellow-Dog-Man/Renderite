using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Unity
{
    public class LODGroupRenderableManager :
        RenderableStateChangeManager<LODGroupRenderable, LODGroupRenderablesUpdate, LODGroupState, LODGroupRenderableManager.UpdateState>
    {
        public struct UpdateState
        {
            public UnmanagedSpan<LODState> lodStates;
            public UnmanagedSpan<int> rendererIds;
        }

        public LODGroupRenderableManager(RenderSpace space) : base(space)
        {
        }

        protected override void ApplyState(ref LODGroupState update, LODGroupRenderable handler, ref UpdateState updateData, LODGroupRenderablesUpdate batch)
        {
            handler.ApplyState(ref update, ref updateData.lodStates, ref updateData.rendererIds);
        }

        protected override int GetRenderableIndex(ref LODGroupState state) => state.renderableIndex;

        protected override UpdateState InitUpdateData(LODGroupRenderablesUpdate batch)
        {
            var state = new UpdateState();

            state.lodStates = RenderingManager.Instance.SharedMemory.AccessDataUnmanaged(batch.lodStates);
            state.rendererIds = RenderingManager.Instance.SharedMemory.AccessDataUnmanaged(batch.packedMeshRendererIds);

            return state;
        }
    }
}
