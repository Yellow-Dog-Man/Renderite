using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class RenderDecouplingConfig : RendererCommand
    {
        public float decoupleActivateInterval;
        public float decoupledMaxAssetProcessingTime; 
        public int recoupleFrameCount;

        public override void Pack(ref MemoryPacker packer)
        {
            packer.Write(decoupleActivateInterval);
            packer.Write(decoupledMaxAssetProcessingTime);
            packer.Write(recoupleFrameCount);
        }

        public override void Unpack(ref MemoryUnpacker unpacker)
        {
            unpacker.Read(ref decoupleActivateInterval);
            unpacker.Read(ref decoupledMaxAssetProcessingTime);
            unpacker.Read(ref recoupleFrameCount);
        }
    }
}
