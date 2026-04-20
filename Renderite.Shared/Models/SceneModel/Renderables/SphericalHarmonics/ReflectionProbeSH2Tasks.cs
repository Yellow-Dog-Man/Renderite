using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class ReflectionProbeSH2Tasks : RenderablesUpdate
    {
        /// <summary>
        /// Buffer containing tasks for computing SH2 from reflection probes, as well as
        /// data for the results which will be filled by the renderer
        /// </summary>
        
        public SharedMemoryBufferDescriptor<ReflectionProbeSH2Task> tasks;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(tasks);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref tasks);
        }
    }
}
