using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class RendererParentWindow : RendererCommand
    {
        public long windowHandle;

        public override void Pack(ref MemoryPacker packer)
        {
            packer.Write(windowHandle);
        }

        public override void Unpack(ref MemoryUnpacker unpacker)
        {
            unpacker.Read(ref windowHandle);
        }
    }
}
