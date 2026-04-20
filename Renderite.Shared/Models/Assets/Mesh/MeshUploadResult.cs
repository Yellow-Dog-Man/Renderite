using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class MeshUploadResult : AssetCommand
    {
        public bool instanceChanged;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(instanceChanged);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref instanceChanged);
        }
    }
}
