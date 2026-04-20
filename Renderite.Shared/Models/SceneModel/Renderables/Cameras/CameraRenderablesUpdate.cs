using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class CameraRenderablesUpdate : RenderablesStateUpdate<CameraState>
    {
        /// <summary>
        /// Buffer containing transform ID's for selective/excluded rendering
        /// </summary>
        
        public SharedMemoryBufferDescriptor<int> transformIds;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(transformIds);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref transformIds);
        }
    }
}
