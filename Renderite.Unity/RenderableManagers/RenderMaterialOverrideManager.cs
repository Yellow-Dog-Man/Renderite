using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Unity
{
    public struct RenderMaterialOverrideUpdateData
    {
        public UnmanagedSpan<MaterialOverrideState> materialStates;
    }

    public class RenderMaterialOverrideManager : RenderableStateChangeManager<RenderMaterialOverrideRenderable,
        RenderMaterialOverridesUpdate, RenderMaterialOverrideState, RenderMaterialOverrideUpdateData>, IDisposable
    {
        public RenderMaterialOverrideManager(RenderSpace space) : base(space)
        {
        }

        protected override void ApplyState(ref RenderMaterialOverrideState update, RenderMaterialOverrideRenderable handler, ref RenderMaterialOverrideUpdateData updateData, RenderMaterialOverridesUpdate batch)
        {
            handler.ApplyState(ref update, updateData.materialStates);

            if (update.materrialOverrideCount > 0)
                updateData.materialStates = updateData.materialStates.Slice(update.materrialOverrideCount);
        }

        protected override int GetRenderableIndex(ref RenderMaterialOverrideState state) => state.renderableIndex;

        protected override RenderMaterialOverrideUpdateData InitUpdateData(RenderMaterialOverridesUpdate batch)
        {
            var data = new RenderMaterialOverrideUpdateData();

            data.materialStates = RenderingManager.Instance.SharedMemory.AccessDataUnmanaged(batch.materialOverrideStates);

            return data;
        }

        public void Dispose()
        {
            for (int i = 0; i < RenderableCount; i++)
                this[i].Remove(true);
        }
    }
}
