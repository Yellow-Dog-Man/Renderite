using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public class LODGroupRenderable : Renderable
    {
        LODGroup lodGroup;

        public void ApplyState(ref LODGroupState state, ref UnmanagedSpan<LODState> lodStates, ref UnmanagedSpan<int> rendererIds)
        {
            var lods = new LOD[state.lodCount];

            int rendererIndex = 0;

            for(int i = 0; i < lods.Length; i++)
            {
                ref var lod = ref lods[i];
                var lodState = lodStates[i];

                lod.screenRelativeTransitionHeight = lodState.screenRelativeTransitionHeight;
                lod.fadeTransitionWidth = lodState.fadeTransitionWidth;

                var renderers = new Renderer[lodState.rendererCount];

                for(int r = 0; r < renderers.Length; r++)
                    renderers[r] = MeshRendererHelper.GetMeshRenderable(rendererIds[rendererIndex++], Space)?.Renderer;

                lod.renderers = renderers;
            }

            lodGroup.SetLODs(lods);
            lodGroup.RecalculateBounds();

            // Slice the spans with states to consume the data we used
            if(state.lodCount > 0)
                lodStates = lodStates.Slice(state.lodCount);

            if(rendererIndex > 0)
                rendererIds = rendererIds.Slice(rendererIndex);
        }

        protected override void Setup(Transform root)
        {
            lodGroup = root.gameObject.AddComponent<LODGroup>();
        }

        protected override void Cleanup()
        {
            UnityEngine.Object.Destroy(lodGroup);
        }
    }
}
