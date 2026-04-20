using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public abstract class AssetCommand : RendererCommand
    {
        public int assetId = -1;

        public override void Pack(ref MemoryPacker packer)
        {
            packer.Write(assetId);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            packer.Read(ref assetId);
        }
    }
}
