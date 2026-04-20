using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class VideoAudioTrack : IMemoryPackable
    {
        public int index;
        public int channelCount;
        public int sampleRate;

        public string name;
        public string languageCode;

        public void Pack(ref MemoryPacker packer)
        {
            packer.Write(index);
            packer.Write(channelCount);
            packer.Write(sampleRate);

            packer.Write(name);
            packer.Write(languageCode);
        }

        public void Unpack(ref MemoryUnpacker unpacker)
        {
            unpacker.Read(ref index);
            unpacker.Read(ref channelCount);
            unpacker.Read(ref sampleRate);

            unpacker.Read(ref name);
            unpacker.Read(ref languageCode);
        }

        public override string ToString() => $"Audio Track {index}. Channels: {channelCount}, SampleRate: {sampleRate}, Name: {name}, Language: {languageCode}";
    }
}
