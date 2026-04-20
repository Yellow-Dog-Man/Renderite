using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class TouchState : IMemoryPackable
    {
        public int touchId;

        public RenderVector2 position;
        public bool isPressing;
        public float pressure;

        public void Pack(ref MemoryPacker packer)
        {
            packer.Write(touchId);

            packer.Write(position);
            packer.Write(isPressing);
            packer.Write(pressure);
        }

        public void Unpack(ref MemoryUnpacker unpacker)
        {
            unpacker.Read(ref touchId);

            unpacker.Read(ref position);
            unpacker.Read(ref isPressing);
            unpacker.Read(ref pressure);
        }
    }
}
