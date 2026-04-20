using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class ReflectionProbeRenderResult : RendererCommand
    {
        /// <summary>
        /// Unique identifier for this render task
        /// </summary>
        
        public int renderTaskId = -1;

        /// <summary>
        /// Indicates if the render was successful and the data is valid.
        /// If this is false, then the data will likely be garbage and shouldn't be used
        /// </summary>
        
        public bool success;

        public override void Pack(ref MemoryPacker packer)
        {
            packer.Write(renderTaskId);
            packer.Write(success);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            packer.Read(ref renderTaskId);
            packer.Read(ref success);
        }
    }
}
