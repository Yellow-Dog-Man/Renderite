using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public class ReflectionProbeManager : RenderableStateChangeManager<ReflectionProbeRenderable, ReflectionProbeRenderablesUpdate,
        ReflectionProbeState, EmptyUpdateData>
    {
        public ReflectionProbeManager(RenderSpace space) : base(space)
        {
        }

        public void HandleRenderTasks(List<ReflectionProbeRenderTask> tasks)
        {
            foreach (var task in tasks)
                this[task.renderableIndex].RenderToTexture(task);
        }

        protected override void ApplyState(ref ReflectionProbeState update, ReflectionProbeRenderable probeHandler, ref EmptyUpdateData updateData, ReflectionProbeRenderablesUpdate batch)
        {
            var cubemaps = RenderingManager.Instance.Cubemaps;

            probeHandler.ApplyState(ref update, cubemaps);
        }

        protected override void ApplyUpdate(ReflectionProbeRenderablesUpdate updateBatch)
        {
            // Update the states first
            base.ApplyUpdate(updateBatch);

            if (!updateBatch.changedProbesToRender.IsEmpty)
            {
                var probes = RenderingManager.Instance.SharedMemory.AccessData(updateBatch.changedProbesToRender);

                for (int i = 0; i < probes.Length; i++)
                {
                    ref var task = ref probes[i];

                    if (task.renderableIndex < 0)
                        break;

                    var probeHandler = this[task.renderableIndex];

                    probeHandler.StartRender(task.uniqueId);
                }
            }
        }

        protected override int GetRenderableIndex(ref ReflectionProbeState state) => state.renderableIndex;
        protected override EmptyUpdateData InitUpdateData(ReflectionProbeRenderablesUpdate batch) => default;
    }
}
