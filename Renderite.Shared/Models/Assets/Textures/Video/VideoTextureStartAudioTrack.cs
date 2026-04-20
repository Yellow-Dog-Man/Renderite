using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class VideoTextureStartAudioTrack : AssetCommand
    {
        public int audioTrackIndex;

        public int queueCapacity;
        public string queueName;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(audioTrackIndex);

            packer.Write(queueCapacity);
            packer.Write(queueName);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref audioTrackIndex);

            packer.Read(ref queueCapacity);
            packer.Read(ref queueName);
        }
    }
}
