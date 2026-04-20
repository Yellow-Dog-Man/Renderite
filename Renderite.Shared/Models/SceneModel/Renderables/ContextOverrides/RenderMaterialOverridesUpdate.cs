using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class RenderMaterialOverridesUpdate : RenderContextOverridesUpdate<RenderMaterialOverrideState>
    {
        public SharedMemoryBufferDescriptor<MaterialOverrideState> materialOverrideStates;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(materialOverrideStates);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref materialOverrideStates);
        }
    }
}
