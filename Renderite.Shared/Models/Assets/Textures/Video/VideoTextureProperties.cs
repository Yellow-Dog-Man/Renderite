using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class VideoTextureProperties : AssetCommand
    {
        public TextureFilterMode filterMode;
        public int anisoLevel;

        public TextureWrapMode wrapU;
        public TextureWrapMode wrapV;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(filterMode);
            packer.Write(anisoLevel);

            packer.Write(wrapU);
            packer.Write(wrapV);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref filterMode);
            packer.Read(ref anisoLevel);

            packer.Read(ref wrapU);
            packer.Read(ref wrapV);
        }
    }
}
