using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public abstract class RenderBufferUpload : AssetCommand
    {
        
        public SharedMemoryBufferDescriptor<byte> buffer;


        public bool IsEmpty => buffer.IsEmpty;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(buffer);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref buffer);
        }
    }
}
