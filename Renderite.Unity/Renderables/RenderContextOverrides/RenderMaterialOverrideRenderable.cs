using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Unity
{
    public class RenderMaterialOverrideRenderable : RenderContextOverride
    {
        class MaterialOverride
        {
            public int index;
            public UnityEngine.Material original;
            public UnityEngine.Material replacement;
        }

        IMeshRenderable targetMesh;
        List<MaterialOverride> overrides = new List<MaterialOverride>();

        protected override void Override()
        {
            var renderer = targetMesh?.Renderer;

            if (renderer == null)
                return;

            UnityEngine.Material[] materials = renderer.sharedMaterials;

            foreach (var o in overrides)
            {
                if (o.index < 0 || o.index >= materials.Length)
                    continue;

                o.original = materials[o.index];
                materials[o.index] = o.replacement;
            }

            renderer.sharedMaterials = materials;
        }

        protected override void Restore()
        {
            var renderer = targetMesh?.Renderer;

            if (renderer == null)
                return;

            UnityEngine.Material[] materials = renderer.sharedMaterials;

            foreach (var o in overrides)
            {
                if (o.index < 0 || o.index >= materials.Length)
                    continue;

                materials[o.index] = o.original;
                o.original = null;
            }

            renderer.sharedMaterials = materials;
        }

        public void ApplyState(ref RenderMaterialOverrideState state, UnmanagedSpan<MaterialOverrideState> materialOverrides)
        {
            BeginUpdateSetup(state.context);

            var mesh = MeshRendererHelper.GetMeshRenderable(state.packedMeshRendererIndex, Space);

            UpdateSetup(mesh, materialOverrides.Slice(0, state.materrialOverrideCount));

            FinishUpdateSetup();
        }

        protected void UpdateSetup(IMeshRenderable renderer, UnmanagedSpan<MaterialOverrideState> newOverrides)
        {
            targetMesh = renderer;

            while (overrides.Count > newOverrides.Length)
                overrides.RemoveAt(overrides.Count - 1);

            while (newOverrides.Length > overrides.Count)
                overrides.Add(new MaterialOverride());

            var materials = RenderingManager.Instance.Materials.Materials;

            for (int i = 0; i < newOverrides.Length; i++)
            {
                var target = overrides[i];
                var source = newOverrides[i];

                target.index = source.materialSlotIndex;
                target.replacement = materials.GetAsset(source.materialAssetId)?.Material ?? RenderingManager.Instance.NullMaterial;
            }
        }
    }
}
