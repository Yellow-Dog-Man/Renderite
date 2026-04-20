using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public abstract class RenderablesUpdate : IMemoryPackable
    {
        /// <summary>
        /// This buffer describes any removed renderables. These are done in sequence, because the ID's will
        /// be remapped appropriately from the end. This way we don't need to explicitly set new ID's, things
        /// are kept in sync implicitly.
        /// </summary>
        
        public SharedMemoryBufferDescriptor<int> removals;

        /// <summary>
        /// This buffer describes new allocations of renderables. Each integer represent the transform ID that
        /// it will be bound to. We don't need to provide the IDs of the renderables, because those are sequential
        /// </summary>
        
        public SharedMemoryBufferDescriptor<int> additions;

        public virtual void Pack(ref MemoryPacker packer)
        {
            packer.Write(removals);
            packer.Write(additions);
        }

        public virtual void Unpack(ref MemoryUnpacker packer)
        {
            packer.Read(ref removals);
            packer.Read(ref additions);
        }
    }
}
