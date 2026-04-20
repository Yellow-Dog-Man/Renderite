using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class QualityConfig : RendererCommand
    {
        public int perPixelLights;
        public ShadowCascadeMode shadowCascades;
        public ShadowResolutionMode shadowResolution;
        public float shadowDistance;

        public SkinWeightMode skinWeightMode;

        public override void Pack(ref MemoryPacker packer)
        {
            packer.Write(perPixelLights);
            packer.Write(shadowCascades);
            packer.Write(shadowResolution);
            packer.Write(shadowDistance);
            packer.Write(skinWeightMode);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            packer.Read(ref perPixelLights);
            packer.Read(ref shadowCascades);
            packer.Read(ref shadowResolution);
            packer.Read(ref shadowDistance);
            packer.Read(ref skinWeightMode);
        }
    }
}
