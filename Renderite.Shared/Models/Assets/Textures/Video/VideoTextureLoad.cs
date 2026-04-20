using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class VideoTextureLoad : AssetCommand
    {
        public string source;
        public string overrideEngine;
        public string mimeType;
        public bool isStream;

        public int audioSystemSampleRate;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(source);
            packer.Write(overrideEngine);
            packer.Write(mimeType);
            packer.Write(isStream);
            packer.Write(audioSystemSampleRate);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref source);
            packer.Read(ref overrideEngine);
            packer.Read(ref mimeType);
            packer.Read(ref isStream);
            packer.Read(ref audioSystemSampleRate);
        }
    }
}
