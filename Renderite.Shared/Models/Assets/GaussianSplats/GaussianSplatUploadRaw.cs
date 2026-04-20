using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class GaussianSplatUploadRaw : GaussianSplatUpload
    {
        public SharedMemoryBufferDescriptor<byte> alphasBuffer;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(alphasBuffer);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref alphasBuffer);
        }
    }
}
