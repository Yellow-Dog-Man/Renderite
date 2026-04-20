using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class LightsBufferRendererSubmission : RendererCommand
    {
        /// <summary>
        /// Which lights buffer renderer is this submission for.
        /// IMPORTANT!!! This is not a renderable index, but rather a unique ID for renderable.
        /// This is done because the renderableindex can change when they're moved around and
        /// could get out of sync.
        /// </summary>
        
        public int lightsBufferUniqueId = -1;

        /// <summary>
        /// How many lights there are
        /// </summary>
        
        public int lightsCount;

        /// <summary>
        /// Buffer of lights to render
        /// </summary>
        
        public SharedMemoryBufferDescriptor<LightData> lights;

        public override void Pack(ref MemoryPacker packer)
        {
            packer.Write(lightsBufferUniqueId);
            packer.Write(lightsCount);
            packer.Write(lights);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            packer.Read(ref lightsBufferUniqueId);
            packer.Read(ref lightsCount);
            packer.Read(ref lights);
        }
    }
}
