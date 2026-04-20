using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class LODGroupRenderablesUpdate : RenderablesStateUpdate<LODGroupState>
    {
        /// <summary>
        /// Buffer containing the LOD states for each LOD group
        /// </summary>
        public SharedMemoryBufferDescriptor<LODState> lodStates;

        /// <summary>
        /// Mesh renderer IDs for each of the LOD states
        /// </summary>
        public SharedMemoryBufferDescriptor<int> packedMeshRendererIds;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(lodStates);
            packer.Write(packedMeshRendererIds);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref lodStates);
            packer.Read(ref packedMeshRendererIds);
        }
    }
}
