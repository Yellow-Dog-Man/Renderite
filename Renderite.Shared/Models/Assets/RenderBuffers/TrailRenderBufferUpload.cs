using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class TrailRenderBufferUpload : RenderBufferUpload
    {
        
        public int trailsCount;
        
        public int trailPointCount;

        
        public int trailsOffset;
        
        public int positionsOffset;
        
        public int colorsOffset;
        
        public int sizesOffset;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(trailsCount);
            packer.Write(trailPointCount);

            packer.Write(trailsOffset);
            packer.Write(positionsOffset);
            packer.Write(colorsOffset);
            packer.Write(sizesOffset);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref trailsCount);
            packer.Read(ref trailPointCount);

            packer.Read(ref trailsOffset);
            packer.Read(ref positionsOffset);
            packer.Read(ref colorsOffset);
            packer.Read(ref sizesOffset);
        }
    }
}
