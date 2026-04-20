using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class VideoTextureReady : AssetCommand
    {
        public double length;
        public RenderVector2i size;
        public bool hasAlpha;

        public string playbackEngine;

        public bool instanceChanged;

        public List<VideoAudioTrack> audioTracks;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(length);
            packer.Write(size);
            packer.Write(hasAlpha);

            packer.Write(playbackEngine);

            packer.Write(instanceChanged);

            packer.WriteObjectList(audioTracks);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref length);
            packer.Read(ref size);
            packer.Read(ref hasAlpha);

            packer.Read(ref playbackEngine);

            packer.Read(ref instanceChanged);

            packer.ReadObjectList(ref audioTracks);
        }
    }
}
