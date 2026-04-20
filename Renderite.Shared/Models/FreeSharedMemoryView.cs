using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class FreeSharedMemoryView : RendererCommand
    {
        /// <summary>
        /// ID of the shared memory buffer to release
        /// </summary>
        public int bufferId;

        public override void Pack(ref MemoryPacker packer)
        {
            packer.Write(bufferId);
        }

        public override void Unpack(ref MemoryUnpacker unpacker)
        {
            unpacker.Read(ref bufferId);
        }
    }
}
