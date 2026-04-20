using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class RenderTransformOverridesUpdate : RenderContextOverridesUpdate<RenderTransformOverrideState>
    {
        public SharedMemoryBufferDescriptor<int> skinnedMeshRenderersIndexes;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(skinnedMeshRenderersIndexes);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref skinnedMeshRenderersIndexes);
        }
    }
}
