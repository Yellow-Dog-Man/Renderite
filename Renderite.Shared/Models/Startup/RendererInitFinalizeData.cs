using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    /// <summary>
    /// Once the engine itself is initialized, it'll send this message to let the renderer know that it's ready
    /// to start the main frame loop and hide the initialization visuals.
    /// </summary>
    public class RendererInitFinalizeData : RendererCommand
    {
        public override void Pack(ref MemoryPacker packer)
        {
            
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            
        }
    }
}
