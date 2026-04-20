using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public class ReflectionProbeSH2Manager : RenderableManager<ReflectionProbeSH2Renderable, ReflectionProbeSH2Tasks>
    {
        public ReflectionProbeSH2Manager(RenderSpace space) : base(space)
        {
        }


        protected override ReflectionProbeSH2Renderable AllocateRenderable(Transform rootTransform, bool isInUse)
        {
            var probe = new ReflectionProbeSH2Renderable();
            probe.Setup(Space, rootTransform, !isInUse);
            return probe;
        }

        protected override void ApplyUpdate(ReflectionProbeSH2Tasks updateBatch)
        {
            if (!updateBatch.tasks.IsEmpty)
            {
                var updates = RenderingManager.Instance.SharedMemory.AccessData(updateBatch.tasks);
                var probes = Space.ReflectionProbes;

                for (int i = 0; i < updates.Length; i++)
                {
                    ref var update = ref updates[i];

                    if (update.renderableIndex < 0)
                        break;

                    var sh2Handler = this[update.renderableIndex];

                    if(update.reflectionProbeRenderableIndex < 0)
                    {
                        // Invalid probe, just fail the task
                        update.result = ComputeResult.Failed;
                        continue;
                    }

                    var probe = probes[update.reflectionProbeRenderableIndex];

                    if (RenderingManager.IsDebug)
                        Debug.Log($"[{RenderingManager.Instance.LastFrameIndex}] Computing SH2 for {update.renderableIndex}. ProbeIndex: {update.reflectionProbeRenderableIndex}.");

                    update.result = sh2Handler.Compute(probe.Probe, out update.resultData);

                    if (RenderingManager.IsDebug)
                        Debug.Log($"RESULT [{RenderingManager.Instance.LastFrameIndex}] - for computing SH2 for {update.renderableIndex}. ProbeIndex: {update.reflectionProbeRenderableIndex} - {update.result}\n{update.resultData}");
                }
            }
        }
    }
}
