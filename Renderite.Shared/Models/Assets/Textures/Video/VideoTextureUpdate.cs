using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class VideoTextureUpdate : AssetCommand
    {
        public double position;

        public bool play;
        public bool loop;

        public DateTime decodedTime;

        public double AdjustedPosition => position + (DateTime.UtcNow - decodedTime).TotalSeconds;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(position);
            packer.Write(play, loop);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref position);
            packer.Read(out play, out loop);

            decodedTime = DateTime.UtcNow;
        }
    }
}
