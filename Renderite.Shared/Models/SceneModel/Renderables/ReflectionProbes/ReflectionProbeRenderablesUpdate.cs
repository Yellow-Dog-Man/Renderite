using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class ReflectionProbeRenderablesUpdate : RenderablesStateUpdate<ReflectionProbeState>
    {
        /// <summary>
        /// List of renderable indexes of probes that should be re-rendered in the OnChanges mode
        /// </summary>
        
        public SharedMemoryBufferDescriptor<ReflectionProbeChangeRenderTask> changedProbesToRender;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(changedProbesToRender);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref changedProbesToRender);
        }
    }
}
