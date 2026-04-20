using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class LightsBufferRendererConsumed : RendererCommand
    {
        /// <summary>
        /// Which lights buffer renderer is this submission for
        /// </summary>
        
        public int globalUniqueId;

        public override void Pack(ref MemoryPacker packer)
        {
            packer.Write(globalUniqueId);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            packer.Read(ref globalUniqueId);
        }
    }
}
