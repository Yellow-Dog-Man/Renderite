using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class ResolutionConfig : RendererCommand
    {
        
        public RenderVector2i resolution;

        
        public bool fullscreen;

        public override void Pack(ref MemoryPacker packer)
        {
            packer.Write(resolution);
            packer.Write(fullscreen);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            packer.Read(ref resolution);
            packer.Read(ref fullscreen);
        }
    }
}
