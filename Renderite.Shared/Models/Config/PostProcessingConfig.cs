using System;
using System.Collections.Generic;
using System.Text;



namespace Renderite.Shared
{
    public class PostProcessingConfig : RendererCommand
    {
        
        public float motionBlurIntensity;

        
        public float bloomIntensity;

        
        public float ambientOcclusionIntensity;

        
        public bool screenSpaceReflections;

        
        public AntiAliasingMethod antialiasing;

        public override void Pack(ref MemoryPacker packer)
        {
            packer.Write(motionBlurIntensity);
            packer.Write(bloomIntensity);
            packer.Write(ambientOcclusionIntensity);
            packer.Write(screenSpaceReflections);
            packer.Write(antialiasing);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            packer.Read(ref motionBlurIntensity);
            packer.Read(ref bloomIntensity);
            packer.Read(ref ambientOcclusionIntensity);
            packer.Read(ref screenSpaceReflections);
            packer.Read(ref antialiasing);
        }
    }
}
