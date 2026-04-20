using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class GaussianSplatUploadEncoded : GaussianSplatUpload
    {
        public GaussianVectorFormat positionsFormat;
        public GaussianRotationFormat rotationsFormat;
        public GaussianVectorFormat scalesFormat;
        public GaussianColorFormat colorsFormat;
        public GaussianSHFormat shFormat;

        public int texture2DtextureAssetId;
        public int shIndexesOffset;
        public int chunkCount;

        public SharedMemoryBufferDescriptor<byte> shBuffer;
        public SharedMemoryBufferDescriptor<byte> chunksBuffer;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(positionsFormat);
            packer.Write(rotationsFormat);
            packer.Write(scalesFormat);
            packer.Write(colorsFormat);
            packer.Write(shFormat);

            packer.Write(texture2DtextureAssetId);
            packer.Write(shIndexesOffset);
            packer.Write(chunkCount);

            packer.Write(shBuffer);
            packer.Write(chunksBuffer);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref positionsFormat);
            packer.Read(ref rotationsFormat);
            packer.Read(ref scalesFormat);
            packer.Read(ref colorsFormat);
            packer.Read(ref shFormat);

            packer.Read(ref texture2DtextureAssetId);
            packer.Read(ref shIndexesOffset);
            packer.Read(ref chunkCount);

            packer.Read(ref shBuffer);
            packer.Read(ref chunksBuffer);
        }
    }
}
