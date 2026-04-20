using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Unity
{
    public struct RenderTransformOverrideUpdateData
    {
        public UnmanagedSpan<int> skinnedMeshIndexes;
    }

    public class RenderTransformOverrideManager : RenderableStateChangeManager<RenderTransformOverrideRenderable,
        RenderTransformOverridesUpdate, RenderTransformOverrideState, RenderTransformOverrideUpdateData>, IDisposable
    {
        public RenderTransformOverrideManager(RenderSpace space) : base(space)
        {
        }

        protected override void ApplyState(ref RenderTransformOverrideState update, RenderTransformOverrideRenderable handler, ref RenderTransformOverrideUpdateData updateData, RenderTransformOverridesUpdate batch)
        {
            handler.ApplyState(ref update, updateData.skinnedMeshIndexes);

            if(update.skinnedMeshRendererCount > 0)
                updateData.skinnedMeshIndexes = updateData.skinnedMeshIndexes.Slice(update.skinnedMeshRendererCount);
        }

        protected override int GetRenderableIndex(ref RenderTransformOverrideState state) => state.renderableIndex;

        protected override RenderTransformOverrideUpdateData InitUpdateData(RenderTransformOverridesUpdate batch)
        {
            var data = new RenderTransformOverrideUpdateData();

            data.skinnedMeshIndexes = RenderingManager.Instance.SharedMemory.AccessDataUnmanaged(batch.skinnedMeshRenderersIndexes);

            return data;
        }

        public void Dispose()
        {
            for (int i = 0; i < RenderableCount; i++)
                this[i].Remove(true);
        }
    }
}
