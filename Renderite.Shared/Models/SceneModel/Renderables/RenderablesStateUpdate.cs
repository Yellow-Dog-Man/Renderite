using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public abstract class RenderablesStateUpdate<TState> : RenderablesUpdate
        where TState : unmanaged
    {
        
        public SharedMemoryBufferDescriptor<TState> states;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(states);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref states);
        }
    }
}
