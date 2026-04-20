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
    public class BillboardRenderBufferRenderer : ParticleBasedPointRenderBufferRenderer<BillboardRenderBufferState>
    {
        protected override RotationMode RotationHandling => rotationMode;

        RotationMode rotationMode;

        protected override PointRenderBufferAsset ExtractBuffer(ref BillboardRenderBufferState state)
        {
            return RenderingManager.Instance.PointRenderBuffers.GetAsset(state.pointRenderBufferAssetId);
        }

        protected override void ApplyState(UnityEngine.ParticleSystem system, ParticleSystemRenderer renderer,
            ref BillboardRenderBufferState state)
        {
            renderer.sharedMaterial = RenderingManager.Instance.Materials.Materials.GetAsset(state.materialAssetId)?.Material;
            renderer.motionVectorGenerationMode = state.motionVectorMode.ToUnity();
            renderer.minParticleSize = state.minBillboardScreenSize;
            renderer.maxParticleSize = state.maxBillboardScreenSize;

            renderer.allowRoll = state.alignment != Renderite.Shared.BillboardAlignment.Facing;

            switch (state.alignment)
            {
                default:
                case Renderite.Shared.BillboardAlignment.View:
                    renderer.alignment = ParticleSystemRenderSpace.View;
                    renderer.renderMode = ParticleSystemRenderMode.Billboard;
                    rotationMode = RotationMode.EulerAngles;
                    break;

                case Renderite.Shared.BillboardAlignment.Facing:
                    renderer.alignment = ParticleSystemRenderSpace.Facing;
                    renderer.renderMode = ParticleSystemRenderMode.Billboard;
                    rotationMode = RotationMode.EulerAngles;
                    break;

                case Renderite.Shared.BillboardAlignment.Local:
                    renderer.alignment = ParticleSystemRenderSpace.Velocity;
                    renderer.renderMode = ParticleSystemRenderMode.Billboard;
                    rotationMode = RotationMode.VelocityAndRotationForward;
                    break;

                case Renderite.Shared.BillboardAlignment.Global:
                    renderer.alignment = ParticleSystemRenderSpace.Velocity;
                    renderer.renderMode = ParticleSystemRenderMode.Billboard;
                    rotationMode = RotationMode.VelocityAndRotationForward;
                    break;

                case Renderite.Shared.BillboardAlignment.Direction:
                    renderer.alignment = ParticleSystemRenderSpace.View;
                    renderer.renderMode = ParticleSystemRenderMode.Stretch;
                    rotationMode = RotationMode.VelocityOnly;
                    break;
            }
        }
    }
}
