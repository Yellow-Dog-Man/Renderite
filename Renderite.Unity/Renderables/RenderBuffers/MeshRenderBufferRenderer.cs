using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using Renderite.Shared;

namespace Renderite.Unity
{
    public class MeshRenderBufferRenderer : ParticleBasedPointRenderBufferRenderer<MeshRenderBufferState>
    {
        protected override RotationMode RotationHandling => rotationMode;

        RotationMode rotationMode;


        protected override void ParticleSystemAllocated(ParticleSystem system, ParticleSystemRenderer renderer)
        {
            base.ParticleSystemAllocated(system, renderer);

            renderer.renderMode = ParticleSystemRenderMode.Mesh;
        }

        protected override void ApplyState(ParticleSystem system, ParticleSystemRenderer renderer,
            ref MeshRenderBufferState state)
        {
            renderer.sharedMaterial = RenderingManager.Instance.Materials.Materials.GetAsset(state.materialAssetId)?.Material;
            renderer.mesh = RenderingManager.Instance.Meshes.GetAsset(state.meshAssetId)?.Mesh;

            renderer.allowRoll = state.alignment != Renderite.Shared.MeshAlignment.Facing;

            switch (state.alignment)
            {
                default:
                case Renderite.Shared.MeshAlignment.View:
                    renderer.alignment = ParticleSystemRenderSpace.View;
                    rotationMode = RotationMode.EulerAngles;
                    break;

                case Renderite.Shared.MeshAlignment.Facing:
                    renderer.alignment = ParticleSystemRenderSpace.Facing;
                    rotationMode = RotationMode.EulerAngles;
                    break;

                case Renderite.Shared.MeshAlignment.Local:
                    renderer.alignment = ParticleSystemRenderSpace.Local;
                    rotationMode = RotationMode.EulerAngles;
                    break;

                case Renderite.Shared.MeshAlignment.Global:
                    renderer.alignment = ParticleSystemRenderSpace.Local;
                    rotationMode = RotationMode.EulerAngles;
                    break;
            }
        }

        protected override PointRenderBufferAsset ExtractBuffer(ref MeshRenderBufferState state)
        {
            return RenderingManager.Instance.PointRenderBuffers.GetAsset(state.pointRenderBufferAssetId);
        }
    }
}
