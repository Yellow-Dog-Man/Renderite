using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class MeshRenderablesUpdate : RenderablesUpdate
    {
        /// <summary>
        /// Buffer describing updates mesh renderer states
        /// </summary>
        
        public SharedMemoryBufferDescriptor<MeshRendererState> meshStates;

        /// <summary>
        /// This buffer contains indexes of material/property block assetIDs for any materials that need to be assigned
        /// as part of the mesh renderer states.
        /// </summary>
        
        public SharedMemoryBufferDescriptor<int> meshMaterialsAndPropertyBlocks;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(meshStates);
            packer.Write(meshMaterialsAndPropertyBlocks);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref meshStates);
            packer.Read(ref meshMaterialsAndPropertyBlocks);
        }
    }
}
