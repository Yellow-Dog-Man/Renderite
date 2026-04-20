using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class DesktopTexturePropertiesUpdate : AssetCommand
    {
        public RenderVector2i size;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(size);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref size);
        }
    }
}
