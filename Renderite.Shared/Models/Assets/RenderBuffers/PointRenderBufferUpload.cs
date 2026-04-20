using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class PointRenderBufferUpload : RenderBufferUpload
    {
        
        public int count;

        
        public int positionsOffset;
        
        public int rotationsOffset;
        
        public int sizesOffset;
        
        public int colorsOffset;
        
        public int frameIndexesOffset;

        
        public RenderVector2i frameGridSize;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(count);

            packer.Write(positionsOffset);
            packer.Write(rotationsOffset);
            packer.Write(sizesOffset);
            packer.Write(colorsOffset);
            packer.Write(frameIndexesOffset);

            packer.Write(frameGridSize);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref count);

            packer.Read(ref positionsOffset);
            packer.Read(ref rotationsOffset);
            packer.Read(ref sizesOffset);
            packer.Read(ref colorsOffset);
            packer.Read(ref frameIndexesOffset);

            packer.Read(ref frameGridSize);
        }
    }
}
