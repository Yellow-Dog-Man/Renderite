using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public abstract class GaussianSplatUpload : AssetCommand
    {
        public int splatCount;
        public RenderBoundingBox bounds;

        public SharedMemoryBufferDescriptor<byte> positionsBuffer;
        public SharedMemoryBufferDescriptor<byte> rotationsBuffer;
        public SharedMemoryBufferDescriptor<byte> scalesBuffer;
        public SharedMemoryBufferDescriptor<byte> colorsBuffer;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(splatCount);
            packer.Write(bounds);

            packer.Write(positionsBuffer);
            packer.Write(rotationsBuffer);
            packer.Write(scalesBuffer);
            packer.Write(colorsBuffer);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref splatCount);
            packer.Read(ref bounds);

            packer.Read(ref positionsBuffer);
            packer.Read(ref rotationsBuffer);
            packer.Read(ref scalesBuffer);
            packer.Read(ref colorsBuffer);
        }
    }
}
