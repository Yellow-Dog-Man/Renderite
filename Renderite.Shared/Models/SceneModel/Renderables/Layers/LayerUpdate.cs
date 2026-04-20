using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class LayerUpdate : RenderablesUpdate
    {
        /// <summary>
        /// Buffer containing assignments to a type of layer to all the newly added
        /// renderables
        /// </summary>
        
        public SharedMemoryBufferDescriptor<LayerType> layerAssignments;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(layerAssignments);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref layerAssignments);
        }
    }
}
