using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class GaussianSplatConfig : RendererCommand
    {
        public float sortingMegaOperationsPerCamera;

        public override void Pack(ref MemoryPacker packer)
        {
            packer.Write(sortingMegaOperationsPerCamera);
        }

        public override void Unpack(ref MemoryUnpacker unpacker)
        {
            unpacker.Read(ref sortingMegaOperationsPerCamera);
        }
    }
}
